using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
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
    public class BoilerDrawingService : IBoilerDrawingService
    {
        private readonly ApplicationDbContext _dbcontext;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPaymentService _paymentService;

        public BoilerDrawingService(ApplicationDbContext dbcontext, IWebHostEnvironment environment, IConfiguration config, IHttpContextAccessor httpContextAccessor, IPaymentService paymentService)
        {
            _dbcontext = dbcontext;
            _environment = environment;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _paymentService = paymentService;
        }

        private async Task<string> GenerateBoilerDrawingRegistrationNoAsync()
        {
            const string prefix = "BD-";

            var last = await _dbcontext.BoilerDrawingApplications
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => x.BoilerDrawingRegistrationNo)
                .FirstOrDefaultAsync();

            int next = 1;

            if (!string.IsNullOrEmpty(last))
            {
                var number = last.Replace(prefix, "");
                if (int.TryParse(number, out int n))
                    next = n + 1;
            }

            return $"{prefix}{next:D5}";
        }


        private async Task<string> GenerateApplicationNumberAsync(string type)
        {
            var year = DateTime.Now.Year;

            string prefix = type switch
            {
                "new" => $"BD{year}/CIFB/",
                "amend" => $"BDAMD{year}/CIFB/",
                "renew" => $"BDREN{year}/CIFB/",
                "close" => $"BDCLOSE{year}/CIFB/",
                _ => throw new Exception("Invalid application type")
            };

            var lastApp = await _dbcontext.BoilerDrawingApplications
                .Where(x => x.ApplicationId.StartsWith(prefix))
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => x.ApplicationId)
                .FirstOrDefaultAsync();

            int next = 1;

            if (!string.IsNullOrEmpty(lastApp))
            {
                var lastPart = lastApp.Split('/').Last();
                if (int.TryParse(lastPart, out int lastNo))
                    next = lastNo + 1;
            }

            return $"{prefix}{next:D4}";
        }



        public async Task<string> SaveBoilerDrawingAsync(  BoilerDrawingCreateDto dto,  Guid userId,   string? type,   string? boilerDrawingRegistrationNo)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            type = type?.ToLower() ?? "new";

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                BoilerDrawingApplication? baseRecord = null;

                /* ================= AMEND VALIDATION ================= */

                if (type == "amend")
                {
                    if (string.IsNullOrWhiteSpace(boilerDrawingRegistrationNo))
                        throw new Exception("BoilerDrawingRegistrationNo required for amendment.");

                    baseRecord = await _dbcontext.BoilerDrawingApplications
                        .Where(x => x.BoilerDrawingRegistrationNo == boilerDrawingRegistrationNo &&
                                    x.Status == "Approved")
                        .OrderByDescending(x => x.Version)
                        .FirstOrDefaultAsync();

                    if (baseRecord == null)
                        throw new Exception("Approved record not found for amendment.");

                    var pendingExists = await _dbcontext.BoilerDrawingApplications
                        .AnyAsync(x =>
                            x.BoilerDrawingRegistrationNo == boilerDrawingRegistrationNo &&
                            x.Status == "Pending" &&
                            x.Type == "amend");

                    if (pendingExists)
                        throw new Exception("Amendment already pending.");
                }

                /* ================= REGISTRATION NUMBER ================= */

                string regNo = type == "new"
                    ? await GenerateBoilerDrawingRegistrationNoAsync()
                    : boilerDrawingRegistrationNo!;

                /* ================= APPLICATION NUMBER ================= */

                var applicationId = await GenerateApplicationNumberAsync(type);

                /* ================= VERSION ================= */

                var version = type == "amend"
                    ? baseRecord!.Version + 0.1m
                    : 1.0m;

                /* ================= VALIDITY ================= */

                DateTime? validFrom;
                DateTime? validUpto;

                if (type == "new")
                {
                    validFrom = DateTime.Today;
                    validUpto = validFrom.Value.AddYears(1);
                }
                else
                {
                    validFrom = baseRecord!.ValidFrom ?? baseRecord.CreatedDate;
                    validUpto = baseRecord.ValidUpto ?? validFrom.Value.AddYears(1);
                }

                /* ================= INSERT ================= */

                var registration = new BoilerDrawingApplication
                {
                    Id = Guid.NewGuid(),

                    ApplicationId = applicationId,
                    BoilerDrawingRegistrationNo = regNo,

                    FactoryRegistrationNumber =
                        dto.FactoryRegistrationNumber ?? baseRecord?.FactoryRegistrationNumber,

                    FactoryDetailjson =
                        dto.FactoryDetailjson ?? baseRecord?.FactoryDetailjson,

                    MakerNumber = dto.MakerNumber ?? baseRecord?.MakerNumber,
                    MakerNameAndAddress = dto.MakerNameAndAddress ?? baseRecord?.MakerNameAndAddress,

                    HeatingSurfaceArea = dto.HeatingSurfaceArea ?? baseRecord?.HeatingSurfaceArea,
                    EvaporationCapacity = dto.EvaporationCapacity ?? baseRecord?.EvaporationCapacity,
                    IntendedWorkingPressure =
                        dto.IntendedWorkingPressure ?? baseRecord?.IntendedWorkingPressure,

                    BoilerType = dto.BoilerType ?? baseRecord?.BoilerType,
                    DrawingNo = dto.DrawingNo ?? baseRecord?.DrawingNo,

                    BoilerDrawing = dto.BoilerDrawing ?? baseRecord?.BoilerDrawing,
                    FeedPipelineDrawing = dto.FeedPipelineDrawing ?? baseRecord?.FeedPipelineDrawing,
                    PressurePartCalculation =
                        dto.PressurePartCalculation ?? baseRecord?.PressurePartCalculation,

                    ValidFrom = validFrom,
                    ValidUpto = validUpto,

                    Amount = type == "new" ? 5000m : 100m,

                    Status = "Pending",
                    Type = type,
                    Version = version,

                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,

                    IsActive = true
                };

                _dbcontext.BoilerDrawingApplications.Add(registration);

                await _dbcontext.SaveChangesAsync();

                // Auto-generate application PDF
                try { await GenerateDrawingPdfAsync(registration.ApplicationId); } catch { /* PDF generation failure should not block submission */ }

                if (type == "new")
                {
                    var module = await _dbcontext.Set<FormModule>()
                        .FirstOrDefaultAsync(m => m.Name == ApplicationTypeNames.BoilerDrawingRegistration)
                        ?? throw new Exception("BoilerDrawingRegistration module not found.");

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

                    var history = new ApplicationHistory
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
                    _dbcontext.ApplicationHistories.Add(history);

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

                return applicationId;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }


        public async Task<string> RenewBoilerDrawingAsync(  BoilerDrawingRenewalDto dto,  Guid userId)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.BoilerDrawingRegistrationNo))
                throw new ArgumentException("BoilerDrawingRegistrationNo is required");

            if (dto.RenewalYears <= 0)
                throw new ArgumentException("RenewalYears must be greater than zero");

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
               

                var history = await _dbcontext.BoilerDrawingApplications
                    .Where(x => x.BoilerDrawingRegistrationNo == dto.BoilerDrawingRegistrationNo)
                    .OrderByDescending(x => x.Version)
                    .ToListAsync();

                if (!history.Any())
                    throw new Exception("Boiler drawing registration not found.");


                var pending = history.FirstOrDefault(x => x.Status == "Pending");

                if (pending != null)
                    throw new Exception(
                        $"Cannot renew. A {pending.Type?.ToUpper()} request (Version {pending.Version}) is already pending."
                    );

              

                var latest = history.First();

                if (!latest.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("Only the latest APPROVED version can be renewed.");


                DateTime validFrom = latest.ValidFrom ?? DateTime.Now;

                DateTime baseDate =
                    (latest.ValidUpto == null || latest.ValidUpto < DateTime.Now)
                    ? DateTime.Now
                    : latest.ValidUpto.Value;

                DateTime newValidUpto = baseDate.AddYears(dto.RenewalYears);

               

                var newVersion = Math.Round(latest.Version + 0.1m, 1);


                var applicationId = await GenerateApplicationNumberAsync("renew");

               

                var renewed = new BoilerDrawingApplication
                {
                    Id = Guid.NewGuid(),

                    ApplicationId = applicationId,
                    BoilerDrawingRegistrationNo = latest.BoilerDrawingRegistrationNo,

                    FactoryRegistrationNumber = latest.FactoryRegistrationNumber,
                    FactoryDetailjson = latest.FactoryDetailjson,

                    MakerNumber = latest.MakerNumber,
                    MakerNameAndAddress = latest.MakerNameAndAddress,

                    HeatingSurfaceArea = latest.HeatingSurfaceArea,
                    EvaporationCapacity = latest.EvaporationCapacity,
                    IntendedWorkingPressure = latest.IntendedWorkingPressure,

                    BoilerType = latest.BoilerType,
                    DrawingNo = latest.DrawingNo,

                    BoilerDrawing = latest.BoilerDrawing,
                    FeedPipelineDrawing = latest.FeedPipelineDrawing,
                    PressurePartCalculation = latest.PressurePartCalculation,

                    ValidFrom = validFrom,
                    ValidUpto = newValidUpto,

                    Type = "renew",
                    Version = newVersion,
                    Status = "Pending",

                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    IsActive = true
                };

                _dbcontext.BoilerDrawingApplications.Add(renewed);

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


        public async Task<BoilerDrawingDetailsDto?> GetByApplicationIdAsync(string applicationId)
        {
            var x = await _dbcontext.BoilerDrawingApplications
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

            return new BoilerDrawingDetailsDto
            {
                ApplicationId = x.ApplicationId,
                BoilerDrawingRegistrationNo = x.BoilerDrawingRegistrationNo,

                FactoryRegistrationNumber = x.FactoryRegistrationNumber,
                FactoryDetailjson = x.FactoryDetailjson,

                MakerNumber = x.MakerNumber,
                MakerNameAndAddress = x.MakerNameAndAddress,

                HeatingSurfaceArea = x.HeatingSurfaceArea,
                EvaporationCapacity = x.EvaporationCapacity,
                IntendedWorkingPressure = x.IntendedWorkingPressure,

                BoilerType = x.BoilerType,
                DrawingNo = x.DrawingNo,

                BoilerDrawing = x.BoilerDrawing,
                FeedPipelineDrawing = x.FeedPipelineDrawing,
                PressurePartCalculation = x.PressurePartCalculation,

                ValidFrom = x.ValidFrom,
                ValidUpto = x.ValidUpto,

                Type = x.Type,
                Version = x.Version,
                Status = x.Status,

                ApplicationPDFUrl = x.ApplicationPDFUrl,
                ObjectionLetterUrl = objectionLetter?.FileUrl,
                CertificateUrl = certificate?.CertificateUrl,
                TransactionHistory = transactionHistory
            };
        }

        public async Task<bool> UpdateBoilerDrawingAsync(string applicationId, BoilerDrawingCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
                return false;

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                var registration = await _dbcontext.BoilerDrawingApplications
                    .FirstOrDefaultAsync(x => x.ApplicationId == applicationId);

                if (registration == null)
                    return false;

                if (!registration.Status!.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("Only pending applications can be updated.");

                /* ================= GENERAL ================= */

                registration.FactoryRegistrationNumber = dto.FactoryRegistrationNumber;
                registration.FactoryDetailjson = dto.FactoryDetailjson;

                /* ================= DRAWING DETAILS ================= */

                registration.MakerNumber = dto.MakerNumber;
                registration.MakerNameAndAddress = dto.MakerNameAndAddress;

                registration.HeatingSurfaceArea = dto.HeatingSurfaceArea;
                registration.EvaporationCapacity = dto.EvaporationCapacity;
                registration.IntendedWorkingPressure = dto.IntendedWorkingPressure;

                registration.BoilerType = dto.BoilerType;
                registration.DrawingNo = dto.DrawingNo;

                /* ================= DOCUMENTS ================= */

                registration.BoilerDrawing = dto.BoilerDrawing;
                registration.FeedPipelineDrawing = dto.FeedPipelineDrawing;
                registration.PressurePartCalculation = dto.PressurePartCalculation;

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


        public async Task<BoilerDrawingDetailsDto?> GetLatestApprovedByRegistrationNoAsync(string registrationNo)
        {
            if (string.IsNullOrWhiteSpace(registrationNo))
                throw new ArgumentException("RegistrationNo is required.");

            var latest = await _dbcontext.BoilerDrawingApplications
                .Where(x => x.BoilerDrawingRegistrationNo == registrationNo)
                .OrderByDescending(x => x.Version)
                .FirstOrDefaultAsync();

            if (latest == null)
                return null;

            if (!latest.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                return null;

            return new BoilerDrawingDetailsDto
            {
                ApplicationId = latest.ApplicationId,
                BoilerDrawingRegistrationNo = latest.BoilerDrawingRegistrationNo,

                FactoryRegistrationNumber = latest.FactoryRegistrationNumber,
                FactoryDetailjson = latest.FactoryDetailjson,

                MakerNumber = latest.MakerNumber,
                MakerNameAndAddress = latest.MakerNameAndAddress,

                HeatingSurfaceArea = latest.HeatingSurfaceArea,
                EvaporationCapacity = latest.EvaporationCapacity,
                IntendedWorkingPressure = latest.IntendedWorkingPressure,

                BoilerType = latest.BoilerType,
                DrawingNo = latest.DrawingNo,

                BoilerDrawing = latest.BoilerDrawing,
                FeedPipelineDrawing = latest.FeedPipelineDrawing,
                PressurePartCalculation = latest.PressurePartCalculation,

                ValidFrom = latest.ValidFrom,
                ValidUpto = latest.ValidUpto,

                Type = latest.Type,
                Version = latest.Version,
                Status = latest.Status
            };
        }

        public async Task<List<BoilerDrawingDetailsDto>> GetAllAsync()
        {
            var records = await _dbcontext.BoilerDrawingApplications
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            var result = new List<BoilerDrawingDetailsDto>();

            foreach (var x in records)
            {
                result.Add(new BoilerDrawingDetailsDto
                {
                    ApplicationId = x.ApplicationId,
                    BoilerDrawingRegistrationNo = x.BoilerDrawingRegistrationNo,

                    FactoryRegistrationNumber = x.FactoryRegistrationNumber,
                    FactoryDetailjson = x.FactoryDetailjson,

                    MakerNumber = x.MakerNumber,
                    MakerNameAndAddress = x.MakerNameAndAddress,

                    HeatingSurfaceArea = x.HeatingSurfaceArea,
                    EvaporationCapacity = x.EvaporationCapacity,
                    IntendedWorkingPressure = x.IntendedWorkingPressure,

                    BoilerType = x.BoilerType,
                    DrawingNo = x.DrawingNo,

                    BoilerDrawing = x.BoilerDrawing,
                    FeedPipelineDrawing = x.FeedPipelineDrawing,
                    PressurePartCalculation = x.PressurePartCalculation,

                    ValidFrom = x.ValidFrom,
                    ValidUpto = x.ValidUpto,

                    Type = x.Type,
                    Version = x.Version,
                    Status = x.Status
                });
            }

            return result;
        }


        public async Task<string> CloseBoilerDrawingAsync(     BoilerDrawingClosureDto dto,    Guid userId) 
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.BoilerDrawingRegistrationNo))
                throw new ArgumentException("BoilerDrawingRegistrationNo is required");

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                /* ================= HISTORY ================= */

                var history = await _dbcontext.BoilerDrawingApplications
                    .Where(x => x.BoilerDrawingRegistrationNo == dto.BoilerDrawingRegistrationNo)
                    .OrderByDescending(x => x.Version)
                    .ToListAsync();

                if (!history.Any())
                    throw new Exception("Boiler drawing registration not found.");

                /* ================= BLOCK IF PENDING ================= */

                var pending = history.FirstOrDefault(x => x.Status == "Pending");

                if (pending != null)
                    throw new Exception(
                        $"Cannot close. A {pending.Type?.ToUpper()} request (Version {pending.Version}) is already pending."
                    );

                /* ================= LATEST MUST BE APPROVED ================= */

                var latest = history.First();

                if (!latest.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("Only the latest APPROVED version can be closed.");

                /* ================= PREVENT MULTIPLE CLOSURE ================= */

                var alreadyClosed = await _dbcontext.BoilerDrawingClosures
                    .AnyAsync(x =>
                        x.BoilerDrawingRegistrationNo == dto.BoilerDrawingRegistrationNo &&
                        x.Status == "Approved");

                if (alreadyClosed)
                    throw new Exception("This boiler drawing registration is already closed.");

                var pendingClosure = await _dbcontext.BoilerDrawingClosures
                    .AnyAsync(x =>
                        x.BoilerDrawingRegistrationNo == dto.BoilerDrawingRegistrationNo &&
                        x.Status == "Pending");

                if (pendingClosure)
                    throw new Exception("Closure request already submitted and pending.");

                /* ================= APPLICATION NUMBER ================= */

                var applicationId = await GenerateApplicationNumberAsync("close");

                /* ================= INSERT CLOSURE ================= */

                var closure = new BoilerDrawingClosure
                {
                    Id = Guid.NewGuid(),

                    ApplicationId = applicationId,

                    BoilerDrawingRegistrationNo = dto.BoilerDrawingRegistrationNo,

                    ClosureReason = dto.ClosureReason,

                    ClosureDate = dto.ClosureDate,

                    Remarks = dto.Remarks,

                    DocumentPath = dto.DocumentPath,

                    Type = "close",

                    Status = "Pending",

                    CreatedDate = DateTime.Now,

                    UpdatedDate = DateTime.Now,

                    IsActive = true
                };

                _dbcontext.BoilerDrawingClosures.Add(closure);

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


        public async Task<string> GenerateDrawingPdfAsync(string applicationId)
        {
            var entity = await _dbcontext.BoilerDrawingApplications
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (entity == null)
                throw new Exception("Boiler Drawing application not found");

            var uploadPath = Path.Combine(_environment.WebRootPath, "boiler-drawing-forms");
            Directory.CreateDirectory(uploadPath);

            var fileName = $"drawing_application_{entity.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/boiler-drawing-forms/{fileName}";

            var sections = new List<PdfSection>
            {
                new PdfSection
                {
                    Title = "Application Info",
                    Rows = new List<(string, string?)>
                    {
                        ("Application Id", entity.ApplicationId),
                        ("Boiler Drawing Registration No", entity.BoilerDrawingRegistrationNo),
                        ("Factory Registration Number", entity.FactoryRegistrationNumber),
                        ("Type", entity.Type),
                        ("Status", entity.Status)
                    }
                },
                new PdfSection
                {
                    Title = "Drawing Details",
                    Rows = new List<(string, string?)>
                    {
                        ("Maker Number", entity.MakerNumber),
                        ("Maker Name And Address", entity.MakerNameAndAddress),
                        ("Heating Surface Area", entity.HeatingSurfaceArea),
                        ("Evaporation Capacity", entity.EvaporationCapacity),
                        ("Intended Working Pressure", entity.IntendedWorkingPressure),
                        ("Boiler Type", entity.BoilerType),
                        ("Drawing No", entity.DrawingNo)
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
                "Form-BDR1",
                "(See Indian Boilers Act, 1923)",
                "Application for Boiler Drawing Registration",
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
            var fileName = $"drawing_objection_{safeAppId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
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
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new BoilerPageBorderEventHandler());
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
            subject.Add(new Text("Boiler Drawing Registration").SetFont(regularFont));
            document.Add(subject);

            document.Add(new Paragraph("The details of your boiler drawing as per application and submitted documents are shown below:-").SetFont(regularFont).SetMarginBottom(5));

            var table = new PdfTable(new float[] { 150, 1 }).UseAllAvailableWidth();
            PdfCell Fmt(string text, PdfFont font) => new PdfCell().Add(new Paragraph(text ?? "-").SetFont(font).SetFontSize(12)).SetPadding(5);
            table.AddCell(Fmt("Registration No", boldFont)); table.AddCell(Fmt(dto.BoilerRegistrationNo, regularFont));
            table.AddCell(Fmt("Boiler Type", boldFont)); table.AddCell(Fmt(dto.BoilerType, regularFont));
            table.AddCell(Fmt("Heating Surface Area", boldFont)); table.AddCell(Fmt(dto.HeatingSurfaceArea?.ToString(), regularFont));
            table.AddCell(Fmt("Evaporation Capacity", boldFont)); table.AddCell(Fmt(dto.EvaporationCapacity?.ToString(), regularFont));
            table.AddCell(Fmt("Working Pressure", boldFont)); table.AddCell(Fmt(dto.WorkingPressure?.ToString(), regularFont));
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
            var entity = await _dbcontext.BoilerDrawingApplications.FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
            if (entity == null) throw new Exception("Boiler Drawing application not found");

            var safeAppId = (entity.ApplicationId ?? applicationId).Replace("/", "_").Replace("\\", "_");
            var fileName = $"drawing_certificate_{safeAppId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
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
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new BoilerCertFooterEventHandler(boldFont, regularFont, footerDate, postName, userName));
            using var document = new Document(pdf);
            document.SetMargins(40, 40, 130, 40);

            // Header
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

            document.Add(new Paragraph($"Registration No.:-  {entity.BoilerDrawingRegistrationNo ?? "-"}").SetFont(boldFont).SetFontSize(10).SetMarginBottom(6f));

            document.Add(new Paragraph("Sub:-  Approval of Boiler Drawing Registration").SetFont(boldFont).SetFontSize(11).SetMarginBottom(2f));
            document.Add(new Paragraph("The details of your application as per submitted documents are shown below:-").SetFont(regularFont).SetFontSize(11).SetMarginBottom(6f));

            var blackBorder = new SolidBorder(new DeviceRgb(0, 0, 0), 0.75f);
            PdfCell BlackCell(string text, PdfFont font, float size = 10f) => new PdfCell().Add(new Paragraph(text ?? "-").SetFont(font).SetFontSize(size)).SetBorderTop(blackBorder).SetBorderBottom(blackBorder).SetBorderLeft(blackBorder).SetBorderRight(blackBorder).SetPadding(5f);

            var detailsTable = new PdfTable(new float[] { 150f, 350f }).UseAllAvailableWidth().SetMarginBottom(10f);
            detailsTable.AddCell(BlackCell("Maker Number", boldFont)); detailsTable.AddCell(BlackCell(entity.MakerNumber ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Boiler Type", boldFont)); detailsTable.AddCell(BlackCell(entity.BoilerType ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Heating Surface Area", boldFont)); detailsTable.AddCell(BlackCell(entity.HeatingSurfaceArea ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Evaporation Capacity", boldFont)); detailsTable.AddCell(BlackCell(entity.EvaporationCapacity ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Working Pressure", boldFont)); detailsTable.AddCell(BlackCell(entity.IntendedWorkingPressure ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Drawing No", boldFont)); detailsTable.AddCell(BlackCell(entity.DrawingNo ?? "-", regularFont));
            document.Add(detailsTable);

            document.Add(new Paragraph("Your Boiler Drawing is approved under the Indian Boilers Act, 1923 and the rules made thereunder.").SetFont(regularFont).SetFontSize(11).SetMarginBottom(20f));

            var pageWidth = pdf.GetDefaultPageSize().GetWidth();
            document.Add(new Paragraph("This is a computer generated certificate and bears scanned signature. No physical signature is required on this approval. You can verify this approval by visiting www.rajfab.rajasthan.gov.in and entering Application No./ID after clicking the link for verification on the page.").SetFont(regularFont).SetFontSize(6.5f).SetFontColor(ColorConstants.GRAY).SetTextAlignment(TextAlignment.JUSTIFIED).SetMultipliedLeading(1.1f).SetFixedPosition(35, 8, pageWidth - 70));

            return fileUrl;
        }

        private sealed class BoilerPageBorderEventHandler : AbstractPdfDocumentEventHandler
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

        private sealed class BoilerCertFooterEventHandler : AbstractPdfDocumentEventHandler
        {
            private readonly PdfFont _boldFont, _regularFont;
            private readonly DateOnly _date;
            private readonly string _postName, _userName;

            public BoilerCertFooterEventHandler(PdfFont boldFont, PdfFont regularFont, DateOnly date, string postName, string userName)
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

                float zoneH = 65f, zoneY = lineY + 4f, belowY = lineY - 4f - zoneH;
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