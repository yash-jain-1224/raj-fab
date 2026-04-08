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

using static System.Net.Mime.MediaTypeNames;
using ImageDataFactory = iText.IO.Image.ImageDataFactory;
using PdfCell = iText.Layout.Element.Cell;
using PdfDoc = iText.Layout.Document;
using PdfImage = iText.Layout.Element.Image;
using PdfTable = iText.Layout.Element.Table;
using PdfWriter = iText.Kernel.Pdf.PdfWriter;
using Text = iText.Layout.Element.Text;
using RajFabAPI.Constants;
using iText.Kernel.Pdf.Event;
using iText.Kernel.Pdf.Canvas;
using System.Text.Json;
using iText.IO.Font.Constants;

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

        private async Task<string> GenerateApplicationNumberAsync()
        {
            var year = DateTime.Now.Year;
            string prefix = $"FMC-";

            // Get latest application number
            var lastApp = await _context.ManagerChanges
                .Where(x => x.ApplicationNumber.StartsWith(prefix)
                        && x.ApplicationNumber.Contains($"/CIFB/{year}"))
                .OrderByDescending(x => x.CreatedAt)
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
                        join estReg in _context.EstablishmentRegistrations.AsNoTracking()
                            on mc.FactoryRegistrationNumber equals estReg.RegistrationNumber into estRegJoin
                        from estRegistration in estRegJoin.DefaultIfEmpty()
                        join est in _context.EstablishmentDetails.AsNoTracking()
                            on estRegistration.EstablishmentDetailId equals est.Id into estJoin
                        from establishment in estJoin.DefaultIfEmpty()
                        join city in _context.Cities.AsNoTracking()
                            on Guid.Parse(establishment.SubDivisionId) equals city.Id into cityJoin
                        from cityDetail in cityJoin.DefaultIfEmpty()
                        join district in _context.Districts.AsNoTracking()
                            on cityDetail.DistrictId equals district.Id into districtJoin
                        from districtDetail in districtJoin.DefaultIfEmpty()
                        join division in _context.Divisions.AsNoTracking()
                            on districtDetail.DivisionId equals division.Id into divisionJoin
                        from divisionDetail in divisionJoin.DefaultIfEmpty()
                        select new ManagerChangeGetResponseDto
                        {
                            ManagerChangeId = mc.Id,
                            ApplicationNumber = mc.ApplicationNumber,
                            Status = mc.Status,
                            DateOfAppointment = mc.DateOfAppointment,
                            SubmittedDate = mc.CreatedAt,
                            OldManager = oldManager == null ? null : new PersonBasicDto
                            {
                                Id = oldManager.Id,
                                Name = oldManager.Name,
                                Designation = oldManager.Designation,
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
                                Designation = newManager.Designation,
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
                                FactoryRegistrationNumber = estRegistration.RegistrationNumber,
                                FactoryName = establishment.EstablishmentName,
                                AddressLine1 = establishment.AddressLine1,
                                AddressLine2 = establishment.AddressLine2,
                                Pincode = establishment.Pincode,
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
                    Designation = dto.NewManagerDesignation,
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
                    FactoryRegistrationNumber = dto.FactoryRegistrationNumber,
                    OldManagerId = dto.OldManagerId,
                    NewManagerId = newManager.Id,
                    ApplicationNumber = await GenerateApplicationNumberAsync(),
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
                    .Where(m => m.EstablishmentRegistrationId == dto.FactoryRegistrationNumber.ToString())
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

                await tx.CommitAsync();
                return new ManagerChangeResponseDto
                {
                    ManagerChangeId = managerChange.Id,
                    NewManagerId = newManager.Id,
                    ApplicationNumber = managerChange.ApplicationNumber,
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
                newManager.Designation = dto.NewManagerDesignation ?? newManager.Designation;
                newManager.RelationType = dto.NewManagerRelation ?? newManager.RelationType;
                newManager.RelativeName = dto.NewManagerFatherOrHusbandName ?? newManager.RelativeName;
                newManager.AddressLine1 = dto.NewManagerAddressLine1 ?? newManager.AddressLine1;
                newManager.AddressLine2 = dto.NewManagerAddressLine2 ?? newManager.AddressLine2;
                newManager.District = dto.NewManagerDistrict ?? newManager.District;
                newManager.Tehsil = dto.NewManagerState ?? newManager.Tehsil;
                newManager.Area = dto.NewManagerCity ?? newManager.Area;
                newManager.Pincode = dto.NewManagerPincode ?? newManager.Pincode;
                newManager.Email = dto.NewManagerEmail ?? newManager.Email;
                newManager.Telephone = dto.NewManagerMobile ?? newManager.Telephone;
                newManager.Mobile = dto.NewManagerMobile ?? newManager.Mobile;
                newManager.UpdatedAt = DateTime.Now;

                _context.PersonDetails.Update(newManager);
                await _context.SaveChangesAsync();

                // 3?? Update ManagerChange fields
                if (dto.OldManagerId.HasValue && dto.OldManagerId != Guid.Empty)
                    managerChange.OldManagerId = dto.OldManagerId.Value;

                managerChange.DateOfAppointment = dto.NewManagerDateOfAppointment ?? managerChange.DateOfAppointment;
                managerChange.Status = dto.Status ?? managerChange.Status;

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

                        var establishmentReg = await _context.EstablishmentRegistrations
                            .Where(er => er.EstablishmentRegistrationId == managerChange.FactoryRegistrationNumber.ToString())
                            .Select(er => new
                            {
                                er.EstablishmentDetailId,
                                er.FactoryCategoryId,
                                SubDivisionId = _context.EstablishmentDetails
                                    .Where(ed => ed.Id == er.EstablishmentDetailId)
                                    .Select(ed => ed.SubDivisionId)
                            })
                            .FirstOrDefaultAsync();

                        var areaId = await _context.EstablishmentDetails
                            .Where(ed => ed.Id == establishmentReg.EstablishmentDetailId)
                            .Select(ed => ed.SubDivisionId)
                            .FirstOrDefaultAsync();

                        if (Guid.TryParse(areaId, out var parsedAreaId))
                        {
                            var officeApplicationArea = await _context.OfficeApplicationAreas
                                .FirstOrDefaultAsync(oaa => oaa.CityId == parsedAreaId);

                            if (officeApplicationArea != null)
                            {
                                var factoryCategoryId = establishmentReg.FactoryCategoryId;

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
                    ApplicationNumber = managerChange.ApplicationNumber
                };
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<ManagerChangeApplicationDto> GetByIdAsync(Guid managerChangeId)
        {
            // Single query approach: all joins, null-safe, one DB round-trip
            var managerChangeDto = await (
                from mc in _context.ManagerChanges.AsNoTracking()
                where mc.Id == managerChangeId

                // Old Manager
                join oldMgr in _context.PersonDetails.AsNoTracking()
                    on mc.OldManagerId equals oldMgr.Id into oldMgrJoin
                from oldManager in oldMgrJoin.DefaultIfEmpty()

                    // New Manager
                join newMgr in _context.PersonDetails.AsNoTracking()
                    on mc.NewManagerId equals newMgr.Id into newMgrJoin
                from newManager in newMgrJoin.DefaultIfEmpty()

                    // Establishment Registration
                join estReg in _context.EstablishmentRegistrations.AsNoTracking()
                    on mc.FactoryRegistrationNumber equals estReg.RegistrationNumber into estRegJoin
                from estRegistration in estRegJoin.DefaultIfEmpty()

                    // Establishment
                join est in _context.EstablishmentDetails.AsNoTracking()
                    on estRegistration.EstablishmentDetailId equals est.Id into estJoin
                from establishment in estJoin.DefaultIfEmpty()

                join ft in _context.FactoryTypes.AsNoTracking()
                    on establishment.FactoryTypeId equals ft.Id into ftJoin
                from factoryType in ftJoin.DefaultIfEmpty()

                    // Mapping: only type "Factory"
                join map in _context.EstablishmentEntityMapping.AsNoTracking()
                    on estRegistration.EstablishmentRegistrationId equals map.EstablishmentRegistrationId.ToString() into mapJoin
                from mapping in mapJoin
                    .Where(x => x.EntityType == "Factory")
                    .DefaultIfEmpty()

                    // Factory Details
                join f in _context.FactoryDetails.AsNoTracking()
                    on mapping.EntityId equals f.Id into factoryJoin
                from factory in factoryJoin.DefaultIfEmpty()

                    // SubDivision (City)
                join city in _context.Cities.AsNoTracking()
                    on factory.SubDivisionId equals city.Id.ToString() into cityJoin
                from subDivision in cityJoin.DefaultIfEmpty()

                    // Tehsil
                join tehsil in _context.Tehsils.AsNoTracking()
                    on factory.TehsilId equals tehsil.Id.ToString() into tehsilJoin
                from tehsilData in tehsilJoin.DefaultIfEmpty()

                    // District via SubDivision
                join district in _context.Districts.AsNoTracking()
                    on subDivision.DistrictId equals district.Id into districtJoin
                from districtData in districtJoin.DefaultIfEmpty()

                select new ManagerChangeGetResponseDto
                {
                    ManagerChangeId = mc.Id,
                    ApplicationNumber = mc.ApplicationNumber,
                    Status = mc.Status,
                    DateOfAppointment = mc.DateOfAppointment,
                    SubmittedDate = mc.CreatedAt,
                    ApplicationPDFUrl = mc.ApplicationPDFUrl,
                    ObjectionLetterUrl = mc.ObjectionLetterUrl,
                    // Factory DTO
                    Factory = (factory == null && establishment == null) ? null : new FactoryBasicDto
                    {
                        ManufacturingDetail = factory.ManufacturingDetail,
                        NumberOfWorker = factory.NumberOfWorker,
                        SanctionedLoad = factory.SanctionedLoad,
                        SanctionedLoadUnit = factory.SanctionedLoadUnit,

                        FactoryRegistrationNumber = mc.FactoryRegistrationNumber,

                        // Name from Establishment
                        FactoryName = establishment != null ? establishment.EstablishmentName : null,
                        FactoryTypeId = establishment != null ? establishment.FactoryTypeId : null,
                        FactoryTypeName = factoryType != null ? factoryType.Name : null,

                        // Address from FactoryDetails
                        AddressLine1 = factory != null ? factory.AddressLine1 : null,
                        AddressLine2 = factory != null ? factory.AddressLine2 : null,
                        Area = factory != null ? factory.Area : null,
                        Pincode = factory != null ? factory.Pincode : null,
                        Email = factory != null ? factory.Email : null,
                        Telephone = factory != null ? factory.Telephone : null,
                        Mobile = factory != null ? factory.Mobile : null,

                        // Location Names
                        SubDivisionName = subDivision != null ? subDivision.Name : null,
                        TehsilName = tehsilData != null ? tehsilData.Name : null,
                        DistrictName = districtData != null ? districtData.Name : null,
                    },

                    // Old Manager DTO
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
                        RelationType = oldManager.RelationType,
                        RelativeName = oldManager.RelativeName,
                        Designation = oldManager.Designation ?? ""
                    },

                    // New Manager DTO
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
                        RelationType = newManager.RelationType,
                        RelativeName = newManager.RelativeName,
                        Designation = newManager.Designation ?? ""
                    }
                }
            ).FirstOrDefaultAsync();

            var activeCertificate = await _context.Set<Certificate>()
                        .AsNoTracking()
                        .Where(c => c.ApplicationId == managerChangeDto.ManagerChangeId.ToString())
                        .OrderByDescending(c => c.CertificateVersion)
                        .FirstOrDefaultAsync();

            if (managerChangeDto == null)
                throw new Exception("Manager change application not found");

            var applicationHistory = await _context.Set<ApplicationHistory>()
                    .AsNoTracking()
                    .Where(x => x.ApplicationId == managerChangeDto.ManagerChangeId.ToString())
                    .OrderByDescending(x => x.ActionDate)
                    .ToListAsync();

            managerChangeDto.CertificatePDFUrl = activeCertificate?.CertificateUrl;

            return new ManagerChangeApplicationDto
            {
                ApplicationDetails = managerChangeDto,
                ApplicationHistory = applicationHistory,
            };
        }

        public async Task<bool> UpdateStatusAndRemark(string applicationId, string status)
        {
            try
            {
                var managerChange = await _context.ManagerChanges.FirstOrDefaultAsync(x => x.Id == Guid.Parse(applicationId));
                if (managerChange == null)
                    return false;
                managerChange.Status = status;
                managerChange.UpdatedAt = DateTime.Now;
                _ = await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //public async Task<ManagerChangeGetResponseDto> GetByIdAsync(Guid managerChangeId)
        //{
        //    try
        //    {
        //        // Load the manager change along with old and new manager in one go
        //        var managerChange = await _context.ManagerChanges
        //            .AsNoTracking()
        //            .Where(mc => mc.Id == managerChangeId)
        //            .Select(mc => new
        //            {
        //                mc,
        //                OldManager = _context.PersonDetails
        //                    .AsNoTracking()
        //                    .FirstOrDefault(p => p.Id == mc.OldManagerId),
        //                NewManager = _context.PersonDetails
        //                    .AsNoTracking()
        //                    .FirstOrDefault(p => p.Id == mc.NewManagerId),
        //                FactoryReg = _context.EstablishmentRegistrations
        //                    .AsNoTracking()
        //                    .Where(f => f.RegistrationNumber == mc.FactoryRegistrationNumber)
        //                    .Select(f => new
        //                    {
        //                        FactoryRegistrationNumber = mc.FactoryRegistrationNumber,
        //                        f.EstablishmentDetailId,
        //                        f.EstablishmentRegistrationId
        //                    })
        //                    .FirstOrDefault()
        //            })
        //            .FirstOrDefaultAsync();

        //        if (managerChange?.mc == null)
        //            throw new Exception("Manager change application not found");

        //        // Load establishment + area/district/division in one query if possible
        //        EstablishmentDetail? est = null;

        //        if (managerChange.FactoryReg?.EstablishmentDetailId != null)
        //        {
        //            est = await _context.EstablishmentDetails
        //                .AsNoTracking()
        //                .FirstOrDefaultAsync(e => e.Id == managerChange.FactoryReg.EstablishmentDetailId);
        //        }
        //        var mappings = await _context.EstablishmentEntityMapping
        //                    .AsNoTracking()
        //                    .Where(x => x.EntityId == Guid.Parse(managerChange.FactoryReg.EstablishmentRegistrationId) && x.EntityType == "Factory")
        //                    .FirstOrDefaultAsync();
        //        if (mappings == null)
        //        {
        //            // handle gracefully
        //            return new ManagerChangeGetResponseDto
        //            {
        //                ManagerChangeId = managerChange.mc.Id,
        //                ApplicationNumber = managerChange.mc.ApplicationNumber,
        //                Status = managerChange.mc.Status,
        //                DateOfAppointment = managerChange.mc.DateOfAppointment,
        //                Factory = null
        //            };
        //        }

        //        var factoryData = await (
        //            from f in _context.FactoryDetails.AsNoTracking()
        //            where f.Id == mappings.EntityId

        //            // SubDivision (City)
        //            join city in _context.Cities.AsNoTracking()
        //            on f.SubDivisionId equals city.Id.ToString() into cityJoin
        //            from subDivision in cityJoin.DefaultIfEmpty()

        //                // Tehsil
        //            join tehsil in _context.Tehsils.AsNoTracking()
        //            on f.TehsilId equals tehsil.Id.ToString() into tehsilJoin
        //            from tehsilData in tehsilJoin.DefaultIfEmpty()

        //                // District via City
        //            join district in _context.Districts.AsNoTracking()
        //            on subDivision.DistrictId equals district.Id into districtJoin
        //            from districtData in districtJoin.DefaultIfEmpty()

        //            select new
        //            {
        //                f,
        //                SubDivisionName = subDivision.Name,
        //                TehsilName = tehsilData.Name,
        //                DistrictName = districtData.Name
        //            }
        //            ).FirstOrDefaultAsync();

        //        // Prepare factory DTO
        //        var factoryDto = est == null && factoryData == null ? null : new FactoryBasicDto
        //        {
        //            FactoryRegistrationNumber = managerChange.FactoryReg.FactoryRegistrationNumber,
        //            FactoryName = est?.EstablishmentName,
        //            AddressLine1 = factoryData.f.AddressLine1,
        //            AddressLine2 = factoryData?.f?.AddressLine2,
        //            Area = factoryData?.f?.Area,
        //            Pincode = factoryData?.f?.Pincode,
        //            Email = factoryData?.f?.Email,
        //            Telephone = factoryData?.f?.Telephone,
        //            Mobile = factoryData?.f?.Mobile,
        //            DistrictName = factoryData?.DistrictName
        //        };

        //        return new ManagerChangeGetResponseDto
        //        {
        //            ManagerChangeId = managerChange.mc.Id,
        //            ApplicationNumber = managerChange.mc.ApplicationNumber,
        //            Status = managerChange.mc.Status,
        //            DateOfAppointment = managerChange.mc.DateOfAppointment,

        //            Factory = factoryDto,

        //            OldManager = managerChange.OldManager == null ? null : new PersonBasicDto
        //            {
        //                Id = managerChange.OldManager.Id,
        //                Name = managerChange.OldManager.Name,
        //                AddressLine1 = managerChange.OldManager.AddressLine1,
        //                AddressLine2 = managerChange.OldManager.AddressLine2,
        //                District = managerChange.OldManager.District,
        //                Tehsil = managerChange.OldManager.Tehsil,
        //                Area = managerChange.OldManager.Area,
        //                Pincode = managerChange.OldManager.Pincode,
        //                Email = managerChange.OldManager.Email,
        //                Telephone = managerChange.OldManager.Telephone,
        //                Mobile = managerChange.OldManager.Mobile,
        //                RelationType = managerChange.OldManager.RelationType,
        //                RelativeName = managerChange.OldManager.RelativeName,
        //                Designation = managerChange.OldManager.Designation ?? ""
        //            },

        //            NewManager = managerChange.NewManager == null ? null : new PersonBasicDto
        //            {
        //                Id = managerChange.NewManager.Id,
        //                Name = managerChange.NewManager.Name,
        //                AddressLine1 = managerChange.NewManager.AddressLine1,
        //                AddressLine2 = managerChange.NewManager.AddressLine2,
        //                District = managerChange.NewManager.District,
        //                Tehsil = managerChange.NewManager.Tehsil,
        //                Area = managerChange.NewManager.Area,
        //                Pincode = managerChange.NewManager.Pincode,
        //                Email = managerChange.NewManager.Email,
        //                Telephone = managerChange.NewManager.Telephone,
        //                Mobile = managerChange.NewManager.Mobile,
        //                RelationType = managerChange.NewManager.RelationType,
        //                RelativeName = managerChange.NewManager.RelativeName,
        //                Designation = managerChange.NewManager.Designation ?? ""
        //            }
        //        };
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}

        public async Task<string> GenerateManagerChangePdfAsync(Guid managerChangeId)
        {
            var res = await GetByIdAsync(managerChangeId);
            var data = res.ApplicationDetails;

            var folderName = "manager-change-forms";
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
            var boldFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

            DateOnly footerDate = DateOnly.FromDateTime(DateTime.Today);
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE,
                new ManagerChangePageBorderAndFooterEventHandler(boldFont, regularFont, DateTime.Now.ToString("dd/MM/yyyy")));

            using var document = new PdfDoc(pdf);
            document.SetMargins(40, 40, 40, 40); // top, right, bottom, left

            // ─────────────────────────────────────────────
            // HEADER (centered)
            // ─────────────────────────────────────────────
            _ = document.Add(new Paragraph("Form-11")
                .SetFont(boldFont)
                .SetFontSize(13)
                .SetTextAlignment(TextAlignment.CENTER));

            _ = document.Add(new Paragraph("(See rule 14)")
                .SetFont(regularFont)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER));

            _ = document.Add(new Paragraph("Notice of Change of Manager")
                .SetFont(boldFont)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(8));
            document.Add(new Paragraph("\n").SetFontSize(10));

            void AddTwoPartSection(
                string title,
                (string Label, string? Value)[] leftRows,
                (string Label, string? Value)[] rightRows)
            {
                // Title spans full width
                document.Add(new Paragraph(title)
                    .SetFont(boldFont)
                    .SetFontSize(11)
                    .SetMarginBottom(6f));

                // Create left table (2 columns)
                var leftTable = new PdfTable(new float[] { 2, 3 }).UseAllAvailableWidth();
                foreach (var (label, value) in leftRows)
                {
                    _ = leftTable.AddCell(new PdfCell()
                        .Add(new Paragraph(label).SetFont(boldFont).SetFontSize(9))
                        .SetBorder(new SolidBorder(0.5f)));

                    _ = leftTable.AddCell(new PdfCell()
                        .Add(new Paragraph(value ?? "—").SetFont(regularFont).SetFontSize(9))
                        .SetBorder(new SolidBorder(0.5f)));
                }

                // Create right table (2 columns)
                var rightTable = new PdfTable(new float[] { 2, 3 }).UseAllAvailableWidth();
                foreach (var (label, value) in rightRows)
                {
                    _ = rightTable.AddCell(new PdfCell()
                        .Add(new Paragraph(label).SetFont(boldFont).SetFontSize(9))
                        .SetBorder(new SolidBorder(0.5f)));

                    _ = rightTable.AddCell(new PdfCell()
                        .Add(new Paragraph(value ?? "—").SetFont(regularFont).SetFontSize(9))
                        .SetBorder(new SolidBorder(0.5f)));
                }

                // Create container table with two equal-width columns
                var containerTable = new PdfTable(new float[] { 1, 1 }).UseAllAvailableWidth();
                _ = containerTable.AddCell(new PdfCell().Add(leftTable).SetBorder(Border.NO_BORDER));
                _ = containerTable.AddCell(new PdfCell().Add(rightTable).SetBorder(Border.NO_BORDER));

                // Add container table to document
                document.Add(containerTable);
            }

            var headerTable = new PdfTable(new float[] { 360f, 160f })
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER);

            headerTable.AddCell(new PdfCell()
                .Add(new Paragraph($"Manager Change Application No.: {data.ApplicationNumber}")
                    .SetFont(boldFont)
                    .SetFontSize(10))
                .SetBorder(Border.NO_BORDER));

            headerTable.AddCell(new PdfCell()
                .Add(new Paragraph($"Date: {DateTime.Now:dd-MM-yyyy}")
                    .SetFont(boldFont)
                    .SetFontSize(10)
                    .SetTextAlignment(TextAlignment.RIGHT))
                .SetBorder(Border.NO_BORDER));

            document.Add(headerTable);
            document.Add(new Paragraph("\n").SetFontSize(5));
            // ─────────────────────────────────────────────
            // SECTION 1 – Factory Details
            // ─────────────────────────────────────────────
            var factoryTable = new PdfTable(new float[] { 260f, 260f })
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER);

            var factoryLeft = new List<(string Label, string? Value)>
            {
                ("Factory Name:", data.Factory?.FactoryName),
                ("Factory Registration No.:", data.Factory?.FactoryRegistrationNumber?.ToString()),
                  ("Address:", $"{data.Factory?.AddressLine1}, {data.Factory?.AddressLine2}"),
                ("District:", data.Factory?.DistrictName),
                ("Sub-division:", data.Factory?.SubDivisionName),
                ("Tehsil:", data.Factory?.TehsilName),
                ("Area:", data.Factory?.Area),
                ("Pincode:", data.Factory?.Pincode),
                ("Email:", data.Factory?.Email),
                ("Mobile:", data.Factory?.Mobile),
            };

            if (!string.IsNullOrWhiteSpace(data.Factory?.Telephone))
                factoryLeft.Add(("Telephone:", data.Factory.Telephone));

            AddTwoColumnSection(document, "1. Factory Details", factoryLeft, boldFont, regularFont);

            var outgoingLeft = new List<(string Label, string? Value)>
            {
                ("Name:", data.OldManager?.Name),
                ("Designation:", data.OldManager?.Designation),
                ($"{Capitalize(data.OldManager?.RelationType)}'s Name:", data.OldManager?.RelativeName),
                ("Address:", $"{data.OldManager?.AddressLine1}, {data.OldManager?.AddressLine2}"),
                                ("District:", data.OldManager?.District),
                ("Tehsil:", data.OldManager?.Tehsil),
                ("Area:", data.OldManager?.Area),
                ("Pincode:", data.OldManager?.Pincode),
                ("Email:", data.OldManager?.Email),
                ("Mobile:", data.OldManager?.Mobile),
            };

            if (!string.IsNullOrWhiteSpace(data.OldManager?.Telephone))
                outgoingLeft.Add(("Telephone:", data.OldManager.Telephone));

            AddTwoColumnSection(document, "2. Outgoing Manager", outgoingLeft, boldFont, regularFont);

            var newManagerLeft = new List<(string Label, string? Value)>
            {
                ("Name:", data.NewManager?.Name),
                ("Designation:", data.NewManager?.Designation),
                ($"{Capitalize(data.NewManager?.RelationType)}'s Name:", data.NewManager?.RelativeName),
                ("Address:", $"{data.NewManager?.AddressLine1}, {data.NewManager?.AddressLine2}"),
                                ("District:", data.NewManager?.District),
                ("Tehsil:", data.NewManager?.Tehsil),
                ("Area:", data.NewManager?.Area),
                ("Pincode:", data.NewManager?.Pincode),
                ("Email:", data.NewManager?.Email),
                ("Mobile:", data.NewManager?.Mobile),
            };

            if (!string.IsNullOrWhiteSpace(data.NewManager?.Telephone))
                newManagerLeft.Add(("Telephone:", data.NewManager.Telephone));

            document.Add(new Paragraph("\n").SetFontSize(110)); // spacing

            AddTwoColumnSection(document, "3. New Manager", newManagerLeft, boldFont, regularFont);

            // ─────────────────────────────────────────────
            // SECTION 4 – Date of Appointment
            // ─────────────────────────────────────────────
            document.Add(new Paragraph("4. Date of Appointment")
                .SetFont(boldFont)
                .SetFontSize(10)
                .SetMarginBottom(4));

            var dateTable = new PdfTable(new float[] { 260f, 260f })
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER);

            dateTable.AddCell(new PdfCell()
                .Add(new Paragraph("Date of Appointment of New Manager:")
                    .SetFont(boldFont).SetFontSize(9))
                .SetBorder(Border.NO_BORDER).SetPaddingLeft(4));
            dateTable.AddCell(new PdfCell()
                .Add(new Paragraph(data.DateOfAppointment.ToString("dd MMM yyyy"))
                    .SetFont(regularFont).SetFontSize(9))
                .SetBorder(Border.NO_BORDER));

            dateTable.AddCell(new PdfCell()  // empty cell to keep two-column layout balanced
                .Add(new Paragraph("").SetFont(boldFont).SetFontSize(9))
                .SetBorder(Border.NO_BORDER));
            dateTable.AddCell(new PdfCell()
                .Add(new Paragraph("").SetFont(regularFont).SetFontSize(9))
                .SetBorder(Border.NO_BORDER));

            document.Add(dateTable);
            document.Add(new Paragraph("\n").SetFontSize(4)); // spacing
            // _ = document.Add(new Paragraph("\n\n\n\n\n\n"));

            // Save the URL to the ManagerChange record so manager eSign can reuse this PDF
            var mcRecord = await _context.ManagerChanges.FirstOrDefaultAsync(m => m.Id == managerChangeId);
            if (mcRecord != null)
            {
                mcRecord.ApplicationPDFUrl = fileUrl;
                await _context.SaveChangesAsync();
            }

            return filePath;
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // Generate Objection Letter — Manager Change
        // ─────────────────────────────────────────────────────────────────────────────
        public async Task<string> GenerateObjectionLetter(ManagerChangeObjectionLetterDto dto, string applicationId)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var fileName = $"objection_manager_change_{applicationId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
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

            var rawLoad = dto.ManagerChangeData.Factory.SanctionedLoad ?? 0;
            var loadUnit = (dto.ManagerChangeData.Factory.SanctionedLoadUnit ?? "HP").ToUpper();

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

            if (dto.ManagerChangeData.Factory.NumberOfWorker < 20)
            {
                Type = "Section 85";
            }
            else if (dto.ManagerChangeData.Factory.NumberOfWorker > 40 && power == 0)
            {
                Type = "2 (1)(w)(ii)";
            }
            else if (dto.ManagerChangeData.Factory.NumberOfWorker >= 20 && power > 0)
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
                .Add(new Paragraph($"Manager Change Application No.:- {dto.ManagerChangeData.ApplicationNumber ?? "-"}")
                    .SetFont(boldFont).SetFontSize(10))
                .SetBorder(Border.NO_BORDER));

            _ = topRow.AddCell(new PdfCell()
                .Add(new Paragraph($"Dated:-  {DateTime.Now.ToString("dd/MM/yyyy")}")
                    .SetFont(boldFont).SetFontSize(10)
                    .SetTextAlignment(TextAlignment.RIGHT))
                .SetBorder(Border.NO_BORDER));

            _ = document.Add(topRow);

            var factoryAddress = $"{dto.ManagerChangeData.Factory?.AddressLine1}, {dto.ManagerChangeData.Factory?.AddressLine2},\n{dto.ManagerChangeData.Factory?.Area}, {dto.ManagerChangeData.Factory?.TehsilName},\n{dto.ManagerChangeData.Factory?.SubDivisionName}, {dto.ManagerChangeData.Factory?.DistrictName}, {dto.ManagerChangeData.Factory?.Pincode}";

            // ═════════════════════════════════════════════════════════════════════════
            // Factory name + address
            // ═════════════════════════════════════════════════════════════════════════
            if (!string.IsNullOrWhiteSpace(dto.ManagerChangeData.Factory.FactoryName))
            {
                _ = document.Add(new Paragraph(dto.ManagerChangeData.Factory.FactoryName)
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
            subPara.Add(new Text("Regarding approval of your Manager Change Application").SetFont(regularFont).SetFontSize(12));
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
            _ = detailsTable.AddCell(RedCell(dto.ManagerChangeData.Factory.ManufacturingDetail ?? "-", regularFont));

            _ = detailsTable.AddCell(RedCell("Type", boldFont));
            _ = detailsTable.AddCell(RedCell(Type, regularFont));

            _ = detailsTable.AddCell(RedCell("Category", boldFont));
            _ = detailsTable.AddCell(RedCell(dto.ManagerChangeData.Factory.FactoryTypeName ?? "-", regularFont));

            _ = detailsTable.AddCell(RedCell("Workers", boldFont));
            _ = detailsTable.AddCell(RedCell(dto.ManagerChangeData.Factory.NumberOfWorker.ToString() ?? "-", regularFont));

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

            var managerChange = await _context.ManagerChanges
                .FirstOrDefaultAsync(x => x.Id == Guid.Parse(applicationId));
            if (managerChange != null)
            {
                managerChange.ObjectionLetterUrl = fileUrl;
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

                var managerChangeData = await GetByIdAsync(Guid.Parse(applicationId));
                if (managerChangeData == null) throw new Exception("Manager change application not found");

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

                var certificateUrl = await GenerateCertificatePdf(dto, applicationId, officePost.PostName + ", " + officePost.CityName, user.FullName, managerChangeData.ApplicationDetails);
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
                    RegistrationNumber = managerChangeData.ApplicationDetails.ApplicationNumber,
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

        public async Task<string> GenerateCertificatePdf(CertificateRequestDto dto, string applicationId, string postName, string userName, ManagerChangeGetResponseDto managerChangeData)
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

            var rawLoad = managerChangeData.Factory.SanctionedLoad ?? 0;
            var loadUnit = (managerChangeData.Factory.SanctionedLoadUnit ?? "HP").ToUpper();

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

            if (managerChangeData.Factory.NumberOfWorker < 20)
            {
                Type = "Section 85";
            }
            else if (managerChangeData.Factory.NumberOfWorker > 40 && power == 0)
            {
                Type = "2 (1)(w)(ii)";
            }
            else if (managerChangeData.Factory.NumberOfWorker >= 20 && power > 0)
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
                .Add(new Paragraph($"Manager Change Application No.:- {managerChangeData.ApplicationNumber ?? "-"}")
                    .SetFont(boldFont).SetFontSize(10))
                .SetBorder(Border.NO_BORDER));

            _ = topRow.AddCell(new PdfCell()
                .Add(new Paragraph($"Dated:-  {DateTime.Now.ToString("dd/MM/yyyy")}")
                    .SetFont(boldFont).SetFontSize(10)
                    .SetTextAlignment(TextAlignment.RIGHT))
                .SetBorder(Border.NO_BORDER));

            _ = document.Add(topRow);

            var factoryAddress = $"{managerChangeData.Factory?.AddressLine1}, {managerChangeData.Factory?.AddressLine2},\n{managerChangeData.Factory?.Area}, {managerChangeData.Factory?.TehsilName},\n{managerChangeData.Factory?.SubDivisionName}, {managerChangeData.Factory?.DistrictName}, {managerChangeData.Factory?.Pincode}";

            // ═════════════════════════════════════════════════════════════════════════
            // Factory name + address
            // ═════════════════════════════════════════════════════════════════════════
            if (!string.IsNullOrWhiteSpace(managerChangeData.Factory.FactoryName))
            {
                _ = document.Add(new Paragraph(managerChangeData.Factory.FactoryName)
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
            _ = detailsTable.AddCell(RedCell(managerChangeData.Factory.ManufacturingDetail ?? "-", regularFont));

            _ = detailsTable.AddCell(RedCell("Type", boldFont));
            _ = detailsTable.AddCell(RedCell(Type, regularFont));

            _ = detailsTable.AddCell(RedCell("Category", boldFont));
            _ = detailsTable.AddCell(RedCell(managerChangeData.Factory.FactoryTypeName ?? "-", regularFont));

            _ = detailsTable.AddCell(RedCell("Workers", boldFont));
            _ = detailsTable.AddCell(RedCell(managerChangeData.Factory.NumberOfWorker.ToString() ?? "-", regularFont));

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

        void AddTwoColumnSection(
            Document document,
            string title,
            IEnumerable<(string Label, string? Value)> leftData,
            PdfFont boldFont,
            PdfFont regularFont)
        {
            document.Add(new Paragraph(title)
                .SetFont(boldFont)
                .SetFontSize(10)
                .SetMarginBottom(4));

            var table = new PdfTable(new float[] { 260f, 260f })
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER);

            var left = leftData.ToList();
            int maxRows = left.Count;

            for (int i = 0; i < maxRows; i++)
            {
                // LEFT COLUMN
                if (i < left.Count)
                {
                    var l = left[i];
                    table.AddCell(new PdfCell()
                        .Add(new Paragraph(l.Label).SetFont(boldFont).SetFontSize(9))
                        .SetBorder(Border.NO_BORDER)
                        .SetPaddingLeft(4));
                    table.AddCell(new PdfCell()
                        .Add(new Paragraph(l.Value ?? "—").SetFont(regularFont).SetFontSize(9))
                        .SetBorder(Border.NO_BORDER));
                }
                else
                {
                    table.AddCell(new PdfCell().SetBorder(Border.NO_BORDER));
                    table.AddCell(new PdfCell().SetBorder(Border.NO_BORDER));
                }
            }

            document.Add(table);
            document.Add(new Paragraph("\n").SetFontSize(4)); // spacing
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

        // ─────────────────────────────────────────────────────────────────────────────
        // Certificate helper cells  (add alongside your existing BuildHeaderCell etc.)
        // ─────────────────────────────────────────────────────────────────────────────
        private static byte[] GenerateQrCodePng(string url)
        {
            using var qrGenerator = new QRCoder.QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(url, QRCoder.QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCoder.PngByteQRCode(qrData);
            return qrCode.GetGraphic(5);
        }

        string Capitalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            input = input.ToLower();
            return char.ToUpper(input[0]) + input.Substring(1);
        }
    };
};
