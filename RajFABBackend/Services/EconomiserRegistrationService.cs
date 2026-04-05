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
                Status = record.Status
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

    }

}
