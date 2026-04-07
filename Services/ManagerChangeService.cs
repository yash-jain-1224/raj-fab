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

        public async Task<ManagerChangeGetResponseDto> GetByIdAsync(Guid managerChangeId)
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

                    // Factory DTO
                    Factory = (factory == null && establishment == null) ? null : new FactoryBasicDto
                    {
                        FactoryRegistrationNumber = mc.FactoryRegistrationNumber,

                        // Name from Establishment
                        FactoryName = establishment != null ? establishment.EstablishmentName : null,

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
                        DistrictName = districtData != null ? districtData.Name : null
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

            if (managerChangeDto == null)
                throw new Exception("Manager change application not found");

            return managerChangeDto;
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
            var data = await GetByIdAsync(managerChangeId);

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
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new PageBorderAndFooterEventHandler(boldFont, regularFont, footerDate));

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

// ─────────────────────────────────────────────
// SECTION 1 – Factory Details
// ─────────────────────────────────────────────
document.Add(new Paragraph("1. Factory Details")
    .SetFont(boldFont)
    .SetFontSize(10)
    .SetMarginBottom(4));

var factoryTable = new PdfTable(new float[] { 260f, 260f })
    .UseAllAvailableWidth()
    .SetBorder(Border.NO_BORDER);

// Left column cells
var factoryLeft = new (string Label, string? Value)[]
{
    ("Factory Name:", data.Factory?.FactoryName),
    ("Factory Registration No.:", data.Factory?.FactoryRegistrationNumber?.ToString()),
    ("Application No.:", data.ApplicationNumber),
};

// Right column cells
var factoryRight = new (string Label, string? Value)[]
{
    ("Address:", $"{data.Factory?.AddressLine1}, {data.Factory?.AddressLine2}"),
    ("District:", data.Factory?.DistrictName),
    ("Pincode:", data.Factory?.Pincode),
};

// Add left column cells (label + value)
foreach (var (label, value) in factoryLeft)
{
    factoryTable.AddCell(new PdfCell()
        .Add(new Paragraph(label).SetFont(boldFont).SetFontSize(9))
        .SetBorder(Border.NO_BORDER).SetPaddingLeft(4));
    factoryTable.AddCell(new PdfCell()
        .Add(new Paragraph(value ?? "—").SetFont(regularFont).SetFontSize(9))
        .SetBorder(Border.NO_BORDER));
}

// Add right column cells (label + value)
foreach (var (label, value) in factoryRight)
{
    factoryTable.AddCell(new PdfCell()
        .Add(new Paragraph(label).SetFont(boldFont).SetFontSize(9))
        .SetBorder(Border.NO_BORDER).SetPaddingLeft(4));
    factoryTable.AddCell(new PdfCell()
        .Add(new Paragraph(value ?? "—").SetFont(regularFont).SetFontSize(9))
        .SetBorder(Border.NO_BORDER));
}

document.Add(factoryTable);
document.Add(new Paragraph("\n").SetFontSize(4)); // spacing

// ─────────────────────────────────────────────
// SECTION 2 – Outgoing Manager
// ─────────────────────────────────────────────
document.Add(new Paragraph("2. Outgoing Manager")
    .SetFont(boldFont)
    .SetFontSize(10)
    .SetMarginBottom(4));

var outgoingTable = new PdfTable(new float[] { 260f, 260f })
    .UseAllAvailableWidth()
    .SetBorder(Border.NO_BORDER);

var outgoingLeft = new (string Label, string? Value)[]
{
    ("Name:", data.OldManager?.Name),
    ("Designation:", data.OldManager?.Designation),
    ($"{data.OldManager?.RelationType?.ToUpper()} Name:", data.OldManager?.RelativeName),
    ("Address:", $"{data.OldManager?.AddressLine1}, {data.OldManager?.AddressLine2}"),
};

var outgoingRight = new (string Label, string? Value)[]
{
    ("District:", data.OldManager?.District),
    ("Tehsil:", data.OldManager?.Tehsil),
    ("Area:", data.OldManager?.Area),
    ("Pincode:", data.OldManager?.Pincode),
    ("Mobile:", data.OldManager?.Mobile),
};

foreach (var (label, value) in outgoingLeft)
{
    outgoingTable.AddCell(new PdfCell()
        .Add(new Paragraph(label).SetFont(boldFont).SetFontSize(9))
        .SetBorder(Border.NO_BORDER).SetPaddingLeft(4));
    outgoingTable.AddCell(new PdfCell()
        .Add(new Paragraph(value ?? "—").SetFont(regularFont).SetFontSize(9))
        .SetBorder(Border.NO_BORDER));
}

foreach (var (label, value) in outgoingRight)
{
    outgoingTable.AddCell(new PdfCell()
        .Add(new Paragraph(label).SetFont(boldFont).SetFontSize(9))
        .SetBorder(Border.NO_BORDER).SetPaddingLeft(4));
    outgoingTable.AddCell(new PdfCell()
        .Add(new Paragraph(value ?? "—").SetFont(regularFont).SetFontSize(9))
        .SetBorder(Border.NO_BORDER));
}

document.Add(outgoingTable);
document.Add(new Paragraph("\n").SetFontSize(4)); // spacing

// ─────────────────────────────────────────────
// SECTION 3 – New Manager
// ─────────────────────────────────────────────
document.Add(new Paragraph("3. New Manager")
    .SetFont(boldFont)
    .SetFontSize(10)
    .SetMarginBottom(4));

var newManagerTable = new PdfTable(new float[] { 260f, 260f })
    .UseAllAvailableWidth()
    .SetBorder(Border.NO_BORDER);

var newManagerLeft = new (string Label, string? Value)[]
{
    ("Name:", data.NewManager?.Name),
    ("Designation:", data.NewManager?.Designation),
    ($"{data.NewManager?.RelationType?.ToUpper()} Name:", data.NewManager?.RelativeName),
    ("Address:", $"{data.NewManager?.AddressLine1}, {data.NewManager?.AddressLine2}"),
};

var newManagerRight = new (string Label, string? Value)[]
{
    ("District:", data.NewManager?.District),
    ("Tehsil:", data.NewManager?.Tehsil),
    ("Area:", data.NewManager?.Area),
    ("Pincode:", data.NewManager?.Pincode),
    ("Mobile:", data.NewManager?.Mobile),
};

foreach (var (label, value) in newManagerLeft)
{
    newManagerTable.AddCell(new PdfCell()
        .Add(new Paragraph(label).SetFont(boldFont).SetFontSize(9))
        .SetBorder(Border.NO_BORDER).SetPaddingLeft(4));
    newManagerTable.AddCell(new PdfCell()
        .Add(new Paragraph(value ?? "—").SetFont(regularFont).SetFontSize(9))
        .SetBorder(Border.NO_BORDER));
}

foreach (var (label, value) in newManagerRight)
{
    newManagerTable.AddCell(new PdfCell()
        .Add(new Paragraph(label).SetFont(boldFont).SetFontSize(9))
        .SetBorder(Border.NO_BORDER).SetPaddingLeft(4));
    newManagerTable.AddCell(new PdfCell()
        .Add(new Paragraph(value ?? "—").SetFont(regularFont).SetFontSize(9))
        .SetBorder(Border.NO_BORDER));
}

document.Add(newManagerTable);
document.Add(new Paragraph("\n").SetFontSize(4)); // spacing

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

        // ─────────────────────────────────────────────────────────────────────────────
        // Draws a border rectangle on every page
        // ─────────────────────────────────────────────────────────────────────────────
        private sealed class PageBorderEventHandler : AbstractPdfDocumentEventHandler
        {
            protected override void OnAcceptedEvent(AbstractPdfDocumentEvent @event)
            {
                if (@event is not PdfDocumentEvent docEvent) return;
                var page = docEvent.GetPage();
                var rect = page.GetPageSize();
                var canvas = new PdfCanvas(page);
                canvas
                    .SetStrokeColor(new DeviceRgb(20, 57, 92))
                    .SetLineWidth(1.5f)
                    .Rectangle(25, 25, rect.GetWidth() - 50, rect.GetHeight() - 50)
                    .Stroke();
                canvas.Release();
            }
        }


        private sealed class PageBorderAndFooterEventHandler : AbstractPdfDocumentEventHandler
        {
            private readonly PdfFont _boldFont;
            private readonly PdfFont _regularFont;
            private readonly DateOnly _date;
            private readonly string _postName;
            private readonly string _userName;
            private readonly byte[]? _qrBytes;

            public PageBorderAndFooterEventHandler(PdfFont boldFont, PdfFont regularFont, DateOnly date, byte[]? qrBytes = null, string postName = "", string userName = "")
            {
                _boldFont = boldFont;
                _regularFont = regularFont;
                _date = date;
                _qrBytes = qrBytes;
                _postName = postName;
                _userName = userName;
            }

            protected override void OnAcceptedEvent(AbstractPdfDocumentEvent @event)
            {
                if (@event is not PdfDocumentEvent docEvent) return;
                var pdfDoc = docEvent.GetDocument();
                var page = docEvent.GetPage();
                var rect = page.GetPageSize();
                var canvas = new PdfCanvas(page);

                // ───── Page Border
                canvas
                    .SetStrokeColor(ColorConstants.BLACK)
                    .SetLineWidth(1.5f)
                    .Rectangle(25, 25, rect.GetWidth() - 50, rect.GetHeight() - 50)
                    .Stroke();

                // ───── Separator Line (above footer)
                float lineY = 70f;
                canvas
                    .SetStrokeColor(new DeviceRgb(180, 180, 180))
                    .SetLineWidth(0.5f)
                    .MoveTo(30, lineY)
                    .LineTo(rect.GetWidth() - 30, lineY)
                    .Stroke();

                canvas.Release();

                // ─────────────────────────────────────────────────────────────────────
                // ABOVE SEPARATOR: [Scanner image same height and widht as above(left)] | [Signature dashed box (right)]
                // ─────────────────────────────────────────────────────────────────────
                float scannerHeight = 65f;
                float zoneY = lineY + 4f;
                float pageWidth = rect.GetWidth();
                float signColWidth = 180f;
                float zoneWidth = pageWidth - 60f;
                float scanColWidth = zoneWidth - signColWidth - 8f; // 8pt gap

                // Left: QR image — same height and width as scannerHeight
                if (_qrBytes != null)
                {
                    using var scannerCanvas = new Canvas(new PdfCanvas(page),
                        new iText.Kernel.Geom.Rectangle(30f, zoneY, scannerHeight, scannerHeight));
                    scannerCanvas.Add(new PdfImage(ImageDataFactory.Create(_qrBytes))
                        .ScaleToFit(scannerHeight, scannerHeight)
                        .SetHorizontalAlignment(HorizontalAlignment.LEFT));
                }

                // Right: signature box — no border, no background (empty space)
                float signBoxX = 30f + scanColWidth + 8f;

                // ─────────────────────────────────────────────────────────────────────
                // BELOW SEPARATOR: Dated (left) | Page N (center) | Signature label (right)
                // ─────────────────────────────────────────────────────────────────────
                float belowY = lineY - 4f - scannerHeight;
                int pageNumber = pdfDoc.GetPageNumber(page);

                // Left: Dated
                using (var leftCanvas = new Canvas(new PdfCanvas(page),
                    new iText.Kernel.Geom.Rectangle(30f, belowY, scanColWidth / 2f, scannerHeight)))
                {
                    leftCanvas.Add(new Paragraph($"Dated: {_date}")
                        .SetFont(_regularFont).SetFontSize(7.5f).SetMargin(0f).SetPaddingTop(6f));
                }

                // Center: Page N
                using (var centerCanvas = new Canvas(new PdfCanvas(page),
                    new iText.Kernel.Geom.Rectangle(0f, belowY, pageWidth, scannerHeight)))
                {
                    centerCanvas.Add(new Paragraph($"Page {pageNumber}")
                        .SetFont(_regularFont).SetFontSize(7.5f)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMargin(0f).SetPaddingTop(6f));
                }

                // Right: signature label — aligned under the signature box above
                using (var signLabelCanvas = new Canvas(new PdfCanvas(page),
                    new iText.Kernel.Geom.Rectangle(signBoxX, belowY, signColWidth, scannerHeight)))
                {
                    if (!string.IsNullOrWhiteSpace(_userName))
                    {
                        // Name (top)
                        signLabelCanvas.Add(new Paragraph($"({_userName ?? "-"})")
                            .SetFont(_boldFont)
                            .SetFontSize(7f)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMargin(0f)
                            .SetPaddingTop(4f));
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
                        .SetMargin(0f));
                }
            }
        }
    };
};
