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
                "amend" => $"STPLAMD{year}/CIFB/",
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

                string registrationNo;

                if (type == "new")
                {
                    registrationNo = await GenerateSteamPipeLineRegistrationNoAsync();
                }
                else
                {
                    registrationNo = steamPipeLineRegistrationNo!;
                }

                int renewalYears;
                DateTime? validFrom;
                DateTime? validUpto;

                if (type == "new")
                {
                    renewalYears = 1; // Always default 1 year for new

                    validFrom = DateTime.Now.Date;
                    validUpto = validFrom.Value.AddYears(1);
                }
                else if (type == "amend")
                {
                    renewalYears = baseRecord?.RenewalYears ?? 1;

                    validFrom = baseRecord?.ValidFrom;
                    validUpto = baseRecord?.ValidUpto;
                }
                else
                {
                    renewalYears = 1;
                    validFrom = null;
                    validUpto = null;
                }
                var newApplicationId = await GenerateApplicationNumberAsync(type);

                var version = type == "amend"  ? baseRecord!.Version + 0.1m : 1.0m;              

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

                    RenewalYears = renewalYears,
                    ValidFrom = validFrom,
                    ValidUpto = validUpto,

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


        public async Task<string> RenewSteamPipeLineAsync(  RenewSteamPipeLineDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                /* ================================================
                   ?? VALIDATION
                ================================================ */

                var pendingExists = await _dbcontext.SteamPipeLineApplications
                    .AnyAsync(x =>
                        x.SteamPipeLineRegistrationNo == dto.SteamPipeLineRegistrationNo &&
                        x.Status == "Pending");

                if (pendingExists)
                    throw new Exception("Previous renewal is still pending.");

                var lastApproved = await _dbcontext.SteamPipeLineApplications
                    .Where(x =>
                        x.SteamPipeLineRegistrationNo == dto.SteamPipeLineRegistrationNo &&
                        x.Status == "Approved")
                    .OrderByDescending(x => x.Version)
                    .FirstOrDefaultAsync();

                if (lastApproved == null)
                    throw new Exception("Approved Steam Pipe Line record not found.");

                /* ================================================
                   ?? VALIDITY CALCULATION
                ================================================ */

                int renewalYears = dto.RenewalYears > 0 ? dto.RenewalYears : 1;

                DateTime baseValidUpto =
                    lastApproved.ValidUpto ?? DateTime.Now;

                DateTime newValidUpto =
                    baseValidUpto.AddYears(renewalYears);

                /* ================================================
                   ?? CREATE NEW APPLICATION
                ================================================ */

                var newApplicationId =
                    await GenerateApplicationNumberAsync("renew");

                var entity = new SteamPipeLineApplication
                {
                    Id = Guid.NewGuid(),

                    ApplicationId = newApplicationId,
                    SteamPipeLineRegistrationNo =
                        lastApproved.SteamPipeLineRegistrationNo,

                    // ?? Clone All Fields
                    BoilerApplicationNo = lastApproved.BoilerApplicationNo,
                    ProposedLayoutDescription = lastApproved.ProposedLayoutDescription,
                    ConsentLetterProvided = lastApproved.ConsentLetterProvided,
                    SteamPipeLineDrawingNo = lastApproved.SteamPipeLineDrawingNo,
                    BoilerMakerRegistrationNo = lastApproved.BoilerMakerRegistrationNo,
                    ErectorName = lastApproved.ErectorName,

                    FactoryRegistrationNumber = lastApproved.FactoryRegistrationNumber,
                    Factorydetailjson = lastApproved.Factorydetailjson,

                    PipeLengthUpTo100mm = lastApproved.PipeLengthUpTo100mm,
                    PipeLengthAbove100mm = lastApproved.PipeLengthAbove100mm,

                    NoOfDeSuperHeaters = lastApproved.NoOfDeSuperHeaters,
                    NoOfSteamReceivers = lastApproved.NoOfSteamReceivers,
                    NoOfFeedHeaters = lastApproved.NoOfFeedHeaters,
                    NoOfSeparatelyFiredSuperHeaters =
                        lastApproved.NoOfSeparatelyFiredSuperHeaters,

                    FormIIPath = lastApproved.FormIIPath,
                    FormIIIPath = lastApproved.FormIIIPath,
                    FormIIIAPath = lastApproved.FormIIIAPath,
                    FormIIIBPath = lastApproved.FormIIIBPath,
                    FormIVPath = lastApproved.FormIVPath,
                    FormIVAPath = lastApproved.FormIVAPath,
                    DrawingPath = lastApproved.DrawingPath,

                    SupportingDocumentsPath =
                        dto.SupportingDocumentsPath ?? lastApproved.SupportingDocumentsPath,

                    // ?? Only Change
                    RenewalYears = renewalYears,
                    ValidFrom = lastApproved.ValidFrom,
                    ValidUpto = newValidUpto,

                    Type = "renew",
                    Version = Math.Round(lastApproved.Version + 0.1m, 1),
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

        public async Task<SteamPipeLineFullResponseDto?> GetSteamPipeLineByApplicationIdAsync(string applicationId)
        {
            var x = await _dbcontext.SteamPipeLineApplications
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (x == null)
                return null;

            return new SteamPipeLineFullResponseDto
            {
                ApplicationId = x.ApplicationId,
                SteamPipeLineRegistrationNo = x.SteamPipeLineRegistrationNo,

                BoilerApplicationNo = x.BoilerApplicationNo,
                ProposedLayoutDescription = x.ProposedLayoutDescription,
                ConsentLetterProvided = x.ConsentLetterProvided,
                SteamPipeLineDrawingNo = x.SteamPipeLineDrawingNo,
                BoilerMakerRegistrationNo = x.BoilerMakerRegistrationNo,
                ErectorName = x.ErectorName,

                FactoryRegistrationNumber = x.FactoryRegistrationNumber,
                Factorydetailjson = x.Factorydetailjson,

                PipeLengthUpTo100mm = x.PipeLengthUpTo100mm,
                PipeLengthAbove100mm = x.PipeLengthAbove100mm,

                NoOfDeSuperHeaters = x.NoOfDeSuperHeaters,
                NoOfSteamReceivers = x.NoOfSteamReceivers,
                NoOfFeedHeaters = x.NoOfFeedHeaters,
                NoOfSeparatelyFiredSuperHeaters = x.NoOfSeparatelyFiredSuperHeaters,

                FormIIPath = x.FormIIPath,
                FormIIIPath = x.FormIIIPath,
                FormIIIAPath = x.FormIIIAPath,
                FormIIIBPath = x.FormIIIBPath,
                FormIVPath = x.FormIVPath,
                FormIVAPath = x.FormIVAPath,
                DrawingPath = x.DrawingPath,
                SupportingDocumentsPath = x.SupportingDocumentsPath,

                RenewalYears = x.RenewalYears,
                ValidFrom = x.ValidFrom,
                ValidUpto = x.ValidUpto,

                Type = x.Type,
                Version = x.Version,
                Status = x.Status,
                IsActive = x.IsActive
            };
        }
        //public async Task<List<SteamPipeLineFullResponseDto>>GetSteamPipeLineByRegistrationNoAsync(string registrationNo)
        //{
        //    var list = await _dbcontext.SteamPipeLineApplications
        //        .Where(x => x.SteamPipeLineRegistrationNo == registrationNo)
        //        .OrderByDescending(x => x.Version)
        //        .ToListAsync();

        //    var result = new List<SteamPipeLineFullResponseDto>();

        //    foreach (var x in list)
        //    {
        //        result.Add(new SteamPipeLineFullResponseDto
        //        {
        //            ApplicationId = x.ApplicationId,
        //            SteamPipeLineRegistrationNo = x.SteamPipeLineRegistrationNo,

        //            BoilerApplicationNo = x.BoilerApplicationNo,
        //            ProposedLayoutDescription = x.ProposedLayoutDescription,
        //            ConsentLetterProvided = x.ConsentLetterProvided,
        //            SteamPipeLineDrawingNo = x.SteamPipeLineDrawingNo,
        //            BoilerMakerRegistrationNo = x.BoilerMakerRegistrationNo,
        //            ErectorName = x.ErectorName,

        //            FactoryRegistrationNumber = x.FactoryRegistrationNumber,
        //            Factorydetailjson = x.Factorydetailjson,

        //            PipeLengthUpTo100mm = x.PipeLengthUpTo100mm,
        //            PipeLengthAbove100mm = x.PipeLengthAbove100mm,

        //            NoOfDeSuperHeaters = x.NoOfDeSuperHeaters,
        //            NoOfSteamReceivers = x.NoOfSteamReceivers,
        //            NoOfFeedHeaters = x.NoOfFeedHeaters,
        //            NoOfSeparatelyFiredSuperHeaters = x.NoOfSeparatelyFiredSuperHeaters,

        //            FormIIPath = x.FormIIPath,
        //            FormIIIPath = x.FormIIIPath,
        //            FormIIIAPath = x.FormIIIAPath,
        //            FormIIIBPath = x.FormIIIBPath,
        //            FormIVPath = x.FormIVPath,
        //            FormIVAPath = x.FormIVAPath,
        //            DrawingPath = x.DrawingPath,
        //            SupportingDocumentsPath = x.SupportingDocumentsPath,

        //            RenewalYears = x.RenewalYears,
        //            ValidFrom = x.ValidFrom,
        //            ValidUpto = x.ValidUpto,

        //            Type = x.Type,
        //            Version = x.Version,
        //            Status = x.Status,
        //            IsActive = x.IsActive
        //        });
        //    }

        //    return result;
        //}


        public async Task<List<SteamPipeLineFullResponseDto>> GetAllSteamPipeLinesAsync()
        {
            var list = await _dbcontext.SteamPipeLineApplications
                .OrderByDescending(x => x.Version)
                .ToListAsync();

            return list.Select(x => new SteamPipeLineFullResponseDto
            {
                ApplicationId = x.ApplicationId,
                SteamPipeLineRegistrationNo = x.SteamPipeLineRegistrationNo,

                BoilerApplicationNo = x.BoilerApplicationNo,
                ProposedLayoutDescription = x.ProposedLayoutDescription,
                ConsentLetterProvided = x.ConsentLetterProvided,
                SteamPipeLineDrawingNo = x.SteamPipeLineDrawingNo,
                BoilerMakerRegistrationNo = x.BoilerMakerRegistrationNo,
                ErectorName = x.ErectorName,

                FactoryRegistrationNumber = x.FactoryRegistrationNumber,
                Factorydetailjson = x.Factorydetailjson,

                PipeLengthUpTo100mm = x.PipeLengthUpTo100mm,
                PipeLengthAbove100mm = x.PipeLengthAbove100mm,

                NoOfDeSuperHeaters = x.NoOfDeSuperHeaters,
                NoOfSteamReceivers = x.NoOfSteamReceivers,
                NoOfFeedHeaters = x.NoOfFeedHeaters,
                NoOfSeparatelyFiredSuperHeaters = x.NoOfSeparatelyFiredSuperHeaters,

                FormIIPath = x.FormIIPath,
                FormIIIPath = x.FormIIIPath,
                FormIIIAPath = x.FormIIIAPath,
                FormIIIBPath = x.FormIIIBPath,
                FormIVPath = x.FormIVPath,
                FormIVAPath = x.FormIVAPath,
                DrawingPath = x.DrawingPath,
                SupportingDocumentsPath = x.SupportingDocumentsPath,

                RenewalYears = x.RenewalYears,
                ValidFrom = x.ValidFrom,
                ValidUpto = x.ValidUpto,

                Type = x.Type,
                Version = x.Version,
                Status = x.Status,
                IsActive = x.IsActive
            }).ToList();
        }

        public async Task<SteamPipeLineFullResponseDto?> GetLatestApprovedByRegistrationNoAsync(string registrationNo)
        {
            var latest = await _dbcontext.SteamPipeLineApplications
                .Where(x => x.SteamPipeLineRegistrationNo == registrationNo)
                .OrderByDescending(x => x.Version)
                .FirstOrDefaultAsync();

            if (latest == null)
                return null;

            // Only return if latest version is Approved
            if (!latest.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                return null;

            return new SteamPipeLineFullResponseDto
            {
                ApplicationId = latest.ApplicationId,
                SteamPipeLineRegistrationNo = latest.SteamPipeLineRegistrationNo,

                BoilerApplicationNo = latest.BoilerApplicationNo,
                ProposedLayoutDescription = latest.ProposedLayoutDescription,
                ConsentLetterProvided = latest.ConsentLetterProvided,
                SteamPipeLineDrawingNo = latest.SteamPipeLineDrawingNo,
                BoilerMakerRegistrationNo = latest.BoilerMakerRegistrationNo,
                ErectorName = latest.ErectorName,

                FactoryRegistrationNumber = latest.FactoryRegistrationNumber,
                Factorydetailjson = latest.Factorydetailjson,

                PipeLengthUpTo100mm = latest.PipeLengthUpTo100mm,
                PipeLengthAbove100mm = latest.PipeLengthAbove100mm,

                NoOfDeSuperHeaters = latest.NoOfDeSuperHeaters,
                NoOfSteamReceivers = latest.NoOfSteamReceivers,
                NoOfFeedHeaters = latest.NoOfFeedHeaters,
                NoOfSeparatelyFiredSuperHeaters = latest.NoOfSeparatelyFiredSuperHeaters,

                FormIIPath = latest.FormIIPath,
                FormIIIPath = latest.FormIIIPath,
                FormIIIAPath = latest.FormIIIAPath,
                FormIIIBPath = latest.FormIIIBPath,
                FormIVPath = latest.FormIVPath,
                FormIVAPath = latest.FormIVAPath,
                DrawingPath = latest.DrawingPath,
                SupportingDocumentsPath = latest.SupportingDocumentsPath,

                RenewalYears = latest.RenewalYears,
                ValidFrom = latest.ValidFrom,
                ValidUpto = latest.ValidUpto,

                Type = latest.Type,
                Version = latest.Version,
                Status = latest.Status,
                IsActive = latest.IsActive
            };
        }

        public async Task<string> UpdateSteamPipeLineAsync(  string applicationId,   CreateSteamPipeLineDto dto)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
                throw new Exception("ApplicationId is required.");

            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                var entity = await _dbcontext.SteamPipeLineApplications
                    .FirstOrDefaultAsync(x => x.ApplicationId == applicationId);

                if (entity == null)
                    throw new Exception("Application not found.");

                // ?? Only pending editable
                if (entity.Status != "Pending")
                    throw new Exception("Only pending applications can be updated.");

                /* ============================================
                   UPDATE FIELDS
                ============================================ */

                entity.BoilerApplicationNo = dto.BoilerApplicationNo;
                entity.ProposedLayoutDescription = dto.ProposedLayout;
                entity.ConsentLetterProvided = dto.ConsentLetterProvided;
                entity.SteamPipeLineDrawingNo = dto.SteamPipeLineDrawingNo;
                entity.BoilerMakerRegistrationNo = dto.BoilerMakerRegistrationNo;
                entity.ErectorName = dto.ErectorName;

                entity.FactoryRegistrationNumber = dto.FactoryRegistrationNumber;
                entity.Factorydetailjson = dto.Factorydetailjson;

                entity.PipeLengthUpTo100mm = dto.PipeLengthUpTo100mm;
                entity.PipeLengthAbove100mm = dto.PipeLengthAbove100mm;

                entity.NoOfDeSuperHeaters = dto.NoOfDeSuperHeaters;
                entity.NoOfSteamReceivers = dto.NoOfSteamReceivers;
                entity.NoOfFeedHeaters = dto.NoOfFeedHeaters;
                entity.NoOfSeparatelyFiredSuperHeaters =
                    dto.NoOfSeparatelyFiredSuperHeaters;

                entity.FormIIPath = dto.FormII;
                entity.FormIIIPath = dto.FormIII;
                entity.FormIIIAPath = dto.FormIIIA;
                entity.FormIIIBPath = dto.FormIIIB;
                entity.FormIVPath = dto.FormIV;
                entity.FormIVAPath = dto.FormIVA;
                entity.DrawingPath = dto.Drawing;
                entity.SupportingDocumentsPath = dto.SupportingDocuments;

                entity.UpdatedAt = DateTime.Now;

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
        public async Task<string> CloseSteamPipeLineAsync( CreateSteamPipeLineCloseDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                if (string.IsNullOrWhiteSpace(dto.SteamPipeLineRegistrationNo))
                    throw new Exception("SteamPipeLineRegistrationNo is required.");

                // ?? Get latest version of STPL
                var latest = await _dbcontext.SteamPipeLineApplications
                    .Where(x => x.SteamPipeLineRegistrationNo ==
                                dto.SteamPipeLineRegistrationNo)
                    .OrderByDescending(x => x.Version)
                    .FirstOrDefaultAsync();

                if (latest == null)
                    throw new Exception("STPL registration not found.");

                if (!latest.Status.Equals("Approved",
                    StringComparison.OrdinalIgnoreCase))
                    throw new Exception("Only approved STPL can be closed.");

                // ?? Block multiple pending close
                var pendingClose = await _dbcontext.SteamPipeLineClosures
                    .AnyAsync(x =>
                        x.SteamPipeLineRegistrationNo ==
                        dto.SteamPipeLineRegistrationNo &&
                        x.Status == "Pending");

                if (pendingClose)
                    throw new Exception("Close request already pending.");

                // ?? Generate Close ApplicationId
                var applicationId =
                    await GenerateApplicationNumberAsync("close");

                // ?? Version (separate close versioning)
                var version = 1.0m;

                var close = new SteamPipeLineClosure
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = applicationId,
                    SteamPipeLineRegistrationNo =
                        dto.SteamPipeLineRegistrationNo,

                    ReasonForClosure = dto.ReasonForClosure,
                    SupportingDocumentPath = dto.SupportingDocument,

                    Type = "close",
                    Version = version,
                    Status = "Pending",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _dbcontext.SteamPipeLineClosures.Add(close);
                await _dbcontext.SaveChangesAsync();

                await tx.CommitAsync();

                return close.ApplicationId;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }


    }
}