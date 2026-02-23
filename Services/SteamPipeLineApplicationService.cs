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
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public SteamPipeLineApplicationService(ApplicationDbContext context)
        {
            _context = context;
        }



        private string GenerateStplApplicationNo()
        {
            var year = DateTime.Now.Year;
            var random = Random.Shared.Next(10000, 99999);
            return $"{year}/STPL/{random}";
        }

        private string GenerateStplBoilerApplicationNo()
        {
            var year = DateTime.Now.Year;
            var random = Random.Shared.Next(10000, 99999);
            return $"{year}/STPL/{random}";
        }


        // ======================================================
        // CREATE / AMENDMENT 
        // ======================================================
        public async Task<Guid> SaveAsync(
        CreateSteamPipeLineDto dto,
        Guid userId,
        string type,
        Guid? applicationId = null)
        {
            SteamPipeLineApplication? approvedBase = null;
            decimal newVersion;
            string applicationNo;
            string boilerApplicationNo;

            // =========================
            // ?? NEW APPLICATION
            // =========================
            if (type == "new")
            {
                newVersion = 1.0m;

                applicationNo = GenerateStplApplicationNo();
                boilerApplicationNo = GenerateStplBoilerApplicationNo();
            }
            // =========================
            // ?? AMENDMENT
            // =========================
            else if (type == "amendment")
            {
                if (applicationId == null)
                    throw new ArgumentException("applicationId is required.");

                approvedBase = await _context.SteamPipeLineApplications
                    .Where(x => x.Id == applicationId && x.Status == "Approved")
                    .OrderByDescending(x => x.Version)
                    .FirstOrDefaultAsync();

                if (approvedBase == null)
                    throw new InvalidOperationException("Approved application not found.");

                // ? Prevent multiple pending for SAME application number
                bool hasPending = await _context.SteamPipeLineApplications.AnyAsync(x =>
                    x.ApplicationNo == approvedBase.ApplicationNo &&
                    x.Status == "Pending");

                if (hasPending)
                    throw new InvalidOperationException("A pending application already exists.");

                newVersion = Math.Round(approvedBase.Version + 0.1m, 1);
                applicationNo = approvedBase.ApplicationNo;
                boilerApplicationNo = approvedBase.BoilerApplicationNo;
            }
            else
            {
                throw new ArgumentException("Invalid application type.");
            }

            // =========================
            // ?? INSERT NEW ROW
            // =========================
            var entity = new SteamPipeLineApplication
            {
                Id = Guid.NewGuid(),
                UserId = userId,

                ApplicationNo = applicationNo,
                BoilerApplicationNo = boilerApplicationNo,

                Type = type,
                Status = "Pending",
                Version = newVersion,

                // =========================
                // Boiler Info
                // =========================
                ProposedLayoutDescription = approvedBase?.ProposedLayoutDescription ?? dto.ProposedLayoutDescription,
                ConsentLetterProvided = approvedBase?.ConsentLetterProvided ?? dto.ConsentLetterProvided,
                SteamPipeLineDrawingNo = approvedBase?.SteamPipeLineDrawingNo ?? dto.SteamPipeLineDrawingNo,
                BoilerMakerRegistrationNo = approvedBase?.BoilerMakerRegistrationNo ?? dto.BoilerMakerRegistrationNo,
                ErectorName = approvedBase?.ErectorName ?? dto.ErectorName,

                // =========================
                // Factory Info
                // =========================
                FactoryName = approvedBase?.FactoryName ?? dto.FactoryName,
                FactoryRegistrationNumber = approvedBase?.FactoryRegistrationNumber ?? dto.FactoryRegistrationNumber,
                OwnerName = approvedBase?.OwnerName ?? dto.OwnerName,

                // =========================
                // Address
                // =========================
                PlotNo = approvedBase?.PlotNo ?? dto.PlotNo,
                Street = approvedBase?.Street ?? dto.Street,
                DivisionId = approvedBase?.DivisionId ?? dto.DivisionId,
                DistrictId = approvedBase?.DistrictId ?? dto.DistrictId,
                AreaId = approvedBase?.AreaId ?? dto.AreaId,
                Pincode = approvedBase?.Pincode ?? dto.Pincode,
                Mobile = approvedBase?.Mobile ?? dto.Mobile,

                // =========================
                // Pipeline Details
                // =========================
                PipeLengthUpTo100mm = approvedBase?.PipeLengthUpTo100mm ?? dto.PipeLengthUpTo100mm,
                PipeLengthAbove100mm = approvedBase?.PipeLengthAbove100mm ?? dto.PipeLengthAbove100mm,

                // =========================
                // Fittings
                // =========================
                NoOfDeSuperHeaters = approvedBase?.NoOfDeSuperHeaters ?? dto.NoOfDeSuperHeaters,
                NoOfSteamReceivers = approvedBase?.NoOfSteamReceivers ?? dto.NoOfSteamReceivers,
                NoOfFeedHeaters = approvedBase?.NoOfFeedHeaters ?? dto.NoOfFeedHeaters,
                NoOfSeparatelyFiredSuperHeaters =
                    approvedBase?.NoOfSeparatelyFiredSuperHeaters ?? dto.NoOfSeparatelyFiredSuperHeaters,

                // =========================
                // Attachments
                // =========================
                FormIIPath = approvedBase?.FormIIPath ?? dto.FormIIPath,
                FormIIIPath = approvedBase?.FormIIIPath ?? dto.FormIIIPath,
                FormIIIAPath = approvedBase?.FormIIIAPath ?? dto.FormIIIAPath,
                FormIIIBPath = approvedBase?.FormIIIBPath ?? dto.FormIIIBPath,
                FormIVPath = approvedBase?.FormIVPath ?? dto.FormIVPath,
                FormIVAPath = approvedBase?.FormIVAPath ?? dto.FormIVAPath,
                DrawingPath = approvedBase?.DrawingPath ?? dto.DrawingPath,
                SupportingDocumentsPath =
                    approvedBase?.SupportingDocumentsPath ?? dto.SupportingDocumentsPath,

                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.SteamPipeLineApplications.Add(entity);
            await _context.SaveChangesAsync();

            return entity.Id;
        }

        // ======================================================
        // GET BY ID
        // ======================================================
        public async Task<SteamPipeLineResponseDto?> GetByIdAsync(Guid id)
        {
            return await _context.SteamPipeLineApplications
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new SteamPipeLineResponseDto
                {

                    ApplicationNo = x.ApplicationNo,
                    Status = x.Status,
                    Version = x.Version,
                    Type = x.Type,

                    BoilerApplicationNo = x.BoilerApplicationNo,
                    ProposedLayoutDescription = x.ProposedLayoutDescription,
                    ConsentLetterProvided = x.ConsentLetterProvided,
                    SteamPipeLineDrawingNo = x.SteamPipeLineDrawingNo,
                    BoilerMakerRegistrationNo = x.BoilerMakerRegistrationNo,
                    ErectorName = x.ErectorName,

                    FactoryName = x.FactoryName,
                    FactoryRegistrationNumber = x.FactoryRegistrationNumber,
                    OwnerName = x.OwnerName,

                    PlotNo = x.PlotNo,
                    Street = x.Street,
                    DivisionId = x.DivisionId,
                    DistrictId = x.DistrictId,
                    AreaId = x.AreaId,
                    Pincode = x.Pincode,
                    Mobile = x.Mobile,

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

                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .FirstOrDefaultAsync();
        }

        // ======================================================
        // GET BY USER
        // ======================================================
        public async Task<List<SteamPipeLineResponseDto>> GetByUserIdAsync(Guid userId)
        {
            return await _context.SteamPipeLineApplications
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new SteamPipeLineResponseDto
                {

                    ApplicationNo = x.ApplicationNo,
                    Status = x.Status,
                    Version = x.Version,
                    Type = x.Type,
                    FactoryName = x.FactoryName,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }

    }
}