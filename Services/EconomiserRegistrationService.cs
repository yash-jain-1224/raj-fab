using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.DTOs.RajFabAPI.DTOs.EconomiserDTOs;
using RajFabAPI.Models;
using RajFabAPI.Models.BoilerModels;
using RajFabAPI.Services.Interface;
using System;
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
    public class EconomiserService : IEconomiserService
    {
        private readonly ApplicationDbContext _dbcontext;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPaymentService _paymentService;

        public EconomiserService(ApplicationDbContext dbcontext, IWebHostEnvironment environment, IConfiguration config, IHttpContextAccessor httpContextAccessor, IPaymentService paymentService)
        {
            _dbcontext = dbcontext;
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
                "new" => $"ECO{year}/CIFB/",
                "amend" => $"ECOAMD{year}/CIFB/",
                "renew" => $"ECOREN{year}/CIFB/",
                "close" => $"ECOCLOSE{year}/CIFB/",
                _ => throw new Exception("Invalid economiser application type")
            };

            var lastApp = await _dbcontext.EconomiserRegistrations
                .Where(x => x.ApplicationId.StartsWith(prefix))
                .OrderByDescending(x => x.CreatedDate)
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

       

        private async Task<string> GenerateEconomiserRegistrationNoAsync()
        {
            var last = await _dbcontext.EconomiserRegistrations
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => x.EconomiserRegistrationNo)
                .FirstOrDefaultAsync();

            int next = 1;

            if (!string.IsNullOrEmpty(last))
            {
                var number = last.Split('-').Last();
                if (int.TryParse(number, out int n))
                    next = n + 1;
            }

            return $"ECO-{next:D5}";
        }



        public async Task<string> SaveEconomiserAsync(   EconomiserCreateDto dto,   Guid userId,   string? type,    string? economiserRegistrationNo)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            type = type?.ToLower() ?? "new";

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                EconomiserRegistration? baseRecord = null;

               

                if (type == "amend")
                {
                    if (string.IsNullOrWhiteSpace(economiserRegistrationNo))
                        throw new Exception("EconomiserRegistrationNo required for amendment.");

                    baseRecord = await _dbcontext.EconomiserRegistrations
                        .Where(x => x.EconomiserRegistrationNo == economiserRegistrationNo
                                    && x.Status == "Approved")
                        .OrderByDescending(x => x.Version)
                        .FirstOrDefaultAsync();

                    if (baseRecord == null)
                        throw new Exception("No approved record found.");

                    bool pendingExists = await _dbcontext.EconomiserRegistrations
                        .AnyAsync(x =>
                            x.EconomiserRegistrationNo == economiserRegistrationNo &&
                            x.Type == "amend" &&
                            x.Status == "Pending");

                    if (pendingExists)
                        throw new Exception("Amendment already pending.");
                }

               

                string regNo;

                if (type == "new")
                {
                    regNo = await GenerateEconomiserRegistrationNoAsync();
                }
                else
                {
                    regNo = economiserRegistrationNo!;
                }


                var applicationNo = await GenerateApplicationNumberAsync(type);

               

                decimal version = type == "amend"
                    ? baseRecord!.Version + 0.1m
                    : 1.0m;

               

                DateTime? validFrom;
                DateTime? validUpto;

                if (type == "new")
                {
                    validFrom = DateTime.Today;
                    validUpto = validFrom.Value.AddYears(1);
                }
                else
                {
                    validFrom = baseRecord!.ValidFrom;
                    validUpto = baseRecord.ValidUpto;
                }

            

                var registration = new EconomiserRegistration
                {
                    Id = Guid.NewGuid(),

                    ApplicationId = applicationNo,
                    EconomiserRegistrationNo = regNo,

                    FactoryRegistrationNumber = dto.FactoryRegistrationNumber ?? baseRecord?.FactoryRegistrationNumber,
                    FactoryDetailJson = dto.FactoryDetailJson ?? baseRecord?.FactoryDetailJson,

                    MakersNumber = dto.MakersNumber ?? baseRecord?.MakersNumber,
                    MakersName = dto.MakersName ?? baseRecord?.MakersName,
                    MakersAddress = dto.MakersAddress ?? baseRecord?.MakersAddress,
                    YearOfMake = dto.YearOfMake ?? baseRecord?.YearOfMake,
                    PressureFrom = dto.PressureFrom ?? baseRecord?.PressureFrom,
                    PressureTo = dto.PressureTo ?? baseRecord?.PressureTo,
                    ErectionType = dto.ErectionType ?? baseRecord?.ErectionType,
                    OutletTemperature = dto.OutletTemperature ?? baseRecord?.OutletTemperature,
                    TotalHeatingSurfaceArea = dto.TotalHeatingSurfaceArea ?? baseRecord?.TotalHeatingSurfaceArea,
                    NumberOfTubes = dto.NumberOfTubes ?? baseRecord?.NumberOfTubes,
                    NumberOfHeaders = dto.NumberOfHeaders ?? baseRecord?.NumberOfHeaders,

                    FormIB = dto.FormIB ?? baseRecord?.FormIB,
                    FormIC = dto.FormIC ?? baseRecord?.FormIC,
                    FormIVA = dto.FormIVA ?? baseRecord?.FormIVA,
                    FormIVB = dto.FormIVB ?? baseRecord?.FormIVB,
                    FormIVC = dto.FormIVC ?? baseRecord?.FormIVC,
                    FormIVD = dto.FormIVD ?? baseRecord?.FormIVD,
                    FormVA = dto.FormVA ?? baseRecord?.FormVA,
                    FormXV = dto.FormXV ?? baseRecord?.FormXV,
                    FormXVI = dto.FormXVI ?? baseRecord?.FormXVI,
                    AttendantCertificate = dto.AttendantCertificate ?? baseRecord?.AttendantCertificate,
                    EngineerCertificate = dto.EngineerCertificate ?? baseRecord?.EngineerCertificate,
                    Drawings = dto.Drawings ?? baseRecord?.Drawings,

                    ValidFrom = validFrom,
                    ValidUpto = validUpto,

                    Amount = type == "new" ? 5000m : 100m,
                    Type = type,
                    Version = version,
                    Status = "Pending",

                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,

                    IsActive = true
                };

                _dbcontext.EconomiserRegistrations.Add(registration);

                await _dbcontext.SaveChangesAsync();

                // Auto-generate application PDF
                try { await GenerateEconomiserPdfAsync(registration.ApplicationId); } catch { /* PDF generation failure should not block submission */ }

                if (type == "new")
                {
                    var module = await _dbcontext.Set<FormModule>()
                        .FirstOrDefaultAsync(m => m.Name == ApplicationTypeNames.EconomiserRegistration)
                        ?? throw new Exception("Economiser Registration module not found.");

                    var appReg = new ApplicationRegistration
                    {
                        Id = Guid.NewGuid().ToString(),
                        ModuleId = module.Id,
                        UserId = userId,
                        ApplicationId = registration.ApplicationId,
                        ApplicationRegistrationNumber = registration.ApplicationId,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };
                    _dbcontext.ApplicationRegistrations.Add(appReg);

                    var appHistory = new ApplicationHistory
                    {
                        ApplicationId = registration.ApplicationId,
                        ApplicationType = module.Name,
                        Action = "Application Submitted",
                        PreviousStatus = null,
                        NewStatus = "Pending",
                        Comments = "Application Submitted and sent for payment",
                        ActionBy = userId.ToString(),
                        ActionByName = "Applicant",
                        ActionDate = DateTime.Now
                    };
                    _dbcontext.ApplicationHistories.Add(appHistory);

                    await _dbcontext.SaveChangesAsync();
                    await tx.CommitAsync();

                    var user = await _dbcontext.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Id == userId)
                        ?? throw new Exception("User not found.");

                    return await _paymentService.ActionRequestPaymentRPP(
                        registration.Amount,
                        user.FullName,
                        user.Mobile,
                        user.Email,
                        user.Username,
                        "4157FE34BBAE3A958D8F58CCBFAD7",
                        "UWf6a7cDCP",
                        registration.ApplicationId!,
                        module.Id.ToString(),
                        userId.ToString()
                    );
                }

                await tx.CommitAsync();

                return registration.ApplicationId;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<string> RenewEconomiserAsync(EconomiserRenewalDto dto, Guid userId)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.EconomiserRegistrationNo))
                throw new ArgumentException("EconomiserRegistrationNo is required");

            if (dto.RenewalYears <= 0)
                throw new ArgumentException("RenewalYears must be greater than zero");

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                
                var latestRecord = await _dbcontext.EconomiserRegistrations
                    .Where(x => x.EconomiserRegistrationNo == dto.EconomiserRegistrationNo)
                    .OrderByDescending(x => x.Version)
                    .FirstOrDefaultAsync();

                if (latestRecord == null)
                    throw new Exception("Economiser registration not found.");

               

                if (latestRecord.Status != "Approved")
                    throw new Exception("Latest application is not approved. Renewal not allowed.");

               

                var pendingRenewal = await _dbcontext.EconomiserRegistrations
                    .AnyAsync(x => x.EconomiserRegistrationNo == dto.EconomiserRegistrationNo
                                && x.Type == "renew"
                                && x.Status == "Pending");

                if (pendingRenewal)
                    throw new Exception("Renewal already pending for this registration.");


                DateTime validFrom = latestRecord.ValidFrom ?? DateTime.Now;

                DateTime baseDate =
                    (latestRecord.ValidUpto == null || latestRecord.ValidUpto < DateTime.Now)
                    ? DateTime.Now
                    : latestRecord.ValidUpto.Value;

                DateTime newValidUpto = baseDate.AddYears(dto.RenewalYears);

               

                var newVersion = Math.Round(latestRecord.Version + 0.1m, 1);

                var applicationId = await GenerateApplicationNumberAsync("renew");

              

                var renewed = new EconomiserRegistration
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = applicationId,
                    EconomiserRegistrationNo = latestRecord.EconomiserRegistrationNo,

                    FactoryRegistrationNumber = latestRecord.FactoryRegistrationNumber,
                    FactoryDetailJson = latestRecord.FactoryDetailJson,

                    MakersNumber = latestRecord.MakersNumber,
                    MakersName = latestRecord.MakersName,
                    MakersAddress = latestRecord.MakersAddress,
                    YearOfMake = latestRecord.YearOfMake,
                    PressureFrom = latestRecord.PressureFrom,
                    PressureTo = latestRecord.PressureTo,
                    ErectionType = latestRecord.ErectionType,
                    OutletTemperature = latestRecord.OutletTemperature,
                    TotalHeatingSurfaceArea = latestRecord.TotalHeatingSurfaceArea,
                    NumberOfTubes = latestRecord.NumberOfTubes,
                    NumberOfHeaders = latestRecord.NumberOfHeaders,

                    FormIB = latestRecord.FormIB,
                    FormIC = latestRecord.FormIC,
                    FormIVA = latestRecord.FormIVA,
                    FormIVB = latestRecord.FormIVB,
                    FormIVC = latestRecord.FormIVC,
                    FormIVD = latestRecord.FormIVD,
                    FormVA = latestRecord.FormVA,
                    FormXV = latestRecord.FormXV,
                    FormXVI = latestRecord.FormXVI,
                    AttendantCertificate = latestRecord.AttendantCertificate,
                    EngineerCertificate = latestRecord.EngineerCertificate,
                    Drawings = latestRecord.Drawings,

                    ValidFrom = validFrom,
                    ValidUpto = newValidUpto,

                    Type = "renew",
                    Version = newVersion,
                    Status = "Pending",

                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    IsActive = true
                };

                _dbcontext.EconomiserRegistrations.Add(renewed);

                await _dbcontext.SaveChangesAsync();
                await tx.CommitAsync();

                return applicationId;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateEconomiserAsync(string applicationId, EconomiserCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
                return false;

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                var registration = await _dbcontext.EconomiserRegistrations
                    .FirstOrDefaultAsync(x => x.ApplicationId == applicationId);

                if (registration == null)
                    return false;

                //if (!registration.Status!.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                //    throw new Exception("Only pending applications can be updated.");

                /* ================= GENERAL INFO ================= */

                registration.FactoryRegistrationNumber = dto.FactoryRegistrationNumber;
                registration.FactoryDetailJson = dto.FactoryDetailJson;

                /* ================= ECONOMISER DETAILS ================= */

                registration.MakersNumber = dto.MakersNumber;
                registration.MakersName = dto.MakersName;
                registration.MakersAddress = dto.MakersAddress;
                registration.YearOfMake = dto.YearOfMake;

                registration.PressureFrom = dto.PressureFrom;
                registration.PressureTo = dto.PressureTo;
                registration.ErectionType = dto.ErectionType;
                registration.OutletTemperature = dto.OutletTemperature;

                registration.TotalHeatingSurfaceArea = dto.TotalHeatingSurfaceArea;
                registration.NumberOfTubes = dto.NumberOfTubes;
                registration.NumberOfHeaders = dto.NumberOfHeaders;

                /* ================= DOCUMENTS ================= */

                registration.FormIB = dto.FormIB;
                registration.FormIC = dto.FormIC;
                registration.FormIVA = dto.FormIVA;
                registration.FormIVB = dto.FormIVB;
                registration.FormIVC = dto.FormIVC;
                registration.FormIVD = dto.FormIVD;
                registration.FormVA = dto.FormVA;
                registration.FormXV = dto.FormXV;
                registration.FormXVI = dto.FormXVI;

                registration.AttendantCertificate = dto.AttendantCertificate;
                registration.EngineerCertificate = dto.EngineerCertificate;
                registration.Drawings = dto.Drawings;

                /* ================= MASTER UPDATE ================= */

                registration.UpdatedDate = DateTime.Now;

                await _dbcontext.SaveChangesAsync();
                await tx.CommitAsync();

                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<EconomiserDetailsDto?> GetByApplicationIdAsync(string applicationId)
        {
            var record = await _dbcontext.EconomiserRegistrations
                .FirstOrDefaultAsync(x => x.ApplicationId == applicationId);

            if (record == null)
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

            return new EconomiserDetailsDto
            {
                ApplicationId = record.ApplicationId,
                EconomiserRegistrationNo = record.EconomiserRegistrationNo,

                FactoryRegistrationNumber = record.FactoryRegistrationNumber,
                FactoryDetailJson = record.FactoryDetailJson,

                MakersNumber = record.MakersNumber,
                MakersName = record.MakersName,
                MakersAddress = record.MakersAddress,
                YearOfMake = record.YearOfMake,
                PressureFrom = record.PressureFrom,
                PressureTo = record.PressureTo,
                ErectionType = record.ErectionType,
                OutletTemperature = record.OutletTemperature,
                TotalHeatingSurfaceArea = record.TotalHeatingSurfaceArea,
                NumberOfTubes = record.NumberOfTubes,
                NumberOfHeaders = record.NumberOfHeaders,

                FormIB = record.FormIB,
                FormIC = record.FormIC,
                FormIVA = record.FormIVA,
                FormIVB = record.FormIVB,
                FormIVC = record.FormIVC,
                FormIVD = record.FormIVD,
                FormVA = record.FormVA,
                FormXV = record.FormXV,
                FormXVI = record.FormXVI,
                AttendantCertificate = record.AttendantCertificate,
                EngineerCertificate = record.EngineerCertificate,
                Drawings = record.Drawings,

                ValidFrom = record.ValidFrom,
                ValidUpto = record.ValidUpto,

                Type = record.Type,
                Version = record.Version,
                Status = record.Status,

                ApplicationPDFUrl = record.ApplicationPDFUrl,
                ObjectionLetterUrl = objectionLetter?.FileUrl,
                CertificateUrl = certificate?.CertificateUrl,
                TransactionHistory = transactionHistory
            };
        }

        public async Task<EconomiserDetailsDto?> GetLatestApprovedByRegistrationNoAsync(string economiserRegistrationNo)
        {
            if (string.IsNullOrWhiteSpace(economiserRegistrationNo))
                throw new ArgumentException("EconomiserRegistrationNo is required.");

            var latest = await _dbcontext.EconomiserRegistrations
                .Where(x => x.EconomiserRegistrationNo == economiserRegistrationNo)
                .OrderByDescending(x => x.Version)
                .FirstOrDefaultAsync();

            if (latest == null)
                return null;

            if (!latest.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                return null;

            return new EconomiserDetailsDto
            {
                ApplicationId = latest.ApplicationId,
                EconomiserRegistrationNo = latest.EconomiserRegistrationNo,

                FactoryRegistrationNumber = latest.FactoryRegistrationNumber,
                FactoryDetailJson = latest.FactoryDetailJson,

                MakersNumber = latest.MakersNumber,
                MakersName = latest.MakersName,
                MakersAddress = latest.MakersAddress,
                YearOfMake = latest.YearOfMake,
                PressureFrom = latest.PressureFrom,
                PressureTo = latest.PressureTo,
                ErectionType = latest.ErectionType,
                OutletTemperature = latest.OutletTemperature,
                TotalHeatingSurfaceArea = latest.TotalHeatingSurfaceArea,
                NumberOfTubes = latest.NumberOfTubes,
                NumberOfHeaders = latest.NumberOfHeaders,

                FormIB = latest.FormIB,
                FormIC = latest.FormIC,
                FormIVA = latest.FormIVA,
                FormIVB = latest.FormIVB,
                FormIVC = latest.FormIVC,
                FormIVD = latest.FormIVD,
                FormVA = latest.FormVA,
                FormXV = latest.FormXV,
                FormXVI = latest.FormXVI,
                AttendantCertificate = latest.AttendantCertificate,
                EngineerCertificate = latest.EngineerCertificate,
                Drawings = latest.Drawings,

                ValidFrom = latest.ValidFrom,
                ValidUpto = latest.ValidUpto,

                Type = latest.Type,
                Version = latest.Version,
                Status = latest.Status
            };
        }

        public async Task<List<EconomiserDetailsDto>> GetAllAsync()
        {
            var records = await _dbcontext.EconomiserRegistrations
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return records.Select(record => new EconomiserDetailsDto
            {
                ApplicationId = record.ApplicationId,
                EconomiserRegistrationNo = record.EconomiserRegistrationNo,

                FactoryRegistrationNumber = record.FactoryRegistrationNumber,
                FactoryDetailJson = record.FactoryDetailJson,

                MakersNumber = record.MakersNumber,
                MakersName = record.MakersName,
                MakersAddress = record.MakersAddress,
                YearOfMake = record.YearOfMake,
                PressureFrom = record.PressureFrom,
                PressureTo = record.PressureTo,
                ErectionType = record.ErectionType,
                OutletTemperature = record.OutletTemperature,
                TotalHeatingSurfaceArea = record.TotalHeatingSurfaceArea,
                NumberOfTubes = record.NumberOfTubes,
                NumberOfHeaders = record.NumberOfHeaders,

                FormIB = record.FormIB,
                FormIC = record.FormIC,
                FormIVA = record.FormIVA,
                FormIVB = record.FormIVB,
                FormIVC = record.FormIVC,
                FormIVD = record.FormIVD,
                FormVA = record.FormVA,
                FormXV = record.FormXV,
                FormXVI = record.FormXVI,
                AttendantCertificate = record.AttendantCertificate,
                EngineerCertificate = record.EngineerCertificate,
                Drawings = record.Drawings,

                ValidFrom = record.ValidFrom,
                ValidUpto = record.ValidUpto,

                Type = record.Type,
                Version = record.Version,
                Status = record.Status
            }).ToList();
        }

        public async Task<string> CloseEconomiserAsync(EconomiserClosureDto dto, Guid userId)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.EconomiserRegistrationNo))
                throw new ArgumentException("EconomiserRegistrationNo is required");

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
               

                var history = await _dbcontext.EconomiserRegistrations
                    .Where(x => x.EconomiserRegistrationNo == dto.EconomiserRegistrationNo)
                    .OrderByDescending(x => x.Version)
                    .ToListAsync();

                if (!history.Any())
                    throw new Exception("Economiser registration not found.");

               

                var pendingWorkflow = history.FirstOrDefault(x => x.Status == "Pending");

                if (pendingWorkflow != null)
                    throw new Exception(
                        $"Cannot close. A {pendingWorkflow.Type?.ToUpper()} request (Version {pendingWorkflow.Version}) is already pending."
                    );

               

                var latestRecord = history.First();

                if (latestRecord.Status != "Approved")
                    throw new Exception("Only the latest APPROVED version can be closed.");

              

                var alreadyClosed = await _dbcontext.EconomiserClosures
                    .AnyAsync(x =>
                        x.EconomiserRegistrationNo == dto.EconomiserRegistrationNo &&
                        x.Status == "Approved");

                if (alreadyClosed)
                    throw new Exception("This Economiser registration is already closed.");

              

                var pendingClosure = await _dbcontext.EconomiserClosures
                    .AnyAsync(x =>
                        x.EconomiserRegistrationNo == dto.EconomiserRegistrationNo &&
                        x.Status == "Pending");

                if (pendingClosure)
                    throw new Exception("Closure request already submitted and pending.");

              

                var applicationId = await GenerateApplicationNumberAsync("close");

               

                var closure = new EconomiserClosure
                {
                    Id = Guid.NewGuid(),
                    EconomiserRegistrationNo = dto.EconomiserRegistrationNo,
                    ApplicationId = applicationId,

                    ClosureReason = dto.ClosureReason,
                    ClosureDate = dto.ClosureDate,

                    Remarks = dto.Remarks,
                    DocumentPath = dto.DocumentPath,

                    Type = "close",
                    Status = "Pending",

                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                _dbcontext.EconomiserClosures.Add(closure);

                await _dbcontext.SaveChangesAsync();

                await tx.CommitAsync();

                return applicationId;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<string> GenerateEconomiserPdfAsync(string applicationId)
        {
            var entity = await _dbcontext.EconomiserRegistrations
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (entity == null)
                throw new Exception("Economiser registration not found");

            var uploadPath = Path.Combine(_environment.WebRootPath, "boiler-economiser-forms");
            Directory.CreateDirectory(uploadPath);

            var fileName = $"economiser_application_{entity.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/boiler-economiser-forms/{fileName}";

            var sections = new List<PdfSection>
            {
                new PdfSection
                {
                    Title = "Application Info",
                    Rows = new List<(string, string?)>
                    {
                        ("Application Id", entity.ApplicationId),
                        ("Economiser Registration No", entity.EconomiserRegistrationNo),
                        ("Type", entity.Type),
                        ("Status", entity.Status),
                        ("Factory Registration Number", entity.FactoryRegistrationNumber)
                    }
                },
                new PdfSection
                {
                    Title = "Economiser Details",
                    Rows = new List<(string, string?)>
                    {
                        ("Makers Number", entity.MakersNumber),
                        ("Makers Name", entity.MakersName),
                        ("Makers Address", entity.MakersAddress),
                        ("Year Of Make", entity.YearOfMake?.ToString()),
                        ("Erection Type", entity.ErectionType)
                    }
                },
                new PdfSection
                {
                    Title = "Technical Details",
                    Rows = new List<(string, string?)>
                    {
                        ("Pressure From", entity.PressureFrom?.ToString()),
                        ("Pressure To", entity.PressureTo?.ToString()),
                        ("Outlet Temperature", entity.OutletTemperature?.ToString()),
                        ("Total Heating Surface Area", entity.TotalHeatingSurfaceArea?.ToString()),
                        ("Number Of Tubes", entity.NumberOfTubes?.ToString()),
                        ("Number Of Headers", entity.NumberOfHeaders?.ToString())
                    }
                },
                new PdfSection
                {
                    Title = "Validity",
                    Rows = new List<(string, string?)>
                    {
                        ("Valid From", entity.ValidFrom?.ToString("dd/MM/yyyy")),
                        ("Valid Upto", entity.ValidUpto?.ToString("dd/MM/yyyy"))
                    }
                }
            };

            BoilerPdfHelper.GeneratePdf(
                filePath,
                "Form-ECO1",
                "(See Indian Boilers Act, 1923)",
                "Application for Economiser Registration",
                entity.ApplicationId ?? "-",
                entity.CreatedDate,
                sections);

            // Save URL back to DB
            entity.ApplicationPDFUrl = fileUrl;
            entity.UpdatedDate = DateTime.Now;
            await _dbcontext.SaveChangesAsync();

            return filePath;
        }

        public async Task<string> GenerateObjectionLetter(BoilerObjectionLetterDto dto, string applicationId)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var safeAppId = applicationId.Replace("/", "_").Replace("\\", "_");
            var fileName = $"economiser_objection_{safeAppId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
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
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new EconomiserPageBorderEventHandler());
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
            subject.Add(new Text("Economiser Registration").SetFont(regularFont));
            document.Add(subject);

            document.Add(new Paragraph("The details of your economiser as per application and submitted documents are shown below:-").SetFont(regularFont).SetMarginBottom(5));

            var table = new PdfTable(new float[] { 150, 1 }).UseAllAvailableWidth();
            PdfCell Fmt(string text, PdfFont font) => new PdfCell().Add(new Paragraph(text ?? "-").SetFont(font).SetFontSize(12)).SetPadding(5);
            table.AddCell(Fmt("Registration No", boldFont)); table.AddCell(Fmt(dto.BoilerRegistrationNo ?? "-", regularFont));
            table.AddCell(Fmt("Heating Surface Area", boldFont)); table.AddCell(Fmt(dto.HeatingSurfaceArea?.ToString() ?? "-", regularFont));
            table.AddCell(Fmt("Evaporation Capacity", boldFont)); table.AddCell(Fmt(dto.EvaporationCapacity?.ToString() ?? "-", regularFont));
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
            var entity = await _dbcontext.EconomiserRegistrations.FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
            if (entity == null) throw new Exception("Economiser application not found");

            var safeAppId = (entity.ApplicationId ?? applicationId).Replace("/", "_").Replace("\\", "_");
            var fileName = $"economiser_certificate_{safeAppId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
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
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new EconomiserCertFooterEventHandler(boldFont, regularFont, footerDate, postName, userName));
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

            document.Add(new Paragraph($"Registration No.:-  {entity.EconomiserRegistrationNo ?? "-"}").SetFont(boldFont).SetFontSize(10).SetMarginBottom(6f));

            document.Add(new Paragraph("Sub:-  Approval of Economiser Registration").SetFont(boldFont).SetFontSize(11).SetMarginBottom(2f));
            document.Add(new Paragraph("The details of your application as per submitted documents are shown below:-").SetFont(regularFont).SetFontSize(11).SetMarginBottom(6f));

            var blackBorder = new SolidBorder(new DeviceRgb(0, 0, 0), 0.75f);
            PdfCell BlackCell(string text, PdfFont font, float size = 10f) => new PdfCell().Add(new Paragraph(text ?? "-").SetFont(font).SetFontSize(size)).SetBorderTop(blackBorder).SetBorderBottom(blackBorder).SetBorderLeft(blackBorder).SetBorderRight(blackBorder).SetPadding(5f);

            var detailsTable = new PdfTable(new float[] { 150f, 350f }).UseAllAvailableWidth().SetMarginBottom(10f);
            detailsTable.AddCell(BlackCell("Makers Name", boldFont)); detailsTable.AddCell(BlackCell(entity.MakersName ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Makers Number", boldFont)); detailsTable.AddCell(BlackCell(entity.MakersNumber ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Year of Make", boldFont)); detailsTable.AddCell(BlackCell(entity.YearOfMake ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Heating Surface Area", boldFont)); detailsTable.AddCell(BlackCell(entity.TotalHeatingSurfaceArea ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Pressure From", boldFont)); detailsTable.AddCell(BlackCell(entity.PressureFrom ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Pressure To", boldFont)); detailsTable.AddCell(BlackCell(entity.PressureTo ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Valid From", boldFont)); detailsTable.AddCell(BlackCell(entity.ValidFrom?.ToString("dd/MM/yyyy") ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Valid Upto", boldFont)); detailsTable.AddCell(BlackCell(entity.ValidUpto?.ToString("dd/MM/yyyy") ?? "-", regularFont));
            document.Add(detailsTable);

            document.Add(new Paragraph("Your Economiser Registration is approved under the Indian Boilers Act, 1923 and the rules made thereunder.").SetFont(regularFont).SetFontSize(11).SetMarginBottom(20f));

            var pageWidth = pdf.GetDefaultPageSize().GetWidth();
            document.Add(new Paragraph("This is a computer generated certificate and bears scanned signature. No physical signature is required on this approval. You can verify this approval by visiting www.rajfab.rajasthan.gov.in and entering Application No./ID after clicking the link for verification on the page.").SetFont(regularFont).SetFontSize(6.5f).SetFontColor(ColorConstants.GRAY).SetTextAlignment(TextAlignment.JUSTIFIED).SetMultipliedLeading(1.1f).SetFixedPosition(35, 8, pageWidth - 70));

            return fileUrl;
        }

        private sealed class EconomiserPageBorderEventHandler : AbstractPdfDocumentEventHandler
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

        private sealed class EconomiserCertFooterEventHandler : AbstractPdfDocumentEventHandler
        {
            private readonly PdfFont _boldFont, _regularFont;
            private readonly DateOnly _date;
            private readonly string _postName, _userName;

            public EconomiserCertFooterEventHandler(PdfFont boldFont, PdfFont regularFont, DateOnly date, string postName, string userName)
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
