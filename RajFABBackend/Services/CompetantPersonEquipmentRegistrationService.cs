using iText.Kernel.Pdf;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Borders;
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.DTOs.CompetantpersonDtos;
using RajFabAPI.Models;
using RajFabAPI.Models.BoilerModels;
using RajFabAPI.Models.CompetentPerson;
using RajFabAPI.Services.Interface;
using System.Text.Json;
using PdfTable = iText.Layout.Element.Table;
using PdfCell = iText.Layout.Element.Cell;

namespace RajFabAPI.Services
{
    public class CompetantPersonEquipmentRegistartionService : ICompetantPersonEquipmentRegistartionService
    {
        private readonly ApplicationDbContext _dbcontext;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CompetantPersonEquipmentRegistartionService(ApplicationDbContext dbcontext, IWebHostEnvironment environment, IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            _dbcontext = dbcontext;
            _environment = environment;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
        }

        private async Task<string> GenerateEquipmentRegistrationNoAsync()
        {
            var last = await _dbcontext.CompetentEquipmentRegistrations
                .Where(x => x.CompetentEquipmentRegistrationNo != null)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.CompetentEquipmentRegistrationNo)
                .FirstOrDefaultAsync();

            int next = 1;

            if (!string.IsNullOrEmpty(last))
            {
                var number = last.Replace("CER-", "");

                if (int.TryParse(number, out int n))  
                    next = n + 1;
            }

            return $"CER-{next:D4}";
        }

        private async Task<string> GenerateEquipmentApplicationNumberAsync(string type)
        {
            var year = DateTime.Now.Year;

            string prefix = type switch
            {
                "new" => $"CE{year}/CIFB/",
                "amend" => $"CEAM{year}/CIFB/",
                _ => throw new Exception("Invalid equipment type")
            };

            var lastApp = await _dbcontext.CompetentEquipmentRegistrations
                .Where(x => x.ApplicationId.StartsWith(prefix))
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.ApplicationId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastApp))
            {
                var lastNumberPart = lastApp.Split('/').Last();

                if (int.TryParse(lastNumberPart, out int lastNumber))
                    nextNumber = lastNumber + 1;
            }

            return $"{prefix}{nextNumber:D4}";
        }


        public async Task<string> SaveCompetentEquipmentAsync(  CreateCompetentEquipmentDto dto,   Guid userId,    string? type,  string? equipmentRegistrationNo) 
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            type = type?.ToLower() ?? "new";

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
             

                var competent = await _dbcontext.CompetentPersonRegistrations
                    .Where(x => x.CompetentRegistrationNo == dto.CompetentRegistrationNo)
                    .OrderByDescending(x => x.Version)
                    .FirstOrDefaultAsync();

                if (competent == null)
                    throw new Exception("Competent registration not found.");

                CompetentEquipmentRegistration? baseRecord = null;

               

                if (type == "amend")
                {
                    if (string.IsNullOrWhiteSpace(equipmentRegistrationNo))
                        throw new Exception("CompetentEquipmentRegistrationNo required.");

                    var pendingExists = await _dbcontext.CompetentEquipmentRegistrations
                        .AnyAsync(x => x.CompetentEquipmentRegistrationNo == equipmentRegistrationNo
                                    && x.Status == "Pending");

                    if (pendingExists)
                        throw new Exception("Previous amendment still pending.");

                    baseRecord = await _dbcontext.CompetentEquipmentRegistrations
                        .Where(x => x.CompetentEquipmentRegistrationNo == equipmentRegistrationNo
                                 && x.Status == "Approved")
                        .OrderByDescending(x => x.Version)
                        .FirstOrDefaultAsync();

                    if (baseRecord == null)
                        throw new Exception("Approved equipment record not found.");
                }


                var applicationNumber = await GenerateEquipmentApplicationNumberAsync(type);


                var finalRegistrationNo =
                    type == "amend"
                    ? baseRecord!.CompetentEquipmentRegistrationNo
                    : await GenerateEquipmentRegistrationNoAsync();

                

                var version =
                    type == "amend"
                    ? baseRecord!.Version + 0.1m
                    : 1.0m;

                

                var registration = new CompetentEquipmentRegistration
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = applicationNumber,

                    CompetentEquipmentRegistrationNo = finalRegistrationNo,

                    CompetentRegistrationNo = dto.CompetentRegistrationNo,

                    Type = type,
                    Status = "Pending",

                    Version = version,

                    RenewalYears = 1,
                    ValidUpto = DateTime.Now.AddYears(1),

                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _dbcontext.CompetentEquipmentRegistrations.Add(registration);
                await _dbcontext.SaveChangesAsync();

               

                foreach (var item in dto.Equipments)
                {
                    var personExists = await _dbcontext.CompetantPersonDetails
                        .AnyAsync(x => x.Id == item.CompetentPersonId);

                    if (!personExists)
                        throw new Exception("Invalid CompetentPersonId.");

                    var equipment = new CompetentPersonEquipment
                    {
                        Id = Guid.NewGuid(),

                        EquipmentRegistrationId = registration.Id,

                        CompetentPersonId = item.CompetentPersonId,

                        EquipmentType = item.EquipmentType,
                        EquipmentName = item.EquipmentName,

                        IdentificationNumber = item.IdentificationNumber,

                        CalibrationCertificateNumber = item.CalibrationCertificateNumber,

                        DateOfCalibration = item.DateOfCalibration,
                        CalibrationValidity = item.CalibrationValidity,

                        CalibrationCertificatePath = item.CalibrationCertificatePath
                    };

                    _dbcontext.CompetentPersonEquipments.Add(equipment);
                }

                await _dbcontext.SaveChangesAsync();

                await tx.CommitAsync();

                return registration.ApplicationId;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }



        public async Task<CompetentEquipmentDetailsDto?> GetByApplicationIdAsync(string applicationId)
        {
            var x = await _dbcontext.CompetentEquipmentRegistrations
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (x == null)
                return null;

            var equipments = await _dbcontext.CompetentPersonEquipments
                .Where(e => e.EquipmentRegistrationId == x.Id)
                .ToListAsync();

            return new CompetentEquipmentDetailsDto
            {
                ApplicationId = x.ApplicationId,
                CompetentRegistrationNo = x.CompetentRegistrationNo,
                CompetentEquipmentRegistrationNo = x.CompetentEquipmentRegistrationNo,

                Type = x.Type,
                Status = x.Status,
                Version = x.Version,
                ValidUpto = x.ValidUpto,

                Equipments = equipments.Select(e => new CompetentEquipmentDto
                {
                    CompetentPersonId = e.CompetentPersonId,
                    EquipmentType = e.EquipmentType,
                    EquipmentName = e.EquipmentName,
                    IdentificationNumber = e.IdentificationNumber,
                    CalibrationCertificateNumber = e.CalibrationCertificateNumber,
                    DateOfCalibration = e.DateOfCalibration,
                    CalibrationValidity = e.CalibrationValidity,
                    CalibrationCertificatePath = e.CalibrationCertificatePath
                }).ToList()
            };
        }

        public async Task<CompetentEquipmentDetailsDto?> GetLatestApprovedByRegistrationNoAsync(string registrationNo)
        {
            if (string.IsNullOrWhiteSpace(registrationNo))
                throw new ArgumentException("CompetentEquipmentRegistrationNo required.");

            var latest = await _dbcontext.CompetentEquipmentRegistrations
                .Where(x => x.CompetentEquipmentRegistrationNo == registrationNo)
                .OrderByDescending(x => x.Version)
                .FirstOrDefaultAsync();

            if (latest == null)
                return null;

            if (!latest.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                return null;

            var equipments = await _dbcontext.CompetentPersonEquipments
                .Where(e => e.EquipmentRegistrationId == latest.Id)
                .ToListAsync();

            return new CompetentEquipmentDetailsDto
            {
                ApplicationId = latest.ApplicationId,
                CompetentRegistrationNo = latest.CompetentRegistrationNo,
                CompetentEquipmentRegistrationNo = latest.CompetentEquipmentRegistrationNo,

                Type = latest.Type,
                Status = latest.Status,
                Version = latest.Version,
                ValidUpto = latest.ValidUpto,

                Equipments = equipments.Select(e => new CompetentEquipmentDto
                {
                    CompetentPersonId = e.CompetentPersonId,
                    EquipmentType = e.EquipmentType,
                    EquipmentName = e.EquipmentName,
                    IdentificationNumber = e.IdentificationNumber,
                    CalibrationCertificateNumber = e.CalibrationCertificateNumber,
                    DateOfCalibration = e.DateOfCalibration,
                    CalibrationValidity = e.CalibrationValidity,
                    CalibrationCertificatePath = e.CalibrationCertificatePath
                }).ToList()
            };
        }

        public async Task<List<CompetentEquipmentDetailsDto>> GetAllAsync()
        {
            var records = await _dbcontext.CompetentEquipmentRegistrations
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var result = new List<CompetentEquipmentDetailsDto>();

            foreach (var x in records)
            {
                var equipments = await _dbcontext.CompetentPersonEquipments
                    .Where(e => e.EquipmentRegistrationId == x.Id)
                    .ToListAsync();

                result.Add(new CompetentEquipmentDetailsDto
                {
                    ApplicationId = x.ApplicationId,
                    CompetentRegistrationNo = x.CompetentRegistrationNo,
                    CompetentEquipmentRegistrationNo = x.CompetentEquipmentRegistrationNo,

                    Type = x.Type,
                    Status = x.Status,
                    Version = x.Version,
                    ValidUpto = x.ValidUpto,

                    Equipments = equipments.Select(e => new CompetentEquipmentDto
                    {
                        CompetentPersonId = e.CompetentPersonId,
                        EquipmentType = e.EquipmentType,
                        EquipmentName = e.EquipmentName,
                        IdentificationNumber = e.IdentificationNumber,
                        CalibrationCertificateNumber = e.CalibrationCertificateNumber,
                        DateOfCalibration = e.DateOfCalibration,
                        CalibrationValidity = e.CalibrationValidity,
                        CalibrationCertificatePath = e.CalibrationCertificatePath
                    }).ToList()
                });
            }

            return result;
        }

        public async Task<string> GenerateCompetentEquipmentPdfAsync(string applicationId)
        {
            var entity = await _dbcontext.CompetentEquipmentRegistrations
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
            if (entity == null) throw new Exception("Competent Equipment registration not found");

            var equipments = await _dbcontext.CompetentPersonEquipments
                .Where(e => e.EquipmentRegistrationId == entity.Id).ToListAsync();

            var uploadPath = Path.Combine(_environment.WebRootPath, "competent-equipment-forms");
            Directory.CreateDirectory(uploadPath);
            var fileName = $"competent_equipment_application_{entity.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/competent-equipment-forms/{fileName}";

            var sections = new List<PdfSection>
            {
                new PdfSection { Title = "Application Info", Rows = new List<(string, string?)> {
                    ("Application Id", entity.ApplicationId),
                    ("Equipment Registration No", entity.CompetentEquipmentRegistrationNo),
                    ("Competent Registration No", entity.CompetentRegistrationNo),
                    ("Type", entity.Type),
                    ("Status", entity.Status),
                    ("Valid Upto", entity.ValidUpto?.ToString("dd/MM/yyyy"))
                }}
            };

            foreach (var (eq, idx) in equipments.Select((e, i) => (e, i)))
            {
                sections.Add(new PdfSection { Title = $"Equipment {idx + 1}", Rows = new List<(string, string?)> {
                    ("Equipment Type", eq.EquipmentType),
                    ("Equipment Name", eq.EquipmentName),
                    ("Identification Number", eq.IdentificationNumber),
                    ("Calibration Certificate No", eq.CalibrationCertificateNumber),
                    ("Date of Calibration", eq.DateOfCalibration?.ToString("dd/MM/yyyy")),
                    ("Calibration Validity", eq.CalibrationValidity?.ToString("dd/MM/yyyy"))
                }});
            }

            BoilerPdfHelper.GeneratePdf(filePath, "Form-CE1", "(See Indian Boilers Act, 1923)", "Application for Competent Equipment Registration", entity.ApplicationId ?? "-", entity.CreatedAt, sections);

            entity.ApplicationPDFUrl = fileUrl;
            entity.UpdatedAt = DateTime.Now;
            await _dbcontext.SaveChangesAsync();
            return filePath;
        }

        public async Task<string> GenerateObjectionLetter(BoilerObjectionLetterDto dto, string applicationId)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            var safeAppId = applicationId.Replace("/", "_").Replace("\\", "_");
            var fileName = $"competent_equipment_objection_{safeAppId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var webRootPath = _environment.WebRootPath ?? throw new InvalidOperationException("wwwroot is not configured.");
            var uploadPath = Path.Combine(webRootPath, "boiler-objection-letters");
            Directory.CreateDirectory(uploadPath);
            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/boiler-objection-letters/{fileName}";

            var boldFont = iText.Kernel.Font.PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regularFont = iText.Kernel.Font.PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            using var writer = new PdfWriter(filePath);
            using var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
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
            subject.Add(new Text("Competent Equipment Registration").SetFont(regularFont));
            document.Add(subject);

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
            var entity = await _dbcontext.CompetentEquipmentRegistrations.FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
            if (entity == null) throw new Exception("Competent Equipment application not found");

            var safeAppId = (entity.ApplicationId ?? applicationId).Replace("/", "_").Replace("\\", "_");
            var fileName = $"competent_equipment_certificate_{safeAppId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var webRootPath = _environment.WebRootPath ?? throw new InvalidOperationException("wwwroot is not configured.");
            var uploadPath = Path.Combine(webRootPath, "certificates");
            Directory.CreateDirectory(uploadPath);
            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/certificates/{fileName}";

            var boldFont = iText.Kernel.Font.PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regularFont = iText.Kernel.Font.PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            using var writer = new PdfWriter(filePath);
            using var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
            using var document = new Document(pdf);
            document.SetMargins(40, 40, 130, 40);

            document.Add(new Paragraph("Government of Rajasthan").SetFont(boldFont).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(1f));
            document.Add(new Paragraph("Factories and Boilers Inspection Department").SetFont(boldFont).SetFontSize(11).SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(1f));
            document.Add(new Paragraph("6-C, Jhalana Institutional Area, Jaipur, 302004").SetFont(regularFont).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(10f));

            var topRow = new PdfTable(new float[] { 1f, 1f }).UseAllAvailableWidth().SetBorder(Border.NO_BORDER).SetMarginBottom(2f);
            topRow.AddCell(new PdfCell().Add(new Paragraph($"Application No.:-  {entity.ApplicationId}").SetFont(boldFont).SetFontSize(10)).SetBorder(Border.NO_BORDER));
            topRow.AddCell(new PdfCell().Add(new Paragraph($"Dated:-  {DateTime.Now:dd/MM/yyyy}").SetFont(boldFont).SetFontSize(10).SetTextAlignment(TextAlignment.RIGHT)).SetBorder(Border.NO_BORDER));
            document.Add(topRow);

            document.Add(new Paragraph($"Equipment Registration No.:-  {entity.CompetentEquipmentRegistrationNo ?? "-"}").SetFont(boldFont).SetFontSize(10).SetMarginBottom(6f));
            document.Add(new Paragraph("Sub:-  Approval of Competent Equipment Registration").SetFont(boldFont).SetFontSize(11).SetMarginBottom(2f));
            document.Add(new Paragraph("Your Competent Equipment Registration is approved under the Indian Boilers Act, 1923.").SetFont(regularFont).SetFontSize(11).SetMarginBottom(6f));

            document.Add(new Paragraph($"\n\n({userName})").SetTextAlignment(TextAlignment.RIGHT));
            document.Add(new Paragraph(postName).SetTextAlignment(TextAlignment.RIGHT));

            var pageWidth = pdf.GetDefaultPageSize().GetWidth();
            document.Add(new Paragraph("This is a computer generated certificate. No physical signature is required.").SetFont(regularFont).SetFontSize(6.5f).SetFontColor(ColorConstants.GRAY).SetTextAlignment(TextAlignment.JUSTIFIED).SetFixedPosition(35, 8, pageWidth - 70));

            return fileUrl;
        }
    }
}