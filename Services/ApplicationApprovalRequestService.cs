using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
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

        public ApplicationApprovalRequestService(ApplicationDbContext db, ILogger<ApplicationApprovalRequestService> logger, 
            IEstablishmentRegistrationService establishmentRegistrationService, 
            ICommencementCessationService commencementCessationService,
            IFactoryMapApprovalService factoryMapApprovalService
            )
        {
            _db = db;
            _logger = logger;
            _establishmentRegistrationService = establishmentRegistrationService;
            _commencementCessationService = commencementCessationService;
            _factoryMapApprovalService = factoryMapApprovalService;
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
                entity.UpdatedDate = DateTime.Now;
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
            catch(Exception ex)
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
                var appApprovalRequests = await _db.Set<ApplicationApprovalRequest>()
                    .Where(x => x.ApplicationWorkFlowLevelId == workflowLevel.Id)
                    .GroupBy(x => x.ApplicationRegistrationId)
                    .Select(g => g.OrderByDescending(x => x.CreatedDate).FirstOrDefault())
                    .ToListAsync();

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
                        var mapApproval = _db.FactoryMapApprovals
                            .Include(x => x.MapApprovalFactoryDetails).FirstOrDefault(x => x.Id == appRegistration.ApplicationId);
                        if (mapApproval != null)
                        {
                            result.Add(new ApplicationApprovalDashboardDto
                            {
                                ApprovalRequestId = item.Id,
                                ModuleId = appRegistration.ModuleId,
                                ApplicationId = Guid.Parse(appRegistration.ApplicationId),
                                CreatedDate = appRegistration.CreatedDate,
                                ApplicationType = appRegistration.ApplicationTypeName,
                                ApplicationTitle = mapApproval.MapApprovalFactoryDetails.FactoryName,
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
                            Status = factoryLicense.Status,
                            TotalEmployees = 0
                        });
                    }
                }
            }
            return result.OrderByDescending(x => x.CreatedDate).ToList();
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
        }

    }
}
