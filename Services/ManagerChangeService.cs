using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;
using static RajFabAPI.Constants.AppConstants;
using PdfCell = iText.Layout.Element.Cell;
using PdfDoc = iText.Layout.Document;
using PdfTable = iText.Layout.Element.Table;
using PdfWriter = iText.Kernel.Pdf.PdfWriter;

namespace RajFabAPI.Services
{  
    public class ManagerChangeService : IManagerChangeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaymentService _payment;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ManagerChangeService(ApplicationDbContext context, IWebHostEnvironment environment, IPaymentService payment,
            IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _payment = payment;
            _environment = environment;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
        }

        public string GenerateAcknowledgementNumber()
        {
            var year = DateTime.Now.Year;
            var sequence = DateTime.Now.Ticks.ToString().Substring(8, 6);
            return $"FMC-{year}{sequence}";
        }

        public async Task<IEnumerable<ManagerChangeGetResponseDto>> GetAllAsync(Guid userId)
        {
            var moduleId = await _context.Modules
                .Where(m => m.Name == ApplicationTypeNames.ManagerChange)
                .Select(m => m.Id)
                .SingleAsync();

            var appRegs = await _context.ApplicationRegistrations
                .AsNoTracking()
                .Where(a => a.UserId == userId && a.ModuleId == moduleId)
                .ToListAsync();

            var query = from appReg in appRegs
                        let appGuid = Guid.Parse(appReg.ApplicationId)
                        join mc in _context.ManagerChanges.AsNoTracking()
                            on appGuid equals mc.Id
                        join oldMgr in _context.PersonDetails.AsNoTracking()
                            on mc.OldManagerId equals oldMgr.Id into oldMgrJoin
                        from oldManager in oldMgrJoin.DefaultIfEmpty()
                        join newMgr in _context.PersonDetails.AsNoTracking()
                            on mc.NewManagerId equals newMgr.Id into newMgrJoin
                        from newManager in newMgrJoin.DefaultIfEmpty()
                        join factoryReg in _context.EstablishmentRegistrations.AsNoTracking()
                            on mc.FactoryRegistrationId.ToString().ToUpper() equals factoryReg.EstablishmentRegistrationId into factoryJoin
                        from factory in factoryJoin.DefaultIfEmpty()
                        join est in _context.EstablishmentDetails.AsNoTracking()
                            on factory.EstablishmentDetailId equals est.Id into estJoin
                        from establishment in estJoin.DefaultIfEmpty()
                        join area in _context.Areas.AsNoTracking()
                            on Guid.Parse(establishment.SubDivisionId) equals area.Id into areaJoin
                        from areaDetail in areaJoin.DefaultIfEmpty()
                        join district in _context.Districts.AsNoTracking()
                            on areaDetail.DistrictId equals district.Id into districtJoin
                        from districtDetail in districtJoin.DefaultIfEmpty()
                        join division in _context.Divisions.AsNoTracking()
                            on districtDetail.DivisionId equals division.Id into divisionJoin
                        from divisionDetail in divisionJoin.DefaultIfEmpty()
                        select new ManagerChangeGetResponseDto
                        {
                            ManagerChangeId = mc.Id,
                            AcknowledgementNumber = mc.AcknowledgementNumber,
                            Status = mc.Status,
                            DateOfAppointment = mc.DateOfAppointment,
                            SignatureOfOccupier = mc.SignatureofOccupier,
                            SignatureOfNewManager = mc.SignatureOfNewManager,
                            SubmittedDate = mc.CreatedAt,
                            OldManager = oldManager == null ? null : new PersonBasicDto
                            {
                                Id = oldManager.Id,
                                Name = oldManager.Name,
                                AddressLine1 = oldManager.AddressLine1,
                                AddressLine2 = oldManager.AddressLine2,
                                District = oldManager.District,
                                Tehsil = oldManager.Tehsil,
                                Area = oldManager.Area,
                                Pincode = oldManager.Pincode,
                                Email = oldManager.Email,
                                Telephone = oldManager.Telephone,
                                Mobile = oldManager.Mobile,
                            },

                            NewManager = newManager == null ? null : new PersonBasicDto
                            {
                                Id = newManager.Id,
                                Name = newManager.Name,
                                AddressLine1 = newManager.AddressLine1,
                                AddressLine2 = newManager.AddressLine2,
                                District = newManager.District,
                                Tehsil = newManager.Tehsil,
                                Area = newManager.Area,
                                Pincode = newManager.Pincode,
                                Email = newManager.Email,
                                Telephone = newManager.Telephone,
                                Mobile = newManager.Mobile,
                            },

                            Factory = establishment == null ? null : new FactoryBasicDto
                            {
                                FactoryRegistrationId = Guid.Parse(factory.EstablishmentRegistrationId),
                                FactoryName = establishment.EstablishmentName,
                                AddressLine1 = establishment.AddressLine1,
                                AddressLine2 = establishment.AddressLine2,
                                Pincode = establishment.Pincode,
                                Area = areaDetail != null ? areaDetail.Name : null,
                                DistrictName = districtDetail != null ? districtDetail.Name : null,
                            }
                        };

            return query.ToList();
        }

        public async Task<ManagerChangeResponseDto> CreateAsync(CreateManagerChangeRequestDto dto, Guid userId)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1?? Insert New Manager
                var newManager = new PersonDetail
                {
                    Id = Guid.NewGuid(),
                    Role = "ManagerOrAgent",
                    Name = dto.NewManagerName,
                    RelationType = dto.NewManagerRelation,
                    RelativeName = dto.NewManagerFatherOrHusbandName,
                    AddressLine1 = dto.NewManagerAddressLine1,
                    AddressLine2 = dto.NewManagerAddressLine2,
                    District = dto.NewManagerDistrict,
                    Tehsil = dto.NewManagerTehsil,
                    Area = dto.NewManagerArea,
                    Pincode = dto.NewManagerPincode,
                    Email = dto.NewManagerEmail,
                    Telephone = dto.NewManagerTelephone,
                    Mobile = dto.NewManagerMobile,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.PersonDetails.Add(newManager);
                await _context.SaveChangesAsync();

                // 2?? Insert ManagerChange record
                var managerChange = new ManagerChange
                {
                    Id = Guid.NewGuid(),
                    FactoryRegistrationId = dto.FactoryRegistrationId,
                    OldManagerId = dto.OldManagerId,
                    NewManagerId = newManager.Id,
                    AcknowledgementNumber = GenerateAcknowledgementNumber(),
                    SignatureofOccupier = dto.SignatureofOccupier,
                    SignatureOfNewManager = dto.SignatureOfNewManager,
                    DateOfAppointment = dto.NewManagerDateOfAppointment,
                    Version = 1.0m,  // default version
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsActive = true
                };

                _context.ManagerChanges.Add(managerChange);
                await _context.SaveChangesAsync();

                // 3?? Create ApplicationRegistration
                var module = await _context.Set<FormModule>()
                    .FirstAsync(m => m.Name == ApplicationTypeNames.ManagerChange);

                var EstablishmentDetailId = await _context.Set<EstablishmentRegistration>()
                    .Where(m => m.EstablishmentRegistrationId == dto.FactoryRegistrationId.ToString())
                    .Select(m => m.EstablishmentDetailId)
                    .FirstOrDefaultAsync();

                var appReg = new ApplicationRegistration
                {
                    Id = Guid.NewGuid().ToString(),
                    ModuleId = module.Id,
                    UserId = userId,
                    ApplicationId = managerChange.Id.ToString(),
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                _context.ApplicationRegistrations.Add(appReg);
                await _context.SaveChangesAsync();

                // Workflow / ApprovalRequest is created AFTER both eSigns complete
                // (occupier first, then manager) via UpdateApplicationESignData.
                // Do NOT create it here to avoid duplicate entries.

                var User = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                var html = await _payment.ActionRequestPaymentRPP(1000, User.FullName, User.Mobile, User.Email, User.Username, "4157FE34BBAE3A958D8F58CCBFAD7", "UWf6a7cDCP", managerChange.AcknowledgementNumber, module.Id.ToString(), userId.ToString()) ?? "";

                await tx.CommitAsync();
                //return html;
                return new ManagerChangeResponseDto
                {
                    ManagerChangeId = managerChange.Id,
                    NewManagerId = newManager.Id,
                    AcknowledgementNumber = managerChange.AcknowledgementNumber,
                    Message = "Application created successfully."
                };
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<ManagerChangeResponseDto> UpdateAsync(Guid managerChangeId, UpdateManagerChangeRequestDto dto)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1?? Get existing ManagerChange
                var managerChange = await _context.ManagerChanges
                    .FirstOrDefaultAsync(mc => mc.Id == managerChangeId);

                if (managerChange == null)
                    throw new Exception("Manager change application not found");

                // 2?? Update New Manager details if provided
                var newManager = await _context.PersonDetails
                    .FirstOrDefaultAsync(p => p.Id == managerChange.NewManagerId);

                if (newManager == null)
                    throw new Exception("New Manager details not found");

                newManager.Name = dto.NewManagerName ?? newManager.Name;
                newManager.RelationType = dto.NewManagerRelation ?? newManager.RelationType;
                newManager.RelativeName = dto.NewManagerFatherOrHusbandName ?? newManager.RelativeName;
                newManager.AddressLine1 = dto.NewManagerAddressLine1 ?? newManager.AddressLine1;
                newManager.AddressLine2 = dto.NewManagerAddressLine2 ?? newManager.AddressLine2;
                newManager.District = dto.NewManagerDistrict ?? newManager.District;
                newManager.Tehsil = dto.NewManagerState ?? newManager.Tehsil;
                newManager.Area = dto.NewManagerCity ?? newManager.Area;
                newManager.Pincode = dto.NewManagerPincode ?? newManager.Pincode;
                newManager.Email = dto.NewManagerEmail ?? newManager.Email;
                newManager.Telephone = dto.NewManagerMobile ?? newManager.Telephone ;
                newManager.Mobile = dto.NewManagerMobile ?? newManager.Mobile;
                newManager.UpdatedAt = DateTime.Now;

                _context.PersonDetails.Update(newManager);
                await _context.SaveChangesAsync();

                // 3?? Update ManagerChange fields
                if (dto.OldManagerId.HasValue && dto.OldManagerId != Guid.Empty)
                    managerChange.OldManagerId = dto.OldManagerId.Value;

                managerChange.DateOfAppointment = dto.NewManagerDateOfAppointment ?? managerChange.DateOfAppointment;
                managerChange.Status = dto.Status ?? managerChange.Status;
                managerChange.SignatureofOccupier = dto.SignatureofOccupier ?? managerChange.SignatureofOccupier;
                managerChange.SignatureOfNewManager = dto.SignatureOfNewManager ?? managerChange.SignatureOfNewManager;

                // Increment version on update
                managerChange.Version += 0.1m;
                managerChange.UpdatedAt = DateTime.Now;

                _context.ManagerChanges.Update(managerChange);
                await _context.SaveChangesAsync();

                // 4?? Optional: Handle workflow if status changed
                if (!string.IsNullOrEmpty(dto.Status))
                {
                    var appReg = await _context.ApplicationRegistrations
                        .FirstOrDefaultAsync(ar => ar.ApplicationId == managerChange.Id.ToString());

                    if (appReg != null)
                    {
                        var module = await _context.Modules
                            .FirstOrDefaultAsync(m => m.Name == ApplicationTypeNames.ManagerChange);

                        var establishmentDetailId = await _context.EstablishmentRegistrations
                            .Where(er => er.EstablishmentRegistrationId == managerChange.FactoryRegistrationId.ToString())
                            .Select(er => er.EstablishmentDetailId)
                            .FirstOrDefaultAsync();

                        var areaId = await _context.EstablishmentDetails
                            .Where(ed => ed.Id == establishmentDetailId)
                            .Select(ed => ed.SubDivisionId)
                            .FirstOrDefaultAsync();

                        if (Guid.TryParse(areaId, out var parsedAreaId))
                        {
                            var officeApplicationArea = await _context.OfficeApplicationAreas
                                .FirstOrDefaultAsync(oaa => oaa.CityId == parsedAreaId);

                            if (officeApplicationArea != null)
                            {
                                var factoryCategoryId = Guid.Parse("EB857143-2FBB-4C6E-88F8-888C3D6DB671");

                                var workflow = await _context.ApplicationWorkFlows
                                    .FirstOrDefaultAsync(wf =>
                                        wf.ModuleId == module.Id &&
                                        wf.FactoryCategoryId == factoryCategoryId &&
                                        wf.OfficeId == officeApplicationArea.OfficeId);

                                var workflowId = workflow?.Id ?? Guid.Empty;

                                var workflowLevel = await _context.ApplicationWorkFlowLevels
                                    .Where(wfl => wfl.ApplicationWorkFlowId == workflowId)
                                    .OrderBy(wfl => wfl.LevelNumber)
                                    .FirstOrDefaultAsync();

                                if (workflowLevel != null)
                                {
                                    var existingApproval = await _context.ApplicationApprovalRequests
                                        .FirstOrDefaultAsync(ar =>
                                            ar.ApplicationRegistrationId == appReg.Id &&
                                            ar.ApplicationWorkFlowLevelId == workflowLevel.Id);

                                    if (existingApproval == null)
                                    {
                                        _context.ApplicationApprovalRequests.Add(new ApplicationApprovalRequest
                                        {
                                            ModuleId = module.Id,
                                            ApplicationRegistrationId = appReg.Id,
                                            ApplicationWorkFlowLevelId = workflowLevel.Id,
                                            Status = "Pending",
                                            CreatedDate = DateTime.Now,
                                            UpdatedDate = DateTime.Now
                                        });

                                        await _context.SaveChangesAsync();
                                    }
                                }
                            }
                        }
                    }
                }

                await tx.CommitAsync();

                return new ManagerChangeResponseDto
                {
                    ManagerChangeId = managerChange.Id,
                    NewManagerId = newManager.Id,
                    Message = "Manager change updated successfully.",
                    AcknowledgementNumber = managerChange.AcknowledgementNumber
                };
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<ManagerChangeGetResponseDto> GetByIdAsync(Guid managerChangeId)
        {
            // Load the manager change along with old and new manager in one go
            var managerChange = await _context.ManagerChanges
                .AsNoTracking()
                .Where(mc => mc.Id == managerChangeId)
                .Select(mc => new
                {
                    mc,
                    OldManager = _context.PersonDetails
                        .AsNoTracking()
                        .FirstOrDefault(p => p.Id == mc.OldManagerId),
                    NewManager = _context.PersonDetails
                        .AsNoTracking()
                        .FirstOrDefault(p => p.Id == mc.NewManagerId),
                    FactoryReg = _context.EstablishmentRegistrations
                        .AsNoTracking()
                        .Where(f => f.EstablishmentRegistrationId == mc.FactoryRegistrationId.ToString())
                        .Select(f => new
                        {
                            FactoryRegistrationId = Guid.Parse(f.EstablishmentRegistrationId),
                            f.EstablishmentDetailId
                        })
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (managerChange?.mc == null)
                throw new Exception("Manager change application not found");

            // Load establishment + area/district/division in one query if possible
            EstablishmentDetail? est = null;
            Area? area = null;
            District? district = null;
            Division? division = null;

            if (managerChange.FactoryReg?.EstablishmentDetailId != null)
            {
                est = await _context.EstablishmentDetails
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == managerChange.FactoryReg.EstablishmentDetailId);

                if (est != null && !string.IsNullOrEmpty(est.SubDivisionId))
                {
                    var areaId = Guid.Parse(est.SubDivisionId);
                    area = await _context.Areas.AsNoTracking().FirstOrDefaultAsync(a => a.Id == areaId);

                    if (area != null)
                    {
                        district = await _context.Districts.AsNoTracking().FirstOrDefaultAsync(d => d.Id == area.DistrictId);
                        if (district != null)
                            division = await _context.Divisions.AsNoTracking().FirstOrDefaultAsync(v => v.Id == district.DivisionId);
                    }
                }
            }

            // Prepare factory DTO
            var factoryDto = est == null ? null : new FactoryBasicDto
            {
                FactoryRegistrationId = managerChange.FactoryReg.FactoryRegistrationId,
                FactoryName = est.EstablishmentName,
                AddressLine1 = est.AddressLine1,
                AddressLine2 = est.AddressLine2,
                DistrictName = district?.Name,
                Area = est.Area,
                Pincode = est.Pincode,
                Email = est.Email,
                Telephone = est.Telephone,
                Mobile = est.Mobile,
            };

            return new ManagerChangeGetResponseDto
            {
                ManagerChangeId = managerChange.mc.Id,
                AcknowledgementNumber = managerChange.mc.AcknowledgementNumber,
                Status = managerChange.mc.Status,
                DateOfAppointment = managerChange.mc.DateOfAppointment,
                SignatureOfOccupier = managerChange.mc.SignatureofOccupier,
                SignatureOfNewManager = managerChange.mc.SignatureOfNewManager,

                Factory = factoryDto,

                OldManager = managerChange.OldManager == null ? null : new PersonBasicDto
                {
                    Id = managerChange.OldManager.Id,
                    Name = managerChange.OldManager.Name,
                    AddressLine1 = managerChange.OldManager.AddressLine1,
                    AddressLine2 = managerChange.OldManager.AddressLine2,
                    District = managerChange.OldManager.District,
                    Tehsil = managerChange.OldManager.Tehsil,
                    Area = managerChange.OldManager.Area,
                    Pincode = managerChange.OldManager.Pincode,
                    Email = managerChange.OldManager.Email,
                    Telephone = managerChange.OldManager.Telephone,
                    Mobile = managerChange.OldManager.Mobile,
                    RelationType = managerChange.OldManager.RelationType,
                    RelativeName = managerChange.OldManager.RelativeName,
                    Designation = managerChange.OldManager.Designation ?? ""
                },

                NewManager = managerChange.NewManager == null ? null : new PersonBasicDto
                {
                    Id = managerChange.NewManager.Id,
                    Name = managerChange.NewManager.Name,
                    AddressLine1 = managerChange.NewManager.AddressLine1,
                    AddressLine2 = managerChange.NewManager.AddressLine2,
                    District = managerChange.NewManager.District,
                    Tehsil = managerChange.NewManager.Tehsil,
                    Area = managerChange.NewManager.Area,
                    Pincode = managerChange.NewManager.Pincode,
                    Email = managerChange.NewManager.Email,
                    Telephone = managerChange.NewManager.Telephone,
                    Mobile = managerChange.NewManager.Mobile,
                    RelationType = managerChange.NewManager.RelationType,
                    RelativeName = managerChange.NewManager.RelativeName,
                    Designation = managerChange.NewManager.Designation ?? ""
                }
            };
        }

        public async Task<string> GenerateManagerChangePdfAsync(Guid managerChangeId)
        {
            var data = await GetByIdAsync(managerChangeId);

            var folderName = "manager-change-pdfs";
            var folderPath = Path.Combine(_environment.WebRootPath, folderName);
            Directory.CreateDirectory(folderPath);
            var fileName = $"MC_{managerChangeId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(folderPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/{folderName}/{fileName}";

            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            using var doc = new PdfDoc(pdf);

            var bold = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
            var regular = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

            // Header
            doc.Add(new Paragraph("Form – 11").SetFont(bold).SetFontSize(14).SetTextAlignment(TextAlignment.CENTER));
            doc.Add(new Paragraph("(See Rule 14)").SetFont(regular).SetFontSize(11).SetTextAlignment(TextAlignment.CENTER));
            doc.Add(new Paragraph("Notice of Change of Manager").SetFont(bold).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER));
            doc.Add(new Paragraph("\n"));

            // Helper to add section table
            void AddSection(string title, (string Label, string? Value)[] rows)
            {
                doc.Add(new Paragraph(title).SetFont(bold).SetFontSize(11));
                var table = new PdfTable(new float[] { 2, 3 }).UseAllAvailableWidth();
                foreach (var (label, value) in rows)
                {
                    _ = table.AddCell(new PdfCell().Add(new Paragraph(label).SetFont(bold).SetFontSize(9)).SetBorder(new SolidBorder(0.5f)));
                    _ = table.AddCell(new PdfCell().Add(new Paragraph(value ?? "—").SetFont(regular).SetFontSize(9)).SetBorder(new SolidBorder(0.5f)));
                }
                doc.Add(table);
                doc.Add(new Paragraph("\n"));
            }

            AddSection("1. Factory Details", new[]
            {
                ("Factory Name", data.Factory?.FactoryName),
                ("Factory Registration No.", data.Factory?.FactoryRegistrationId.ToString()),
                ("Application No.", data.AcknowledgementNumber),
                ("Address", $"{data.Factory?.AddressLine1}, {data.Factory?.AddressLine2}"),
                ("District", data.Factory?.DistrictName),
                ("Pincode", data.Factory?.Pincode),
            });

            AddSection("2. Outgoing Manager", new[]
            {
                ("Name", data.OldManager?.Name),
                ("Designation", data.OldManager?.Designation),
                (data.OldManager?.RelationType?.ToUpper() + " Name", data.OldManager?.RelativeName),
                ("Address", $"{data.OldManager?.AddressLine1}, {data.OldManager?.AddressLine2}"),
                ("District", data.OldManager?.District),
                ("Tehsil", data.OldManager?.Tehsil),
                ("Area", data.OldManager?.Area),
                ("Pincode", data.OldManager?.Pincode),
                ("Mobile", data.OldManager?.Mobile),
            });

            AddSection("3. New Manager", new[]
            {
                ("Name", data.NewManager?.Name),
                ("Designation", data.NewManager?.Designation),
                (data.NewManager?.RelationType?.ToUpper() + " Name", data.NewManager?.RelativeName),
                ("Address", $"{data.NewManager?.AddressLine1}, {data.NewManager?.AddressLine2}"),
                ("District", data.NewManager?.District),
                ("Tehsil", data.NewManager?.Tehsil),
                ("Area", data.NewManager?.Area),
                ("Pincode", data.NewManager?.Pincode),
                ("Mobile", data.NewManager?.Mobile),
            });

            AddSection("4. Date of Appointment", new[]
            {
                ("Date of Appointment of New Manager", data.DateOfAppointment.ToString("dd MMM yyyy")),
            });

            // Save the URL to the ManagerChange record so manager eSign can reuse this PDF
            var mcRecord = await _context.ManagerChanges.FirstOrDefaultAsync(m => m.Id == managerChangeId);
            if (mcRecord != null)
            {
                mcRecord.ApplicationPDFUrl = fileUrl;
                await _context.SaveChangesAsync();
            }

            return filePath;
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

    };
};
