using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
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
using RajFabAPI.Models.FactoryModels;
using RajFabAPI.Services.Interface;
using static RajFabAPI.Constants.AppConstants;
using ImageDataFactory = iText.IO.Image.ImageDataFactory;
using PdfDocument = iText.Kernel.Pdf.PdfDocument;
using PdfFont = iText.Kernel.Font.PdfFont;
using PdfImage = iText.Layout.Element.Image;
using PdfWriter = iText.Kernel.Pdf.PdfWriter;
using PdfCell = iText.Layout.Element.Cell;
using PdfDoc = iText.Layout.Document;
using PdfTable = iText.Layout.Element.Table;
using Text = iText.Layout.Element.Text;
using System.Text.Json;

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
                MapApprovalDetails = await _context.FactoryMapApprovals
                    .Where(m => m.FactoryRegistrationNumber == factoryLicense.FactoryRegistrationNumber
                            && m.Status == "Approved")
                    .OrderByDescending(m => m.Version)
                    .FirstOrDefaultAsync(),
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
                    NoOfYears = dto.NoOfYears ?? 1,
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
                    Type = string.IsNullOrEmpty(type) ? "new" : type,
                    WorkersProposedMale = dto.WorkersProposedMale,
                    WorkersProposedFemale = dto.WorkersProposedFemale,
                    WorkersProposedTransgender = dto.WorkersProposedTransgender,
                    WorkersLastYearMale = dto.WorkersLastYearMale,
                    WorkersLastYearFemale = dto.WorkersLastYearFemale,
                    WorkersLastYearTransgender = dto.WorkersLastYearTransgender,
                    WorkersOrdinaryMale = dto.WorkersOrdinaryMale,
                    WorkersOrdinaryFemale = dto.WorkersOrdinaryFemale,
                    WorkersOrdinaryTransgender = dto.WorkersOrdinaryTransgender,
                    SanctionedLoad = dto.SanctionedLoad,
                    SanctionedLoadUnit = dto.SanctionedLoadUnit,
                    ManufacturingProcessLast12Months = dto.ManufacturingProcessLast12Months,
                    ManufacturingProcessNext12Months = dto.ManufacturingProcessNext12Months,
                    DateOfStartProduction = dto.DateOfStartProduction,
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
            license.NoOfYears = dto.NoOfYears ?? license.NoOfYears;
            license.ValidFrom = dto.ValidFrom;
            license.ValidTo = dto.ValidTo;
            license.Place = dto.Place;
            license.Status = "Pending";
            license.Date = dto.Date;
            license.ManagerSignature = dto.ManagerSignature;
            license.OccupierSignature = dto.OccupierSignature;
            license.AuthorisedSignature = dto.AuthorisedSignature;
            license.WorkersProposedMale = dto.WorkersProposedMale;
            license.WorkersProposedFemale = dto.WorkersProposedFemale;
            license.WorkersProposedTransgender = dto.WorkersProposedTransgender;
            license.WorkersLastYearMale = dto.WorkersLastYearMale;
            license.WorkersLastYearFemale = dto.WorkersLastYearFemale;
            license.WorkersLastYearTransgender = dto.WorkersLastYearTransgender;
            license.WorkersOrdinaryMale = dto.WorkersOrdinaryMale;
            license.WorkersOrdinaryFemale = dto.WorkersOrdinaryFemale;
            license.WorkersOrdinaryTransgender = dto.WorkersOrdinaryTransgender;
            license.SanctionedLoad = dto.SanctionedLoad;
            license.SanctionedLoadUnit = dto.SanctionedLoadUnit;
            license.ManufacturingProcessLast12Months = dto.ManufacturingProcessLast12Months;
            license.ManufacturingProcessNext12Months = dto.ManufacturingProcessNext12Months;
            license.DateOfStartProduction = dto.DateOfStartProduction;
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

            var entityData = await _context.Set<EstablishmentEntityMapping>()
                            .FirstOrDefaultAsync(ed => ed.EstablishmentRegistrationId == estReg.EstablishmentRegistrationId);

            if (entityData == null)
            {
                return license;
            }

            FactoryDetail? factorydata = null;

            if (entityData.EntityType == "Factory")
            {
                factorydata = await _context.Set<FactoryDetail>()
                    .FirstOrDefaultAsync(f => f.Id == entityData.EntityId);
            }

            if (factorydata == null)
            {
                return license;
            }

            if (!Guid.TryParse(factorydata.SubDivisionId, out Guid subDivisionGuid))
            {
                return license;
            }

            int totalWorkers = factorydata.NumberOfWorker ?? 0;


            var workerRange = await _context.Set<WorkerRange>()
                .FirstOrDefaultAsync(wr => totalWorkers >= wr.MinWorkers && totalWorkers <= wr.MaxWorkers);

            var factoryType = await _context.FactoryTypes.FirstOrDefaultAsync(x => x.Name == "Not Applicable");
            var factoryCategory = await _context.Set<FactoryCategory>()
                .FirstOrDefaultAsync(fc => fc.WorkerRangeId == workerRange.Id && fc.FactoryTypeId == factoryType.Id);

            //if (!Guid.TryParse(estDetail.SubDivisionId, out var subDivisionGuid))
            //    return license;

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
            var estDetail = dto.EstFullDetails?.EstablishmentDetail;
            var occupier = dto.EstFullDetails?.Factory?.EmployerDetail;
            var manager = dto.EstFullDetails?.Factory?.ManagerDetail;
            var factory = dto.EstFullDetails?.Factory;
            var MapApprovalDetails = dto.MapApprovalDetails;
            var premiseOwner = string.IsNullOrWhiteSpace(dto.MapApprovalDetails.PremiseOwnerDetails)
                    ? null
                    : JsonSerializer.Deserialize<OccupierDetailsModel>(dto.MapApprovalDetails.PremiseOwnerDetails);

            var folderName = isCertificate ? "certificates" : "factory-license-forms";
            var fileName = $"factory_license_{appData.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

            var uploadPath = Path.Combine(_environment.WebRootPath, folderName);
            _ = Directory.CreateDirectory(uploadPath);
            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/{folderName}/{fileName}";

            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE,
                new MapApprovalPageBorderAndFooterEventHandler(boldFont, regularFont, appData.Date.ToString("dd/MM/yyyy")));
            using var document = new PdfDoc(pdf);
            document.SetMargins(40, 40, 130, 40);

            // ─────────────────────────────────────────
            // HEADER (centered) — matches GenerateEstablishmentPdf
            // ─────────────────────────────────────────
            _ = document.Add(new Paragraph("Form-5")
            .SetFont(boldFont).SetFontSize(13)
            .SetTextAlignment(TextAlignment.CENTER));

            _ = document.Add(new Paragraph("(See rule 6(1), 12, 13(3), 16(2) and 17(2))")
                .SetFont(regularFont).SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER));

            _ = document.Add(new Paragraph(
                    "Application for License/Renewal of License \n / Amendment to License/Transfer of License of Factory")
                .SetFont(boldFont).SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(8));

            // ─────────────────────────────────────────
            // Licence Number (left) + Date (right)
            // ─────────────────────────────────────────
            var licNoRow = new PdfTable(new float[] { 1f, 1f }).UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER).SetMarginBottom(4);
            _ = licNoRow.AddCell(new PdfCell()
                .Add(new Paragraph($"Factory Licence Application No.:  {appData.FactoryLicenseNumber ?? "-"}")
                    .SetFont(boldFont).SetFontSize(9))
                .SetBorder(Border.NO_BORDER).SetPaddingLeft(4));
            _ = licNoRow.AddCell(new PdfCell()
                .Add(new Paragraph($"Date:  {appData.Date:dd/MM/yyyy}")
                    .SetFont(boldFont).SetFontSize(9)
                    .SetTextAlignment(TextAlignment.RIGHT))
                .SetBorder(Border.NO_BORDER));
            _ = document.Add(licNoRow);

            var licTable = new PdfTable(new float[] { 130f, 130f, 130f, 130f })
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER);

            _ = document.Add(new Paragraph("1.    Period of Licence")
            .SetFont(boldFont).SetFontSize(10).SetMarginBottom(2));
            var licItems = new[]
            {
                ("1. Period of Licence:", $"From {appData.ValidFrom:dd/MM/yyyy}  To  {appData.ValidTo:dd/MM/yyyy}  ({appData.NoOfYears} Year(s))"),
            };

            foreach (var (label, value) in licItems)
            {
                licTable.AddCell(new PdfCell(1, 2)
                    .Add(new Paragraph(label).SetFont(boldFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER).SetPaddingLeft(4));
                licTable.AddCell(new PdfCell(1, 2)
                    .Add(new Paragraph(value).SetFont(regularFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER));
            }
            document.Add(licTable);
            document.Add(new Paragraph("\n").SetFontSize(4));

            var genTable = new PdfTable(new float[] { 130f, 130f, 130f, 130f })
               .UseAllAvailableWidth()
               .SetBorder(Border.NO_BORDER);

            _ = document.Add(new Paragraph("2.    General Information")
            .SetFont(boldFont).SetFontSize(10).SetMarginBottom(2));

            var genItems = new[]
            {
                ("2a. Name of the Factory:", estDetail?.Name ?? "-"),
                ("2b. Factory Registration Number:", appData.FactoryRegistrationNumber ?? "-"),
            };
            foreach (var (label, value) in genItems)
            {
                genTable.AddCell(new PdfCell(1, 2)
                    .Add(new Paragraph(label).SetFont(boldFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER).SetPaddingLeft(4));
                genTable.AddCell(new PdfCell(1, 2)
                    .Add(new Paragraph(value).SetFont(regularFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER));
            }

            document.Add(genTable);
            document.Add(new Paragraph("\n").SetFontSize(4));

            var addressTable = new PdfTable(new float[] { 130f, 130f, 130f, 130f })
               .UseAllAvailableWidth()
               .SetBorder(Border.NO_BORDER);

            _ = document.Add(new Paragraph("3.    Address and Contact Information Of Factory")
            .SetFont(boldFont).SetFontSize(10).SetMarginBottom(2));

            var addressItems = new[]
            {
                ("3. Full Postal Address along with Pincode:", LicFormatAddress(
                    factory?.AddressLine1, factory?.AddressLine2,
                    factory?.Area, factory?.TehsilName, factory?.SubDivisionName, factory?.DistrictName, factory?.Pincode)),
                ("  Contact Number:", factory?.Mobile ?? estDetail?.Mobile ?? "-"),
                ("  E-Mail ID:", factory?.Email ?? estDetail?.Email ?? "-"),
            };
            foreach (var (label, value) in addressItems)
            {
                addressTable.AddCell(new PdfCell(1, 2)
                    .Add(new Paragraph(label).SetFont(boldFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER).SetPaddingLeft(4));
                addressTable.AddCell(new PdfCell(1, 2)
                    .Add(new Paragraph(value).SetFont(regularFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER));
            }

            document.Add(addressTable);
            document.Add(new Paragraph("\n").SetFontSize(4));

            var natureTable = new PdfTable(new float[] { 130f, 130f, 130f, 130f })
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER);

            _ = document.Add(new Paragraph("4.    Nature of Manufacturing Processes")
            .SetFont(boldFont).SetFontSize(10).SetMarginBottom(2));

            var natureItems = new[]
            {
                ("4a. Date Of Start Production:", "-"),
                ("4b. Manufacturing Process carried on in factory\n in last 12 months:", "-"),
                ("4c. Manufacturing Process to be carried on in factory during the next 12 months:", "-"),
            };
            foreach (var (label, value) in natureItems)
            {
                natureTable.AddCell(new PdfCell(1, 2)
                    .Add(new Paragraph(label).SetFont(boldFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER).SetPaddingLeft(4));
                natureTable.AddCell(new PdfCell(1, 2)
                    .Add(new Paragraph(value).SetFont(regularFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER));
            }

            document.Add(natureTable);
            document.Add(new Paragraph("\n").SetFontSize(4));

            // ═══════════════════════════════════════════════════════════════════
            // SECTION 5 — Maximum Workers (table)
            // ═══════════════════════════════════════════════════════════════════
            document.Add(new Paragraph()
                .Add(new Text("5.").SetFont(boldFont).SetFontSize(9))
                .Add(new Text("Workers employed:").SetFont(boldFont).SetFontSize(9))
                .SetMarginBottom(2f));

            var workerTable = new PdfTable(new float[] { 3f, 1f, 1f, 1f, 1f })
                .UseAllAvailableWidth()
                .SetMarginLeft(30f)
                .SetMarginBottom(4f);

            void WCell(PdfTable tbl, string txt, bool hdr = false, bool center = false)
            {
                var p = new Paragraph(txt).SetFont(hdr ? boldFont : regularFont).SetFontSize(8.5f).SetMargin(0);
                if (center) p.SetTextAlignment(TextAlignment.CENTER);
                tbl.AddCell(new PdfCell().Add(p).SetPadding(3f));
            }

            WCell(workerTable, "", hdr: true);
            WCell(workerTable, "Male", hdr: true, center: true);
            WCell(workerTable, "Female", hdr: true, center: true);
            WCell(workerTable, "Transgender", hdr: true, center: true);
            WCell(workerTable, "Total", hdr: true, center: true);

            // Adults
            int totalWorkers = estDetail?.TotalNumberOfEmployee ?? 0;
            WCell(workerTable, "Maximum number of workers proposed to be employed during the year", hdr: true);
            WCell(workerTable, "-", center: true);
            WCell(workerTable, "-", center: true);
            WCell(workerTable, "-", center: true);
            WCell(workerTable, totalWorkers > 0 ? totalWorkers.ToString() : "-", center: true);

            // Adolescents
            WCell(workerTable, "Maximum number of workers employed during the last twelve months on any day", hdr: true);
            WCell(workerTable, "-", center: true);
            WCell(workerTable, "-", center: true);
            WCell(workerTable, "-", center: true);
            WCell(workerTable, "-", center: true);

            // Total
            WCell(workerTable, "Maximum number of workers ordinarily employed in the factory", hdr: true);
            WCell(workerTable, "-", center: true);
            WCell(workerTable, "-", center: true);
            WCell(workerTable, "-", center: true);
            WCell(workerTable, totalWorkers > 0 ? totalWorkers.ToString() : "-", center: true);

            document.Add(workerTable);

            var powerTable = new PdfTable(new float[] { 130f, 130f, 130f, 130f })
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER);

            _ = document.Add(new Paragraph("6.    Power Installed")
            .SetFont(boldFont).SetFontSize(10).SetMarginBottom(2));
            var powerItems = new[]
            {
                ("Total rated Horse Power or amount\nof Power(installed or to be installed or used)\nwhichever is maximum :", factory.SanctionedLoad.HasValue ? $"{factory.SanctionedLoad} {factory.SanctionedLoadUnit}" : "-"),
                ("K.No. of consumer:", "-"),
            };

            foreach (var (label, value) in powerItems)
            {
                powerTable.AddCell(new PdfCell(1, 2)
                    .Add(new Paragraph(label).SetFont(boldFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER).SetPaddingLeft(4));
                powerTable.AddCell(new PdfCell(1, 2)
                    .Add(new Paragraph(value).SetFont(regularFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER));
            }
            document.Add(powerTable);
            document.Add(new Paragraph("\n").SetFontSize(4));

            var managerTable = new PdfTable(new float[] { 130f, 130f, 130f, 130f })
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER);

            _ = document.Add(new Paragraph("7.    Particulars of Factory Manager")
            .SetFont(boldFont).SetFontSize(10).SetMarginBottom(2));

            var managerItems = new[]
            {
                ("Name:", manager.Name ??"-"),
                ("Address:", LicFormatAddress(
                    manager?.AddressLine1, manager?.AddressLine2,
                    manager?.Area, manager?.Tehsil, manager?.District, manager?.Pincode) ?? "-"),
                ("Contact No. and E-Mail ID:", $"{manager?.Mobile}, {manager?.Email}"  ?? "-"),
            };
            foreach (var (label, value) in managerItems)
            {
                managerTable.AddCell(new PdfCell(1, 2)
                    .Add(new Paragraph(label).SetFont(boldFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER).SetPaddingLeft(4));
                managerTable.AddCell(new PdfCell(1, 2)
                    .Add(new Paragraph(value).SetFont(regularFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER));
            }

            document.Add(managerTable);
            document.Add(new Paragraph("\n").SetFontSize(4));

            var occupierTable = new PdfTable(new float[] { 130f, 130f, 130f, 130f })
               .UseAllAvailableWidth()
               .SetBorder(Border.NO_BORDER);

            _ = document.Add(new Paragraph("8.    Particulars of Occupier")
            .SetFont(boldFont).SetFontSize(10).SetMarginBottom(2));

            var occupierItems = new[]
            {
                ("Name:", occupier.Name ??"-"),
                ("Address:", LicFormatAddress(
                    occupier?.AddressLine1, occupier?.AddressLine2,
                    occupier?.Area, occupier?.Tehsil, occupier?.District, occupier?.Pincode) ?? "-"),
                ("Contact No. and E-Mail ID:", $"{occupier?.Mobile}, {occupier?.Email}"  ?? "-"),
            };
            foreach (var (label, value) in occupierItems)
            {
                occupierTable.AddCell(new PdfCell(1, 2)
                    .Add(new Paragraph(label).SetFont(boldFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER).SetPaddingLeft(4));
                occupierTable.AddCell(new PdfCell(1, 2)
                    .Add(new Paragraph(value).SetFont(regularFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER));
            }

            document.Add(occupierTable);
            document.Add(new Paragraph("\n").SetFontSize(4));

            var land9aTable = new PdfTable(new float[] { 130f, 130f, 130f, 130f })
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER);

            _ = document.Add(new Paragraph("9.    Land & Building")
            .SetFont(boldFont).SetFontSize(10).SetMarginBottom(2));

            _ = document.Add(new Paragraph("9a.    Owner of the premises")
            .SetFont(boldFont).SetFontSize(10).SetMarginBottom(2));
            var land9aItems = new[]
            {
                ("Name:", premiseOwner?.name ??"-"),
                ("Address:", LicFormatAddress(
                    premiseOwner?.addressLine1, premiseOwner?.addressLine2,
                    premiseOwner?.area, premiseOwner?.tehsil, premiseOwner?.district, premiseOwner?.pincode) ?? "-"),
                ("Contact No. and E-Mail ID:", $"{premiseOwner?.mobile}, {premiseOwner?.email}"  ?? "-"),
            };
            foreach (var (label, value) in land9aItems)
            {
                land9aTable.AddCell(new PdfCell(1, 2)
                    .Add(new Paragraph(label).SetFont(boldFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER).SetPaddingLeft(4));
                land9aTable.AddCell(new PdfCell(1, 2)
                    .Add(new Paragraph(value).SetFont(regularFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER));
            }

            document.Add(land9aTable);

            var land9bTable = new PdfTable(new float[] { 130f, 130f, 130f, 130f })
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER);

            _ = document.Add(new Paragraph("9b.    Plan Approval Details")
           .SetFont(boldFont).SetFontSize(10).SetMarginBottom(2));
            var land9bItems = new[]
            {
                ("Plan No.:", MapApprovalDetails.AcknowledgementNumber ?? "-"),
                ("Date of Approval:", MapApprovalDetails?.UpdatedAt.ToString("dd/MM/yyyy") ?? "-"),
            };
            foreach (var (label, value) in land9bItems)
            {
                land9bTable.AddCell(new PdfCell(1, 2)
                    .Add(new Paragraph(label).SetFont(boldFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER).SetPaddingLeft(4));
                land9bTable.AddCell(new PdfCell(1, 2)
                    .Add(new Paragraph(value).SetFont(regularFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER));
            }
            document.Add(land9bTable);
            document.Add(new Paragraph("\n").SetFontSize(4));

            _ = document.Add(new Paragraph("10.    Other Information")
            .SetFont(boldFont).SetFontSize(10).SetMarginBottom(2));

            _ = document.Add(new Paragraph("10a.    Information of Manufacturing Process")
            .SetFont(boldFont).SetFontSize(10).SetMarginBottom(2));
            var land10aTable = new PdfTable(new float[] { 130f, 130f, 130f, 130f })
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER);
            var land10aItems = new[]
            {
                ("Activity as per National Industrial Classification:", factory.ActivityAsPerNIC),
                ("Details of Selected NIC Code:", factory.NICCodeDetail),
            };
            foreach (var (label, value) in land10aItems)
            {
                land10aTable.AddCell(new PdfCell(1, 2)
                    .Add(new Paragraph(label).SetFont(boldFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER).SetPaddingLeft(4));
                land10aTable.AddCell(new PdfCell(1, 2)
                    .Add(new Paragraph(value).SetFont(regularFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER));
            }
            document.Add(land10aTable);

            _ = document.Add(new Paragraph($"10b.    Identification of the factory(LIN): {factory.IdentificationOfEstablishment ?? "-"}")
           .SetFont(boldFont).SetFontSize(10).SetMarginBottom(2));

            document.Add(new Paragraph("NOTE:").SetFont(boldFont).SetFontSize(10).SetMarginBottom(3));
            document.Add(new Paragraph("     a.  In case of any change in the above information, Department shall be informed in writing.")
                .SetFont(regularFont).SetFontSize(9).SetMarginBottom(2));
            document.Add(new Paragraph("     b.  Seal bearing \"Authorised Signatory\" shall not be used on any document.")
                .SetFont(regularFont).SetFontSize(9).SetMarginBottom(12));

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

        // ─────────────────────────────────────────────────────────────────────────────
        // Generate Objection Letter — Factory License
        // ─────────────────────────────────────────────────────────────────────────────
        public async Task<string> GenerateObjectionLetter(LicenseObjectionLetterDto dto, string licenseId)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var fileName = $"objection_license_{licenseId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
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

            var rawLoad = dto.SanctionLoad ?? 0;
            var loadUnit = (dto.SanctionLoadUnit ?? "HP").ToUpper();

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

            if (dto.MaxWorkers < 20)
            {
                Type = "Section 85";
            }
            else if (dto.MaxWorkers > 40 && power == 0)
            {
                Type = "2 (1)(w)(ii)";
            }
            else if (dto.MaxWorkers >= 20 && power > 0)
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
            // License No  +  Dated
            // ═════════════════════════════════════════════════════════════════════════
            var topRow = new PdfTable(new float[] { 1f, 1f })
                .UseAllAvailableWidth().SetBorder(Border.NO_BORDER).SetMarginBottom(12f);

            _ = topRow.AddCell(new PdfCell()
                .Add(new Paragraph($"License Application No.:-  {dto.LicenseNumber ?? "-"}")
                    .SetFont(boldFont).SetFontSize(12))
                .SetBorder(Border.NO_BORDER));

            _ = topRow.AddCell(new PdfCell()
                .Add(new Paragraph($"Dated:-  {dto.Date:dd/MM/yyyy}")
                    .SetFont(boldFont).SetFontSize(12)
                    .SetTextAlignment(TextAlignment.RIGHT))
                .SetBorder(Border.NO_BORDER));

            _ = document.Add(topRow);

            // ═════════════════════════════════════════════════════════════════════════
            // Factory name + address
            // ═════════════════════════════════════════════════════════════════════════
            if (!string.IsNullOrWhiteSpace(dto.EstablishmentName))
            {
                _ = document.Add(new Paragraph(dto.EstablishmentName)
                    .SetFont(boldFont).SetFontSize(12)
                    .SetMarginBottom(1f));
            }
            if (!string.IsNullOrWhiteSpace(dto.FactoryAddress))
            {
                _ = document.Add(new Paragraph(dto.FactoryAddress)
                    .SetFont(regularFont).SetFontSize(12)
                    .SetMarginBottom(8f));
            }

            // ═════════════════════════════════════════════════════════════════════════
            // Sub:-
            // ═════════════════════════════════════════════════════════════════════════
            var subPara = new Paragraph();
            subPara.Add(new Text("Sub:- ").SetFont(boldFont).SetFontSize(12));
            subPara.Add(new Text("Regarding issuance of your Factory License").SetFont(regularFont).SetFontSize(12));
            _ = document.Add(subPara.SetMarginBottom(4f));

            // ═════════════════════════════════════════════════════════════════════════
            // Intro line
            // ═════════════════════════════════════════════════════════════════════════
            _ = document.Add(new Paragraph(
                    "The details of your factory as per application and documents are shown below:-")
                .SetFont(regularFont).SetFontSize(12)
                .SetMarginBottom(4f));

            // ═════════════════════════════════════════════════════════════════════════
            // Factory details table (red border)
            // ═════════════════════════════════════════════════════════════════════════
            var detailsTable = new PdfTable(new float[] { 200f, 1f })
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
            _ = detailsTable.AddCell(RedCell(dto.ManufacturingProcess ?? "-", regularFont));

            _ = detailsTable.AddCell(RedCell("Type", boldFont));
            _ = detailsTable.AddCell(RedCell(Type, regularFont));

            _ = detailsTable.AddCell(RedCell("Category", boldFont));
            _ = detailsTable.AddCell(RedCell(dto.FactoryTypeName ?? "-", regularFont));

            _ = detailsTable.AddCell(RedCell("Workers", boldFont));
            _ = detailsTable.AddCell(RedCell(dto.MaxWorkers?.ToString() ?? "-", regularFont));

            _ = document.Add(detailsTable);

            // ═════════════════════════════════════════════════════════════════════════
            // Objections heading
            // ═════════════════════════════════════════════════════════════════════════
            _ = document.Add(new Paragraph("Following objections are need to be removed related to your factory - ")
                .SetFont(regularFont).SetFontSize(12)
                .SetMarginBottom(12f));

            // ═════════════════════════════════════════════════════════════════════════
            // Numbered objections list
            // ═════════════════════════════════════════════════════════════════════════
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

            // Save URL to DB
            var license = await _context.FactoryLicenses
                .FirstOrDefaultAsync(x => x.Id == licenseId);
            if (license != null)
            {
                license.ObjectionLetterUrl = fileUrl;
                license.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return fileUrl;
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // Helper — convert base64 string or data-URI to bytes
        // ─────────────────────────────────────────────────────────────────────────────
        private static Task<byte[]?> DownloadImageAsync(string? source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return Task.FromResult<byte[]?>(null);
            try
            {
                if (source.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                {
                    var commaIndex = source.IndexOf(',');
                    if (commaIndex >= 0)
                        return Task.FromResult<byte[]?>(Convert.FromBase64String(source[(commaIndex + 1)..]));
                }
                return Task.FromResult<byte[]?>(Convert.FromBase64String(source));
            }
            catch
            {
                return Task.FromResult<byte[]?>(null);
            }
        }
        // ─────────────────────────────────────────────────────────────────────────────
        // Helpers for GenerateFactoryLicensePdf (mirrors EstablishmentRegistrationService)
        // ─────────────────────────────────────────────────────────────────────────────
        private static string LicFormatAddress(params string?[] parts) =>
            parts?.Where(p => !string.IsNullOrWhiteSpace(p)).Any() == true
                ? string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)))
                : "-";

        private static PdfTable BuildLicenseSectionTable(string[] headers, float[] widths, string[]? values, PdfFont boldFont, PdfFont regularFont)
        {
            var table = new PdfTable(widths)
                .UseAllAvailableWidth()
                .SetMarginLeft(24);

            // Header row
            foreach (var h in headers)
                table.AddCell(new PdfCell()
                    .Add(new Paragraph(h).SetFont(boldFont).SetFontSize(8))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBackgroundColor(new DeviceRgb(240, 240, 240))
                    .SetPadding(4));

            // Number row
            for (int i = 1; i <= headers.Length; i++)
                table.AddCell(new PdfCell()
                    .Add(new Paragraph(i.ToString()).SetFont(regularFont).SetFontSize(8))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(3));

            // Data row
            var dataValues = values ?? Enumerable.Repeat("-----", headers.Length).ToArray();
            foreach (var val in dataValues)
                table.AddCell(new PdfCell()
                    .Add(new Paragraph(string.IsNullOrWhiteSpace(val) ? "-" : val).SetFont(regularFont).SetFontSize(9))
                    .SetPadding(4)
                    .SetTextAlignment(TextAlignment.CENTER));

            return table;
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

        private sealed class MapApprovalPageBorderAndFooterEventHandler : AbstractPdfDocumentEventHandler
        {
            private readonly PdfFont _boldFont;
            private readonly PdfFont _regularFont;
            private readonly string _date;

            public MapApprovalPageBorderAndFooterEventHandler(PdfFont boldFont, PdfFont regularFont, string date)
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
                    manager.Add(new Paragraph("e-sign / Signature of\nManager")
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

    }
}