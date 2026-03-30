using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Models.BoilerModels;
using RajFabAPI.Services.Interface;
using static RajFabAPI.Constants.AppConstants;

namespace RajFabAPI.Services
{
    public class ApplicationApprovalRequestService : IApplicationApprovalRequestService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ApplicationApprovalRequestService> _logger;
        private readonly IEstablishmentRegistrationService _establishmentRegistrationService;
        private readonly ICommencementCessationService _commencementCessationService;
        private readonly IFactoryMapApprovalService _factoryMapApprovalService;
        private readonly IFactoryLicenseService _factoryLicenseService;
        private readonly IBoilerRegistartionService _boilerRegistrationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationApprovalRequestService(ApplicationDbContext db, ILogger<ApplicationApprovalRequestService> logger,
            IEstablishmentRegistrationService establishmentRegistrationService,
            ICommencementCessationService commencementCessationService,
            IFactoryMapApprovalService factoryMapApprovalService,
            IFactoryLicenseService factoryLicenseService,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _db = db;
            _logger = logger;
            _establishmentRegistrationService = establishmentRegistrationService;
            _commencementCessationService = commencementCessationService;
            _factoryMapApprovalService = factoryMapApprovalService;
            _factoryLicenseService = factoryLicenseService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int> CreateAsync(CreateApplicationApprovalRequestDto dto)
        {
            _logger.LogInformation("Creating new application approval request for ModuleId {ModuleId}, ApplicationRegistrationId {ApplicationRegistrationId}", dto.ModuleId, dto.ApplicationRegistrationId);
            var entity = new ApplicationApprovalRequest
            {
                ModuleId = dto.ModuleId,
                ApplicationRegistrationId = dto.ApplicationRegistrationId,
                ApplicationWorkFlowLevelId = dto.ApplicationWorkFlowLevelId,
                Status = dto.Status,
                Remarks = dto.Remarks,
                CreatedBy = dto.CreatedBy,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };
            _db.Set<ApplicationApprovalRequest>().Add(entity);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Application approval request created with Id {Id}", entity.Id);
            return entity.Id;
        }

        public async Task<List<ApplicationApprovalRequestDto>> GetAllAsync()
        {
            _logger.LogInformation("Retrieving all application approval requests");
            var result = await _db.Set<ApplicationApprovalRequest>()
                .Select(a => new ApplicationApprovalRequestDto
                {
                    Id = a.Id,
                    ModuleId = a.ModuleId,
                    ApplicationRegistrationId = a.ApplicationRegistrationId,
                    ApplicationWorkFlowLevelId = a.ApplicationWorkFlowLevelId,
                    Status = a.Status,
                    Remarks = a.Remarks,
                    CreatedBy = a.CreatedBy,
                    CreatedDate = a.CreatedDate,
                    UpdatedDate = a.UpdatedDate
                })
                .ToListAsync();
            _logger.LogInformation("Retrieved {Count} application approval requests", result.Count);
            return result;
        }

        public async Task<ApplicationApprovalRequestDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving application approval request with Id {Id}", id);
            var entity = await _db.Set<ApplicationApprovalRequest>().FirstOrDefaultAsync(a => a.Id == id);
            if (entity == null)
            {
                _logger.LogWarning("Application approval request with Id {Id} not found", id);
                return null;
            }
            _logger.LogInformation("Retrieved application approval request with Id {Id}", id);
            return new ApplicationApprovalRequestDto
            {
                Id = entity.Id,
                ModuleId = entity.ModuleId,
                ApplicationRegistrationId = entity.ApplicationRegistrationId,
                ApplicationWorkFlowLevelId = entity.ApplicationWorkFlowLevelId,
                Status = entity.Status,
                Remarks = entity.Remarks,
                CreatedBy = entity.CreatedBy,
                CreatedDate = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate
            };
        }

        public async Task<ApplicationApprovalRequestDto?> UpdateAsync(int id, UpdateApplicationApprovalRequestDto dto)
        {
            try
            {
                _logger.LogInformation("Updating application approval request with Id {Id} to Status {Status}", id, dto.Status);
                var entity = await _db.Set<ApplicationApprovalRequest>().FirstOrDefaultAsync(a => a.Id == id);
                if (entity == null)
                {
                    _logger.LogWarning("Application approval request with Id {Id} not found for update", id);
                    return null;
                }
                entity.Status = dto.Status;
                entity.Remarks = dto.Remarks;

                var appReg = await _db.Set<ApplicationRegistration>()
                .FirstOrDefaultAsync(r => r.Id == entity.ApplicationRegistrationId);

                if (appReg == null || appReg.ModuleId == Guid.Empty)
                    return null;

                var module = await _db.Set<FormModule>()
                    .FirstOrDefaultAsync(m => m.Id == appReg.ModuleId);

                if (module == null)
                    return null;

                var workflowLevel = await _db.Set<ApplicationWorkFlowLevel>()
                    .Where(wfl => wfl.Id == entity.ApplicationWorkFlowLevelId)
                    .OrderBy(wfl => wfl.LevelNumber)
                    .FirstOrDefaultAsync();

                if (workflowLevel == null)
                    return null;

                var roleInfo = await _db.Set<Role>()
                    .Where(r => r.Id == workflowLevel.RoleId)
                    .Select(r => new
                    {
                        Name = r.Post.Name + ", " + r.Office.City.Name,
                        PostId = r.PostId,
                        PostName = r.Post.Name,
                        OfficeCityName = r.Office.City.Name,
                    })
                    .FirstOrDefaultAsync();
                string actionText = "";
                string commentText = "";

                switch (entity.Status)
                {
                    case ApplicationStatus.Forwarded:
                        var nextLevel = await _db.Set<ApplicationWorkFlowLevel>()
                            .Where(wfl =>
                                wfl.ApplicationWorkFlowId == workflowLevel.ApplicationWorkFlowId &&
                                wfl.LevelNumber > workflowLevel.LevelNumber)
                            .OrderBy(wfl => wfl.LevelNumber)
                            .FirstOrDefaultAsync();

                        string nextRoleName = "Next Level";

                        if (nextLevel != null)
                        {
                            var nextRoleInfo = await _db.Set<Role>()
                                .Where(r => r.Id == nextLevel.RoleId)
                                .Select(r => new
                                {
                                    Name = r.Post.Name + ", " + r.Office.City.Name
                                })
                                .FirstOrDefaultAsync();

                            nextRoleName = nextRoleInfo?.Name ?? "Next Level";
                        }

                        actionText = $"Application Forwarded to {nextRoleName}";
                        commentText = entity.Remarks ?? $"Application forwarded to {nextRoleName}.";
                        break;

                    case ApplicationStatus.Approved:
                        actionText = "Application Approved";
                        commentText = entity.Remarks ?? "Application approved successfully.";
                        break;

                    case ApplicationStatus.Rejected:
                        actionText = "Application Rejected";
                        commentText = entity.Remarks ?? "Application rejected.";
                        break;

                    case ApplicationStatus.ReturnedToApplicant:
                        actionText = "Application Returned to Applicant";
                        commentText = entity.Remarks ?? "Application returned to applicant for correction.";
                        break;

                    default:
                        actionText = "Status Updated";
                        commentText = entity.Remarks ?? "";
                        break;
                }

                entity.UpdatedDate = DateTime.Now;

                var history = new ApplicationHistory
                {
                    ApplicationId = appReg.ApplicationId,
                    ApplicationType = module.Name,
                    Action = actionText,
                    Comments = commentText,
                    ActionBy = roleInfo?.PostId.ToString(),
                    ActionByName = roleInfo?.Name,
                    ActionDate = DateTime.Now
                };

                _db.ApplicationHistories.Add(history);

                await _db.SaveChangesAsync();
                _logger.LogInformation("Application approval request with Id {Id} updated successfully", id);

                if (dto.Status == ApplicationStatus.Forwarded)
                {
                    _logger.LogInformation("Status is Forwarded, finding next workflow level for Id {Id}", id);
                    var currentWorkflowLevel = await _db.Set<ApplicationWorkFlowLevel>().FirstOrDefaultAsync(level => level.Id == entity.ApplicationWorkFlowLevelId);

                    var nextLevelObj = await _db.Set<ApplicationWorkFlowLevel>()
                        .Where(wfl => wfl.ApplicationWorkFlowId == currentWorkflowLevel.ApplicationWorkFlowId && wfl.LevelNumber > currentWorkflowLevel.LevelNumber)
                        .OrderBy(wfl => wfl.LevelNumber)
                        .FirstOrDefaultAsync();
                    if (nextLevelObj != null)
                    {
                        var appApprovalReqObject = new CreateApplicationApprovalRequestDto()
                        {
                            ApplicationRegistrationId = entity.ApplicationRegistrationId,
                            ModuleId = entity.ModuleId,
                            Status = ApplicationStatus.Pending,
                            ApplicationWorkFlowLevelId = nextLevelObj.Id,
                            CreatedBy = entity.CreatedBy // or from context
                        };
                        var newId = await CreateAsync(appApprovalReqObject);
                        _logger.LogInformation("Created new application approval request with Id {NewId} for next level", newId);
                    }
                    else
                    {
                        _logger.LogWarning("No next workflow level found for current level Id {CurrentLevelId}", currentWorkflowLevel.Id);
                    }
                }
                else if (dto.Status == ApplicationStatus.Approved)
                {
                    await UpdateStatusInRespectiveApplications(entity, ApplicationStatus.Approved);
                }
                else if (dto.Status == ApplicationStatus.ReturnedToApplicant)
                {
                    await UpdateStatusInRespectiveApplications(entity, ApplicationStatus.ReturnedToApplicant);
                    var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirst("userId")?.Value;
                    await GenerateObjectionLetterAsync(module.Name, appReg.ApplicationId, entity.Remarks, roleInfo?.PostName, roleInfo?.OfficeCityName, currentUserId);
                }
                return new ApplicationApprovalRequestDto
                {
                    Id = entity.Id,
                    ModuleId = entity.ModuleId,
                    ApplicationRegistrationId = entity.ApplicationRegistrationId,
                    ApplicationWorkFlowLevelId = entity.ApplicationWorkFlowLevelId,
                    Status = entity.Status,
                    Remarks = entity.Remarks,
                    CreatedBy = entity.CreatedBy,
                    CreatedDate = entity.CreatedDate,
                    UpdatedDate = entity.UpdatedDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating application approval request with Id {Id}", id);
                throw;
            }
        }

        public async Task<List<ApplicationApprovalDashboardDto>> GetApplicationsByOfficePostIdAsync(Guid roleId)
        {
            _logger.LogInformation("Retrieving applications assigned to role Id {RoleId}", roleId);
            var result = new List<ApplicationApprovalDashboardDto>();

            var workflowLevels = await _db.Set<ApplicationWorkFlowLevel>().Where(x => x.RoleId == roleId).ToListAsync();

            if (!workflowLevels.Any())
                return result;
            foreach (var workflowLevel in workflowLevels)
            {
                var rawRequests = await _db.Set<ApplicationApprovalRequest>()
                    .Where(x => x.ApplicationWorkFlowLevelId == workflowLevel.Id)
                    .ToListAsync();
                var appApprovalRequests = rawRequests
                    .GroupBy(x => x.ApplicationRegistrationId)
                    .Select(g => g.OrderByDescending(x => x.CreatedDate).First())
                    .ToList();
                foreach (var item in appApprovalRequests)
                {
                    var appRegistrationQuery = from appReg in _db.ApplicationRegistrations
                                               join module in _db.Modules on appReg.ModuleId equals module.Id
                                               where appReg.Id.ToString().ToLower() == item.ApplicationRegistrationId.ToString().ToLower()
                                               select new
                                               {
                                                   ApplicationTypeName = module.Name,
                                                   appReg.ApplicationId,
                                                   appReg.CreatedDate,
                                                   appReg.ModuleId,
                                                   appReg.ApplicationRegistrationNumber
                                               };
                    var appRegistration = await appRegistrationQuery.FirstOrDefaultAsync();
                    if (appRegistration == null) continue;

                    if (appRegistration != null && (appRegistration.ApplicationTypeName == ApplicationTypeNames.NewEstablishment || appRegistration.ApplicationTypeName == ApplicationTypeNames.FactoryAmendment || appRegistration.ApplicationTypeName == ApplicationTypeNames.FactoryRenewal))
                    {
                        var estDetailQuery = from estRegistration in _db.Set<EstablishmentRegistration>()
                                             join establishmentDetail in _db.Set<EstablishmentDetail>() on estRegistration.EstablishmentDetailId equals establishmentDetail.Id
                                             where estRegistration.EstablishmentRegistrationId.ToString() == appRegistration.ApplicationId
                                             select establishmentDetail;
                        var estDetailSingle = await estDetailQuery.FirstOrDefaultAsync();

                        if (estDetailSingle != null)
                        {
                            result.Add(new ApplicationApprovalDashboardDto
                            {
                                ApprovalRequestId = item.Id,
                                ModuleId = appRegistration.ModuleId,
                                ApplicationId = Guid.Parse(appRegistration.ApplicationId),
                                CreatedDate = appRegistration.CreatedDate,
                                ApplicationType = appRegistration.ApplicationTypeName,
                                ApplicationTitle = estDetailSingle.EstablishmentName,
                                ApplicationRegistrationNumber = appRegistration.ApplicationRegistrationNumber,
                                Status = item.Status,
                                TotalEmployees = (estDetailSingle.TotalNumberOfEmployee + estDetailSingle.TotalNumberOfContractEmployee + estDetailSingle.TotalNumberOfInterstateWorker) ?? 0
                            });
                        }

                    }
                    else if (appRegistration != null && appRegistration.ApplicationTypeName == ApplicationTypeNames.MapApproval || appRegistration.ApplicationTypeName == ApplicationTypeNames.MapApprovalAmendment)
                    {
                        var mapApproval = _db.FactoryMapApprovals.FirstOrDefault(x => x.Id == appRegistration.ApplicationId);
                        if (mapApproval != null)
                        {
                            result.Add(new ApplicationApprovalDashboardDto
                            {
                                ApprovalRequestId = item.Id,
                                ModuleId = appRegistration.ModuleId,
                                ApplicationId = Guid.Parse(appRegistration.ApplicationId),
                                CreatedDate = appRegistration.CreatedDate,
                                ApplicationType = appRegistration.ApplicationTypeName,
                                ApplicationTitle = "Map Approval",
                                ApplicationRegistrationNumber = mapApproval.AcknowledgementNumber,
                                Status = item.Status,
                                TotalEmployees = (mapApproval.MaxWorkerMale + mapApproval.MaxWorkerFemale)
                            });
                        }
                    }
                    else if (appRegistration != null && appRegistration.ApplicationTypeName == ApplicationTypeNames.FactoryCommencementCessation)
                    {
                        var commCess = _db.CommencementCessationApplication
                            .FirstOrDefault(x => x.ApplicationId == appRegistration.ApplicationId);
                        var estReg = _db.EstablishmentRegistrations
                            .FirstOrDefault(x => x.RegistrationNumber == commCess.FactoryRegistrationNumber);
                        var estDetails = _db.EstablishmentDetails
                                .FirstOrDefault(x => x.Id == estReg.EstablishmentDetailId);
                        if (commCess != null)
                        {
                            result.Add(new ApplicationApprovalDashboardDto
                            {
                                ApprovalRequestId = item.Id,
                                ModuleId = appRegistration.ModuleId,
                                ApplicationId = Guid.Parse(appRegistration.ApplicationId),
                                CreatedDate = appRegistration.CreatedDate,
                                ApplicationType = appRegistration.ApplicationTypeName,
                                ApplicationTitle = estDetails != null ? estDetails.EstablishmentName : "",
                                ApplicationRegistrationNumber = "",
                                Status = item.Status,
                                TotalEmployees = (estDetails.TotalNumberOfEmployee + estDetails.TotalNumberOfContractEmployee + estDetails.TotalNumberOfInterstateWorker) ?? 0

                            });
                        }
                    }
                    else if (appRegistration.ApplicationTypeName == ApplicationTypeNames.ManagerChange)
                    {
                        // Parse ApplicationId safely
                        if (!Guid.TryParse(appRegistration.ApplicationId, out var managerChangeId))
                            continue; // skip if invalid Guid

                        var managerChange = _db.ManagerChanges
                            .FirstOrDefault(x => x.Id == managerChangeId);
                        var newManagerDetails = _db.PersonDetails
                            .FirstOrDefault(x => x.Id == managerChange.NewManagerId);
                        var oldManagerDetails = _db.ManagerChanges
                            .FirstOrDefault(x => x.Id == managerChange.NewManagerId);
                        var estReg = _db.EstablishmentRegistrations
                            .FirstOrDefault(x => x.EstablishmentRegistrationId == managerChange.FactoryRegistrationId.ToString());
                        var estDetails = _db.EstablishmentDetails
                            .FirstOrDefault(x => x.Id == estReg.EstablishmentDetailId);

                        if (managerChange != null)
                        {
                            result.Add(new ApplicationApprovalDashboardDto
                            {
                                ApprovalRequestId = item.Id,
                                ModuleId = appRegistration.ModuleId,
                                ApplicationId = managerChange.Id,
                                CreatedDate = appRegistration.CreatedDate,
                                ApplicationType = appRegistration.ApplicationTypeName,
                                ApplicationTitle = estDetails.EstablishmentName,
                                ApplicationRegistrationNumber = managerChange.AcknowledgementNumber,
                                Status = item.Status,
                                TotalEmployees = 0
                            });
                        }
                    }
                    else if (
                        appRegistration.ApplicationTypeName == ApplicationTypeNames.FactoryLicense ||
                        appRegistration.ApplicationTypeName == ApplicationTypeNames.FactoryLicenseAmendment ||
                        appRegistration.ApplicationTypeName == ApplicationTypeNames.FactoryLicenseRenewal)
                    {
                        var factoryLicense = _db.FactoryLicenses
                            .FirstOrDefault(x => x.Id == appRegistration.ApplicationId);

                        if (factoryLicense == null)
                            continue;

                        // Get establishment using FactoryRegistrationNumber
                        var estReg = _db.EstablishmentRegistrations
                            .FirstOrDefault(x => x.RegistrationNumber == factoryLicense.FactoryRegistrationNumber);

                        var estDetails = estReg != null
                            ? _db.EstablishmentDetails
                                .FirstOrDefault(x => x.Id == estReg.EstablishmentDetailId)
                            : null;

                        result.Add(new ApplicationApprovalDashboardDto
                        {
                            ApprovalRequestId = item.Id,
                            ModuleId = appRegistration.ModuleId,
                            ApplicationId = Guid.Parse(factoryLicense.Id),
                            CreatedDate = appRegistration.CreatedDate,
                            ApplicationType = appRegistration.ApplicationTypeName, // FactoryLicense / Amendment / Renewal
                            ApplicationTitle = estDetails?.EstablishmentName ?? "Factory License",
                            ApplicationRegistrationNumber = factoryLicense.FactoryLicenseNumber,
                            Status = item.Status,
                            TotalEmployees = 0
                        });
                    }
                    else if (appRegistration.ApplicationTypeName == ApplicationTypeNames.BoilerRegistration ||
                             appRegistration.ApplicationTypeName == ApplicationTypeNames.BoilerInspection)
                    {
                        var boilerReg = _db.BoilerRegistrations
                            .FirstOrDefault(x => x.ApplicationId == appRegistration.ApplicationId);

                        if (boilerReg == null)
                            continue;

                        result.Add(new ApplicationApprovalDashboardDto
                        {
                            ApprovalRequestId = item.Id,
                            ModuleId = appRegistration.ModuleId,
                            ApplicationId = boilerReg.Id,
                            CreatedDate = appRegistration.CreatedDate,
                            ApplicationType = appRegistration.ApplicationTypeName,
                            ApplicationTitle = boilerReg.BoilerRegistrationNo ?? appRegistration.ApplicationTypeName,
                            ApplicationRegistrationNumber = boilerReg.ApplicationId ?? "",
                            Status = item.Status,
                            TotalEmployees = 0
                        });
                    }
                }
            }
            return result
                .GroupBy(x => x.ApplicationId)
                .Select(g => g.OrderByDescending(x => x.CreatedDate).First())
                .OrderByDescending(x => x.CreatedDate)
                .ToList();
        }

        public async Task<bool> IsLastWorkflowLevelAsync(int applicationApprovalRequestId)
        {
            _logger.LogInformation("Checking if workflow level is last for application approval request Id {Id}", applicationApprovalRequestId);
            var request = await _db.Set<ApplicationApprovalRequest>().FirstOrDefaultAsync(a => a.Id == applicationApprovalRequestId);
            if (request == null)
            {
                _logger.LogWarning("Application approval request with Id {Id} not found", applicationApprovalRequestId);
                return false;
            }
            var currentLevel = await _db.Set<ApplicationWorkFlowLevel>().FirstOrDefaultAsync(l => l.Id == request.ApplicationWorkFlowLevelId);
            if (currentLevel == null)
            {
                _logger.LogWarning("Workflow level with Id {Id} not found", request.ApplicationWorkFlowLevelId);
                return false;
            }
            var hasNextLevel = await _db.Set<ApplicationWorkFlowLevel>()
                .AnyAsync(l => l.ApplicationWorkFlowId == currentLevel.ApplicationWorkFlowId && l.LevelNumber > currentLevel.LevelNumber);
            var isLast = !hasNextLevel;
            _logger.LogInformation("Workflow level for request Id {Id} is last: {IsLast}", applicationApprovalRequestId, isLast);
            return request.Status == "Pending";
        }

        public async Task<List<ObjectionLetterHistoryDto>> GetObjectionLettersByApplicationIdAsync(string applicationId)
        {
            return await _db.ApplicationObjectionLetters
                .Where(l => l.ApplicationId == applicationId)
                .OrderBy(l => l.Version)
                .Select(l => new ObjectionLetterHistoryDto
                {
                    Id = l.Id,
                    ApplicationId = l.ApplicationId,
                    ModuleName = l.ModuleName,
                    FileUrl = l.FileUrl,
                    Subject = l.Subject,
                    GeneratedByName = l.GeneratedByName,
                    SignatoryDesignation = l.SignatoryDesignation,
                    SignatoryLocation = l.SignatoryLocation,
                    Version = l.Version,
                    CreatedDate = l.CreatedDate
                })
                .ToListAsync();
        }

        public async Task<RemarkDetailsDto> GetRemarksByApplicationId(string registrationId)
        {
            var appReq = _db.ApplicationRegistrations.FirstOrDefault(x => x.ApplicationId == registrationId);
            var approvalReq = _db.ApplicationApprovalRequests.OrderByDescending(x => x.CreatedDate).FirstOrDefault(x => x.ApplicationRegistrationId == appReq.Id);
            var remarkDetails = new RemarkDetailsDto
            {
                RemarkGivenBy = "Approval authority",
                PendingSince = approvalReq.UpdatedDate,
                Remarks = approvalReq.Remarks
            };
            return remarkDetails;
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // Generate objection letter based on module name — called on ReturnedToApplicant
        // ─────────────────────────────────────────────────────────────────────────────
        private async Task GenerateObjectionLetterAsync(
            string moduleName,
            string applicationId,
            string? subject,
            string? signatoryDesignation,
            string? signatoryLocation,
            string? createdBy)
        {
            // Resolve signatory name from user record
            string? signatoryName = null;
            if (Guid.TryParse(createdBy, out var createdByGuid))
            {
                var user = await _db.Users.FindAsync(createdByGuid);
                signatoryName = user?.FullName;
            }

            // Next version number for this application
            var nextVersion = await _db.ApplicationObjectionLetters
                .CountAsync(l => l.ApplicationId == applicationId) + 1;

            var objections = subject?
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(o => o.Trim())
                .Where(o => !string.IsNullOrEmpty(o))
                .ToList() ?? new List<string>();

            string? fileUrl = null;

            if (moduleName == ApplicationTypeNames.NewEstablishment ||
                moduleName == ApplicationTypeNames.FactoryAmendment ||
                moduleName == ApplicationTypeNames.FactoryRenewal)
            {
                var reg = await _db.EstablishmentRegistrations
                    .FirstOrDefaultAsync(e => e.EstablishmentRegistrationId == applicationId);
                if (reg == null) return;

                EstablishmentDetail? detail = reg.EstablishmentDetailId.HasValue
                    ? await _db.EstablishmentDetails.FindAsync(reg.EstablishmentDetailId.Value)
                    : null;

                var factory = await (
                    from map in _db.EstablishmentEntityMapping
                    where map.EstablishmentRegistrationId == applicationId
                        && map.EntityType == "Factory"

                    join f in _db.Set<FactoryDetail>()
                        on map.EntityId equals f.Id

                    select f
                ).FirstOrDefaultAsync();
                    
                string? factoryTypeName = null;
                if (detail?.FactoryTypeId.HasValue == true)
                {
                    var factoryType = await _db.FactoryTypes.FindAsync(detail.FactoryTypeId.Value);
                    factoryTypeName = factoryType?.Name;
                }

                string? categoryName = null;
                if (reg.FactoryCategoryId.HasValue)
                {
                    var category = await _db.FactoryCategories.FindAsync(reg.FactoryCategoryId.Value);
                    categoryName = category?.Name;
                }

                var address = string.Join("\n", new[]
                    { detail?.AddressLine1, detail?.AddressLine2, detail?.Area, detail?.Pincode }
                    .Where(x => !string.IsNullOrWhiteSpace(x)));

                fileUrl = await _establishmentRegistrationService.GenerateObjectionLetter(
                    new EstablishmentObjectionLetterDto
                    {
                        ManufacturingType =  factory.ManufacturingType,
                        ApplicationId = reg.ApplicationId,
                        Date = DateTime.Today,
                        EstablishmentName = detail?.EstablishmentName ?? "",
                        EstablishmentAddress = address,
                        Subject = subject,
                        FactoryType = factoryTypeName,
                        Category = categoryName,
                        WorkerCount = detail?.TotalNumberOfEmployee,
                        Objections = objections,
                        SignatoryName = signatoryName,
                        SignatoryDesignation = signatoryDesignation,
                        SignatoryLocation = signatoryLocation
                    }, applicationId);
            }
            else if (moduleName == ApplicationTypeNames.MapApproval ||
                     moduleName == ApplicationTypeNames.MapApprovalAmendment)
            {
                var app = await _db.FactoryMapApprovals.FindAsync(applicationId);
                if (app == null) return;

                var mapDetail = await _db.MapApprovalFactoryDetails
                    .FirstOrDefaultAsync(d => d.FactoryMapApprovalId == applicationId);

                var address = string.Join(", ", new[]
                    { mapDetail?.FactoryPlotNo, mapDetail?.FactoryPincode }
                    .Where(x => !string.IsNullOrWhiteSpace(x)));

                fileUrl = await _factoryMapApprovalService.GenerateObjectionLetter(
                    new MapApprovalObjectionLetterDto
                    {
                        ApplicationId = app.AcknowledgementNumber,
                        Date = DateTime.Today,
                        FactoryDetails = app.FactoryDetails,
                        EstablishmentName = mapDetail?.FactoryName ?? "",
                        EstablishmentAddress = address,
                        Subject = subject,
                        PlantParticulars = app.PlantParticulars,
                        ProductName = app.ProductName,
                        ManufacturingProcess = app.ManufacturingProcess,
                        MaxWorkers = app.MaxWorkerMale + app.MaxWorkerFemale,
                        Objections = objections,
                        SignatoryName = signatoryName,
                        SignatoryDesignation = signatoryDesignation,
                        SignatoryLocation = signatoryLocation
                    }, applicationId);
            }
            else if (moduleName == ApplicationTypeNames.FactoryLicense ||
                     moduleName == ApplicationTypeNames.FactoryLicenseAmendment ||
                     moduleName == ApplicationTypeNames.FactoryLicenseRenewal)
            {
                var license = await _db.FactoryLicenses.FirstOrDefaultAsync(f => f.Id == applicationId);
                if (license == null) return;

                EstablishmentDetail? estDetail = null;
                if (!string.IsNullOrWhiteSpace(license.FactoryRegistrationNumber))
                {
                    var estReg = await _db.EstablishmentRegistrations
                        .FirstOrDefaultAsync(e => e.RegistrationNumber == license.FactoryRegistrationNumber);
                    if (estReg?.EstablishmentDetailId.HasValue == true)
                        estDetail = await _db.EstablishmentDetails.FindAsync(estReg.EstablishmentDetailId.Value);
                }

                var address = string.Join(", ", new[]
                    { estDetail?.AddressLine1, estDetail?.AddressLine2, estDetail?.Area, estDetail?.Pincode }
                    .Where(x => !string.IsNullOrWhiteSpace(x)));

                fileUrl = await _factoryLicenseService.GenerateObjectionLetter(
                    new LicenseObjectionLetterDto
                    {
                        ApplicationId = license.Id,
                        Date = DateTime.Today,
                        EstablishmentName = estDetail?.EstablishmentName ?? "",
                        EstablishmentAddress = address,
                        Subject = subject,
                        LicenseNumber = license.FactoryLicenseNumber,
                        RegistrationNumber = license.FactoryRegistrationNumber,
                        ValidFrom = license.ValidFrom,
                        ValidTo = license.ValidTo,
                        NoOfYears = license.NoOfYears,
                        Objections = objections,
                        SignatoryName = signatoryName,
                        SignatoryDesignation = signatoryDesignation,
                        SignatoryLocation = signatoryLocation
                    }, applicationId);
            }
            else if (moduleName == ApplicationTypeNames.BoilerRegistration)
            {
                var app = await _db.BoilerRegistrations.FindAsync(applicationId);
                if (app == null) return;

                var boiler = await _db.BoilerDetails
                    .FirstOrDefaultAsync(b => b.BoilerRegistrationId == app.Id);
                var address = string.Join(", ", new string[] {     boiler?.AddressLine1 ,  boiler?.AddressLine2,  boiler?.Area,  boiler?.PinCode?.ToString() }.Where(x => !string.IsNullOrWhiteSpace(x)));

                fileUrl = await _boilerRegistrationService.GenerateObjectionLetter(
                    new BoilerObjectionLetterDto
                    {
                        ApplicationId = app.ApplicationId, // e.g. BR2026/CIFB/0003
                        Date = DateTime.Today,

                        BoilerRegistrationNo = app.BoilerRegistrationNo,
                        OwnerName = "", // optional if you have
                        Address = address,

                        BoilerType = boiler?.BoilerType.ToString(),
                        BoilerCategory = boiler?.BoilerCategory.ToString(),
                        HeatingSurfaceArea = boiler?.HeatingSurfaceArea,
                        EvaporationCapacity = boiler?.EvaporationCapacity,
                        WorkingPressure = boiler?.IntendedWorkingPressure,
                        YearOfMake = boiler?.YearOfMake,

                        Objections = objections,

                        SignatoryName = signatoryName,
                        SignatoryDesignation = signatoryDesignation,
                        SignatoryLocation = signatoryLocation
                    },
                    applicationId.ToString()
                );
            }
            // Boiler and other modules — no objection letter

            // Save history record
            if (!string.IsNullOrEmpty(fileUrl))
            {
                _db.ApplicationObjectionLetters.Add(new ApplicationObjectionLetter
                {
                    ApplicationId = applicationId,
                    ModuleName = moduleName,
                    FileUrl = fileUrl,
                    Subject = subject,
                    GeneratedBy = createdBy,
                    GeneratedByName = signatoryName,
                    SignatoryDesignation = signatoryDesignation,
                    SignatoryLocation = signatoryLocation,
                    Version = nextVersion,
                    CreatedDate = DateTime.Now
                });
                await _db.SaveChangesAsync();
            }
        }

        private async Task UpdateStatusInRespectiveApplications(ApplicationApprovalRequest entity, string status)
        {
            var appRegDetail = _db.ApplicationRegistrations.FirstOrDefault(x => x.Id == entity.ApplicationRegistrationId && x.ModuleId == entity.ModuleId);
            var regId = appRegDetail?.ApplicationId;
            var module = _db.Modules.FirstOrDefault(x => x.Id == appRegDetail.ModuleId);

            if ((module.Name == ApplicationTypeNames.NewEstablishment) || (module.Name == ApplicationTypeNames.FactoryAmendment) || (module.Name == ApplicationTypeNames.FactoryRenewal))
            {
                await _establishmentRegistrationService.UpdateStatusAndRemark(regId, status);
            }
            else if (module.Name == ApplicationTypeNames.MapApproval || (module.Name == ApplicationTypeNames.MapApprovalAmendment))
            {
                await _factoryMapApprovalService.UpdateStatusAndRemark(regId, status);
            }
            else if (module.Name == ApplicationTypeNames.FactoryCommencementCessation)
            {
                await _commencementCessationService.UpdateStatusAndRemark(regId, status);
            }
            else if (module.Name == ApplicationTypeNames.FactoryLicense ||
                     module.Name == ApplicationTypeNames.FactoryLicenseAmendment ||
                     module.Name == ApplicationTypeNames.FactoryLicenseRenewal)
            {
                await _factoryLicenseService.UpdateStatusAndRemark(regId, status);
            }
            else if (module.Name == ApplicationTypeNames.BoilerRegistration)
            {
                var boilerReg = await _db.BoilerRegistrations
                    .FirstOrDefaultAsync(x => x.ApplicationId == regId);

                if (boilerReg != null)
                {
                    boilerReg.Status = status;
                    boilerReg.UpdatedAt = DateTime.Now;
                    await _db.SaveChangesAsync();
                }
            }
            else if (module.Name == ApplicationTypeNames.BoilerInspection)
            {
                var boilerReg = await _db.BoilerRegistrations
                    .FirstOrDefaultAsync(x => x.ApplicationId == regId);

                if (boilerReg != null)
                {
                    if (status == ApplicationStatus.Approved)
                    {
                        boilerReg.Status = ApplicationStatus.Approved;
                        boilerReg.UpdatedAt = DateTime.Now;

                        // Generate certificate
                        var regNo = boilerReg.BoilerRegistrationNo ?? boilerReg.ApplicationId ?? boilerReg.Id.ToString();
                        var inspectionModule = await _db.Modules.FirstOrDefaultAsync(m => m.Name == ApplicationTypeNames.BoilerInspection);
                        var existingVersion = await _db.Certificates
                            .Where(c => c.RegistrationNumber == regNo)
                            .MaxAsync(c => (decimal?)c.CertificateVersion) ?? 0m;
                        _db.Certificates.Add(new Certificate
                        {
                            RegistrationNumber = regNo,
                            StartDate = DateTime.Now,
                            EndDate = DateTime.Now.AddYears(1),
                            IssuedAt = DateTime.Now,
                            Status = "Issued",
                            ModuleId = inspectionModule?.Id ?? Guid.Empty,
                            CertificateVersion = existingVersion + 1m,
                            ApplicationId = boilerReg.ApplicationId ?? "",
                            CertificateUrl = "",
                            Remarks = "Boiler Inspection Certificate"
                        });
                    }
                    else
                    {
                        boilerReg.Status = status;
                        boilerReg.UpdatedAt = DateTime.Now;
                    }
                    await _db.SaveChangesAsync();
                }
            }
        }

    }
}
