using Azure;
using iText.Commons.Actions;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Event;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.Dtos;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;
using System.Data;
using System.Security.Claims;
using System.Text.Json;
using static RajFabAPI.Constants.AppConstants;
using static System.Net.Mime.MediaTypeNames;
using ImageDataFactory = iText.IO.Image.ImageDataFactory;
using PdfCell = iText.Layout.Element.Cell;
using PdfDocument = iText.Kernel.Pdf.PdfDocument;
using PdfFont = iText.Kernel.Font.PdfFont;
using PdfImage = iText.Layout.Element.Image;
using PdfTable = iText.Layout.Element.Table;
using PdfWriter = iText.Kernel.Pdf.PdfWriter;

namespace RajFabAPI.Services
{
    public class AppealService : IAppealService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEstablishmentRegistrationService _establishmentRegistrationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _config;

        public AppealService(ApplicationDbContext context, IEstablishmentRegistrationService establishmentRegistrationService,
            IConfiguration config, IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _establishmentRegistrationService = establishmentRegistrationService;
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
            _config = config;
        }

        // CREATE
        public async Task<string> CreateAsync(AppealCreateDto dto)
        {
            var userIdString = _httpContextAccessor.HttpContext?
                .User?
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdString))
                throw new UnauthorizedAccessException("User is not authenticated");
            var userId = Guid.TryParse(userIdString, out var parsedGuid) ? parsedGuid : Guid.Empty;

            // 1. Check if any appeals exist for this factory
            var existingAppeal = await _context.Appeals
                .Where(a => a.FactoryRegistrationNumber == dto.FactoryRegistrationNumber)
                .OrderByDescending(a => a.Version)
                .FirstOrDefaultAsync();

            string appealRegistrationNumber;
            decimal newVersion = 1.0m;

            if (existingAppeal != null)
            {
                // If factory exists, use same AppealRegistrationNumber
                appealRegistrationNumber = existingAppeal.AppealRegistrationNumber;
                newVersion = existingAppeal.Version + 1; // increment version
            }
            else
            {
                // New factory, generate new AppealRegistrationNumber
                appealRegistrationNumber = GenerateRegistrationNumber();
            }

            // 2. Create new appeal record
            var appeal = new Appeal
            {
                FactoryRegistrationNumber = dto.FactoryRegistrationNumber,
                AppealRegistrationNumber = appealRegistrationNumber,
                AppealApplicationNumber = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10).ToUpper(),
                DateOfAccident = dto.DateOfAccident,
                DateOfInspection = dto.DateOfInspection,
                NoticeNumber = dto.NoticeNumber,
                NoticeDate = dto.NoticeDate,
                OrderNumber = dto.OrderNumber,
                OrderDate = dto.OrderDate,
                FactsAndGrounds = dto.FactsAndGrounds,
                ReliefSought = dto.ReliefSought,
                ChallanNumber = dto.ChallanNumber,
                EnclosureDetails1 = dto.EnclosureDetails1,
                EnclosureDetails2 = dto.EnclosureDetails2,
                SignatureOfOccupier = dto.SignatureOfOccupier,
                Signature = dto.Signature,
                Place = dto.Place,
                Status = "Pending",
                Date = dto.Date,
                Version = newVersion, // set calculated version
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.Appeals.Add(appeal);
            await _context.SaveChangesAsync();

            // Create initial history entry
            var history = new ApplicationHistory
            {
                ApplicationId = appeal.Id,
                ApplicationType = "Appeal",
                Action = "Submitted",
                PreviousStatus = null,
                NewStatus = "Pending",
                Comments = "Application submitted for review",
                ActionBy = userId.ToString(),
                ActionByName = "Applicant",
                ActionDate = DateTime.Now
            };
            _context.ApplicationHistories.Add(history);
            await _context.SaveChangesAsync();

            var moduleName =  ApplicationTypeNames.Appeal;

            var module = await _context.Set<FormModule>().FirstOrDefaultAsync(m => m.Name == moduleName);

            var appReg = new ApplicationRegistration
            {
                Id = Guid.NewGuid().ToString(),
                ModuleId = module.Id,
                UserId = userId,
                ApplicationId = appeal.Id,
                ApplicationRegistrationNumber = appeal.AppealRegistrationNumber,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };
            _context.Set<ApplicationRegistration>().Add(appReg);
            await _context.SaveChangesAsync();
            return appeal.Id;
        }

        // GET ALL
        public async Task<IEnumerable<AppealListDto>> GetAllAsync()
        {
            return await _context.Appeals
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedAt)
                .Select(a => new AppealListDto
                {
                    Id = a.Id,
                    FactoryRegistrationNumber = a.FactoryRegistrationNumber,
                    DateOfAccident = a.DateOfAccident,
                    NoticeNumber = a.NoticeNumber,
                    OrderNumber = a.OrderNumber,
                    IsActive = a.IsActive,
                    Status = a.Status,
                    ApplicationType = "Appeal",
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();
        }

        // GET BY ID
        public async Task<AppealResDto?> GetByIdAsync(string id)
        {
            var appeal = await _context.Appeals
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
            var estFullDetails = await _establishmentRegistrationService
                .GetFactoryDetailsByFactoryRegistrationNumberAsync(appeal.FactoryRegistrationNumber);
            if (appeal == null) return null;

            return new AppealResDto
            {
                AppealData = new AppealDetailDto
                {
                    Id = appeal.Id,
                    FactoryRegistrationNumber = appeal.FactoryRegistrationNumber,
                    DateOfAccident = appeal.DateOfAccident,
                    DateOfInspection = appeal.DateOfInspection,
                    NoticeNumber = appeal.NoticeNumber,
                    NoticeDate = appeal.NoticeDate,
                    OrderNumber = appeal.OrderNumber,
                    OrderDate = appeal.OrderDate,
                    FactsAndGrounds = appeal.FactsAndGrounds,
                    ReliefSought = appeal.ReliefSought,
                    ChallanNumber = appeal.ChallanNumber,
                    EnclosureDetails1 = appeal.EnclosureDetails1,
                    EnclosureDetails2 = appeal.EnclosureDetails2,
                    SignatureOfOccupier = appeal.SignatureOfOccupier,
                    Signature = appeal.Signature,
                    Place = appeal.Place,
                    Date = appeal.Date,
                    Status = appeal.Status,
                    AppealRegistrationNumber = appeal.AppealRegistrationNumber,
                    AppealApplicationNumber = appeal.AppealApplicationNumber,
                    IsESignCompletedManager = appeal.IsESignCompletedManager,
                    IsESignCompletedOccupier = appeal.IsESignCompletedOccupier,
                    ESignPrnNumberManager = appeal.ESignPrnNumberManager,
                    ESignPrnNumberOccupier = appeal.ESignPrnNumberOccupier,
                    ApplicationPDFUrl = appeal.ApplicationPDFUrl
                },
                EstFullDetails = estFullDetails
            };
            //return MapToDetailDto(appeal);
        }

        // UPDATE
        public async Task<bool> UpdateAsync(string id, AppealUpdateDto dto)
        {
            var existing = await _context.Appeals.FindAsync(id);
            if (existing == null) return false;

            existing.FactoryRegistrationNumber = dto.FactoryRegistrationNumber;
            existing.DateOfAccident = dto.DateOfAccident;
            existing.DateOfInspection = dto.DateOfInspection;
            existing.NoticeNumber = dto.NoticeNumber;
            existing.NoticeDate = dto.NoticeDate;
            existing.OrderNumber = dto.OrderNumber;
            existing.OrderDate = dto.OrderDate;
            existing.FactsAndGrounds = dto.FactsAndGrounds;
            existing.ReliefSought = dto.ReliefSought;
            existing.ChallanNumber = dto.ChallanNumber;
            existing.EnclosureDetails1 = dto.EnclosureDetails1;
            existing.EnclosureDetails2 = dto.EnclosureDetails2;
            existing.SignatureOfOccupier = dto.SignatureOfOccupier;
            existing.Signature = dto.Signature;
            existing.Place = dto.Place;
            existing.Date = dto.Date;
            // existing.Version = dto.Version;
            existing.IsActive = dto.IsActive;

            existing.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // Helper method to map to DetailDto
        private AppealDetailDto MapToDetailDto(Appeal a) => new AppealDetailDto
        {
            Id = a.Id,
            AppealApplicationNumber = a.AppealApplicationNumber,
            AppealRegistrationNumber = a.AppealRegistrationNumber,
            FactoryRegistrationNumber = a.FactoryRegistrationNumber,
            DateOfAccident = a.DateOfAccident,
            DateOfInspection = a.DateOfInspection,
            NoticeNumber = a.NoticeNumber,
            NoticeDate = a.NoticeDate,
            OrderNumber = a.OrderNumber,
            OrderDate = a.OrderDate,
            FactsAndGrounds = a.FactsAndGrounds,
            ReliefSought = a.ReliefSought,
            ChallanNumber = a.ChallanNumber,
            EnclosureDetails1 = a.EnclosureDetails1,
            EnclosureDetails2 = a.EnclosureDetails2,
            SignatureOfOccupier = a.SignatureOfOccupier,
            Signature = a.Signature,
            Place = a.Place,
            Date = a.Date,
            Version = a.Version,
            IsActive = a.IsActive,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt,
            Status = a.Status,
            ApplicationType = "Appeal",
        };

        public string GenerateRegistrationNumber()
        {
            var year = DateTime.Now.Year;
            var sequence = DateTime.Now.Ticks.ToString().Substring(8, 6);
            return $"FA{year}{sequence}";
        }


        public async Task<string> GenerateAppealPdf(AppealResDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var appData = dto.AppealData;
            var factoryData = dto.EstFullDetails?.EstablishmentDetail;
            var occupierData = dto.EstFullDetails?.MainOwnerDetail;

            var fileName = $"appeal_{appData.AppealApplicationNumber}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

            var uploadPath = Path.Combine(_environment.WebRootPath, "appeal-forms");
            Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);
            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");

            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/appeal-forms/{fileName}";

            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            // ================= HEADER =================
            var headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 4 })).UseAllAvailableWidth();
            headerTable.AddCell(new Cell().Add(new PdfImage(ImageDataFactory.Create("wwwroot/Emblem_of_India.png")).ScaleToFit(40, 40)).SetBorder(Border.NO_BORDER));
            headerTable.AddCell(new Cell()
                .Add(new Paragraph("Form - 4").SetFont(boldFont).SetFontSize(18))
                .Add(new Paragraph("(See sub-rule (9) of rule 5)").SetFont(regularFont).SetFontSize(12))
                .Add(new Paragraph("Appeal Form").SetFontColor(ColorConstants.BLUE).SetFontSize(12))
                .SetBorder(Border.NO_BORDER));
            document.Add(headerTable);
            document.Add(new Paragraph().SetMarginBottom(10));

            document.Add(new Paragraph($"Appeal Registration No: {appData?.AppealApplicationNumber ?? "-"}")
                .SetFont(regularFont));

            document.Add(new Paragraph($"Date: {appData?.CreatedAt:dd/MM/yyyy}")
                .SetMarginBottom(15));

            // ====== FACTORY DETAILS ======
            document.Add(SectionTitle("Factory & Occupier Details", boldFont));

            var factoryTable = CreateTable();
            AddRow(factoryTable, "Factory Name", factoryData?.Name, boldFont, regularFont);
            AddRow(factoryTable, "Address",
                $"{factoryData?.AddressLine1}, {factoryData?.AddressLine2}, {factoryData?.AreaName}, {factoryData?.DistrictName} - {factoryData?.Pincode}",
                boldFont, regularFont);
            AddRow(factoryTable, "Email", factoryData?.Email, boldFont, regularFont);
            AddRow(factoryTable, "Mobile", factoryData?.Mobile, boldFont, regularFont);
            AddRow(factoryTable, "Telephone", factoryData?.Telephone, boldFont, regularFont);

            AddRow(factoryTable, "Occupier Name", occupierData?.Name, boldFont, regularFont);
            AddRow(factoryTable, "Designation", occupierData?.Designation, boldFont, regularFont);

            document.Add(factoryTable);

            // ====== APPEAL DETAILS ======
            document.Add(SectionTitle("Appeal Details", boldFont));

            var appTable = CreateTable();

            AddRow(appTable, "Appeal Registration Number", appData?.AppealRegistrationNumber, boldFont, regularFont);
            AddRow(appTable, "Appeal Application Number", appData?.AppealApplicationNumber, boldFont, regularFont);

            AddRow(appTable, "Factory Registration Number", appData?.FactoryRegistrationNumber, boldFont, regularFont);

            AddRow(appTable, "Date of Accident", appData?.DateOfAccident?.ToString("dd/MM/yyyy"), boldFont, regularFont);
            AddRow(appTable, "Date of Inspection", appData?.DateOfInspection?.ToString("dd/MM/yyyy"), boldFont, regularFont);

            AddRow(appTable, "Notice Number", appData?.NoticeNumber, boldFont, regularFont);
            AddRow(appTable, "Notice Date", appData?.NoticeDate?.ToString("dd/MM/yyyy"), boldFont, regularFont);

            AddRow(appTable, "Order Number", appData?.OrderNumber, boldFont, regularFont);
            AddRow(appTable, "Order Date", appData?.OrderDate?.ToString("dd/MM/yyyy"), boldFont, regularFont);

            AddRow(appTable, "Challan Number", appData?.ChallanNumber, boldFont, regularFont);

            AddRow(appTable, "Place", appData?.Place, boldFont, regularFont);
            AddRow(appTable, "Application Date", appData?.Date?.ToString("dd/MM/yyyy"), boldFont, regularFont);

            AddRow(appTable, "Status", appData?.Status, boldFont, regularFont);

            AddRow(appTable, "Manager E-Sign Completed", appData?.IsESignCompletedManager == true ? "Yes" : "No", boldFont, regularFont);
            AddRow(appTable, "Occupier E-Sign Completed", appData?.IsESignCompletedOccupier == true ? "Yes" : "No", boldFont, regularFont);

            AddRow(appTable, "Manager PRN Number", appData?.ESignPrnNumberManager, boldFont, regularFont);
            AddRow(appTable, "Occupier PRN Number", appData?.ESignPrnNumberOccupier, boldFont, regularFont);

            document.Add(appTable);

            var signTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1 }))
                .UseAllAvailableWidth()
                .SetMarginTop(50)
                .SetHeight(120); // total height = height of signature cells

            // Applicant Signature Cell
            var applicantCell = new Cell()
                .SetBorder(new SolidBorder(1))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.BOTTOM)
                .Add(new Paragraph("Signature of Applicant").SetFont(regularFont).SetFontSize(10));

            // Officer Signature Cell
            var officerCell = new Cell()
                .SetBorder(new SolidBorder(1))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.BOTTOM)
                .Add(new Paragraph("Signature of Officer").SetFont(regularFont).SetFontSize(10));

            signTable.AddCell(applicantCell);
            signTable.AddCell(officerCell);

            document.Add(signTable);

            document.Close();

            var commReg = await _context.Appeals.FirstOrDefaultAsync(x => x.Id == dto.AppealData.Id);
            if (commReg != null)
            {
                commReg.ApplicationPDFUrl = fileUrl;
                await _context.SaveChangesAsync();
            }

            return filePath;
        }

        // Helper to create a two-column table
        private Table CreateTable()
        {
            return new Table(UnitValue.CreatePercentArray(new float[] { 1, 2 }))
                .UseAllAvailableWidth()
                .SetMarginBottom(10);
        }

        // Helper to add a row with label and value
        private void AddRow(Table table, string label, string value, PdfFont boldFont, PdfFont regularFont)
        {
            table.AddCell(new Cell().Add(new Paragraph(label).SetFont(boldFont)).SetPadding(5));
            table.AddCell(new Cell().Add(new Paragraph(value ?? "-").SetFont(regularFont)).SetPadding(5));
        }

        // Helper for section titles
        private Paragraph SectionTitle(string text, PdfFont font)
        {
            return new Paragraph(text).SetFont(font).SetFontSize(12).SetMarginTop(15).SetMarginBottom(5);
        }
    }
}
