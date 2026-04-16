using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Models.BoilerModels;
using RajFabAPI.Models.FactoryModels;
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
        private readonly IBoilerDrawingService _boilerDrawingService;
        private readonly IBoilerManufactureService _boilerManufactureService;
        private readonly IBoilerRepairerService _boilerRepairerService;
        private readonly IEconomiserService _economiserService;
        private readonly ISteamPipeLineApplicationService _steamPipeLineService;
        private readonly IWelderApplicationService _welderService;
        private readonly ISMTCRegistrationService _smtcService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationApprovalRequestService(ApplicationDbContext db, ILogger<ApplicationApprovalRequestService> logger,
            IEstablishmentRegistrationService establishmentRegistrationService,
            ICommencementCessationService commencementCessationService,
            IFactoryMapApprovalService factoryMapApprovalService,
            IFactoryLicenseService factoryLicenseService,
            IBoilerRegistartionService boilerRegistrationService,
            IBoilerDrawingService boilerDrawingService,
            IBoilerManufactureService boilerManufactureService,
            IBoilerRepairerService boilerRepairerService,
            IEconomiserService economiserService,
            ISteamPipeLineApplicationService steamPipeLineService,
            IWelderApplicationService welderService,
            ISMTCRegistrationService smtcService,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _db = db;
            _logger = logger;
            _establishmentRegistrationService = establishmentRegistrationService;
            _commencementCessationService = commencementCessationService;
            _factoryMapApprovalService = factoryMapApprovalService;
            _factoryLicenseService = factoryLicenseService;
            _boilerRegistrationService = boilerRegistrationService;
            _boilerDrawingService = boilerDrawingService;
            _boilerManufactureService = boilerManufactureService;
            _boilerRepairerService = boilerRepairerService;
            _economiserService = economiserService;
            _steamPipeLineService = steamPipeLineService;
            _welderService = welderService;
            _smtcService = smtcService;
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
                        actionText = "Objection letter generated and Application returned to applicant";
                        commentText = entity.Remarks ?? "Application returned to applicant for correction.";
                        break;

                    case ApplicationStatus.SentBack:
                        string sentBackTarget = "Previous Level";
                        if (dto.TargetLevelNumber.HasValue)
                        {
                            var targetLevelInfo = await _db.Set<ApplicationWorkFlowLevel>()
                                .Where(wfl =>
                                    wfl.ApplicationWorkFlowId == workflowLevel.ApplicationWorkFlowId &&
                                    wfl.LevelNumber == dto.TargetLevelNumber.Value)
                                .Select(wfl => new { wfl.RoleId })
                                .FirstOrDefaultAsync();
                            if (targetLevelInfo != null)
                            {
                                var targetRoleInfo = await _db.Set<Role>()
                                    .Where(r => r.Id == targetLevelInfo.RoleId)
                                    .Select(r => new { Name = r.Post.Name + ", " + r.Office.City.Name })
                                    .FirstOrDefaultAsync();
                                sentBackTarget = targetRoleInfo?.Name ?? sentBackTarget;
                            }
                        }
                        actionText = $"Application Sent Back to {sentBackTarget}";
                        commentText = entity.Remarks ?? $"Application sent back to {sentBackTarget} for review.";
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
                else if (dto.Status == ApplicationStatus.SentBack)
                {
                    // Route to a previous level — TargetLevelNumber specifies which level
                    var targetLevelNumber = dto.TargetLevelNumber ?? (workflowLevel.LevelNumber - 1);
                    if (targetLevelNumber >= 1)
                    {
                        var targetLevel = await _db.Set<ApplicationWorkFlowLevel>()
                            .Where(wfl =>
                                wfl.ApplicationWorkFlowId == workflowLevel.ApplicationWorkFlowId &&
                                wfl.LevelNumber == targetLevelNumber)
                            .FirstOrDefaultAsync();

                        if (targetLevel != null)
                        {
                            var sentBackRequest = new ApplicationApprovalRequest
                            {
                                ModuleId = entity.ModuleId,
                                ApplicationRegistrationId = entity.ApplicationRegistrationId,
                                ApplicationWorkFlowLevelId = targetLevel.Id,
                                Status = ApplicationStatus.Pending,
                                Direction = "Backward",
                                CreatedBy = entity.CreatedBy,
                                CreatedDate = DateTime.Now,
                                UpdatedDate = DateTime.Now
                            };
                            _db.Set<ApplicationApprovalRequest>().Add(sentBackRequest);
                            await _db.SaveChangesAsync();
                            _logger.LogInformation("Created sent-back approval request at level {Level} for application registration {AppRegId}", targetLevelNumber, entity.ApplicationRegistrationId);
                        }
                    }
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
        public async Task<List<WorkflowLevelInfoDto>> GetPreviousLevelsAsync(int approvalRequestId)
        {
            var request = await _db.Set<ApplicationApprovalRequest>()
                .FirstOrDefaultAsync(a => a.Id == approvalRequestId);

            if (request == null) return new List<WorkflowLevelInfoDto>();

            var currentLevel = await _db.Set<ApplicationWorkFlowLevel>()
                .FirstOrDefaultAsync(l => l.Id == request.ApplicationWorkFlowLevelId);

            if (currentLevel == null || currentLevel.LevelNumber <= 1)
                return new List<WorkflowLevelInfoDto>();

            var previousLevels = await _db.Set<ApplicationWorkFlowLevel>()
                .Where(l => l.ApplicationWorkFlowId == currentLevel.ApplicationWorkFlowId
                         && l.LevelNumber < currentLevel.LevelNumber)
                .OrderBy(l => l.LevelNumber)
                .ToListAsync();

            var result = new List<WorkflowLevelInfoDto>();
            foreach (var level in previousLevels)
            {
                var roleName = await _db.Set<Role>()
                    .Where(r => r.Id == level.RoleId)
                    .Select(r => r.Post.Name + ", " + r.Office.City.Name)
                    .FirstOrDefaultAsync() ?? $"Level {level.LevelNumber}";

                result.Add(new WorkflowLevelInfoDto
                {
                    LevelNumber = level.LevelNumber,
                    RoleName = roleName
                });
            }

            return result;
        }

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
                var reg = await _db.FactoryMapApprovals.FindAsync(applicationId);
                if (reg == null) return;
                string? factoryTypeName = null;
                if (!string.IsNullOrWhiteSpace(reg.ProductName) &&
                    Guid.TryParse(reg.ProductName, out var ftGuid))
                {
                    var ft = await _db.FactoryTypes.FindAsync(ftGuid);
                    factoryTypeName = ft?.Name;
                }

                var mapDetail = await _db.MapApprovalFactoryDetails
                    .FirstOrDefaultAsync(d => d.FactoryMapApprovalId == applicationId);

                var address = string.Join(", ", new[]
                    { mapDetail?.FactoryPlotNo, mapDetail?.FactoryPincode }
                    .Where(x => !string.IsNullOrWhiteSpace(x)));

                fileUrl = await _factoryMapApprovalService.GenerateObjectionLetter(
                    new MapApprovalObjectionLetterDto
                    {
                        ApplicationId = reg.AcknowledgementNumber,
                        Date = DateTime.Today,
                        FactoryDetails = reg.FactoryDetails,
                        EstablishmentName = mapDetail?.FactoryName ?? "",
                        EstablishmentAddress = address,
                        Subject = subject,
                        PlantParticulars = reg.PlantParticulars,
                        ProductName = factoryTypeName,
                        ManufacturingProcess = reg.ManufacturingProcess,
                        MaxWorkers = reg.MaxWorkerMale + reg.MaxWorkerFemale + reg.MaxWorkerTransgender,
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

                EstablishmentRegistration? estReg = null;
                EstablishmentDetail? estDetail = null;
                FactoryDetail? factoryDetail = null;
                FactoryMapApproval? mapApprovalDetail = null;
                string? factoryTypeName = null;

                if (!string.IsNullOrWhiteSpace(license.FactoryRegistrationNumber))
                {
                    estReg = await _db.EstablishmentRegistrations
                        .FirstOrDefaultAsync(e => e.RegistrationNumber == license.FactoryRegistrationNumber && e.Status == ApplicationStatus.Approved);

                    if (estReg != null)
                    {
                        if (estReg.EstablishmentDetailId.HasValue)
                            estDetail = await _db.EstablishmentDetails.FindAsync(estReg.EstablishmentDetailId.Value);

                        // Get FactoryDetail via EstablishmentEntityMapping
                        factoryDetail = await (
                            from map in _db.EstablishmentEntityMapping
                            where map.EstablishmentRegistrationId == estReg.EstablishmentRegistrationId
                                && map.EntityType == "Factory"

                            join f in _db.Set<FactoryDetail>()
                                on map.EntityId equals f.Id

                            join area in _db.Set<Models.City>().AsNoTracking()
                                on f.SubDivisionId.ToString() equals area.Id.ToString() into areaJoin
                            from areaDetail in areaJoin.DefaultIfEmpty()

                            join district in _db.Set<District>().AsNoTracking()
                                on areaDetail.DistrictId equals district.Id into districtJoin
                            from districtDetail in districtJoin.DefaultIfEmpty()

                            join division in _db.Set<Division>().AsNoTracking()
                                on districtDetail.DivisionId equals division.Id into divisionJoin
                            from divisionDetail in divisionJoin.DefaultIfEmpty()

                            select new FactoryDetail
                            {
                                Id = f.Id,
                                ManufacturingType = f.ManufacturingType,
                                ManufacturingDetail = f.ManufacturingDetail,
                                Situation = f.Situation,
                                AddressLine1 = f.AddressLine1,
                                AddressLine2 = f.AddressLine2,
                                SubDivisionId = f.SubDivisionId,

                                SubDivisionName = areaDetail != null ? areaDetail.Name : null,
                                TehsilName = areaDetail != null ? areaDetail.Name : null,
                                DistrictName = districtDetail != null ? districtDetail.Name : null,
                                TehsilId = f.TehsilId,
                                Area = f.Area,
                                Pincode = f.Pincode,
                                Email = f.Email,
                                Telephone = f.Telephone,
                                Mobile = f.Mobile,
                                NumberOfWorker = f.NumberOfWorker,
                                SanctionedLoad = f.SanctionedLoad,
                                SanctionedLoadUnit = f.SanctionedLoadUnit,
                                EmployerId = f.EmployerId,
                                ManagerId = f.ManagerId,

                                OwnershipType = f.OwnershipType,
                                OwnershipSector = f.OwnershipSector,
                                ActivityAsPerNIC = f.ActivityAsPerNIC,
                                NICCodeDetail = f.NICCodeDetail,
                                IdentificationOfEstablishment = f.IdentificationOfEstablishment,

                                CreatedAt = f.CreatedAt,
                                UpdatedAt = f.UpdatedAt
                            }
                        ).FirstOrDefaultAsync();
                    }

                    mapApprovalDetail = await _db.FactoryMapApprovals
                        .FirstOrDefaultAsync(m => m.FactoryRegistrationNumber == license.FactoryRegistrationNumber);
                }

                if (mapApprovalDetail != null &&
                    !string.IsNullOrWhiteSpace(mapApprovalDetail.ProductName) &&
                    Guid.TryParse(mapApprovalDetail.ProductName, out var ftGuid))
                {
                    var ft = await _db.FactoryTypes.FindAsync(ftGuid);
                    factoryTypeName = ft?.Name;
                }

                var factoryAddress = $"{factoryDetail?.AddressLine1}, {factoryDetail?.AddressLine2},\n{factoryDetail?.Area}, {factoryDetail?.TehsilName},\n{factoryDetail?.SubDivisionName}, {factoryDetail?.DistrictName}, {factoryDetail?.Pincode}";

                var mapTotalWorkers = mapApprovalDetail != null
                    ? mapApprovalDetail.MaxWorkerMale + mapApprovalDetail.MaxWorkerFemale + mapApprovalDetail.MaxWorkerTransgender
                    : (int?)null;

                fileUrl = await _factoryLicenseService.GenerateObjectionLetter(
                    new LicenseObjectionLetterDto
                    {
                        ApplicationId = license.Id,
                        Date = DateTime.Today,
                        EstablishmentName = estDetail.EstablishmentName ?? "",
                        FactoryAddress = factoryAddress,
                        SanctionLoad = factoryDetail?.SanctionedLoad ?? 0,
                        SanctionLoadUnit = factoryDetail?.SanctionedLoadUnit ?? "",
                        Subject = subject,
                        LicenseNumber = license.FactoryLicenseNumber,
                        RegistrationNumber = license.FactoryRegistrationNumber,
                        ValidFrom = license.ValidFrom,
                        ValidTo = license.ValidTo,
                        NoOfYears = license.NoOfYears,
                        ManufacturingProcess = factoryDetail?.ManufacturingDetail ?? mapApprovalDetail?.ManufacturingProcess,
                        MaxWorkers = mapTotalWorkers,
                        FactoryTypeName = factoryTypeName,
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
            else if (moduleName == ApplicationTypeNames.BoilerDrawingRegistration ||
                     moduleName == ApplicationTypeNames.BoierDrawingRenewal)
            {
                var app = await _db.BoilerDrawingApplications.FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
                if (app != null)
                {
                    fileUrl = await _boilerDrawingService.GenerateObjectionLetter(
                        new BoilerObjectionLetterDto
                        {
                            ApplicationId = app.ApplicationId,
                            Date = DateTime.Today,
                            BoilerRegistrationNo = app.BoilerDrawingRegistrationNo,
                            OwnerName = "",
                            Address = "",
                            BoilerType = app.BoilerType,
                            HeatingSurfaceArea = decimal.TryParse(app.HeatingSurfaceArea, out var hsa) ? hsa : null,
                            EvaporationCapacity = decimal.TryParse(app.EvaporationCapacity, out var ec) ? ec : null,
                            WorkingPressure = decimal.TryParse(app.IntendedWorkingPressure, out var wp) ? wp : null,
                            Objections = objections,
                            SignatoryName = signatoryName,
                            SignatoryDesignation = signatoryDesignation,
                            SignatoryLocation = signatoryLocation
                        }, applicationId);
                }
            }
            else if (moduleName == ApplicationTypeNames.BoilerManufactureRegistration ||
                     moduleName == ApplicationTypeNames.BoilerManufactureAmend ||
                     moduleName == ApplicationTypeNames.BoilerManufactureRenewal)
            {
                var app = await _db.BoilerManufactureRegistrations.FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
                if (app != null)
                {
                    fileUrl = await _boilerManufactureService.GenerateObjectionLetter(
                        new BoilerObjectionLetterDto
                        {
                            ApplicationId = app.ApplicationId,
                            Date = DateTime.Today,
                            BoilerRegistrationNo = app.ManufactureRegistrationNo,
                            OwnerName = "",
                            Address = "",
                            BoilerCategory = app.BmClassification,
                            Objections = objections,
                            SignatoryName = signatoryName,
                            SignatoryDesignation = signatoryDesignation,
                            SignatoryLocation = signatoryLocation
                        }, applicationId);
                }
            }
            else if (moduleName == ApplicationTypeNames.BoilerRepairerRegistration ||
                     moduleName == ApplicationTypeNames.BoilerRepairerRenew)
            {
                var app = await _db.BoilerRepairerRegistrations.FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
                if (app != null)
                {
                    fileUrl = await _boilerRepairerService.GenerateObjectionLetter(
                        new BoilerObjectionLetterDto
                        {
                            ApplicationId = app.ApplicationId,
                            Date = DateTime.Today,
                            BoilerRegistrationNo = app.RepairerRegistrationNo,
                            OwnerName = "",
                            Address = "",
                            BoilerCategory = app.BrClassification,
                            Objections = objections,
                            SignatoryName = signatoryName,
                            SignatoryDesignation = signatoryDesignation,
                            SignatoryLocation = signatoryLocation
                        }, applicationId);
                }
            }
            else if (moduleName == ApplicationTypeNames.EconomiserRegistration ||
                     moduleName == ApplicationTypeNames.Economiserrenew)
            {
                var app = await _db.EconomiserRegistrations.FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
                if (app != null)
                {
                    fileUrl = await _economiserService.GenerateObjectionLetter(
                        new BoilerObjectionLetterDto
                        {
                            ApplicationId = app.ApplicationId,
                            Date = DateTime.Today,
                            BoilerRegistrationNo = app.EconomiserRegistrationNo,
                            OwnerName = "",
                            Address = "",
                            HeatingSurfaceArea = decimal.TryParse(app.TotalHeatingSurfaceArea, out var eHsa) ? eHsa : null,
                            WorkingPressure = decimal.TryParse(app.PressureTo, out var eWp) ? eWp : null,
                            Objections = objections,
                            SignatoryName = signatoryName,
                            SignatoryDesignation = signatoryDesignation,
                            SignatoryLocation = signatoryLocation
                        }, applicationId);
                }
            }
            else if (moduleName == ApplicationTypeNames.Stplregistration ||
                     moduleName == ApplicationTypeNames.StplAmendment ||
                     moduleName == ApplicationTypeNames.Stplrenew)
            {
                var app = await _db.SteamPipeLineApplications.FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
                if (app != null)
                {
                    fileUrl = await _steamPipeLineService.GenerateObjectionLetter(
                        new BoilerObjectionLetterDto
                        {
                            ApplicationId = app.ApplicationId,
                            Date = DateTime.Today,
                            BoilerRegistrationNo = app.SteamPipeLineRegistrationNo,
                            OwnerName = "",
                            Address = "",
                            Objections = objections,
                            SignatoryName = signatoryName,
                            SignatoryDesignation = signatoryDesignation,
                            SignatoryLocation = signatoryLocation
                        }, applicationId);
                }
            }
            else if (moduleName == ApplicationTypeNames.WelderRegistration ||
                     moduleName == ApplicationTypeNames.WelderRenew)
            {
                var app = await _db.WelderApplications.FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
                if (app != null)
                {
                    fileUrl = await _welderService.GenerateObjectionLetter(
                        new BoilerObjectionLetterDto
                        {
                            ApplicationId = app.ApplicationId,
                            Date = DateTime.Today,
                            BoilerRegistrationNo = app.WelderRegistrationNo,
                            OwnerName = "",
                            Address = "",
                            Objections = objections,
                            SignatoryName = signatoryName,
                            SignatoryDesignation = signatoryDesignation,
                            SignatoryLocation = signatoryLocation
                        }, applicationId);
                }
            }
            else if (moduleName == ApplicationTypeNames.SMTCRegistration)
            {
                var app = await _db.SMTCRegistrations.FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
                if (app != null)
                {
                    fileUrl = await _smtcService.GenerateObjectionLetter(
                        new BoilerObjectionLetterDto
                        {
                            ApplicationId = app.ApplicationId,
                            Date = DateTime.Today,
                            BoilerRegistrationNo = app.SMTCRegistrationNo,
                            OwnerName = "",
                            Address = "",
                            Objections = objections,
                            SignatoryName = signatoryName,
                            SignatoryDesignation = signatoryDesignation,
                            SignatoryLocation = signatoryLocation
                        }, applicationId);
                }
            }

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
            else if (module.Name == ApplicationTypeNames.BoilerRegistration ||
                     module.Name == ApplicationTypeNames.BoilerAmendment ||
                     module.Name == ApplicationTypeNames.BoilerRenewal)
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
            else if (module.Name == ApplicationTypeNames.BoilerManufactureRegistration ||
                     module.Name == ApplicationTypeNames.BoilerManufactureAmend ||
                     module.Name == ApplicationTypeNames.BoilerManufactureRenewal)
            {
                var reg = await _db.BoilerManufactureRegistrations
                    .FirstOrDefaultAsync(x => x.ApplicationId == regId);
                if (reg != null)
                {
                    reg.Status = status;
                    reg.UpdatedAt = DateTime.Now;
                    await _db.SaveChangesAsync();
                }
            }
            else if (module.Name == ApplicationTypeNames.BoilerRepairerRegistration ||
                     module.Name == ApplicationTypeNames.BoilerRepairerRenew)
            {
                var reg = await _db.BoilerRepairerRegistrations
                    .FirstOrDefaultAsync(x => x.ApplicationId == regId);
                if (reg != null)
                {
                    reg.Status = status;
                    reg.UpdatedAt = DateTime.Now;
                    await _db.SaveChangesAsync();
                }
            }
            else if (module.Name == ApplicationTypeNames.Stplregistration ||
                     module.Name == ApplicationTypeNames.StplAmendment ||
                     module.Name == ApplicationTypeNames.Stplrenew)
            {
                var reg = await _db.SteamPipeLineApplications
                    .FirstOrDefaultAsync(x => x.ApplicationId == regId);
                if (reg != null)
                {
                    reg.Status = status;
                    reg.UpdatedAt = DateTime.Now;
                    await _db.SaveChangesAsync();
                }
            }
            else if (module.Name == ApplicationTypeNames.EconomiserRegistration ||
                     module.Name == ApplicationTypeNames.Economiserrenew)
            {
                var reg = await _db.EconomiserRegistrations
                    .FirstOrDefaultAsync(x => x.ApplicationId == regId);
                if (reg != null)
                {
                    reg.Status = status;
                    reg.UpdatedDate = DateTime.Now;
                    await _db.SaveChangesAsync();
                }
            }
            else if (module.Name == ApplicationTypeNames.WelderRegistration ||
                     module.Name == ApplicationTypeNames.WelderRenew)
            {
                var reg = await _db.WelderApplications
                    .FirstOrDefaultAsync(x => x.ApplicationId == regId);
                if (reg != null)
                {
                    reg.Status = status;
                    reg.UpdatedDate = DateTime.Now;
                    await _db.SaveChangesAsync();
                }
            }
            else if (module.Name == ApplicationTypeNames.BoilerDrawingRegistration ||
                     module.Name == ApplicationTypeNames.BoierDrawingRenewal)
            {
                var reg = await _db.BoilerDrawingApplications
                    .FirstOrDefaultAsync(x => x.ApplicationId == regId);
                if (reg != null)
                {
                    reg.Status = status;
                    reg.UpdatedDate = DateTime.Now;
                    await _db.SaveChangesAsync();
                }
            }
            else if (module.Name == ApplicationTypeNames.SMTCRegistration)
            {
                var reg = await _db.SMTCRegistrations
                    .FirstOrDefaultAsync(x => x.ApplicationId == regId);
                if (reg != null)
                {
                    reg.Status = status;
                    reg.UpdatedAt = DateTime.Now;
                    await _db.SaveChangesAsync();
                }
            }
            else if (module.Name == ApplicationTypeNames.CompetentPersonRegistration ||
                     module.Name == ApplicationTypeNames.CompetentPersonRenewal)
            {
                var reg = await _db.CompetentPersonRegistrations
                    .FirstOrDefaultAsync(x => x.ApplicationId == regId);
                if (reg != null)
                {
                    reg.Status = status;
                    reg.UpdatedAt = DateTime.Now;
                    await _db.SaveChangesAsync();
                }
            }
            else if (module.Name == ApplicationTypeNames.CompetentEquipmentRegistration ||
                     module.Name == ApplicationTypeNames.CompetentEquipmentRenewal)
            {
                var reg = await _db.CompetentEquipmentRegistrations
                    .FirstOrDefaultAsync(x => x.ApplicationId == regId);
                if (reg != null)
                {
                    reg.Status = status;
                    reg.UpdatedAt = DateTime.Now;
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
