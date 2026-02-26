using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;
using System.Text.Json;
using static RajFabAPI.Constants.AppConstants;

namespace RajFabAPI.Services
{
    public class SteamPipeLineApplicationService : ISteamPipeLineApplicationService
    {
        private readonly ApplicationDbContext _dbcontext;
        private readonly IWebHostEnvironment _environment;

        public SteamPipeLineApplicationService(ApplicationDbContext context)
        {
            _dbcontext = context;
        }

        private async Task<string> GenerateApplicationNumberAsync(string type)
        {
            var year = DateTime.Now.Year;

            string prefix = type.ToLower() switch
            {
                "new" => $"STPL{year}/CIFB/",
                "amend" => $"STPLMAMD{year}/CIFB/",
                "renew" => $"STPLREN{year}/CIFB/",
                "close" => $"STPLCLOSE{year}/CIFB/",
                _ => throw new Exception("Invalid STPL application type")
            };

            var lastApp = await _dbcontext.SteamPipeLineApplications
                .Where(x => x.ApplicationId.StartsWith(prefix))
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.ApplicationId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastApp))
            {
                var lastPart = lastApp.Split('/').Last();
                if (int.TryParse(lastPart, out int lastNo))
                    nextNumber = lastNo + 1;
            }

            return $"{prefix}{nextNumber:D4}";
        }

        private async Task<string> GenerateSteamPipeLineRegistrationNoAsync()
        {
            var last = await _dbcontext.SteamPipeLineApplications
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.SteamPipeLineRegistrationNo)
                .FirstOrDefaultAsync();

            int next = 1;

            if (!string.IsNullOrEmpty(last))
            {
                var number = last.Split('-').Last();
                if (int.TryParse(number, out int n))
                    next = n + 1;
            }

            return $"STPL-{next:D5}";
        }

        public async Task<string> SaveSteamPipeLineAsync(  CreateSteamPipeLineDto dto,    string? type,  string? steamPipeLineRegistrationNo)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            type = type?.ToLower() ?? "new";

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                SteamPipeLineApplication? baseRecord = null;

                /* ==========================================================
                   ?? AMENDMENT BASED ON REGISTRATION NO
                ========================================================== */

                if (type == "amend")
                {
                    if (string.IsNullOrWhiteSpace(steamPipeLineRegistrationNo))
                        throw new Exception("SteamPipeLineRegistrationNo required for amendment.");

                    baseRecord = await _dbcontext.SteamPipeLineApplications
                        .Where(x =>
                            x.SteamPipeLineRegistrationNo == steamPipeLineRegistrationNo &&
                            x.Status == "Approved")
                        .OrderByDescending(x => x.Version)
                        .FirstOrDefaultAsync();

                    if (baseRecord == null)
                        throw new Exception("No approved record found for this registration.");

                    // Block multiple pending amendment
                    var pendingExists = await _dbcontext.SteamPipeLineApplications
                        .AnyAsync(x =>
                            x.SteamPipeLineRegistrationNo == steamPipeLineRegistrationNo &&
                            x.Status == "Pending" &&
                            x.Type == "amend");

                    if (pendingExists)
                        throw new Exception("Amendment already pending for this registration.");
                }

                /* ==========================================================
                   ?? GENERATE REGISTRATION NO
                ========================================================== */

                string registrationNo;

                if (type == "new")
                {
                    registrationNo = await GenerateSteamPipeLineRegistrationNoAsync();
                }
                else
                {
                    registrationNo = steamPipeLineRegistrationNo!;
                }

                /* ==========================================================
                   ?? GENERATE APPLICATION ID
                ========================================================== */

                var newApplicationId = await GenerateApplicationNumberAsync(type);

                /* ==========================================================
                   ?? VERSION LOGIC
                ========================================================== */

                var version = type == "amend"
                    ? baseRecord!.Version + 0.1m
                    : 1.0m;

                /* ==========================================================
                   ?? INSERT
                ========================================================== */

                var entity = new SteamPipeLineApplication
                {
                    Id = Guid.NewGuid(),
                   

                    ApplicationId = newApplicationId,
                    SteamPipeLineRegistrationNo = registrationNo,

                    BoilerApplicationNo = dto.BoilerApplicationNo ?? baseRecord?.BoilerApplicationNo,
                    ProposedLayoutDescription = dto.ProposedLayout ?? baseRecord?.ProposedLayoutDescription,
                    ConsentLetterProvided = dto.ConsentLetterProvided ?? baseRecord?.ConsentLetterProvided,
                    SteamPipeLineDrawingNo = dto.SteamPipeLineDrawingNo ?? baseRecord?.SteamPipeLineDrawingNo,
                    BoilerMakerRegistrationNo = dto.BoilerMakerRegistrationNo ?? baseRecord?.BoilerMakerRegistrationNo,
                    ErectorName = dto.ErectorName ?? baseRecord?.ErectorName,

                    FactoryRegistrationNumber = dto.FactoryRegistrationNumber ?? baseRecord?.FactoryRegistrationNumber,
                    Factorydetailjson = dto.Factorydetailjson ?? baseRecord?.Factorydetailjson,

                    PipeLengthUpTo100mm = dto.PipeLengthUpTo100mm ?? baseRecord?.PipeLengthUpTo100mm,
                    PipeLengthAbove100mm = dto.PipeLengthAbove100mm ?? baseRecord?.PipeLengthAbove100mm,

                    NoOfDeSuperHeaters = dto.NoOfDeSuperHeaters ?? baseRecord?.NoOfDeSuperHeaters,
                    NoOfSteamReceivers = dto.NoOfSteamReceivers ?? baseRecord?.NoOfSteamReceivers,
                    NoOfFeedHeaters = dto.NoOfFeedHeaters ?? baseRecord?.NoOfFeedHeaters,
                    NoOfSeparatelyFiredSuperHeaters =
                        dto.NoOfSeparatelyFiredSuperHeaters ?? baseRecord?.NoOfSeparatelyFiredSuperHeaters,

                    FormIIPath = dto.FormII ?? baseRecord?.FormIIPath,
                    FormIIIPath = dto.FormIII ?? baseRecord?.FormIIIPath,
                    FormIIIAPath = dto.FormIIIA ?? baseRecord?.FormIIIAPath,
                    FormIIIBPath = dto.FormIIIB ?? baseRecord?.FormIIIBPath,
                    FormIVPath = dto.FormIV ?? baseRecord?.FormIVPath,
                    FormIVAPath = dto.FormIVA ?? baseRecord?.FormIVAPath,
                    DrawingPath = dto.Drawing ?? baseRecord?.DrawingPath,
                    SupportingDocumentsPath = dto.SupportingDocuments ?? baseRecord?.SupportingDocumentsPath,

                    Type = type,
                    Version = version,
                    Status = "Pending",
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _dbcontext.SteamPipeLineApplications.Add(entity);
                await _dbcontext.SaveChangesAsync();

                await tx.CommitAsync();

                return entity.ApplicationId;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }



    }
}