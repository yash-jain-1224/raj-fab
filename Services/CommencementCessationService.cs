using Azure;
using iText.Commons.Actions;
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
using System.Text.Json;
using static RajFabAPI.Constants.AppConstants;
using ImageDataFactory = iText.IO.Image.ImageDataFactory;
using PdfCell = iText.Layout.Element.Cell;
using PdfImage = iText.Layout.Element.Image;
using PdfTable = iText.Layout.Element.Table;

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
                .FirstOrDefaultAsync(x => x.ApplicationId == id);

            if (entity == null)
                return null;

            var estFullDetails = await _establishmentRegistrationService
                .GetFactoryDetailsByFactoryRegistrationNumberAsync(entity.FactoryRegistrationNumber);

            var dto = new CommencementCessationDto
            {
                Id = entity.Id,
                ApplicationId = entity.ApplicationId,
                FactoryRegistrationNumber = entity.FactoryRegistrationNumber,
                Type = entity.Type,
                ApproxDurationOfWork = entity.ApproxDurationOfWork,
                CessationIntimationDate = entity.CessationIntimationDate,
                CessationIntimationEffectiveDate = entity.CessationIntimationEffectiveDate,
                OccupierSignature = entity.OccupierSignature,
                Status = entity.Status,
                CreatedAt = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate,
                Version = entity.Version,
                IsActive = entity.IsActive,
                ApplicationPDFUrl = entity.ApplicationPDFUrl
            };

            return new CommencementCessationResDto
            {
                CommencementCessationData = dto,
                EstFullDetails = estFullDetails
            };
        }
        public async Task<bool> UpdateStatusAndRemark(string registrationId, string status)
        {
            try
            {
                var reg = _context.CommencementCessationApplication.FirstOrDefault(x => x.ApplicationId == registrationId);
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
                    ApplicationId = Guid.NewGuid().ToString().ToUpper(),
                    Type = request.Type,
                    FactoryRegistrationNumber = request.FactoryRegistrationNumber,
                    CessationIntimationDate = request.CessationIntimationDate,
                    CessationIntimationEffectiveDate = request.CessationIntimationEffectiveDate,
                    ApproxDurationOfWork = request.ApproxDurationOfWork,
                    OccupierSignature = request.OccupierSignature,
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


                // Get ModuleId from Modules table (assuming ApplicationTypeId is available in DTO or context)
                var module = await _context.Set<FormModule>().FirstOrDefaultAsync(m => m.Name == "Factory Commencement And Cessation");
                if (module == null)
                    throw new Exception("Module not found for ApplicationTypeId");
                //var EstablishmentDetailId = await _context.Set<EstablishmentRegistration>()
                //                    .Where(m => m.RegistrationNumber == request.FactoryRegistrationNumber)
                //                    .Select(m => m.EstablishmentDetailId)
                //                    .FirstOrDefaultAsync();

                var appReg = new ApplicationRegistration
                {
                    Id = Guid.NewGuid().ToString(),
                    ModuleId = module.Id,
                    UserId = userId,
                    ApplicationId = entity.ApplicationId,
                    ApplicationRegistrationNumber = request.FactoryRegistrationNumber,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };
                _context.Set<ApplicationRegistration>().Add(appReg);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();


                //var areaId = await _context.Set<EstablishmentDetail>()
                //    .Where(m => m.Id == EstablishmentDetailId)
                //    .Select(m => m.SubDivisionId)
                //    .FirstOrDefaultAsync();

                //var factoryCategoryId = Guid.Parse("EB857143-2FBB-4C6E-88F8-888C3D6DB671");

                //var officeApplicationArea = await _context.Set<OfficeApplicationArea>()
                //    .FirstOrDefaultAsync(oaa => oaa.CityId == Guid.Parse(areaId));
                //if (officeApplicationArea != null)
                //{
                //    var officeId = officeApplicationArea?.OfficeId;
                //    var workflow = await _context.Set<ApplicationWorkFlow>()
                //        .FirstOrDefaultAsync(wf => wf.ModuleId == module.Id && wf.FactoryCategoryId == factoryCategoryId && wf.OfficeId == officeId);
                //    var workflowLevel = await _context.Set<ApplicationWorkFlowLevel>()
                //        .Where(wfl => wfl.ApplicationWorkFlowId == (workflow != null ? workflow.Id : Guid.Empty))
                //        .OrderBy(wfl => wfl.LevelNumber)
                //        .FirstOrDefaultAsync();

                //    if (workflow != null)
                //    {
                //        var applicationApprovalRequest = new ApplicationApprovalRequest
                //        {
                //            ModuleId = module.Id,
                //            ApplicationRegistrationId = appReg.Id,
                //            ApplicationWorkFlowLevelId = workflowLevel.Id,
                //            Status = "Pending",
                //            CreatedDate = DateTime.Now,
                //            UpdatedDate = DateTime.Now
                //        };
                //        _context.Set<ApplicationApprovalRequest>().Add(applicationApprovalRequest);
                //        await _context.SaveChangesAsync();
                //    }
                //}

                //return MapToDto(entity);
                return entity.ApplicationId;
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
            entity.CessationIntimationDate = request.CessationIntimationDate;
            entity.CessationIntimationEffectiveDate = request.CessationIntimationEffectiveDate;
            entity.ApproxDurationOfWork = request.ApproxDurationOfWork;
            entity.OccupierSignature = request.OccupierSignature;
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
                CessationIntimationDate = entity.CessationIntimationDate,
                CessationIntimationEffectiveDate = entity.CessationIntimationEffectiveDate,
                ApproxDurationOfWork = entity.ApproxDurationOfWork,
                OccupierSignature = entity.OccupierSignature,
                Status = entity.Status,
                Version = entity.Version,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate
            };
        }
        public string GenerateRegistrationNumber()
        {
            var year = DateTime.Now.Year;
            var sequence = DateTime.Now.Ticks.ToString().Substring(8, 6);
            return $"FC{year}{sequence}";
        }

        public async Task<string> GenerateCommencementCessationPdf(CommencementCessationResDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var appData = dto.CommencementCessationData;
            var factoryData = dto.EstFullDetails?.EstablishmentDetail;
            var occupierData = dto.EstFullDetails?.MainOwnerDetail;

            var fileName = $"commencement_cessation_{appData.ApplicationId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

            var uploadPath = Path.Combine(_environment.WebRootPath, "commencement-cessation-forms");
            Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);

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
                .Add(new Paragraph("Notice of Commencement/Cessation of Establishment").SetFontColor(ColorConstants.BLUE).SetFontSize(12))
                .SetBorder(Border.NO_BORDER));
            document.Add(headerTable);
            document.Add(new Paragraph().SetMarginBottom(10));

            document.Add(new Paragraph($"Acknowledgement No: {appData?.ApplicationId ?? "-"}")
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

            // ====== APPLICATION DETAILS ======
            document.Add(SectionTitle("Application Details", boldFont));

            var appTable = CreateTable();
            AddRow(appTable, "Application Type", appData?.Type, boldFont, regularFont);
            AddRow(appTable, "Approx. Duration of Work", appData?.ApproxDurationOfWork, boldFont, regularFont);
            AddRow(appTable, "Cessation Intimation Date", appData?.CessationIntimationDate?.ToString("dd/MM/yyyy"), boldFont, regularFont);
            AddRow(appTable, "Cessation Effective Date", appData?.CessationIntimationEffectiveDate?.ToString("dd/MM/yyyy"), boldFont, regularFont);
            AddRow(appTable, "Status", appData?.Status, boldFont, regularFont);

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