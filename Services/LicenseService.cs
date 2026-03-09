using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
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
using static RajFabAPI.Constants.AppConstants;
using ImageDataFactory = iText.IO.Image.ImageDataFactory;
using PdfDocument = iText.Kernel.Pdf.PdfDocument;
using PdfFont = iText.Kernel.Font.PdfFont;
using PdfImage = iText.Layout.Element.Image;
using PdfWriter = iText.Kernel.Pdf.PdfWriter;

namespace RajFabAPI.Services
{
    public class FactoryLicenseService : IFactoryLicenseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaymentService _payment;
        private readonly IEstablishmentRegistrationService _establishmentRegistrationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _config;


        public FactoryLicenseService(ApplicationDbContext context, IPaymentService payment,
            IEstablishmentRegistrationService establishmentRegistrationService,
            IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment, IConfiguration config)
        {
            _context = context;
            _payment = payment;
            _establishmentRegistrationService = establishmentRegistrationService;
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
            _config = config;
        }

        public async Task<IEnumerable<FactoryLicense>> GetAllAsync(Guid userId)
        {
            return await _context.FactoryLicenses
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<FactoryLicenseData?> GetByIdAsync(string id)
        {
            var factoryLicense = await _context.FactoryLicenses
                .FirstOrDefaultAsync(x => x.Id == id);
            var estFullDetails = await _establishmentRegistrationService
                .GetFactoryDetailsByFactoryRegistrationNumberAsync(factoryLicense.FactoryRegistrationNumber);
            var applicationHistory = await _context.ApplicationHistories
                .Where(h => h.ApplicationId == id)
                .OrderByDescending(h => h.ActionDate)
                .Select(h => new ApplicationHistoryDto
                {
                    Id = h.Id,
                    Action = h.Action,
                    PreviousStatus = h.PreviousStatus,
                    NewStatus = h.NewStatus,
                    Comments = h.Comments,
                    ActionByName = h.ActionByName,
                    ForwardedToName = h.ForwardedToName,
                    ActionDate = h.ActionDate
                })
                .ToListAsync();
            var activeCertificate = await _context.Set<Certificate>()
                .Where(c => c.ApplicationId == id)
                .OrderByDescending(c => c.CertificateVersion)
                .FirstOrDefaultAsync();

            return new FactoryLicenseData
            {
                FactoryLicense = factoryLicense,
                EstFullDetails = estFullDetails,
                ApplicationHistory = applicationHistory,
                CertificatePDFUrl = activeCertificate?.CertificateUrl
            };
        }

        public async Task<string> CreateAsync(CreateFactoryLicenseDto dto, Guid userId, string type, string FactoryLicenseNumber = "")
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.ValidTo <= dto.ValidFrom)
                throw new ArgumentException("ValidTo must be greater than ValidFrom.");
            var User = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);
            decimal newVersion;
            string finalFactoryLicenseNumber;

            if (type == "new" && string.IsNullOrEmpty(FactoryLicenseNumber))
            {
                finalFactoryLicenseNumber = GenerateLicenseNumber();
                newVersion = 1.0m;
            }
            else if (type == "amendment" || type == "renewal")
            {
                var lastApproved = await _context.FactoryLicenses
                    .Where(r => r.FactoryLicenseNumber == FactoryLicenseNumber && r.Status == ApplicationStatus.Approved)
                    .OrderByDescending(r => r.Version)
                    .FirstOrDefaultAsync();

                if (lastApproved == null)
                    throw new ArgumentException("Existing Factory License not found.");

                newVersion = Math.Round(lastApproved.Version + 0.1m, 1);
                finalFactoryLicenseNumber = lastApproved.FactoryLicenseNumber;
            }
            else
            {
                throw new ArgumentException("Invalid registration type or missing Factory License ID for amendment/renewal.");
            }
            var amount = 100;
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var license = new FactoryLicense
                {
                    Id = Guid.NewGuid().ToString().ToUpper(),
                    FactoryRegistrationNumber = dto.FactoryRegistrationNumber.Trim(),
                    FactoryLicenseNumber = finalFactoryLicenseNumber,
                    ValidFrom = dto.ValidFrom,
                    ValidTo = dto.ValidTo,
                    Place = dto.Place.Trim(),
                    Date = dto.Date,
                    ManagerSignature = dto.ManagerSignature,
                    OccupierSignature = dto.OccupierSignature,
                    AuthorisedSignature = dto.AuthorisedSignature,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Version = newVersion,
                    Status = "Pending",
                    Amount = amount,
                    Type = string.IsNullOrEmpty(type) ? "new" : type
                };
                _context.FactoryLicenses.Add(license);

                // Add application history for submission
                var submittedHistory = new ApplicationHistory
                {
                    ApplicationId = license.Id,
                    ApplicationType = "FactoryLicense",
                    Action = "Application Submitted",
                    PreviousStatus = null,
                    NewStatus = "Pending",
                    Comments = "Application submitted and payment initiated.",
                    ActionBy = userId.ToString(),
                    ActionByName = User?.FullName ?? "Unknown User",
                    ActionDate = DateTime.Now
                };
                _context.ApplicationHistories.Add(submittedHistory);

                string applicationTypeName = type switch
                {
                    "new" => ApplicationTypeNames.FactoryLicense,
                    "amendment" => ApplicationTypeNames.FactoryLicenseAmendment,
                    "renewal" => ApplicationTypeNames.FactoryLicenseRenewal,
                    _ => throw new ArgumentException($"Invalid registration type: {type}")
                };

                var module = await _context.Set<FormModule>()
                    .FirstOrDefaultAsync(m => m.Name == applicationTypeName);
                if (module == null)
                    throw new Exception("Module not found for ApplicationTypeId");

                var appReg = new ApplicationRegistration
                {
                    Id = Guid.NewGuid().ToString(),
                    ModuleId = module.Id,
                    UserId = userId,
                    ApplicationId = license.Id,
                    ApplicationRegistrationNumber = finalFactoryLicenseNumber,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };
                _context.Set<ApplicationRegistration>().Add(appReg);

                //var estReg = await _context.Set<EstablishmentRegistration>()
                //    .Where(er => er.RegistrationNumber == dto.FactoryRegistrationNumber && er.Status == "Approved")
                //    .OrderByDescending(er => er.Version)
                //    .FirstOrDefaultAsync();
                //if (estReg == null) throw new Exception("Establishment details not found for the user.");

                //var estDetail = await _context.Set<EstablishmentDetail>()
                //    .FirstOrDefaultAsync(ed => ed.Id == estReg.EstablishmentDetailId);
                //if (estDetail == null) throw new Exception("Establishment details not found for the user.");

                //int totalWorkers = (estDetail.TotalNumberOfEmployee ?? 0) +
                //                   (estDetail.TotalNumberOfContractEmployee ?? 0) +
                //                   (estDetail.TotalNumberOfInterstateWorker ?? 0);

                //var workerRange = await _context.Set<WorkerRange>()
                //    .FirstOrDefaultAsync(wr => totalWorkers >= wr.MinWorkers && totalWorkers <= wr.MaxWorkers);

                //var factoryType = _context.FactoryTypes.FirstOrDefault(x => x.Name == "Not Applicable");
                //Guid? factoryTypeIdGuid = factoryType?.Id;

                //Guid? workerRangeId = workerRange?.Id;
                //var factoryCategory = await _context.Set<FactoryCategory>()
                //    .FirstOrDefaultAsync(fc => fc.WorkerRangeId == workerRangeId && fc.FactoryTypeId == factoryTypeIdGuid);
                //Guid? factoryCategoryId = factoryCategory?.Id;

                //var officeApplicationArea = await _context.Set<OfficeApplicationArea>()
                //    .FirstOrDefaultAsync(oaa => oaa.CityId == Guid.Parse(estDetail.SubDivisionId));

                //if (officeApplicationArea != null)
                //{
                //    var officeId = officeApplicationArea?.OfficeId;
                //    var workflow = await _context.Set<ApplicationWorkFlow>()
                //        .FirstOrDefaultAsync(wf => wf.ModuleId == module.Id && wf.FactoryCategoryId == factoryCategoryId && wf.OfficeId == officeId);

                //    if (workflow == null) throw new Exception("workflow not found for this module and factory category.");

                //    Guid workflowId = workflow != null ? workflow.Id : Guid.Empty;
                //    var workflowLevel = await _context.Set<ApplicationWorkFlowLevel>()
                //        .Where(wfl => wfl.ApplicationWorkFlowId == workflowId)
                //        .OrderBy(wfl => wfl.LevelNumber)
                //        .FirstOrDefaultAsync();

                //    if (workflow != null && workflowLevel != null)
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
                //    }
                //}

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                var html = await _payment.ActionRequestPaymentRPP(amount, User.FullName, User.Mobile, User.Email, User.Username, "4157FE34BBAE3A958D8F58CCBFAD7", "UWf6a7cDCP", license.Id, module.Id.ToString(), userId.ToString());
                return html;

                //return finalFactoryLicenseNumber;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<FactoryLicense?> UpdateAsync(string id, CreateFactoryLicenseDto dto, Guid userId)
        {
            var license = await _context.FactoryLicenses.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
            if (license == null)
                return null;

            license.FactoryRegistrationNumber = dto.FactoryRegistrationNumber;
            license.ValidFrom = dto.ValidFrom;
            license.ValidTo = dto.ValidTo;
            license.Place = dto.Place;
            license.Status = "Pending";
            license.Date = dto.Date;
            license.ManagerSignature = dto.ManagerSignature;
            license.OccupierSignature = dto.OccupierSignature;
            license.AuthorisedSignature = dto.AuthorisedSignature;
            license.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            string applicationTypeName = license.Type switch
            {
                "new" => ApplicationTypeNames.FactoryLicense,
                "amendment" => ApplicationTypeNames.FactoryLicenseAmendment,
                "renew" => ApplicationTypeNames.FactoryLicenseRenewal,
                _ => throw new ArgumentException($"Invalid registration type: {license.Type}")
            };

            var module = await _context.Set<FormModule>().FirstOrDefaultAsync(m => m.Name == applicationTypeName);
            if (module == null)
                return license;

            var appReg = await _context.ApplicationRegistrations
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync(x => x.ApplicationId == license.Id && x.ModuleId == module.Id);
            if (appReg == null)
                return license;

            // Fetch EstablishmentDetail for the factory registration number
            var estReg = await _context.Set<EstablishmentRegistration>()
                .Where(er => er.RegistrationNumber == dto.FactoryRegistrationNumber && er.Status == "Approved")
                .OrderByDescending(er => er.Version)
                .FirstOrDefaultAsync();

            if (estReg == null)
                return license;

            var estDetail = await _context.Set<EstablishmentDetail>()
                .FirstOrDefaultAsync(ed => ed.Id == estReg.EstablishmentDetailId);
            if (estDetail == null)
                return license;

            // Calculate total workers
            int totalWorkers = (estDetail.TotalNumberOfEmployee ?? 0) +
                               (estDetail.TotalNumberOfContractEmployee ?? 0) +
                               (estDetail.TotalNumberOfInterstateWorker ?? 0);

            var workerRange = await _context.Set<WorkerRange>()
                .FirstOrDefaultAsync(wr => totalWorkers >= wr.MinWorkers && totalWorkers <= wr.MaxWorkers);

            var factoryType = await _context.FactoryTypes.FirstOrDefaultAsync(x => x.Name == "Not Applicable");
            var factoryCategory = await _context.Set<FactoryCategory>()
                .FirstOrDefaultAsync(fc => fc.WorkerRangeId == workerRange.Id && fc.FactoryTypeId == factoryType.Id);

            if (!Guid.TryParse(estDetail.SubDivisionId, out var subDivisionGuid))
                return license;

            var officeApplicationArea = await _context.Set<OfficeApplicationArea>()
                .FirstOrDefaultAsync(oaa => oaa.CityId == subDivisionGuid);
            if (officeApplicationArea == null)
                return license;

            var workflow = await _context.Set<ApplicationWorkFlow>()
                .FirstOrDefaultAsync(wf =>
                    wf.ModuleId == module.Id &&
                    wf.FactoryCategoryId == factoryCategory.Id &&
                    wf.OfficeId == officeApplicationArea.OfficeId);
            if (workflow == null)
                return license;

            var workflowLevel = await _context.Set<ApplicationWorkFlowLevel>()
                .Where(wfl => wfl.ApplicationWorkFlowId == workflow.Id)
                .OrderBy(wfl => wfl.LevelNumber)
                .FirstOrDefaultAsync();
            if (workflowLevel == null)
                return license;

            var applicationApprovalRequest = new ApplicationApprovalRequest
            {
                ModuleId = module.Id,
                ApplicationRegistrationId = appReg.Id,
                ApplicationWorkFlowLevelId = workflowLevel.Id,
                Status = "Pending",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };
            _context.Set<ApplicationApprovalRequest>().Add(applicationApprovalRequest);

            var history = new ApplicationHistory
            {
                ApplicationId = appReg.ApplicationId,
                ApplicationType = module.Name,
                Action = "Application data updated",
                Comments = "Application data updated by citizen",
                ActionBy = userId.ToString(),
                ActionByName = "Applicant",
                ActionDate = DateTime.Now
            };

            _context.ApplicationHistories.Add(history);
            await _context.SaveChangesAsync();

            return license;
        }

        public async Task<bool> UpdateStatusAndRemark(string applicationId, string status)
        {
            var license = await _context.FactoryLicenses.FirstOrDefaultAsync(x => x.Id == applicationId);
            if (license == null)
                return false;
            license.Status = status;
            license.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        // public async Task<bool> DeleteAsync(Guid id)
        // {
        //     var license = await _context.FactoryLicenses.FindAsync(id);
        //     if (license == null)
        //         return false;

        //     // Soft delete
        //     license.IsActive = false;
        //     license.UpdatedAt = DateTime.Now;

        //     await _context.SaveChangesAsync();
        //     return true;
        // }

        public async Task<string> GenerateCertificateAsync(FactoryLicenseCertificateRequestDto dto, Guid userId, string licenseId)
        {
            try
            {
                var license = await _context.FactoryLicenses.FirstOrDefaultAsync(x => x.Id == licenseId)
                    ?? throw new KeyNotFoundException("Factory license not found.");

                var licenseData = await GetByIdAsync(licenseId);

                var certificateUrl = await GenerateFactoryLicensePdf(licenseData, true);

                var module = await _context.Set<FormModule>().FirstOrDefaultAsync(m => m.Name == ApplicationTypeNames.FactoryLicense)
                    ?? throw new InvalidOperationException("FactoryLicense module not found.");

                var certificate = new Certificate
                {
                    RegistrationNumber = license.FactoryLicenseNumber,
                    CertificateVersion = 1.0m,
                    StartDate = DateTime.TryParse(dto.StartDate, out var start) ? start : DateTime.Now,
                    EndDate = DateTime.TryParse(dto.EndDate, out var end) ? end : DateTime.Now.AddYears(1),
                    CertificateUrl = certificateUrl,
                    IssuedByUserId = userId,
                    IssuedAt = DateTime.TryParse(dto.IssuedAt, out var issuedAt) ? issuedAt : DateTime.Now,
                    Place = dto.Place ?? string.Empty,
                    Signature = dto.Signature,
                    Status = "Issued",
                    ModuleId = module.Id,
                    Remarks = dto.Remarks ?? string.Empty,
                    ApplicationId = licenseId,
                    IsESignCompleted = false
                };
                _context.Set<Certificate>().Add(certificate);

                var history = new ApplicationHistory
                {
                    ApplicationId = licenseId,
                    ApplicationType = "FactoryLicense",
                    Action = "Certificate Generated",
                    PreviousStatus = license.Status,
                    NewStatus = license.Status,
                    Comments = "Factory license certificate generated",
                    ActionBy = userId.ToString(),
                    ActionDate = DateTime.Now
                };
                _context.ApplicationHistories.Add(history);

                await _context.SaveChangesAsync();

                return certificate.Id.ToString();
            }
            catch
            {
                throw;
            }
        }

        public string GenerateLicenseNumber()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"FLN{timestamp}{random}";
        }

        public async Task<string> GenerateFactoryLicensePdf(FactoryLicenseData dto, bool isCertificate = false)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var appData = dto.FactoryLicense;
            var factoryData = dto.EstFullDetails?.EstablishmentDetail;
            var occupierData = dto.EstFullDetails?.MainOwnerDetail;

           var folderName = isCertificate ? "certificates" : "factory-license-forms";

            var fileName = $"factory_license_{appData.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

            var uploadPath = Path.Combine(_environment.WebRootPath, folderName);
            Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);
            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");

            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/{folderName}/{fileName}";

            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            // ================= HEADER =================
            var headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 4 })).UseAllAvailableWidth();
            headerTable.AddCell(new Cell().Add(new PdfImage(ImageDataFactory.Create("wwwroot/Emblem_of_India.png")).ScaleToFit(40, 40)).SetBorder(Border.NO_BORDER));
            headerTable.AddCell(new Cell()
                .Add(new Paragraph("Form - 5").SetFont(boldFont).SetFontSize(18))
                .Add(new Paragraph("(See sub-rule (9) of rule 5)").SetFont(regularFont).SetFontSize(12))
                .Add(new Paragraph("Factory License Form").SetFontColor(ColorConstants.BLUE).SetFontSize(12))
                .SetBorder(Border.NO_BORDER));
            document.Add(headerTable);
            document.Add(new Paragraph().SetMarginBottom(10));

            document.Add(new Paragraph($"Factory License No: {appData?.FactoryLicenseNumber ?? "-"}")
                .SetFont(regularFont));

            document.Add(new Paragraph($"Date: {appData?.CreatedAt:dd/MM/yyyy}")
                .SetMarginBottom(15));

            // ====== FACTORY DETAILS ======
            _ = document.Add(SectionTitle("Factory & Occupier Details", boldFont));

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

            // ====== Factory License DETAILS ======
            document.Add(SectionTitle("Factory License Details", boldFont));

            var appTable = CreateTable();

            AddRow(appTable, "Factory Registration Number", appData?.FactoryRegistrationNumber, boldFont, regularFont);
            AddRow(appTable, "License Type", appData?.Type, boldFont, regularFont);
            AddRow(appTable, "Status", appData?.Status, boldFont, regularFont);

            AddRow(appTable, "Number of Years", appData?.NoOfYears.ToString(), boldFont, regularFont);
            AddRow(appTable, "Valid From", appData?.ValidFrom.ToString("dd/MM/yyyy"), boldFont, regularFont);
            AddRow(appTable, "Valid To", appData?.ValidTo.ToString("dd/MM/yyyy"), boldFont, regularFont);

            AddRow(appTable, "Place", appData?.Place, boldFont, regularFont);
            AddRow(appTable, "Application Date", appData?.Date.ToString("dd/MM/yyyy"), boldFont, regularFont);

            AddRow(appTable, "Amount", appData?.Amount.ToString("0.00"), boldFont, regularFont);

            AddRow(appTable, "Payment Completed", appData?.IsPaymentCompleted == true ? "Yes" : "No", boldFont, regularFont);
            AddRow(appTable, "Manager E-Sign Completed", appData?.IsESignCompletedManager == true ? "Yes" : "No", boldFont, regularFont);
            AddRow(appTable, "Occupier E-Sign Completed", appData?.IsESignCompletedOccupier == true ? "Yes" : "No", boldFont, regularFont);

            AddRow(appTable, "PRN Number", appData?.ESignPrnNumber, boldFont, regularFont);

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

            var licenseReg = await _context.FactoryLicenses
                .FirstOrDefaultAsync(x => x.Id == dto.FactoryLicense.Id);
            if (licenseReg != null)
            {
                licenseReg.ApplicationPDFUrl = fileUrl;
                await _context.SaveChangesAsync();
            }

            return fileUrl;
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