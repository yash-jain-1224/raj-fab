using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Event;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;
using System.Data;
using System.Security.Claims;
using static RajFabAPI.Constants.AppConstants;

using ImageDataFactory = iText.IO.Image.ImageDataFactory;
using PdfCell = iText.Layout.Element.Cell;
using PdfDoc = iText.Layout.Document;
using PdfImage = iText.Layout.Element.Image;
using PdfTable = iText.Layout.Element.Table;
using PdfWriter = iText.Kernel.Pdf.PdfWriter;
using Text = iText.Layout.Element.Text;

namespace RajFabAPI.Services
{
    public class CommencementCessationService : ICommencementCessationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _config;
        private readonly IEstablishmentRegistrationService _establishmentRegistrationService;

        public CommencementCessationService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment, IConfiguration config, IEstablishmentRegistrationService establishmentRegistrationService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
            _config = config;
            _establishmentRegistrationService = establishmentRegistrationService;
        }

        public async Task<List<CommencementCessationDto>> GetAllAsync()
        {
            var entities = await _context.CommencementCessationApplication.ToListAsync();
            return entities.Select(MapToDto).ToList();
        }
        public async Task<CommencementCessationResDto?> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            var entity = await _context.CommencementCessationApplication
                .FirstOrDefaultAsync(x => x.Id == Guid.Parse(id));

            if (entity == null)
                return null;

            var estFullDetails = await _establishmentRegistrationService
                .GetFactoryDetailsByFactoryRegistrationNumberAsync(entity.FactoryRegistrationNumber);

            var dto = new CommencementCessationDto
            {
                Id = entity.Id,
                ApplicationNumber = entity.ApplicationNumber,
                FactoryRegistrationNumber = entity.FactoryRegistrationNumber,
                Type = entity.Type,
                ApproxDurationOfWork = entity.ApproxDurationOfWork,
                OnDate = entity.OnDate,
                FromDate = entity.FromDate,
                Reason = entity.Reason,
                DateOfCessation = entity.DateOfCessation,
                Status = entity.Status,
                CreatedAt = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate,
                Version = entity.Version,
                IsActive = entity.IsActive,
                ApplicationPDFUrl = entity.ApplicationPDFUrl,
                ObjectionLetterUrl = entity.ObjectionLetterUrl,
            };

            var activeCertificate = await _context.Set<Certificate>()
                        .AsNoTracking()
                        .Where(c => c.ApplicationId == dto.Id.ToString())
                        .OrderByDescending(c => c.CertificateVersion)
                        .FirstOrDefaultAsync();

            var applicationHistory = await _context.Set<ApplicationHistory>()
                    .AsNoTracking()
                    .Where(x => x.ApplicationId == entity.Id.ToString())
                    .OrderByDescending(x => x.ActionDate)
                    .ToListAsync();

            dto.CertificatePDFUrl = activeCertificate?.CertificateUrl;

            return new CommencementCessationResDto
            {
                CommencementCessationData = dto,
                EstFullDetails = estFullDetails,
                ApplicationHistory = applicationHistory
            };
        }
        public async Task<bool> UpdateStatusAndRemark(string Id, string status)
        {
            try
            {
                var reg = _context.CommencementCessationApplication.FirstOrDefault(x => x.Id == Guid.Parse(Id));
                if (reg == null)
                    return false;
                reg.Status = status;
                reg.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<string?> CreateAsync(CommencementCessationRequestDto request)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var userIdString = _httpContextAccessor.HttpContext?
                .User?
                .FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrWhiteSpace(userIdString))
                    throw new UnauthorizedAccessException("User is not authenticated");
                var userId = Guid.TryParse(userIdString, out var parsedGuid) ? parsedGuid : Guid.Empty;

                var entity = new CommencementCessationApplication
                {
                    Type = request.Type,
                    FactoryRegistrationNumber = request.FactoryRegistrationNumber,
                    ApplicationNumber = await GenerateApplicationNumberAsync(),
                    Reason = request.Reason,
                    OnDate = request.OnDate,
                    FromDate = request.FromDate,
                    ApproxDurationOfWork = request?.ApproxDurationOfWork,
                    DateOfCessation = request?.DateOfCessation,
                    Status = "Pending",
                    Version = 1.0m,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                _context.CommencementCessationApplication.Add(entity);
                var result = await _context.SaveChangesAsync();

                if (result <= 0)
                {
                    throw new Exception("Failed to create Commencement/Cessation application.");
                }

                var module = await _context.Set<FormModule>().FirstOrDefaultAsync(m => m.Name == "Factory Commencement And Cessation");
                if (module == null)
                    throw new Exception("Module not found for ApplicationTypeId");

                var appReg = new ApplicationRegistration
                {
                    Id = Guid.NewGuid().ToString(),
                    ModuleId = module.Id,
                    UserId = userId,
                    ApplicationId = entity.Id.ToString(),
                    ApplicationRegistrationNumber = request.FactoryRegistrationNumber,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };
                _context.Set<ApplicationRegistration>().Add(appReg);

                var history = new ApplicationHistory
                {
                    ApplicationId = appReg.ApplicationId,
                    ApplicationType = module.Name,
                    Action = "Application Submitted",
                    PreviousStatus = null,
                    NewStatus = "Pending",
                    Comments = "Application Submitted and sent for E-Sign",
                    ActionBy = "Applicant",
                    ActionDate = DateTime.Now
                };
                _context.ApplicationHistories.Add(history);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return entity.Id.ToString();
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<CommencementCessationDto> UpdateAsync(Guid id, CommencementCessationRequestDto request)
        {
            var entity = await _context.CommencementCessationApplication.FindAsync(id);
            if (entity == null)
            {
                throw new Exception("Application not found.");
            }

            // Update fields
            entity.Type = request.Type;
            entity.FactoryRegistrationNumber = request.FactoryRegistrationNumber;
            entity.FromDate = request.FromDate;
            entity.OnDate = request.OnDate;
            entity.Reason = request.Reason;
            entity.ApproxDurationOfWork = request.ApproxDurationOfWork;
            entity.DateOfCessation = request.DateOfCessation;
            entity.UpdatedDate = DateTime.Now;

            // Optional: Update version if needed
            entity.Version += 0.1m;

            _context.CommencementCessationApplication.Update(entity);
            var result = await _context.SaveChangesAsync();

            if (result <= 0)
            {
                throw new Exception("Failed to update application.");
            }

            return MapToDto(entity);
        }

        private static CommencementCessationDto MapToDto(CommencementCessationApplication entity)
        {
            return new CommencementCessationDto
            {
                Id = entity.Id,
                Type = entity.Type,
                FactoryRegistrationNumber = entity.FactoryRegistrationNumber,
                OnDate = entity.OnDate,
                FromDate = entity.FromDate,
                ApproxDurationOfWork = entity.ApproxDurationOfWork,
                Reason = entity.Reason,
                DateOfCessation = entity.DateOfCessation,
                Status = entity.Status,
                Version = entity.Version,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate
            };
        }
        private async Task<string> GenerateApplicationNumberAsync()
        {
            var year = DateTime.Now.Year;
            string prefix = $"FC-";

            // Get latest application number
            var lastApp = await _context.CommencementCessationApplication
                .Where(x => x.ApplicationNumber.StartsWith(prefix)
                        && x.ApplicationNumber.Contains($"/CIFB/{year}"))
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => x.ApplicationNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastApp))
            {
                // Format: FMC-000000/CIFB/2026
                int dashIndex = lastApp.IndexOf('-');
                int slashIndex = lastApp.IndexOf("/CIFB");

                if (dashIndex != -1 && slashIndex != -1)
                {
                    var numberPart = lastApp.Substring(dashIndex + 1, slashIndex - (dashIndex + 1));

                    if (int.TryParse(numberPart, out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }
            }

            return $"{prefix}{nextNumber:D6}/CIFB/{year}";
        }

        public async Task<string> GenerateCommencementCessationPdf(CommencementCessationResDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var appData = dto.CommencementCessationData;
            var factoryName = dto.EstFullDetails?.EstablishmentDetail.Name;
            var factoryData = dto.EstFullDetails?.Factory;

            var occupierData = dto.EstFullDetails?.MainOwnerDetail;

            var folderName = "commencement-cessation-forms";
            var folderPath = Path.Combine(_environment.WebRootPath, folderName);
            Directory.CreateDirectory(folderPath);

            var fileName = $"commencement-cessation_{appData.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(folderPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");

            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/{folderName}/{fileName}";

            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);

            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // Footer + Border (same as manager change)
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE,
                new ManagerChangePageBorderAndFooterEventHandler(
                    boldFont, regularFont, DateTime.Now.ToString("dd/MM/yyyy")));

            using var document = new Document(pdf);
            document.SetMargins(40, 40, 40, 40);

            // ─────────────────────────────────────────────
            // HEADER (Centered)
            // ─────────────────────────────────────────────
            document.Add(new Paragraph("Form - 4")
                .SetFont(boldFont)
                .SetFontSize(13)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("(See sub-rule (9) of rule 5)")
                .SetFont(regularFont)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("Notice of " + Capitalize(appData.Type) + " of Establishment")
                .SetFont(boldFont)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10));

            // Application No + Date
            var headerTable = new Table(new float[] { 360f, 160f })
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER);

            headerTable.AddCell(new Cell()
                .Add(new Paragraph($"{Capitalize(appData.Type)} No: {appData?.ApplicationNumber}")
                    .SetFont(boldFont).SetFontSize(10))
                .SetBorder(Border.NO_BORDER));

            headerTable.AddCell(new Cell()
                .Add(new Paragraph($"Date: {appData?.CreatedAt:dd-MM-yyyy}")
                    .SetFont(boldFont).SetFontSize(10)
                    .SetTextAlignment(TextAlignment.RIGHT))
                .SetBorder(Border.NO_BORDER));

            document.Add(headerTable);
            document.Add(new Paragraph("\n").SetFontSize(5));

            Cell NoBorderCell(string text, PdfFont font)
            {
                return new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetPaddingBottom(4)
                    .Add(new Paragraph(text)
                        .SetFont(font)
                        .SetFontSize(9)
                        .SetMargin(0));
            }
            void AddTwoColumnSection(string title, List<(string Label, string? Value)> rows)
            {
                document.Add(new Paragraph(title)
                    .SetFont(boldFont)
                    .SetFontSize(11)
                    .SetMarginBottom(6));
                var table = new PdfTable(new float[] { 260f, 260f })
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER);

                foreach (var (label, value) in rows)
                {
                    table.AddCell(NoBorderCell(label, boldFont));
                    table.AddCell(NoBorderCell(value ?? "—", regularFont));
                }
                document.Add(table.SetMarginBottom(10));
            }

            // ─────────────────────────────────────────────
            // SECTION 1 – Factory
            // ─────────────────────────────────────────────
            var factoryRows = new List<(string, string?)>
            {
                ("Factory Registration Number", appData?.FactoryRegistrationNumber),
                ("Factory Name", factoryName),
                ("Address", $"{factoryData?.AddressLine1}, {factoryData?.AddressLine2}"),
                ("District", factoryData?.DistrictName),
                ("Sub-division", factoryData?.SubDivisionName),
                ("Tehsil", factoryData?.TehsilName),
                ("Area", factoryData?.Area),
                ("Pincode", factoryData?.Pincode),
                ("Email", factoryData?.Email),
                ("Mobile", factoryData?.Mobile),
                ("Telephone", factoryData?.Telephone),
            };

            AddTwoColumnSection("1. Factory Details", factoryRows);

            // ─────────────────────────────────────────────
            // SECTION 1 – Occupier
            // ─────────────────────────────────────────────
            var occupierRows = new List<(string, string?)>
            {
                ("Occupier Name", occupierData?.Name),
                ("Designation", occupierData?.Designation),

                ("Address", $"{occupierData?.AddressLine1}, {occupierData?.AddressLine2}"),
                ("District", occupierData?.District),
                ("Sub-division", occupierData?.Tehsil),
                ("Tehsil", occupierData?.Area),
                ("Pincode", occupierData?.Pincode),
                ("Email", occupierData?.Email),
                ("Mobile", occupierData?.Mobile),
                ("Telephone", occupierData?.Telephone),
            };

            AddTwoColumnSection("2. Occupier Details", occupierRows);

            // ─────────────────────────────────────────────
            // SECTION 2 – Application Details
            // ─────────────────────────────────────────────
            var appRows = new List<(string, string?)>
            {
                ("Application Type", Capitalize(appData?.Type)),
                ("Approx. Duration", appData?.ApproxDurationOfWork),
                ("Reason", appData?.Reason),
                ("Cessation Intimation Date", appData?.FromDate?.ToString("dd/MM/yyyy")),
                ("Cessation Effective Date", appData?.OnDate?.ToString("dd/MM/yyyy")),
                ("Status", appData?.Status),
            };
            document.Add(new AreaBreak());
            AddTwoColumnSection("3. Application Details", appRows);

            document.Close();

            // Save PDF URL
            var commReg = await _context.CommencementCessationApplication
                .FirstOrDefaultAsync(x => x.Id == appData.Id);

            if (commReg != null)
            {
                commReg.ApplicationPDFUrl = fileUrl;
                await _context.SaveChangesAsync();
            }

            return filePath;
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // Generate Objection Letter — Commencement Cessation
        // ─────────────────────────────────────────────────────────────────────────────
        public async Task<string> GenerateObjectionLetter(CommencementCessationObjectionLetterDto dto, string applicationId)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var fileName = $"objection_commencement_cessation_{applicationId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var webRootPath = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
                throw new InvalidOperationException("wwwroot is not configured.");

            var uploadPath = Path.Combine(webRootPath, "objection-letters");
            _ = Directory.CreateDirectory(uploadPath);
            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/objection-letters/{fileName}";

            // ── Fonts ─────────────────────────────────────────────────────────────────
            var boldFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

            // ── PDF setup ─────────────────────────────────────────────────────────────
            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new PageBorderEventHandler());
            using var document = new PdfDoc(pdf);
            document.SetMargins(50, 50, 65, 50);

            var rawLoad = dto.CommencementCessationData.EstFullDetails.Factory.SanctionedLoad ?? 0;
            var loadUnit = (dto.CommencementCessationData.EstFullDetails.Factory.SanctionedLoadUnit ?? "HP").ToUpper();

            // Normalize any unit to KW first, then convert to target
            decimal ToKW(decimal val, string unit) => unit switch
            {
                "HP" => val * 0.746m,
                "KW" => val,
                "KVA" => val,          // assuming power factor = 1
                "MW" => val * 1000m,
                "MVA" => val * 1000m,
                _ => val
            };
            decimal ConvertToHP(decimal val, string unit) => ToKW(val, unit) / 0.746m;

            var Type = "-";
            var power = ConvertToHP(rawLoad, loadUnit); // in H.P. This is a placeholder. The actual power value should come from the application data (e.g., dto.PowerInHP).

            if (dto.CommencementCessationData.EstFullDetails.Factory.NumberOfWorker < 20)
            {
                Type = "Section 85";
            }
            else if (dto.CommencementCessationData.EstFullDetails.Factory.NumberOfWorker > 40 && power == 0)
            {
                Type = "2 (1)(w)(ii)";
            }
            else if (dto.CommencementCessationData.EstFullDetails.Factory.NumberOfWorker >= 20 && power > 0)
            {
                Type = "2 (1)(w)(i)";
            }

            // ═════════════════════════════════════════════════════════════════════════
            // HEADER
            // ═════════════════════════════════════════════════════════════════════════
            _ = document.Add(new Paragraph("Government of Rajasthan")
                .SetFont(boldFont).SetFontSize(14)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(1f));

            _ = document.Add(new Paragraph("Factories and Boilers Inspection Department")
                .SetFont(boldFont).SetFontSize(13)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(1f));

            _ = document.Add(new Paragraph("6-C, Jhalana Institutional Area, Jaipur, 302004")
                .SetFont(regularFont).SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10f));

            // ═════════════════════════════════════════════════════════════════════════
            // Application Id  +  Dated
            // ═════════════════════════════════════════════════════════════════════════
            var topRow = new PdfTable(new float[] { 1f, 1f })
                .UseAllAvailableWidth().SetBorder(Border.NO_BORDER).SetMarginBottom(12f);

            _ = topRow.AddCell(new PdfCell()
                .Add(new Paragraph($"Commencement/Cessation Application No.:- {dto.CommencementCessationData.CommencementCessationData.ApplicationNumber ?? "-"}")
                    .SetFont(boldFont).SetFontSize(10))
                .SetBorder(Border.NO_BORDER));

            _ = topRow.AddCell(new PdfCell()
                .Add(new Paragraph($"Dated:-  {DateTime.Now.ToString("dd/MM/yyyy")}")
                    .SetFont(boldFont).SetFontSize(10)
                    .SetTextAlignment(TextAlignment.RIGHT))
                .SetBorder(Border.NO_BORDER));

            _ = document.Add(topRow);

            var factoryAddress = $"{dto.CommencementCessationData.EstFullDetails.Factory?.AddressLine1}, {dto.CommencementCessationData.EstFullDetails.Factory?.AddressLine2},\n{dto.CommencementCessationData.EstFullDetails.Factory?.Area}, {dto.CommencementCessationData.EstFullDetails.Factory?.TehsilName},\n{dto.CommencementCessationData.EstFullDetails.Factory?.SubDivisionName}, {dto.CommencementCessationData.EstFullDetails.Factory?.DistrictName}, {dto.CommencementCessationData.EstFullDetails.Factory?.Pincode}";

            // ═════════════════════════════════════════════════════════════════════════
            // Factory name + address
            // ═════════════════════════════════════════════════════════════════════════
            if (!string.IsNullOrWhiteSpace(dto.CommencementCessationData.EstFullDetails.EstablishmentDetail.Name))
            {
                _ = document.Add(new Paragraph(dto.CommencementCessationData.EstFullDetails.EstablishmentDetail.Name)
                    .SetFont(boldFont).SetFontSize(12)
                    .SetMarginBottom(1f));
            }
            if (!string.IsNullOrWhiteSpace(factoryAddress))
            {
                _ = document.Add(new Paragraph(factoryAddress)
                    .SetFont(regularFont).SetFontSize(12)
                    .SetMarginBottom(8f));
            }

            // ═════════════════════════════════════════════════════════════════════════
            // Sub:-
            // ═════════════════════════════════════════════════════════════════════════
            var subPara = new Paragraph();
            subPara.Add(new Text("Sub:- ").SetFont(boldFont).SetFontSize(12));
            subPara.Add(new Text("Regarding approval of Commencement Cessation Application").SetFont(regularFont).SetFontSize(12));
            _ = document.Add(subPara.SetMarginBottom(4f));

            // ═════════════════════════════════════════════════════════════════════════
            // Intro line
            // ═════════════════════════════════════════════════════════════════════════
            _ = document.Add(new Paragraph(
                    "The details of your factory as per application, drawings and documents are shown below:-")
                .SetFont(regularFont).SetFontSize(12)
                .SetMarginBottom(4f));

            // ═════════════════════════════════════════════════════════════════════════
            // Factory details table (red border)
            // ═════════════════════════════════════════════════════════════════════════
            var detailsTable = new PdfTable(new float[] { 150f, 1f })
                .UseAllAvailableWidth().SetMarginBottom(12f);

            PdfCell RedCell(string text, PdfFont font, float fontSize = 12f)
            {
                var border = new iText.Layout.Borders.SolidBorder(new DeviceRgb(220, 0, 0), 1.5f);
                return new PdfCell()
                    .Add(new Paragraph(text ?? "-").SetFont(font).SetFontSize(fontSize))
                    .SetBorderTop(border).SetBorderBottom(border)
                    .SetBorderLeft(border).SetBorderRight(border)
                    .SetPadding(5f);
            }

            _ = detailsTable.AddCell(RedCell("Manufacturing Process", boldFont));
            _ = detailsTable.AddCell(RedCell(dto.CommencementCessationData.EstFullDetails.Factory.ManufacturingDetail ?? "-", regularFont));

            _ = detailsTable.AddCell(RedCell("Type", boldFont));
            _ = detailsTable.AddCell(RedCell(Type, regularFont));

            _ = detailsTable.AddCell(RedCell("Category", boldFont));
            _ = detailsTable.AddCell(RedCell(dto.CommencementCessationData.EstFullDetails.EstablishmentDetail.FactoryTypeName ?? "-", regularFont));

            _ = detailsTable.AddCell(RedCell("Workers", boldFont));
            _ = detailsTable.AddCell(RedCell(dto.CommencementCessationData.EstFullDetails.Factory.NumberOfWorker.ToString() ?? "-", regularFont));

            _ = document.Add(detailsTable);

            // ═════════════════════════════════════════════════════════════════════════
            // Objections heading
            // ═════════════════════════════════════════════════════════════════════════
            _ = document.Add(new Paragraph("Following objections are need to be removed related to your factory - ")
                .SetFont(regularFont).SetFontSize(12)
                .SetMarginBottom(12f));

            // ═════════════════════════════════════════════════════════════════════════
            // Numbered objections list
            // ═══════════════════════════════════════════════════════════════════════
            if (dto.Objections != null && dto.Objections.Any())
            {
                for (int i = 0; i < dto.Objections.Count; i++)
                {
                    _ = document.Add(new Paragraph($"{i + 1}.{dto.Objections[i]}")
                        .SetFont(regularFont).SetFontSize(12)
                        .SetMarginBottom(6f));
                }
            }

            _ = document.Add(new Paragraph("").SetMarginBottom(10f));

            // ═════════════════════════════════════════════════════════════════════════
            // Closing line
            // ═════════════════════════════════════════════════════════════════════════
            _ = document.Add(new Paragraph(
                    "Please comply with the above observations and submit relevant details/documents")
                .SetFont(regularFont).SetFontSize(12)
                .SetMarginBottom(30f));

            // ═════════════════════════════════════════════════════════════════════════
            // Signature block (right-aligned)
            // ═════════════════════════════════════════════════════════════════════════
            var imageData = ImageDataFactory.Create("wwwroot/chief_signature.jpg");

            var sigOuterTable = new PdfTable(new float[] { 1f, 1f })
                .UseAllAvailableWidth().SetBorder(Border.NO_BORDER).SetMarginBottom(8f);

            sigOuterTable.AddCell(new PdfCell().SetBorder(Border.NO_BORDER));

            var sigCell = new PdfCell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT);

            var innerDiv = new Div()
                .SetWidth(250)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetHorizontalAlignment(HorizontalAlignment.RIGHT);

            innerDiv.Add(new PdfImage(imageData).ScaleToFit(150, 50).SetHorizontalAlignment(HorizontalAlignment.CENTER));
            innerDiv.Add(new Paragraph($"( {dto.SignatoryName} )").SetFont(regularFont).SetFontSize(12).SetMarginTop(2f));
            innerDiv.Add(new Paragraph(dto.SignatoryDesignation).SetFont(regularFont).SetFontSize(12).SetMarginTop(2f));
            innerDiv.Add(new Paragraph(dto.SignatoryLocation).SetFont(regularFont).SetFontSize(12).SetMarginTop(0f));

            sigCell.Add(innerDiv);
            sigOuterTable.AddCell(sigCell);
            document.Add(sigOuterTable);

            // ═════════════════════════════════════════════════════════════════════════
            // Footer disclaimer
            // ═════════════════════════════════════════════════════════════════════════
            var pageWidth = pdf.GetDefaultPageSize().GetWidth();
            _ = document.Add(new Paragraph(
                    "This is a computer generated certificate and bears scanned signature. No physical signature is required on this document. You " +
                    "can verify this document by visiting rajnivesh.rajasthan.gov.in or rajfab.rajasthan.gov.in and entering Application No./ID after clicking the link for " +
                    "verification on the page.")
                .SetFont(regularFont).SetFontSize(7)
                .SetFontColor(ColorConstants.GRAY)
                .SetTextAlignment(TextAlignment.JUSTIFIED)
                .SetFixedPosition(35, 33, pageWidth - 70));

            document.Close();

            var commencementCessationApplication = await _context.CommencementCessationApplication
                .FirstOrDefaultAsync(x => x.Id == Guid.Parse(applicationId));
            if (commencementCessationApplication != null)
            {
                commencementCessationApplication.ObjectionLetterUrl = fileUrl;
                await _context.SaveChangesAsync();
            }

            return fileUrl;
        }

        public async Task<string> GenerateCertificateAsync(CertificateRequestDto dto, Guid userId, string applicationId)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));
                var appId = applicationId.ToLower();
                var appReg = await _context.ApplicationRegistrations
                    .FirstOrDefaultAsync(r => r.ApplicationId.ToLower() == appId);

                if (appReg == null)
                    throw new KeyNotFoundException("Application registration not found");

                var CommencementCessationData = await GetByIdAsync(applicationId);
                if (CommencementCessationData == null) throw new Exception("Commencement Cessation application not found");

                // Get user details (Approval Authority - the signer)
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                    throw new KeyNotFoundException("User not found");

                var officePost = await (
                    from ur in _context.UserRoles
                    join r in _context.Roles on ur.RoleId equals r.Id
                    join p in _context.Posts on r.PostId equals p.Id
                    join o in _context.Offices on r.OfficeId equals o.Id
                    join c in _context.Cities on o.CityId equals c.Id
                    where ur.UserId == userId
                    select new
                    {
                        OfficeName = o.Name,
                        PostName = p.Name,
                        CityName = c.Name,
                        PostId = p.Id
                    }
                ).FirstOrDefaultAsync();

                if (officePost == null)
                {
                    throw new Exception("No office post found for this user");
                }

                var certificateUrl = await GenerateCertificatePdf(dto, applicationId, officePost.PostName + ", " + officePost.CityName, user.FullName, CommencementCessationData);
                var module = await _context.Modules
                .Where(m => m.Name == ApplicationTypeNames.NewEstablishment)
                .FirstOrDefaultAsync();

                if (module == null || module.Id == Guid.Empty)
                {
                    throw new Exception("Module not found");
                }

                var certificate = new Certificate
                {
                    Id = Guid.NewGuid(),
                    RegistrationNumber = CommencementCessationData.CommencementCessationData.ApplicationNumber,
                    ApplicationId = appReg.ApplicationId,
                    CertificateVersion = 1.0m,
                    CertificateUrl = certificateUrl,
                    IssuedAt = DateTime.Now,
                    IssuedByUserId = userId,
                    Status = "PendingESign",
                    ModuleId = module.Id,
                    StartDate = DateTime.Today,
                    EndDate = null, // Certificates can be evergreen or have fixed validity based on requirements
                    Remarks = dto.Remarks
                };

                _ = _context.Certificates.Add(certificate);

                // Create application history with dynamic ActionBy
                var history = new ApplicationHistory
                {
                    ApplicationId = appReg.ApplicationId,
                    ApplicationType = module.Name,
                    Action = "Certificate Generated",
                    PreviousStatus = null,
                    NewStatus = "",
                    Comments = $"Certificate Generated by {officePost.PostName}, {officePost.CityName}",
                    ActionBy = officePost.PostId.ToString(),
                    ActionByName = $"{officePost.PostName}, {officePost.CityName}",
                    ActionDate = DateTime.Now
                };

                _context.ApplicationHistories.Add(history);

                _ = await _context.SaveChangesAsync();

                return certificate.Id.ToString();
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> GenerateCertificatePdf(CertificateRequestDto dto, string applicationId, string postName, string userName, CommencementCessationResDto commencementCessationData)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var fileName = $"certificate_manager_change_{applicationId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var webRootPath = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
                throw new InvalidOperationException("wwwroot is not configured.");

            var uploadPath = Path.Combine(webRootPath, "certificates");
            _ = Directory.CreateDirectory(uploadPath);
            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/certificates/{fileName}";

            // ── Fonts ─────────────────────────────────────────────────────────────────
            var boldFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

            // ── PDF setup ─────────────────────────────────────────────────────────────
            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new PageBorderEventHandler());
            using var document = new PdfDoc(pdf);
            document.SetMargins(50, 50, 65, 50);

            var rawLoad = commencementCessationData.EstFullDetails.Factory.SanctionedLoad ?? 0;
            var loadUnit = (commencementCessationData.EstFullDetails.Factory.SanctionedLoadUnit ?? "HP").ToUpper();

            // Normalize any unit to KW first, then convert to target
            decimal ToKW(decimal val, string unit) => unit switch
            {
                "HP" => val * 0.746m,
                "KW" => val,
                "KVA" => val,          // assuming power factor = 1
                "MW" => val * 1000m,
                "MVA" => val * 1000m,
                _ => val
            };
            decimal ConvertToHP(decimal val, string unit) => ToKW(val, unit) / 0.746m;

            var Type = "-";
            var power = ConvertToHP(rawLoad, loadUnit); // in H.P. This is a placeholder. The actual power value should come from the application data (e.g., dto.PowerInHP).

            if (commencementCessationData.EstFullDetails.Factory.NumberOfWorker < 20)
            {
                Type = "Section 85";
            }
            else if (commencementCessationData.EstFullDetails.Factory.NumberOfWorker > 40 && power == 0)
            {
                Type = "2 (1)(w)(ii)";
            }
            else if (commencementCessationData.EstFullDetails.Factory.NumberOfWorker >= 20 && power > 0)
            {
                Type = "2 (1)(w)(i)";
            }

            // ═════════════════════════════════════════════════════════════════════════
            // HEADER
            // ═════════════════════════════════════════════════════════════════════════
            _ = document.Add(new Paragraph("Government of Rajasthan")
                .SetFont(boldFont).SetFontSize(14)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(1f));

            _ = document.Add(new Paragraph("Factories and Boilers Inspection Department")
                .SetFont(boldFont).SetFontSize(13)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(1f));

            _ = document.Add(new Paragraph("6-C, Jhalana Institutional Area, Jaipur, 302004")
                .SetFont(regularFont).SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10f));

            // ═════════════════════════════════════════════════════════════════════════
            // Application Id  +  Dated
            // ═════════════════════════════════════════════════════════════════════════
            var topRow = new PdfTable(new float[] { 1f, 1f })
                .UseAllAvailableWidth().SetBorder(Border.NO_BORDER).SetMarginBottom(12f);

            _ = topRow.AddCell(new PdfCell()
                .Add(new Paragraph($"Manager Change Application No.:- {commencementCessationData.CommencementCessationData.ApplicationNumber ?? "-"}")
                    .SetFont(boldFont).SetFontSize(10))
                .SetBorder(Border.NO_BORDER));

            _ = topRow.AddCell(new PdfCell()
                .Add(new Paragraph($"Dated:-  {DateTime.Now.ToString("dd/MM/yyyy")}")
                    .SetFont(boldFont).SetFontSize(10)
                    .SetTextAlignment(TextAlignment.RIGHT))
                .SetBorder(Border.NO_BORDER));

            _ = document.Add(topRow);

            var factoryAddress = $"{commencementCessationData.EstFullDetails.Factory?.AddressLine1}, {commencementCessationData.EstFullDetails.Factory?.AddressLine2},\n{commencementCessationData.EstFullDetails.Factory?.Area}, {commencementCessationData.EstFullDetails.Factory?.TehsilName},\n{commencementCessationData.EstFullDetails.Factory?.SubDivisionName}, {commencementCessationData.EstFullDetails.Factory?.DistrictName}, {commencementCessationData.EstFullDetails.Factory?.Pincode}";

            // ═════════════════════════════════════════════════════════════════════════
            // Factory name + address
            // ═════════════════════════════════════════════════════════════════════════
            if (!string.IsNullOrWhiteSpace(commencementCessationData.EstFullDetails.EstablishmentDetail.Name))
            {
                _ = document.Add(new Paragraph(commencementCessationData.EstFullDetails.EstablishmentDetail.Name)
                    .SetFont(boldFont).SetFontSize(12)
                    .SetMarginBottom(1f));
            }
            if (!string.IsNullOrWhiteSpace(factoryAddress))
            {
                _ = document.Add(new Paragraph(factoryAddress)
                    .SetFont(regularFont).SetFontSize(12)
                    .SetMarginBottom(8f));
            }

            // ═════════════════════════════════════════════════════════════════════════
            // Sub:-
            // ═════════════════════════════════════════════════════════════════════════
            var subPara = new Paragraph();
            subPara.Add(new Text("Sub:- ").SetFont(boldFont).SetFontSize(12));
            subPara.Add(new Text("Certificate of your Manager Change Application").SetFont(regularFont).SetFontSize(12));
            _ = document.Add(subPara.SetMarginBottom(4f));

            // ═════════════════════════════════════════════════════════════════════════
            // Intro line
            // ═════════════════════════════════════════════════════════════════════════
            _ = document.Add(new Paragraph(
                    "The details of your factory as per application, drawings and documents are shown below:-")
                .SetFont(regularFont).SetFontSize(12)
                .SetMarginBottom(4f));

            // ═════════════════════════════════════════════════════════════════════════
            // Factory details table (red border)
            // ═════════════════════════════════════════════════════════════════════════
            var detailsTable = new PdfTable(new float[] { 150f, 1f })
                .UseAllAvailableWidth().SetMarginBottom(12f);

            PdfCell RedCell(string text, PdfFont font, float fontSize = 12f)
            {
                var border = new iText.Layout.Borders.SolidBorder(new DeviceRgb(220, 0, 0), 1.5f);
                return new PdfCell()
                    .Add(new Paragraph(text ?? "-").SetFont(font).SetFontSize(fontSize))
                    .SetBorderTop(border).SetBorderBottom(border)
                    .SetBorderLeft(border).SetBorderRight(border)
                    .SetPadding(5f);
            }

            _ = detailsTable.AddCell(RedCell("Manufacturing Process", boldFont));
            _ = detailsTable.AddCell(RedCell(commencementCessationData.EstFullDetails.Factory.ManufacturingDetail ?? "-", regularFont));

            _ = detailsTable.AddCell(RedCell("Type", boldFont));
            _ = detailsTable.AddCell(RedCell(Type, regularFont));

            _ = detailsTable.AddCell(RedCell("Category", boldFont));
            _ = detailsTable.AddCell(RedCell(commencementCessationData.EstFullDetails.EstablishmentDetail.FactoryTypeName ?? "-", regularFont));

            _ = detailsTable.AddCell(RedCell("Workers", boldFont));
            _ = detailsTable.AddCell(RedCell(commencementCessationData.EstFullDetails.Factory.NumberOfWorker.ToString() ?? "-", regularFont));

            _ = document.Add(detailsTable);

            _ = document.Add(new Paragraph("Remarks of registering officers:" +
                   $" {dto.Remarks}")
               .SetFont(regularFont).SetFontSize(10)
               .SetMarginBottom(16f));

            _ = document.Add(new Paragraph("").SetMarginBottom(10f));

            // ═════════════════════════════════════════════════════════════════════════
            // Signature block (right-aligned)
            // ═════════════════════════════════════════════════════════════════════════
            var imageData = ImageDataFactory.Create("wwwroot/chief_signature.jpg");

            var sigOuterTable = new PdfTable(new float[] { 1f, 1f })
                .UseAllAvailableWidth().SetBorder(Border.NO_BORDER).SetMarginBottom(8f);

            sigOuterTable.AddCell(new PdfCell().SetBorder(Border.NO_BORDER));

            var sigCell = new PdfCell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT);

            var innerDiv = new Div()
                .SetWidth(250)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetHorizontalAlignment(HorizontalAlignment.RIGHT);

            innerDiv.Add(new PdfImage(imageData).ScaleToFit(150, 50).SetHorizontalAlignment(HorizontalAlignment.CENTER));
            innerDiv.Add(new Paragraph($"( {userName} )").SetFont(regularFont).SetFontSize(12).SetMarginTop(2f));
            innerDiv.Add(new Paragraph(postName).SetFont(regularFont).SetFontSize(12).SetMarginTop(2f));

            sigCell.Add(innerDiv);
            sigOuterTable.AddCell(sigCell);
            document.Add(sigOuterTable);

            // ═════════════════════════════════════════════════════════════════════════
            // Footer disclaimer
            // ═════════════════════════════════════════════════════════════════════════
            var pageWidth = pdf.GetDefaultPageSize().GetWidth();
            _ = document.Add(new Paragraph(
                    "This is a computer generated certificate and bears scanned signature. No physical signature is required on this document. You " +
                    "can verify this document by visiting rajnivesh.rajasthan.gov.in or rajfab.rajasthan.gov.in and entering Application No./ID after clicking the link for " +
                    "verification on the page.")
                .SetFont(regularFont).SetFontSize(7)
                .SetFontColor(ColorConstants.GRAY)
                .SetTextAlignment(TextAlignment.JUSTIFIED)
                .SetFixedPosition(35, 33, pageWidth - 70));

            document.Close();

            return fileUrl;
        }

        public async Task<AreaHierarchyDto?> GetAreaHierarchyAsync(string? areaIdStr)
        {
            if (string.IsNullOrEmpty(areaIdStr))
                return null;

            if (!Guid.TryParse(areaIdStr, out var areaId))
                return null;

            // Get Area
            var area = await _context.Areas
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == areaId);

            if (area == null) return null;

            // Get District
            var district = await _context.Districts
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == area.DistrictId);

            // Get Division
            var division = district != null
                ? await _context.Divisions.AsNoTracking().FirstOrDefaultAsync(v => v.Id == district.DivisionId)
                : null;

            return new AreaHierarchyDto
            {
                AreaId = area.Id,
                AreaName = area.Name,
                DistrictId = district?.Id ?? Guid.Empty,
                DistrictName = district?.Name,
                DivisionId = division?.Id ?? Guid.Empty,
                DivisionName = division?.Name
            };
        }

        private sealed class PageBorderAndFooterEventHandler : AbstractPdfDocumentEventHandler
        {
            private readonly PdfFont _boldFont;
            private readonly PdfFont _regularFont;
            private readonly DateOnly _date;
            private readonly string _postName;
            private readonly string _userName;
            private readonly string? _signatureUrl;

            public PageBorderAndFooterEventHandler(PdfFont boldFont, PdfFont regularFont, DateOnly date, string? signatureUrl = null, string postName = "", string userName = "")
            {
                _boldFont = boldFont;
                _regularFont = regularFont;
                _date = date;
                _postName = postName;
                _userName = userName;
                _signatureUrl = signatureUrl;
            }

            protected override void OnAcceptedEvent(AbstractPdfDocumentEvent @event)
            {
                if (@event is not PdfDocumentEvent docEvent) return;
                var pdfDoc = docEvent.GetDocument();
                var page = docEvent.GetPage();
                var rect = page.GetPageSize();

                // ONE PdfCanvas per page — reused for all drawing operations.
                // Creating multiple PdfCanvas(page) instances for the same page appends
                // independent content streams; when all are released at document close
                // iText7 tries to finalise the pages tree multiple times → error.
                var pdfCanvas = new PdfCanvas(page);

                float pageWidth = rect.GetWidth();
                float pageHeight = rect.GetHeight();

                // ───── Page Border
                pdfCanvas
                    .SetStrokeColor(ColorConstants.BLACK)
                    .SetLineWidth(1.5f)
                    .Rectangle(25, 25, pageWidth - 50, pageHeight - 50)
                    .Stroke();

                // ───── Separator Line (above footer zone)
                float lineY = 70f;
                pdfCanvas
                    .SetStrokeColor(new DeviceRgb(180, 180, 180))
                    .SetLineWidth(0.5f)
                    .MoveTo(30, lineY)
                    .LineTo(pageWidth - 30, lineY)
                    .Stroke();

                float zoneHeight = 65f;
                float zoneY = lineY + 4f;
                float signColWidth = 180f;
                float stampBoxWidth = 220f;
                float belowY = lineY - 4f - zoneHeight;
                float signBoxX = pageWidth - 30f - signColWidth;
                int pageNumber = pdfDoc.GetPageNumber(page);

                // ── All Canvas wrappers share the same PdfCanvas ──────────────────────

                // Right: Signature image
                if (!string.IsNullOrWhiteSpace(_signatureUrl))
                {
                    float sigX = pageWidth - 30f - signColWidth;
                    using (var sigCanvas = new Canvas(pdfCanvas,
                        new iText.Kernel.Geom.Rectangle(sigX, zoneY, signColWidth, zoneHeight)))
                    {
                        sigCanvas.Add(new PdfImage(ImageDataFactory.Create(_signatureUrl))
                            .ScaleToFit(signColWidth, zoneHeight - 10f)
                            .SetHorizontalAlignment(HorizontalAlignment.CENTER));
                    }
                }

                // Below separator — Left: Dated
                using (var leftCanvas = new Canvas(pdfCanvas,
                    new iText.Kernel.Geom.Rectangle(30f, belowY, stampBoxWidth / 2f, zoneHeight)))
                {
                    leftCanvas.Add(new Paragraph($"Dated: {_date}")
                        .SetFont(_regularFont).SetFontSize(7.5f).SetMargin(0f).SetPaddingTop(6f));
                }

                // Below separator — Center: Page N
                using (var centerCanvas = new Canvas(pdfCanvas,
                    new iText.Kernel.Geom.Rectangle(0f, belowY, pageWidth, zoneHeight)))
                {
                    centerCanvas.Add(new Paragraph($"Page {pageNumber}")
                        .SetFont(_regularFont).SetFontSize(7.5f)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMargin(0f).SetPaddingTop(6f));
                }

                // Below separator — Right: signature label
                using (var signLabelCanvas = new Canvas(pdfCanvas,
                    new iText.Kernel.Geom.Rectangle(signBoxX, belowY, signColWidth, zoneHeight)))
                {
                    if (!string.IsNullOrWhiteSpace(_userName))
                    {
                        // Name (top)
                        signLabelCanvas.Add(new Paragraph($"({_userName ?? "-"})")
                            .SetFont(_boldFont)
                            .SetFontSize(7f)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMargin(0f)
                            .SetPaddingTop(2f));
                    }
                    if (!string.IsNullOrWhiteSpace(_postName))
                    {
                        // Post name (middle)
                        signLabelCanvas.Add(new Paragraph(_postName ?? "-")
                            .SetFont(_regularFont)
                            .SetFontSize(6.5f)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMargin(0f)
                            .SetPaddingTop(1f));
                    }
                    // Signature label (bottom)
                    signLabelCanvas.Add(new Paragraph("Signature / E-sign / Digital sign")
                        .SetFont(_regularFont)
                        .SetFontSize(6.5f)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMargin(0f)
                        .SetPaddingTop(4f));
                }

                // Release the single PdfCanvas only after all drawing is complete
                pdfCanvas.Release();
            }
        }

        private sealed class PageBorderEventHandler : AbstractPdfDocumentEventHandler
        {
            protected override void OnAcceptedEvent(AbstractPdfDocumentEvent @event)
            {
                if (@event is not PdfDocumentEvent docEvent) return;
                var page = docEvent.GetPage();
                var rect = page.GetPageSize();
                var canvas = new PdfCanvas(page);
                canvas
                    .SetStrokeColor(ColorConstants.BLACK)
                    .SetLineWidth(1.5f)
                    .Rectangle(25, 25, rect.GetWidth() - 50, rect.GetHeight() - 50)
                    .Stroke();
                canvas.Release();
            }
        }

        private sealed class ManagerChangePageBorderAndFooterEventHandler : AbstractPdfDocumentEventHandler
        {
            private readonly PdfFont _boldFont;
            private readonly PdfFont _regularFont;
            private readonly string _date;

            public ManagerChangePageBorderAndFooterEventHandler(PdfFont boldFont, PdfFont regularFont, string date)
            {
                _boldFont = boldFont;
                _regularFont = regularFont;
                _date = date;
            }

            protected override void OnAcceptedEvent(AbstractPdfDocumentEvent @event)
            {
                if (@event is not PdfDocumentEvent docEvent) return;

                var pdfDoc = docEvent.GetDocument();
                var page = docEvent.GetPage();
                var rect = page.GetPageSize();

                float pageWidth = rect.GetWidth();
                float footerY = 35f;
                float lineY = 65f;

                float rightMargin = 30f;
                float signBlockWidth = 120f;
                float signBlockHeight = 30f;
                float gap = 8f;

                float occupierX = pageWidth - rightMargin - signBlockWidth;
                float managerX = occupierX - gap - signBlockWidth;

                int pageNumber = pdfDoc.GetPageNumber(page);

                // ── Single PdfCanvas for ALL drawing on this page ─────────────────────
                var pdfCanvas = new PdfCanvas(page);

                // ───── Border
                pdfCanvas
                    .SetStrokeColor(ColorConstants.BLACK)
                    .SetLineWidth(1.5f)
                    .Rectangle(25, 25, pageWidth - 50, rect.GetHeight() - 50)
                    .Stroke();

                // ───── Separator line
                pdfCanvas
                    .SetStrokeColor(new DeviceRgb(180, 180, 180))
                    .SetLineWidth(0.5f)
                    .MoveTo(30, lineY)
                    .LineTo(pageWidth - 30, lineY)
                    .Stroke();

                // ───── LEFT: Date
                using (var left = new Canvas(pdfCanvas,
                    new iText.Kernel.Geom.Rectangle(30, footerY, 150, signBlockHeight)))
                {
                    left.Add(new Paragraph($"Dated: {_date}")
                        .SetFont(_regularFont)
                        .SetFontSize(9)
                        .SetMargin(0));
                }

                // ───── CENTER: Page number
                using (var center = new Canvas(pdfCanvas,
                    new iText.Kernel.Geom.Rectangle(0, footerY, pageWidth, signBlockHeight)))
                {
                    center.Add(new Paragraph($"Page {pageNumber}")
                        .SetFont(_regularFont)
                        .SetFontSize(9)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMargin(0));
                }

                // ───── RIGHT: Manager signature
                using (var manager = new Canvas(pdfCanvas,
                    new iText.Kernel.Geom.Rectangle(managerX, footerY, signBlockWidth, signBlockHeight)))
                {
                    manager.Add(new Paragraph("")
                        .SetFont(_regularFont)
                        .SetFontSize(9)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMargin(0));
                }

                // ───── FAR RIGHT: Occupier signature
                using (var occupier = new Canvas(pdfCanvas,
                    new iText.Kernel.Geom.Rectangle(occupierX, footerY, signBlockWidth, signBlockHeight)))
                {
                    occupier.Add(new Paragraph("e-sign / Signature of\nOccupier")
                        .SetFont(_regularFont)
                        .SetFontSize(9)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMargin(0));
                }

                // ── Release ONCE after all drawing is complete ────────────────────────
                pdfCanvas.Release();
            }
        }

        string Capitalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            input = input.ToLower();
            return char.ToUpper(input[0]) + input.Substring(1);
        }
    }
}