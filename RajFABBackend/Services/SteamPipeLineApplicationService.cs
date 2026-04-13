using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;
using System.Text.Json;
using static RajFabAPI.Constants.AppConstants;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using PdfTable = iText.Layout.Element.Table;
using PdfCell = iText.Layout.Element.Cell;
using iText.Kernel.Pdf.Event;
using iText.Kernel.Pdf.Canvas;
using iText.Layout.Borders;

namespace RajFabAPI.Services
{
    public class SteamPipeLineApplicationService : ISteamPipeLineApplicationService
    {
        private readonly ApplicationDbContext _dbcontext;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPaymentService _paymentService;

        public SteamPipeLineApplicationService(ApplicationDbContext context, IWebHostEnvironment environment, IConfiguration config, IHttpContextAccessor httpContextAccessor, IPaymentService paymentService)
        {
            _dbcontext = context;
            _environment = environment;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _paymentService = paymentService;
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

        public async Task<string> SaveSteamPipeLineAsync(  CreateSteamPipeLineDto dto,  Guid userId,  string? type,  string? steamPipeLineRegistrationNo)
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

                    Amount = type == "new" ? 5000m : 100m,
                    Type = type,
                    Version = version,
                    Status = "Pending",
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _dbcontext.SteamPipeLineApplications.Add(entity);
                await _dbcontext.SaveChangesAsync();

                // Auto-generate application PDF
                try { await GenerateStplPdfAsync(entity.ApplicationId); } catch { /* PDF generation failure should not block submission */ }

                if (type == "new")
                {
                    var module = await _dbcontext.Set<FormModule>()
                        .FirstOrDefaultAsync(m => m.Name == ApplicationTypeNames.Stplregistration)
                        ?? throw new Exception("Stpl Registration module not found.");

                    var appReg = new ApplicationRegistration
                    {
                        Id = Guid.NewGuid().ToString(),
                        ModuleId = module.Id,
                        UserId = userId,
                        ApplicationId = entity.ApplicationId,
                        ApplicationRegistrationNumber = entity.ApplicationId,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };
                    _dbcontext.ApplicationRegistrations.Add(appReg);

                    var history = new ApplicationHistory
                    {
                        ApplicationId = entity.ApplicationId,
                        ApplicationType = module.Name,
                        Action = "Application Submitted",
                        PreviousStatus = null,
                        NewStatus = "Pending",
                        Comments = "Application Submitted and sent for payment",
                        ActionBy = userId.ToString(),
                        ActionByName = "Applicant",
                        ActionDate = DateTime.Now
                    };
                    _dbcontext.ApplicationHistories.Add(history);

                    await _dbcontext.SaveChangesAsync();
                    await tx.CommitAsync();

                    var user = await _dbcontext.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Id == userId)
                        ?? throw new Exception("User not found.");

                    return await _paymentService.ActionRequestPaymentRPP(
                        entity.Amount,
                        user.FullName,
                        user.Mobile,
                        user.Email,
                        user.Username,
                        "4157FE34BBAE3A958D8F58CCBFAD7",
                        "UWf6a7cDCP",
                        entity.ApplicationId!,
                        module.Id.ToString(),
                        userId.ToString()
                    );
                }

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

            var objectionLetter = await _dbcontext.ApplicationObjectionLetters
                .Where(o => o.ApplicationId == applicationId)
                .OrderByDescending(o => o.CreatedDate)
                .FirstOrDefaultAsync();

            var certificate = await _dbcontext.Certificates
                .Where(c => c.ApplicationId == applicationId)
                .OrderByDescending(c => c.IssuedAt)
                .FirstOrDefaultAsync();

            var transactionHistory = await _dbcontext.Set<Transaction>()
                .AsNoTracking()
                .Where(t => t.ApplicationId == applicationId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

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
                IsActive = x.IsActive,

                ApplicationPDFUrl = x.ApplicationPDFUrl,
                ObjectionLetterUrl = objectionLetter?.FileUrl,
                CertificateUrl = certificate?.CertificateUrl,
                TransactionHistory = transactionHistory
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


        public async Task<string> GenerateStplPdfAsync(string applicationId)
        {
            var entity = await _dbcontext.SteamPipeLineApplications
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (entity == null)
                throw new Exception("Steam Pipeline application not found");

            var uploadPath = Path.Combine(_environment.WebRootPath, "boiler-stpl-forms");
            Directory.CreateDirectory(uploadPath);

            var fileName = $"stpl_application_{entity.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/boiler-stpl-forms/{fileName}";

            var sections = new List<PdfSection>
            {
                new PdfSection
                {
                    Title = "Application Info",
                    Rows = new List<(string, string?)>
                    {
                        ("Application Id", entity.ApplicationId),
                        ("Registration No", entity.SteamPipeLineRegistrationNo),
                        ("Type", entity.Type),
                        ("Status", entity.Status),
                        ("Created At", entity.CreatedAt.ToString("dd/MM/yyyy"))
                    }
                },
                new PdfSection
                {
                    Title = "Pipeline Details",
                    Rows = new List<(string, string?)>
                    {
                        ("Boiler Application No", entity.BoilerApplicationNo),
                        ("Steam Pipeline Drawing No", entity.SteamPipeLineDrawingNo),
                        ("Boiler Maker Registration No", entity.BoilerMakerRegistrationNo),
                        ("Erector Name", entity.ErectorName),
                        ("Factory Registration Number", entity.FactoryRegistrationNumber)
                    }
                },
                new PdfSection
                {
                    Title = "Pipe Specifications",
                    Rows = new List<(string, string?)>
                    {
                        ("Pipe Length Up To 100mm", entity.PipeLengthUpTo100mm?.ToString()),
                        ("Pipe Length Above 100mm", entity.PipeLengthAbove100mm?.ToString()),
                        ("No Of De-Super Heaters", entity.NoOfDeSuperHeaters?.ToString()),
                        ("No Of Steam Receivers", entity.NoOfSteamReceivers?.ToString()),
                        ("No Of Feed Heaters", entity.NoOfFeedHeaters?.ToString()),
                        ("No Of Separately Fired Super Heaters", entity.NoOfSeparatelyFiredSuperHeaters?.ToString())
                    }
                },
                new PdfSection
                {
                    Title = "Validity",
                    Rows = new List<(string, string?)>
                    {
                        ("Valid From", entity.ValidFrom?.ToString("dd/MM/yyyy")),
                        ("Valid Upto", entity.ValidUpto?.ToString("dd/MM/yyyy")),
                        ("Renewal Years", entity.RenewalYears.ToString())
                    }
                }
            };

            BoilerPdfHelper.GeneratePdf(
                filePath,
                "Form-STPL1",
                "(See Indian Boilers Act, 1923)",
                "Application for Steam Pipeline Registration",
                entity.ApplicationId ?? "-",
                entity.CreatedAt,
                sections);

            // Save URL back to DB
            entity.ApplicationPDFUrl = fileUrl;
            entity.UpdatedAt = DateTime.Now;
            await _dbcontext.SaveChangesAsync();

            return filePath;
        }

        public async Task<string> GenerateObjectionLetter(BoilerObjectionLetterDto dto, string applicationId)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var safeAppId = applicationId.Replace("/", "_").Replace("\\", "_");
            var fileName = $"stpl_objection_{safeAppId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var webRootPath = _environment.WebRootPath ?? throw new InvalidOperationException("wwwroot is not configured.");
            var uploadPath = Path.Combine(webRootPath, "boiler-objection-letters");
            Directory.CreateDirectory(uploadPath);
            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/boiler-objection-letters/{fileName}";

            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new StplPageBorderEventHandler());
            using var document = new Document(pdf);
            document.SetMargins(50, 50, 65, 50);

            document.Add(new Paragraph("Government of Rajasthan").SetFont(boldFont).SetFontSize(14).SetTextAlignment(TextAlignment.CENTER));
            document.Add(new Paragraph("Factories and Boilers Inspection Department").SetFont(boldFont).SetFontSize(13).SetTextAlignment(TextAlignment.CENTER));
            document.Add(new Paragraph("6-C, Jhalana Institutional Area, Jaipur, 302004").SetFont(regularFont).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(10));

            var topTable = new PdfTable(new float[] { 1, 1 }).UseAllAvailableWidth();
            topTable.AddCell(new PdfCell().Add(new Paragraph($"Application Id:- {dto.ApplicationId}").SetFont(boldFont)).SetBorder(Border.NO_BORDER));
            topTable.AddCell(new PdfCell().Add(new Paragraph($"Dated:- {dto.Date:dd/MM/yyyy}").SetFont(boldFont).SetTextAlignment(TextAlignment.RIGHT)).SetBorder(Border.NO_BORDER));
            document.Add(topTable);

            document.Add(new Paragraph(dto.OwnerName ?? "-").SetFont(regularFont));
            document.Add(new Paragraph(dto.Address ?? "-").SetFont(regularFont).SetMarginBottom(10));

            var subject = new Paragraph();
            subject.Add(new Text("Sub:- ").SetFont(boldFont));
            subject.Add(new Text("Steam Pipeline Registration").SetFont(regularFont));
            document.Add(subject);

            document.Add(new Paragraph("The details of your steam pipeline as per application and submitted documents are shown below:-").SetFont(regularFont).SetMarginBottom(5));

            var table = new PdfTable(new float[] { 150, 1 }).UseAllAvailableWidth();
            PdfCell Fmt(string text, PdfFont font) => new PdfCell().Add(new Paragraph(text ?? "-").SetFont(font).SetFontSize(12)).SetPadding(5);
            table.AddCell(Fmt("Registration No", boldFont)); table.AddCell(Fmt(dto.BoilerRegistrationNo ?? "-", regularFont));
            table.AddCell(Fmt("Working Pressure", boldFont)); table.AddCell(Fmt(dto.WorkingPressure?.ToString() ?? "-", regularFont));
            document.Add(table);

            document.Add(new Paragraph("Following objections need to be removed:").SetFont(regularFont).SetMarginTop(10));
            if (dto.Objections != null)
                for (int i = 0; i < dto.Objections.Count; i++)
                    document.Add(new Paragraph($"{i + 1}. {dto.Objections[i]}").SetFont(regularFont));

            document.Add(new Paragraph("Please comply with the above observations and submit relevant documents").SetFont(regularFont).SetMarginTop(15));
            document.Add(new Paragraph("\n\n"));
            document.Add(new Paragraph($"({dto.SignatoryName})").SetTextAlignment(TextAlignment.RIGHT));
            document.Add(new Paragraph(dto.SignatoryDesignation).SetTextAlignment(TextAlignment.RIGHT));
            document.Add(new Paragraph(dto.SignatoryLocation).SetTextAlignment(TextAlignment.RIGHT));

            var pageWidth = pdf.GetDefaultPageSize().GetWidth();
            document.Add(new Paragraph("This is a computer generated document. No physical signature is required.").SetFontSize(8).SetFixedPosition(35, 30, pageWidth - 70));
            document.Close();

            return fileUrl;
        }

        public async Task<string> GenerateCertificatePdfAsync(string applicationId, string postName, string userName)
        {
            var entity = await _dbcontext.SteamPipeLineApplications.FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
            if (entity == null) throw new Exception("Steam Pipeline application not found");

            var safeAppId = (entity.ApplicationId ?? applicationId).Replace("/", "_").Replace("\\", "_");
            var fileName = $"stpl_certificate_{safeAppId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var webRootPath = _environment.WebRootPath ?? throw new InvalidOperationException("wwwroot is not configured.");
            var uploadPath = Path.Combine(webRootPath, "certificates");
            Directory.CreateDirectory(uploadPath);
            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/certificates/{fileName}";

            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var footerDate = DateOnly.FromDateTime(DateTime.Today);

            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new StplCertFooterEventHandler(boldFont, regularFont, footerDate, postName, userName));
            using var document = new Document(pdf);
            document.SetMargins(40, 40, 130, 40);

            var headerTable = new PdfTable(new float[] { 90f, 320f, 90f }).UseAllAvailableWidth().SetBorder(Border.NO_BORDER).SetMarginBottom(6f);
            headerTable.AddCell(new PdfCell().SetBorder(Border.NO_BORDER));
            var centerCell = new PdfCell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER);
            centerCell.Add(new Paragraph("Government of Rajasthan").SetFont(boldFont).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(1f));
            centerCell.Add(new Paragraph("Factories and Boilers Inspection Department").SetFont(boldFont).SetFontSize(11).SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(1f));
            centerCell.Add(new Paragraph("6-C, Jhalana Institutional Area, Jaipur, 302004").SetFont(regularFont).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(4f));
            headerTable.AddCell(centerCell);
            headerTable.AddCell(new PdfCell().SetBorder(Border.NO_BORDER));
            document.Add(headerTable);

            var topRow = new PdfTable(new float[] { 1f, 1f }).UseAllAvailableWidth().SetBorder(Border.NO_BORDER).SetMarginBottom(2f);
            topRow.AddCell(new PdfCell().Add(new Paragraph($"Application No.:-  {entity.ApplicationId}").SetFont(boldFont).SetFontSize(10)).SetBorder(Border.NO_BORDER));
            topRow.AddCell(new PdfCell().Add(new Paragraph($"Dated:-  {DateTime.Now:dd/MM/yyyy}").SetFont(boldFont).SetFontSize(10).SetTextAlignment(TextAlignment.RIGHT)).SetBorder(Border.NO_BORDER));
            document.Add(topRow);

            document.Add(new Paragraph($"Registration No.:-  {entity.SteamPipeLineRegistrationNo ?? "-"}").SetFont(boldFont).SetFontSize(10).SetMarginBottom(6f));

            document.Add(new Paragraph("Sub:-  Approval of Steam Pipeline Registration").SetFont(boldFont).SetFontSize(11).SetMarginBottom(2f));
            document.Add(new Paragraph("The details of your application as per submitted documents are shown below:-").SetFont(regularFont).SetFontSize(11).SetMarginBottom(6f));

            var blackBorder = new SolidBorder(new DeviceRgb(0, 0, 0), 0.75f);
            PdfCell BlackCell(string text, PdfFont font, float size = 10f) => new PdfCell().Add(new Paragraph(text ?? "-").SetFont(font).SetFontSize(size)).SetBorderTop(blackBorder).SetBorderBottom(blackBorder).SetBorderLeft(blackBorder).SetBorderRight(blackBorder).SetPadding(5f);

            var detailsTable = new PdfTable(new float[] { 150f, 350f }).UseAllAvailableWidth().SetMarginBottom(10f);
            detailsTable.AddCell(BlackCell("Factory Reg. No", boldFont)); detailsTable.AddCell(BlackCell(entity.FactoryRegistrationNumber ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Drawing No", boldFont)); detailsTable.AddCell(BlackCell(entity.SteamPipeLineDrawingNo ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Pipe Length ≤100mm", boldFont)); detailsTable.AddCell(BlackCell(entity.PipeLengthUpTo100mm?.ToString() ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Pipe Length >100mm", boldFont)); detailsTable.AddCell(BlackCell(entity.PipeLengthAbove100mm?.ToString() ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Valid From", boldFont)); detailsTable.AddCell(BlackCell(entity.ValidFrom?.ToString("dd/MM/yyyy") ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Valid Upto", boldFont)); detailsTable.AddCell(BlackCell(entity.ValidUpto?.ToString("dd/MM/yyyy") ?? "-", regularFont));
            document.Add(detailsTable);

            document.Add(new Paragraph("Your Steam Pipeline Registration is approved under the Indian Boilers Act, 1923 and the rules made thereunder.").SetFont(regularFont).SetFontSize(11).SetMarginBottom(20f));

            var pageWidth = pdf.GetDefaultPageSize().GetWidth();
            document.Add(new Paragraph("This is a computer generated certificate and bears scanned signature. No physical signature is required on this approval. You can verify this approval by visiting www.rajfab.rajasthan.gov.in and entering Application No./ID after clicking the link for verification on the page.").SetFont(regularFont).SetFontSize(6.5f).SetFontColor(ColorConstants.GRAY).SetTextAlignment(TextAlignment.JUSTIFIED).SetMultipliedLeading(1.1f).SetFixedPosition(35, 8, pageWidth - 70));

            return fileUrl;
        }

        private sealed class StplPageBorderEventHandler : AbstractPdfDocumentEventHandler
        {
            protected override void OnAcceptedEvent(AbstractPdfDocumentEvent @event)
            {
                if (@event is not PdfDocumentEvent docEvent) return;
                var page = docEvent.GetPage();
                var rect = page.GetPageSize();
                var canvas = new PdfCanvas(page);
                canvas.SetStrokeColor(new DeviceRgb(20, 57, 92)).SetLineWidth(1.5f).Rectangle(25, 25, rect.GetWidth() - 50, rect.GetHeight() - 50).Stroke();
                canvas.Release();
            }
        }

        private sealed class StplCertFooterEventHandler : AbstractPdfDocumentEventHandler
        {
            private readonly PdfFont _boldFont, _regularFont;
            private readonly DateOnly _date;
            private readonly string _postName, _userName;

            public StplCertFooterEventHandler(PdfFont boldFont, PdfFont regularFont, DateOnly date, string postName, string userName)
            { _boldFont = boldFont; _regularFont = regularFont; _date = date; _postName = postName; _userName = userName; }

            protected override void OnAcceptedEvent(AbstractPdfDocumentEvent @event)
            {
                if (@event is not PdfDocumentEvent docEvent) return;
                var pdfDoc = docEvent.GetDocument();
                var page = docEvent.GetPage();
                var rect = page.GetPageSize();
                var pdfCanvas = new PdfCanvas(page);
                float pw = rect.GetWidth(), ph = rect.GetHeight();

                pdfCanvas.SetStrokeColor(ColorConstants.BLACK).SetLineWidth(1.5f).Rectangle(25, 25, pw - 50, ph - 50).Stroke();
                float lineY = 70f;
                pdfCanvas.SetStrokeColor(new DeviceRgb(180, 180, 180)).SetLineWidth(0.5f).MoveTo(30, lineY).LineTo(pw - 30, lineY).Stroke();

                float zoneH = 65f, belowY = lineY - 4f - zoneH;
                float signW = 180f, signX = pw - 30f - signW;
                int pageNum = pdfDoc.GetPageNumber(page);

                using (var c = new Canvas(pdfCanvas, new iText.Kernel.Geom.Rectangle(30f, belowY, 110f, zoneH)))
                    c.Add(new Paragraph($"Dated: {_date}").SetFont(_regularFont).SetFontSize(7.5f).SetMargin(0f).SetPaddingTop(6f));
                using (var c = new Canvas(pdfCanvas, new iText.Kernel.Geom.Rectangle(0f, belowY, pw, zoneH)))
                    c.Add(new Paragraph($"Page {pageNum}").SetFont(_regularFont).SetFontSize(7.5f).SetTextAlignment(TextAlignment.CENTER).SetMargin(0f).SetPaddingTop(6f));
                using (var c = new Canvas(pdfCanvas, new iText.Kernel.Geom.Rectangle(signX, belowY, signW, zoneH)))
                {
                    if (!string.IsNullOrWhiteSpace(_userName))
                        c.Add(new Paragraph($"({_userName})").SetFont(_boldFont).SetFontSize(7f).SetTextAlignment(TextAlignment.CENTER).SetMargin(0f).SetPaddingTop(2f));
                    if (!string.IsNullOrWhiteSpace(_postName))
                        c.Add(new Paragraph(_postName).SetFont(_regularFont).SetFontSize(6.5f).SetTextAlignment(TextAlignment.CENTER).SetMargin(0f).SetPaddingTop(1f));
                    c.Add(new Paragraph("Signature / E-sign / Digital sign").SetFont(_regularFont).SetFontSize(6.5f).SetTextAlignment(TextAlignment.CENTER).SetMargin(0f).SetPaddingTop(4f));
                }
                pdfCanvas.Release();
            }
        }

    }
}