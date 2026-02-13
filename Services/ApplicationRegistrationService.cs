using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static RajFabAPI.Constants.AppConstants;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RajFabAPI.Services
{
    public class ApplicationRegistrationService : IApplicationRegistrationService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ApplicationRegistrationService> _logger;
        private readonly IEstablishmentRegistrationService _establishmentService;

        public ApplicationRegistrationService(ApplicationDbContext db, ILogger<ApplicationRegistrationService> logger, IEstablishmentRegistrationService establishmentService)
        {
            _db = db;
            _logger = logger;
            _establishmentService = establishmentService;
        }

        public async Task<List<ApplicationRegistrationDto>> GetAllAsync()
        {
            _logger.LogInformation("Retrieving all application registrations");
            var result = await _db.Set<ApplicationRegistration>()
                .Select(a => new ApplicationRegistrationDto
                {
                    Id = Guid.Parse(a.Id),
                    ModuleId = a.ModuleId,
                    ApplicationId = Guid.Parse(a.ApplicationId),
                    CreatedDate = a.CreatedDate,
                    UpdatedDate = a.UpdatedDate
                })
                .ToListAsync();
            _logger.LogInformation("Retrieved {Count} application registrations", result.Count);
            return result;
        }

        public async Task<ApplicationRegistrationDto?> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Retrieving application registration with Id {Id}", id);
            var entity = await _db.Set<ApplicationRegistration>().FirstOrDefaultAsync(a => a.Id == id.ToString());
            if (entity == null)
            {
                _logger.LogWarning("Application registration with Id {Id} not found", id);
                return null;
            }
            _logger.LogInformation("Retrieved application registration with Id {Id}", id);
            return new ApplicationRegistrationDto
            {
                Id = Guid.Parse(entity.Id),
                ModuleId = entity.ModuleId,
                ApplicationId = Guid.Parse(entity.ApplicationId),
                CreatedDate = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate
            };
        }

        public async Task<Guid> CreateAsync(CreateApplicationRegistrationDto dto)
        {
            _logger.LogInformation("Creating new application registration for ModuleId {ModuleId}, ApplicationId {ApplicationId}", dto.ModuleId, dto.ApplicationId);
            var entity = new ApplicationRegistration
            {
                Id = Guid.NewGuid().ToString(),
                ModuleId = dto.ModuleId,
                ApplicationId = dto.ApplicationId.ToString(),
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };
            _db.Set<ApplicationRegistration>().Add(entity);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Application registration created with Id {Id}", entity.Id);
            return Guid.Parse(entity.Id);
        }

        public async Task<ApplicationRegistrationDto?> UpdateAsync(Guid id, ApplicationRegistrationDto dto)
        {
            _logger.LogInformation("Updating application registration with Id {Id}", id);
            var entity = await _db.Set<ApplicationRegistration>().FirstOrDefaultAsync(a => a.Id == id.ToString());
            if (entity == null)
            {
                _logger.LogWarning("Application registration with Id {Id} not found for update", id);
                return null;
            }
            entity.ModuleId = dto.ModuleId;
            entity.ApplicationId = dto.ApplicationId.ToString();
            entity.UpdatedDate = DateTime.Now;
            await _db.SaveChangesAsync();
            _logger.LogInformation("Application registration with Id {Id} updated successfully", id);
            return new ApplicationRegistrationDto
            {
                Id = id,
                ModuleId = entity.ModuleId,
                ApplicationId = dto.ApplicationId,
                CreatedDate = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting application registration with Id {Id}", id);
            var entity = await _db.Set<ApplicationRegistration>().FirstOrDefaultAsync(a => a.Id == id.ToString());
            if (entity == null)
            {
                _logger.LogWarning("Application registration with Id {Id} not found for deletion", id);
                return false;
            }
            _db.Set<ApplicationRegistration>().Remove(entity);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Application registration with Id {Id} deleted successfully", id);
            return true;
        }

        public async Task<List<ApplicationUserDashboardDto>> GetByUserIdAsync(Guid userId)
        {
            List<ApplicationUserDashboardDto> applicationUserDashboardDtos = new List<ApplicationUserDashboardDto>();
            var appRegistrationQuery = from appReg in _db.Set<ApplicationRegistration>()
                                       join module in _db.Set<Models.FormModule>() on appReg.ModuleId equals module.Id
                                       where appReg.UserId == userId
                                       orderby appReg.CreatedDate descending
                                       select new
                                       {
                                           appReg.Id,
                                           ApplicationTypeName = module.Name,
                                           appReg.ApplicationId,
                                           appReg.CreatedDate,
                                           appReg.ModuleId,
                                       };
            var appRegistrations = await appRegistrationQuery.ToListAsync();

            foreach (var appRegistration in appRegistrations)
            {
                if (appRegistration.ApplicationTypeName == ApplicationTypeNames.NewEstablishment || appRegistration.ApplicationTypeName == ApplicationTypeNames.FactoryAmendment || appRegistration.ApplicationTypeName == ApplicationTypeNames.FactoryRenewal)
                {
                    var estDetail = from estRegistration in _db.Set<EstablishmentRegistration>()
                                    join establishmentDetail in _db.Set<EstablishmentDetail>() on estRegistration.EstablishmentDetailId equals establishmentDetail.Id
                                    where estRegistration.EstablishmentRegistrationId.ToString() == appRegistration.ApplicationId.ToString()
                                    select new
                                    {
                                        estRegistration.Status,
                                        estRegistration.CreatedDate,
                                        establishmentDetail.EstablishmentName,
                                        estRegistration.IsESignCompleted,
                                        estRegistration.IsPaymentCompleted
                                    };
                    var estDetailSingle = await estDetail.FirstOrDefaultAsync();

                    applicationUserDashboardDtos.Add(new ApplicationUserDashboardDto
                    {
                        ApplicationRegistrationId = appRegistration.Id,
                        ApplicationType = appRegistration.ApplicationTypeName,
                        Status = estDetailSingle.Status,
                        CreatedDate = appRegistration.CreatedDate,
                        ApplicationId = Guid.Parse(appRegistration.ApplicationId),
                        ApplicationTitle = estDetailSingle != null ? estDetailSingle.EstablishmentName : "",
                        IsPaymentCompleted = estDetailSingle.IsPaymentCompleted,
                        IsESignCompleted = estDetailSingle.IsESignCompleted,
                    });
                }
                else if (appRegistration.ApplicationTypeName == ApplicationTypeNames.MapApproval)
                {
                    var mapApproval = _db.FactoryMapApprovals
                            .Include(x => x.MapApprovalFactoryDetails).FirstOrDefault(x => x.Id == appRegistration.ApplicationId);
                    if (mapApproval != null)
                    {
                        applicationUserDashboardDtos.Add(new ApplicationUserDashboardDto
                        {
                            ApplicationRegistrationId = appRegistration.Id,
                            ApplicationType = ApplicationTypeNames.MapApproval,
                            Status = mapApproval.Status,
                            CreatedDate = appRegistration.CreatedDate,
                            ApplicationId = Guid.Parse(appRegistration.ApplicationId),
                            ApplicationTitle = mapApproval != null ? mapApproval.MapApprovalFactoryDetails?.FactoryName : "",
                        });
                    }
                }
                else if (appRegistration.ApplicationTypeName == ApplicationTypeNames.FactoryCommencementCessation)
                {
                    var commCess = _db.CommencementCessationApplication
                            .FirstOrDefault(x => x.ApplicationId == appRegistration.ApplicationId);
                    var estReg = _db.EstablishmentRegistrations
                            .FirstOrDefault(x => x.RegistrationNumber == commCess.FactoryRegistrationNumber);
                    var estDetails = _db.EstablishmentDetails
                            .FirstOrDefault(x => x.Id == estReg.EstablishmentDetailId);
                    if (commCess != null)
                    {
                        applicationUserDashboardDtos.Add(new ApplicationUserDashboardDto
                        {
                            ApplicationRegistrationId = appRegistration.Id,
                            ApplicationType = ApplicationTypeNames.FactoryCommencementCessation,
                            Status = commCess.Status,
                            CreatedDate = appRegistration.CreatedDate,
                            ApplicationId = Guid.Parse(appRegistration.ApplicationId),
                            ApplicationTitle = estDetails != null ? estDetails.EstablishmentName : "",
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
                    var estReg = _db.EstablishmentRegistrations
                        .FirstOrDefault(x => x.EstablishmentRegistrationId == managerChange.FactoryRegistrationId.ToString());
                    var estDetails = _db.EstablishmentDetails
                        .FirstOrDefault(x => x.Id == estReg.EstablishmentDetailId);

                    if (managerChange != null)
                    {
                        applicationUserDashboardDtos.Add(new ApplicationUserDashboardDto
                        {
                            ApplicationRegistrationId = appRegistration.Id,
                            ApplicationType = ApplicationTypeNames.ManagerChange,
                            Status = managerChange.Status,
                            CreatedDate = appRegistration.CreatedDate,
                            ApplicationId = Guid.Parse(appRegistration.ApplicationId),
                            ApplicationTitle = estDetails.EstablishmentName
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

                    applicationUserDashboardDtos.Add(new ApplicationUserDashboardDto
                    {
                        ApplicationRegistrationId = appRegistration.Id,
                        ApplicationType = appRegistration.ApplicationTypeName, // FactoryLicense / Amendment / Renewal
                        Status = factoryLicense.Status,
                        CreatedDate = appRegistration.CreatedDate,
                        ApplicationId = Guid.Parse(factoryLicense.Id),
                        ApplicationTitle = estDetails?.EstablishmentName ?? "Factory License"
                    });
                }
            }
            return applicationUserDashboardDtos;
        }

        public async Task<EstablishmentRegistrationDetailsDto?> GetRegistrationDetailsAsync(string registrationNumber)
        {
            var applicationData = _db.ApplicationRegistrations.AsNoTracking().FirstOrDefault(x => x.ApplicationRegistrationNumber == registrationNumber);
            _logger.LogInformation("Retrieving establishment registration details for RegistrationId {registrationNumber}", registrationNumber);
            var details = await _establishmentService.GetRegistrationDetailsAsync(applicationData.ApplicationId);
            _logger.LogInformation("Retrieved establishment registration details for RegistrationId {RegistrationId}", registrationNumber);
            return details;
        }
        public async Task<bool> UpdatePaymentStatusAsync(
            string applicationId)
        {
            using var dbTx = await _db.Database.BeginTransactionAsync();

            try
            {
                var applicationData = await (
                    from appReg in _db.Set<ApplicationRegistration>()
                    join module in _db.Set<FormModule>()
                        on appReg.ModuleId equals module.Id
                    where appReg.ApplicationId == applicationId
                    select new
                    {
                        Application = appReg,
                        ModuleName = module.Name
                    }
                ).FirstOrDefaultAsync();

                if (applicationData == null)
                    return false;

                if (applicationData.ModuleName == ApplicationTypeNames.NewEstablishment)
                {

                    var establishment = await _db.Set<EstablishmentRegistration>()
                        .FirstOrDefaultAsync(x =>
                            x.EstablishmentRegistrationId.ToString() ==
                            applicationId);

                    if (establishment != null)
                    {
                        establishment.IsPaymentCompleted = true;
                        establishment.UpdatedDate = DateTime.UtcNow;
                    }

                    await _db.SaveChangesAsync();
                    await dbTx.CommitAsync();

                    return true;
                }

                return false;
            }
            catch
            {
                await dbTx.RollbackAsync();
                throw;
            }
        }

    }
}