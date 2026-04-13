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
    public class SMTCRegistrationService: ISMTCRegistrationService
    {
        private readonly ApplicationDbContext _dbcontext;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPaymentService _paymentService;

        public SMTCRegistrationService(ApplicationDbContext context, IWebHostEnvironment environment, IConfiguration config, IHttpContextAccessor httpContextAccessor, IPaymentService paymentService)
        {
            _dbcontext = context;
            _environment = environment;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _paymentService = paymentService;
        }



        private async Task<string> GenerateSMTCApplicationNumberAsync(string type)
        {
            var year = DateTime.Now.Year;

            string prefix = type switch
            {
                "new" => $"SMTC{year}/CIFB/",
                "amend" => $"SMTCAM{year}/CIFB/",
                "renew" => $"SMTCRN{year}/CIFB/",
                _ => throw new Exception("Invalid type")
            };

            var last = await _dbcontext.SMTCRegistrations
                .Where(x => x.ApplicationId.StartsWith(prefix))
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.ApplicationId)
                .FirstOrDefaultAsync();

            int next = 1;

            if (last != null)
            {
                var num = last.Split('/').Last();
                int.TryParse(num, out next);
                next++;
            }

            return $"{prefix}{next:D4}";
        }


        private async Task<string> GenerateSMTCRegistrationNoAsync()
        {
            var last = await _dbcontext.SMTCRegistrations
                .Where(x => x.SMTCRegistrationNo != null)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.SMTCRegistrationNo)
                .FirstOrDefaultAsync();

            int next = 1;

            if (last != null)
            {
                var num = last.Split('-').Last();
                int.TryParse(num, out next);
                next++;
            }

            return $"SMTC-{next:D4}";
        }

        public async Task<string> SaveSMTCAsync(  CreateSMTCRegistrationDto dto,   Guid userId,   string? type,    string? smtcRegistrationNo)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            type = type?.ToLower() ?? "new";

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                SMTCRegistration? baseRecord = null;

                if (type == "amend")
                {
                    if (string.IsNullOrWhiteSpace(smtcRegistrationNo))
                        throw new Exception("SMTCRegistrationNo required.");

                    var pendingExists = await _dbcontext.SMTCRegistrations
                        .AnyAsync(x => x.SMTCRegistrationNo == smtcRegistrationNo
                                    && x.Status == "Pending");

                    if (pendingExists)
                        throw new Exception("Previous amendment still pending.");

                    baseRecord = await _dbcontext.SMTCRegistrations
                        .Where(x => x.SMTCRegistrationNo == smtcRegistrationNo
                                 && x.Status == "Approved")
                        .OrderByDescending(x => x.Version)
                        .FirstOrDefaultAsync();

                    if (baseRecord == null)
                        throw new Exception("Approved record not found.");
                }

               

                var applicationNumber = await GenerateSMTCApplicationNumberAsync(type);

                var finalRegistrationNo =
                    type == "amend"
                    ? baseRecord!.SMTCRegistrationNo
                    : await GenerateSMTCRegistrationNoAsync();

                var version =
                    type == "amend"
                    ? baseRecord!.Version + 0.1m
                    : 1.0m;

                

                var registration = new SMTCRegistration
                {
                    Id = Guid.NewGuid(),

                    ApplicationId = applicationNumber,

                    SMTCRegistrationNo = finalRegistrationNo,

                    FactoryRegistrationNo = dto.FactoryRegistrationNo,

                    TrainingCenterAvailable = dto.TrainingCenterAvailable,

                    SeatingCapacity = dto.SeatingCapacity,

                    TrainingCenterPhotoPath = dto.TrainingCenterPhotoPath,

                    AudioVideoFacility = dto.AudioVideoFacility,

                    Comments = dto.Comments,

                    Type = type,

                    Status = "Pending",

                    Version = version,

                    Amount = type == "new" ? 5000m : 100m,

                    ValidUpto = DateTime.Now.AddYears(1),

                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _dbcontext.SMTCRegistrations.Add(registration);

                await _dbcontext.SaveChangesAsync();

               

                foreach (var trainer in dto.Trainers)
                {
                    var trainerEntity = new SMTCTrainerDetail
                    {
                        Id = Guid.NewGuid(),

                        SMTCRegistrationId = registration.Id,

                        TrainerName = trainer.TrainerName,

                        TotalYearsExperience = trainer.TotalYearsExperience,

                        Mobile = trainer.Mobile,

                        PhotoPath = trainer.PhotoPath,

                        DegreeDocumentPath = trainer.DegreeDocumentPath
                    };

                    _dbcontext.SMTCTrainerDetails.Add(trainerEntity);

                    await _dbcontext.SaveChangesAsync();

                   

                    if (trainer.EducationDetails != null && trainer.EducationDetails.Any())
                    {
                        foreach (var edu in trainer.EducationDetails)
                        {
                            var education = new SMTCTrainerEducationDetail
                            {
                                Id = Guid.NewGuid(),

                                TrainerId = trainerEntity.Id,

                                EducationType = edu.EducationType,

                                Course = edu.Course,

                                Degree = edu.Degree,

                                UniversityCollege = edu.UniversityCollege,

                                PassingYear = edu.PassingYear,

                                Specialization = edu.Specialization
                            };

                            _dbcontext.SMTCTrainerEducationDetails.Add(education);
                        }
                    }
                }

                await _dbcontext.SaveChangesAsync();

                // Auto-generate application PDF
                try { await GenerateSmtcPdfAsync(registration.ApplicationId); } catch { /* PDF generation failure should not block submission */ }

                if (type == "new")
                {
                    var module = await _dbcontext.Set<FormModule>()
                        .FirstOrDefaultAsync(m => m.Name == "SMTC Registration")
                        ?? throw new Exception("SMTC Registration module not found.");

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

                return registration.ApplicationId;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<SMTCRegistrationDetailsDto?> GetByApplicationIdAsync(string applicationId)
        {
            var x = await _dbcontext.SMTCRegistrations
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (x == null)
                return null;

            // Look up PDF URLs
            var objectionLetter = await _dbcontext.ApplicationObjectionLetters
                .AsNoTracking()
                .Where(o => o.ApplicationId == applicationId)
                .OrderByDescending(o => o.CreatedDate)
                .FirstOrDefaultAsync();

            var cert = await _dbcontext.Certificates
                .AsNoTracking()
                .Where(c => c.ApplicationId == applicationId ||
                    (!string.IsNullOrEmpty(x.SMTCRegistrationNo) && c.RegistrationNumber == x.SMTCRegistrationNo))
                .OrderByDescending(c => c.IssuedAt)
                .FirstOrDefaultAsync();

            var transactionHistory = await _dbcontext.Set<Transaction>()
                .AsNoTracking()
                .Where(t => t.ApplicationId == applicationId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var trainers = await _dbcontext.SMTCTrainerDetails
                .AsNoTracking()
                .Where(t => t.SMTCRegistrationId == x.Id)
                .ToListAsync();

            var education = await (
                from e in _dbcontext.SMTCTrainerEducationDetails.AsNoTracking()
                join t in _dbcontext.SMTCTrainerDetails.AsNoTracking()
                    on e.TrainerId equals t.Id
                where t.SMTCRegistrationId == x.Id
                select e
            ).ToListAsync();

            return new SMTCRegistrationDetailsDto
            {
                ApplicationId = x.ApplicationId,
                SMTCRegistrationNo = x.SMTCRegistrationNo,
                FactoryRegistrationNo = x.FactoryRegistrationNo,

                TrainingCenterAvailable = x.TrainingCenterAvailable,
                SeatingCapacity = x.SeatingCapacity,
                TrainingCenterPhotoPath = x.TrainingCenterPhotoPath,
                AudioVideoFacility = x.AudioVideoFacility,
                Comments = x.Comments,

                Type = x.Type,
                Status = x.Status,
                Version = x.Version,
                ValidUpto = x.ValidUpto,

                ApplicationPDFUrl = x.ApplicationPDFUrl,
                ObjectionLetterUrl = objectionLetter?.FileUrl,
                CertificateUrl = cert?.CertificateUrl,
                TransactionHistory = transactionHistory,

                Trainers = trainers.Select(t => new SMTCTrainerDto
                {
                    TrainerName = t.TrainerName,
                    TotalYearsExperience = t.TotalYearsExperience,
                    Mobile = t.Mobile,
                    PhotoPath = t.PhotoPath,
                    DegreeDocumentPath = t.DegreeDocumentPath,

                    EducationDetails = education
                        .Where(e => e.TrainerId == t.Id)
                        .Select(e => new SMTCEducationDto
                        {
                            EducationType = e.EducationType,
                            Course = e.Course,
                            Degree = e.Degree,
                            UniversityCollege = e.UniversityCollege,
                            PassingYear = e.PassingYear,
                            Specialization = e.Specialization
                        }).ToList()
                }).ToList()
            };
        }

        public async Task<SMTCRegistrationDetailsDto?> GetLatestApprovedByRegistrationNoAsync(string registrationNo)
        {
            if (string.IsNullOrWhiteSpace(registrationNo))
                throw new ArgumentException("SMTCRegistrationNo required.");

            var latest = await _dbcontext.SMTCRegistrations
                .Where(x => x.SMTCRegistrationNo == registrationNo)
                .OrderByDescending(x => x.Version)
                .FirstOrDefaultAsync();

            if (latest == null)
                return null;

            if (!latest.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                return null;

            var trainers = await _dbcontext.SMTCTrainerDetails
                .Where(t => t.SMTCRegistrationId == latest.Id)
                .ToListAsync();

            var trainerIds = trainers.Select(t => t.Id).ToList();

            var education = await (
      from e in _dbcontext.SMTCTrainerEducationDetails
      join t in _dbcontext.SMTCTrainerDetails
          on e.TrainerId equals t.Id
      where t.SMTCRegistrationId == latest.Id
      select e
  ).ToListAsync();

            return new SMTCRegistrationDetailsDto
            {
                ApplicationId = latest.ApplicationId,
                SMTCRegistrationNo = latest.SMTCRegistrationNo,
                FactoryRegistrationNo = latest.FactoryRegistrationNo,

                TrainingCenterAvailable = latest.TrainingCenterAvailable,
                SeatingCapacity = latest.SeatingCapacity,
                TrainingCenterPhotoPath = latest.TrainingCenterPhotoPath,
                AudioVideoFacility = latest.AudioVideoFacility,
                Comments = latest.Comments,

                Type = latest.Type,
                Status = latest.Status,
                Version = latest.Version,
                ValidUpto = latest.ValidUpto,

                Trainers = trainers.Select(t => new SMTCTrainerDto
                {
                    TrainerName = t.TrainerName,
                    TotalYearsExperience = t.TotalYearsExperience,
                    Mobile = t.Mobile,
                    PhotoPath = t.PhotoPath,
                    DegreeDocumentPath = t.DegreeDocumentPath,

                    EducationDetails = education
                        .Where(e => e.TrainerId == t.Id)
                        .Select(e => new SMTCEducationDto
                        {
                            EducationType = e.EducationType,
                            Course = e.Course,
                            Degree = e.Degree,
                            UniversityCollege = e.UniversityCollege,
                            PassingYear = e.PassingYear,
                            Specialization = e.Specialization
                        }).ToList()
                }).ToList()
            };
        }

        public async Task<List<SMTCRegistrationDetailsDto>> GetAllAsync()
        {
            var records = await _dbcontext.SMTCRegistrations
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var trainers = await _dbcontext.SMTCTrainerDetails.ToListAsync();

            var education = await _dbcontext.SMTCTrainerEducationDetails.ToListAsync();

            var result = new List<SMTCRegistrationDetailsDto>();

            foreach (var x in records)
            {
                var registrationTrainers = trainers
                    .Where(t => t.SMTCRegistrationId == x.Id)
                    .ToList();

                result.Add(new SMTCRegistrationDetailsDto
                {
                    ApplicationId = x.ApplicationId,
                    SMTCRegistrationNo = x.SMTCRegistrationNo,
                    FactoryRegistrationNo = x.FactoryRegistrationNo,

                    TrainingCenterAvailable = x.TrainingCenterAvailable,
                    SeatingCapacity = x.SeatingCapacity,
                    TrainingCenterPhotoPath = x.TrainingCenterPhotoPath,
                    AudioVideoFacility = x.AudioVideoFacility,
                    Comments = x.Comments,

                    Type = x.Type,
                    Status = x.Status,
                    Version = x.Version,
                    ValidUpto = x.ValidUpto,

                    Trainers = registrationTrainers.Select(t => new SMTCTrainerDto
                    {
                        TrainerName = t.TrainerName,
                        TotalYearsExperience = t.TotalYearsExperience,
                        Mobile = t.Mobile,
                        PhotoPath = t.PhotoPath,
                        DegreeDocumentPath = t.DegreeDocumentPath,

                        EducationDetails = education
                            .Where(e => e.TrainerId == t.Id)
                            .Select(e => new SMTCEducationDto
                            {
                                EducationType = e.EducationType,
                                Course = e.Course,
                                Degree = e.Degree,
                                UniversityCollege = e.UniversityCollege,
                                PassingYear = e.PassingYear,
                                Specialization = e.Specialization
                            }).ToList()
                    }).ToList()
                });
            }

            return result;
        }


        public async Task<string> GenerateSmtcPdfAsync(string applicationId)
        {
            var entity = await _dbcontext.SMTCRegistrations
                .Include(x => x.Trainers)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (entity == null)
                throw new Exception("SMTC Registration not found");

            var uploadPath = Path.Combine(_environment.WebRootPath, "boiler-smtc-forms");
            Directory.CreateDirectory(uploadPath);

            var fileName = $"smtc_application_{entity.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/boiler-smtc-forms/{fileName}";

            var sections = new List<PdfSection>
            {
                new PdfSection
                {
                    Title = "Application Info",
                    Rows = new List<(string, string?)>
                    {
                        ("Application Id", entity.ApplicationId),
                        ("SMTC Registration No", entity.SMTCRegistrationNo),
                        ("Factory Registration No", entity.FactoryRegistrationNo),
                        ("Type", entity.Type),
                        ("Status", entity.Status)
                    }
                },
                new PdfSection
                {
                    Title = "Training Center Details",
                    Rows = new List<(string, string?)>
                    {
                        ("Training Center Available", entity.TrainingCenterAvailable.ToString()),
                        ("Seating Capacity", entity.SeatingCapacity?.ToString()),
                        ("Audio Video Facility", entity.AudioVideoFacility?.ToString()),
                        ("Comments", entity.Comments)
                    }
                },
                new PdfSection
                {
                    Title = "Validity",
                    Rows = new List<(string, string?)>
                    {
                        ("Valid Upto", entity.ValidUpto?.ToString("dd/MM/yyyy"))
                    }
                }
            };

            BoilerPdfHelper.GeneratePdf(
                filePath,
                "Form-SMTC1",
                "(See Indian Boilers Act, 1923)",
                "Application for SMTC Registration",
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
            var fileName = $"smtc_objection_{safeAppId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
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
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new SmtcPageBorderEventHandler());
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
            subject.Add(new Text("SMTC Registration").SetFont(regularFont));
            document.Add(subject);

            document.Add(new Paragraph("The details of your SMTC as per application and submitted documents are shown below:-").SetFont(regularFont).SetMarginBottom(5));

            var table = new PdfTable(new float[] { 150, 1 }).UseAllAvailableWidth();
            PdfCell Fmt(string text, PdfFont font) => new PdfCell().Add(new Paragraph(text ?? "-").SetFont(font).SetFontSize(12)).SetPadding(5);
            table.AddCell(Fmt("Registration No", boldFont)); table.AddCell(Fmt(dto.BoilerRegistrationNo ?? "-", regularFont));
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
            var entity = await _dbcontext.SMTCRegistrations.FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
            if (entity == null) throw new Exception("SMTC application not found");

            var safeAppId = (entity.ApplicationId ?? applicationId).Replace("/", "_").Replace("\\", "_");
            var fileName = $"smtc_certificate_{safeAppId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
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
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new SmtcCertFooterEventHandler(boldFont, regularFont, footerDate, postName, userName));
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

            document.Add(new Paragraph($"Registration No.:-  {entity.SMTCRegistrationNo ?? "-"}").SetFont(boldFont).SetFontSize(10).SetMarginBottom(6f));

            document.Add(new Paragraph("Sub:-  Approval of SMTC Registration").SetFont(boldFont).SetFontSize(11).SetMarginBottom(2f));
            document.Add(new Paragraph("The details of your application as per submitted documents are shown below:-").SetFont(regularFont).SetFontSize(11).SetMarginBottom(6f));

            var blackBorder = new SolidBorder(new DeviceRgb(0, 0, 0), 0.75f);
            PdfCell BlackCell(string text, PdfFont font, float size = 10f) => new PdfCell().Add(new Paragraph(text ?? "-").SetFont(font).SetFontSize(size)).SetBorderTop(blackBorder).SetBorderBottom(blackBorder).SetBorderLeft(blackBorder).SetBorderRight(blackBorder).SetPadding(5f);

            var detailsTable = new PdfTable(new float[] { 150f, 350f }).UseAllAvailableWidth().SetMarginBottom(10f);
            detailsTable.AddCell(BlackCell("Factory Reg. No", boldFont)); detailsTable.AddCell(BlackCell(entity.FactoryRegistrationNo ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Seating Capacity", boldFont)); detailsTable.AddCell(BlackCell(entity.SeatingCapacity?.ToString() ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Training Center", boldFont)); detailsTable.AddCell(BlackCell(entity.TrainingCenterAvailable ? "Yes" : "No", regularFont));
            detailsTable.AddCell(BlackCell("A/V Facility", boldFont)); detailsTable.AddCell(BlackCell(entity.AudioVideoFacility == true ? "Yes" : "No", regularFont));
            detailsTable.AddCell(BlackCell("Valid Upto", boldFont)); detailsTable.AddCell(BlackCell(entity.ValidUpto?.ToString("dd/MM/yyyy") ?? "-", regularFont));
            document.Add(detailsTable);

            document.Add(new Paragraph("Your SMTC Registration is approved under the Indian Boilers Act, 1923 and the rules made thereunder.").SetFont(regularFont).SetFontSize(11).SetMarginBottom(20f));

            var pageWidth = pdf.GetDefaultPageSize().GetWidth();
            document.Add(new Paragraph("This is a computer generated certificate and bears scanned signature. No physical signature is required on this approval. You can verify this approval by visiting www.rajfab.rajasthan.gov.in and entering Application No./ID after clicking the link for verification on the page.").SetFont(regularFont).SetFontSize(6.5f).SetFontColor(ColorConstants.GRAY).SetTextAlignment(TextAlignment.JUSTIFIED).SetMultipliedLeading(1.1f).SetFixedPosition(35, 8, pageWidth - 70));

            return fileUrl;
        }

        private sealed class SmtcPageBorderEventHandler : AbstractPdfDocumentEventHandler
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

        private sealed class SmtcCertFooterEventHandler : AbstractPdfDocumentEventHandler
        {
            private readonly PdfFont _boldFont, _regularFont;
            private readonly DateOnly _date;
            private readonly string _postName, _userName;

            public SmtcCertFooterEventHandler(PdfFont boldFont, PdfFont regularFont, DateOnly date, string postName, string userName)
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