using iText.Commons.Actions.Contexts;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Models.BoilerModels;
using RajFabAPI.Models.FactoryModels;
using RajFabAPI.Services.Interface;
using Serilog.Context;
using System.Reflection;
using System.Text.Json;
using static RajFabAPI.Constants.AppConstants;
using static System.Net.Mime.MediaTypeNames;

namespace RajFabAPI.Services
{
    public class ApplicationRegistrationService : IApplicationRegistrationService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ApplicationRegistrationService> _logger;
        private readonly IEstablishmentRegistrationService _establishmentService;
        private readonly IApplicationWorkFlowService _applicationWorkFlowService;
        private readonly IWebHostEnvironment _environment;
        public ApplicationRegistrationService(IWebHostEnvironment environment, ApplicationDbContext db, ILogger<ApplicationRegistrationService> logger, IEstablishmentRegistrationService establishmentService, IApplicationWorkFlowService applicationWorkFlowService)
        {
            _db = db;
            _logger = logger;
            _establishmentService = establishmentService;
            _applicationWorkFlowService = applicationWorkFlowService;
            _environment = environment;
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
                    var latestStatus = await _db.Transactions
                        .Where(x => x.ApplicationId == appRegistration.ApplicationId)
                        .OrderByDescending(x => x.Id)
                        .Select(x => x.Status)
                        .FirstOrDefaultAsync();

                    applicationUserDashboardDtos.Add(new ApplicationUserDashboardDto
                    {
                        ApplicationRegistrationId = appRegistration.Id,
                        ApplicationType = appRegistration.ApplicationTypeName,
                        Status = estDetailSingle.Status,
                        CreatedDate = appRegistration.CreatedDate,
                        ApplicationId = appRegistration.ApplicationId,
                        ApplicationTitle = estDetailSingle != null ? estDetailSingle.EstablishmentName : "",
                        IsPaymentCompleted = estDetailSingle.IsPaymentCompleted,
                        IsESignCompleted = estDetailSingle.IsESignCompleted,
                        IsPaymentPending = latestStatus == "PENDING"
                    });
                }
                else if (appRegistration.ApplicationTypeName == ApplicationTypeNames.MapApproval || appRegistration.ApplicationTypeName == ApplicationTypeNames.MapApprovalAmendment)
                {
                    var mapApproval = _db.FactoryMapApprovals.FirstOrDefault(x => x.Id == appRegistration.ApplicationId);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var factoryDetails = JsonSerializer.Deserialize<FactoryDetailsModel>(
                        mapApproval.FactoryDetails,
                        options
                    );

                    if (mapApproval != null)
                    {
                        applicationUserDashboardDtos.Add(new ApplicationUserDashboardDto
                        {
                            ApplicationRegistrationId = appRegistration.Id,
                            ApplicationType = appRegistration.ApplicationTypeName,
                            Status = mapApproval.Status,
                            CreatedDate = appRegistration.CreatedDate,
                            ApplicationId = appRegistration.ApplicationId,
                            ApplicationTitle = factoryDetails != null ? factoryDetails.name : "",
                            IsESignCompleted = mapApproval.IsESignCompleted,
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
                            ApplicationId = appRegistration.ApplicationId,
                            ApplicationTitle = estDetails != null ? estDetails.EstablishmentName : "",
                            IsESignCompleted = commCess.IsESignCompleted,
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
                            ApplicationId = appRegistration.ApplicationId,
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
                        ApplicationId = factoryLicense.Id,
                        ApplicationTitle = estDetails?.EstablishmentName ?? "Factory License",
                        IsPaymentCompleted = factoryLicense.IsPaymentCompleted,
                        //IsESignCompleted = factoryLicense.IsESignCompletedManager && factoryLicense.IsESignCompletedOccupier
                        IsESignCompleted = factoryLicense.IsESignCompletedOccupier
                    });
                }
                else if (appRegistration.ApplicationTypeName == ApplicationTypeNames.BoilerRegistration)
                {
                    var boilerReg = await _db.Set<BoilerRegistration>()
                        .FirstOrDefaultAsync(x => x.ApplicationId == appRegistration.ApplicationId);

                    if (boilerReg == null)
                        continue;

                    var latestStatus = await _db.Transactions
                        .Where(x => x.ApplicationId == appRegistration.ApplicationId)
                        .OrderByDescending(x => x.Id)
                        .Select(x => x.Status)
                        .FirstOrDefaultAsync();

                    applicationUserDashboardDtos.Add(new ApplicationUserDashboardDto
                    {
                        ApplicationRegistrationId = appRegistration.Id,
                        ApplicationType = appRegistration.ApplicationTypeName,
                        Status = boilerReg.Status,
                        CreatedDate = appRegistration.CreatedDate,
                        ApplicationId = appRegistration.ApplicationId,
                        ApplicationTitle = boilerReg.BoilerRegistrationNo ?? "Boiler Registration",
                        IsPaymentCompleted = boilerReg.IsPaymentCompleted,
                        IsESignCompleted = boilerReg.IsESignCompleted,
                        IsPaymentPending = latestStatus == "PENDING"
                    });
                }
                else if (
                    appRegistration.ApplicationTypeName == ApplicationTypeNames.Appeal)
                {
                    var appeal = _db.Appeals
                        .FirstOrDefault(x => x.Id == appRegistration.ApplicationId);

                    if (appeal == null)
                        continue;

                    // Get establishment using FactoryRegistrationNumber
                    var estReg = _db.EstablishmentRegistrations
                        .FirstOrDefault(x => x.RegistrationNumber == appeal.FactoryRegistrationNumber);

                    var estDetails = estReg != null
                        ? _db.EstablishmentDetails
                            .FirstOrDefault(x => x.Id == estReg.EstablishmentDetailId)
                        : null;

                    applicationUserDashboardDtos.Add(new ApplicationUserDashboardDto
                    {
                        ApplicationRegistrationId = appRegistration.Id,
                        ApplicationType = appRegistration.ApplicationTypeName, // FactoryLicense / Amendment / Renewal
                        Status = appeal.Status,
                        CreatedDate = appRegistration.CreatedDate,
                        ApplicationId = appeal.Id,
                        ApplicationTitle = estDetails?.EstablishmentName ?? "Appeal",
                        IsESignCompleted = appeal.IsESignCompleted
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

                if (applicationData.ModuleName == ApplicationTypeNames.NewEstablishment || applicationData.ModuleName == ApplicationTypeNames.FactoryAmendment || applicationData.ModuleName == ApplicationTypeNames.FactoryRenewal)
                {
                    var estReg = await _db.Set<EstablishmentRegistration>()
                        .FirstOrDefaultAsync(x =>
                            x.EstablishmentRegistrationId.ToString() ==
                            applicationId);

                    if (estReg != null)
                    {
                        estReg.IsPaymentCompleted = true;
                        estReg.UpdatedDate = DateTime.Now;
                    }

                    await _db.SaveChangesAsync();
                    await dbTx.CommitAsync();

                    return true;
                }
                else if (applicationData.ModuleName == ApplicationTypeNames.FactoryLicense || applicationData.ModuleName == ApplicationTypeNames.FactoryLicenseAmendment || applicationData.ModuleName == ApplicationTypeNames.FactoryLicenseRenewal)
                {
                    var factoryLicense = await _db.Set<FactoryLicense>()
                        .FirstOrDefaultAsync(x =>
                            x.Id.ToString() ==
                            applicationId);

                    if (factoryLicense != null)
                    {
                        factoryLicense.IsPaymentCompleted = true;
                        factoryLicense.UpdatedAt = DateTime.Now;
                    }

                    await _db.SaveChangesAsync();
                    await dbTx.CommitAsync();

                    return true;
                }
                else if (applicationData.ModuleName == ApplicationTypeNames.BoilerRegistration)
                {
                    var boilerReg = await _db.Set<BoilerRegistration>()
                        .FirstOrDefaultAsync(x => x.ApplicationId == applicationId);

                    if (boilerReg != null)
                    {
                        boilerReg.IsPaymentCompleted = true;
                        boilerReg.UpdatedAt = DateTime.Now;
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

        public async Task<bool> UpdateApplicationESignData(string prnNumber, string signedPDFBase64)
        {
            _logger.LogInformation("===== UpdateApplicationESignData STARTED =====");

            using (LogContext.PushProperty("PRN", prnNumber))
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(prnNumber))
                    {
                        _logger.LogWarning("PRN number is empty");
                        return false;
                    }

                    _logger.LogInformation("Fetching ApplicationRegistration for PRN: {PRN}", prnNumber);

                    var appReg = await _db.Set<ApplicationRegistration>()
                        .FirstOrDefaultAsync(r => r.ESignPrnNumber == prnNumber);

                    if (appReg == null)
                    {
                        _logger.LogWarning("ApplicationRegistration not found for PRN: {PRN}", prnNumber);
                        return false;
                    }

                    if (appReg.ModuleId == Guid.Empty)
                    {
                        _logger.LogWarning("ModuleId is empty for ApplicationRegistrationId: {Id}", appReg.Id);
                        return false;
                    }

                    _logger.LogInformation("ApplicationRegistration found -> Id: {Id}, ModuleId: {ModuleId}", appReg.Id, appReg.ModuleId);

                    var module = await _db.Set<FormModule>()
                        .FirstOrDefaultAsync(m => m.Id == appReg.ModuleId);

                    if (module == null)
                    {
                        _logger.LogWarning("Module not found for ModuleId: {ModuleId}", appReg.ModuleId);
                        return false;
                    }

                    _logger.LogInformation("Module identified: {ModuleName}", module.Name);

                    byte[]? pdfBytes = null;

                    if (!string.IsNullOrWhiteSpace(signedPDFBase64))
                    {
                        _logger.LogInformation("Converting signed PDF Base64 to byte array");

                        string cleanBase64 = signedPDFBase64.Contains(",")
                            ? signedPDFBase64.Split(',')[1]
                            : signedPDFBase64;

                        pdfBytes = Convert.FromBase64String(cleanBase64);

                        _logger.LogInformation("PDF converted successfully. Byte length: {Length}", pdfBytes.Length);
                    }

                    int totalWorkers = 0;
                    Guid? factoryTypeId = null;
                    Guid? subDivisionId = null;
                    string applicationUrl = null;
                    Guid? factoryCategoryId = null;

                    using var transaction = await _db.Database.BeginTransactionAsync();

                    _logger.LogInformation("Database transaction started");

                    if (module.Name == ApplicationTypeNames.NewEstablishment ||
                        module.Name == ApplicationTypeNames.FactoryAmendment ||
                        module.Name == ApplicationTypeNames.FactoryRenewal)
                    {
                        _logger.LogInformation("Processing Establishment module workflow");

                        var estReg = await _db.Set<EstablishmentRegistration>()
                            .FirstOrDefaultAsync(x => x.EstablishmentRegistrationId == appReg.ApplicationId);

                        if (estReg == null)
                        {
                            _logger.LogWarning("EstablishmentRegistration not found for ApplicationId: {AppId}", appReg.ApplicationId);
                            return false;
                        }

                        var estDetails = await _db.Set<EstablishmentDetail>()
                            .FirstOrDefaultAsync(ed => ed.Id == estReg.EstablishmentDetailId);

                        var entityData = await _db.Set<EstablishmentEntityMapping>()
                            .FirstOrDefaultAsync(ed => ed.EstablishmentRegistrationId == estReg.EstablishmentRegistrationId);

                        if (entityData == null)
                        {
                            _logger.LogWarning("Entity mapping not found");
                            return false;
                        }

                        FactoryDetail? factorydata = null;

                        if (entityData.EntityType == "Factory")
                        {
                            factorydata = await _db.Set<FactoryDetail>()
                                .FirstOrDefaultAsync(f => f.Id == entityData.EntityId);
                        }

                        if (factorydata == null)
                        {
                            _logger.LogWarning("Factory details not found");
                            return false;
                        }

                        if (!Guid.TryParse(factorydata.SubDivisionId, out Guid parsedSubDiv))
                        {
                            _logger.LogWarning("Invalid SubDivisionId: {SubDivision}", factorydata.SubDivisionId);
                            return false;
                        }

                        totalWorkers = factorydata.NumberOfWorker ?? 0;
                        //totalWorkers =
                        //    (estDetails.TotalNumberOfEmployee ?? 0) +
                        //    (estDetails.TotalNumberOfContractEmployee ?? 0) +
                        //    (estDetails.TotalNumberOfInterstateWorker ?? 0);

                        factoryTypeId = estDetails.FactoryTypeId ?? Guid.Empty;
                        subDivisionId = parsedSubDiv;

                        _logger.LogInformation(
                            "Computed Workers: {Workers}, FactoryTypeId: {FactoryTypeId}, SubDivisionId: {SubDivisionId}",
                            totalWorkers,
                            factoryTypeId,
                            subDivisionId
                        );

                        estReg.IsESignCompleted = true;
                        estReg.UpdatedDate = DateTime.Now;
                        applicationUrl = estReg.ApplicationPDFUrl;
                    }
                    else if (module.Name == ApplicationTypeNames.MapApproval ||
                             module.Name == ApplicationTypeNames.MapApprovalAmendment)
                    {
                        _logger.LogInformation("Processing MapApproval workflow");

                        var mapReg = await _db.Set<FactoryMapApproval>()
                            .FirstOrDefaultAsync(x => x.Id == appReg.ApplicationId);

                        if (mapReg == null)
                        {
                            _logger.LogWarning("MapApproval record not found");
                            return false;
                        }

                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        var factoryDetails = JsonSerializer.Deserialize<FactoryDetailsModel>(
                            mapReg.FactoryDetails,
                            options
                        );

                        if (!Guid.TryParse(factoryDetails.subDivisionId, out Guid parsedSubDiv))
                        {
                            _logger.LogWarning("Invalid SubDivisionId from MapApproval");
                            return false;
                        }

                        totalWorkers =
                            (mapReg?.MaxWorkerMale ?? 0) +
                            (mapReg?.MaxWorkerFemale ?? 0) +
                            (mapReg?.MaxWorkerTransgender ?? 0);

                        Guid.TryParse(mapReg?.ProductName, out Guid parsedFactoryTypeId);
                        factoryTypeId = parsedFactoryTypeId;
                        subDivisionId = parsedSubDiv;

                        _logger.LogInformation(
                            "MapApproval Workers: {Workers}, FactoryTypeId: {FactoryTypeId}, SubDivisionId: {SubDivisionId}",
                            totalWorkers,
                            factoryTypeId,
                            subDivisionId
                        );

                        mapReg.IsESignCompleted = true;
                        mapReg.UpdatedAt = DateTime.Now;
                        applicationUrl = mapReg.ApplicationPDFUrl;
                    }
                    else if (module.Name == ApplicationTypeNames.FactoryLicense ||
                         module.Name == ApplicationTypeNames.FactoryLicenseRenewal ||
                         module.Name == ApplicationTypeNames.FactoryLicenseAmendment)
                    {
                        _logger.LogInformation("Processing FactoryLicense workflow for ApplicationId: {ApplicationId}", appReg.ApplicationId);

                        var factoryLicenseReg = await _db.Set<FactoryLicense>()
                            .FirstOrDefaultAsync(x => x.Id == appReg.ApplicationId);

                        if (factoryLicenseReg == null)
                        {
                            _logger.LogWarning("FactoryLicense record not found for ApplicationId: {ApplicationId}", appReg.ApplicationId);
                            return false;
                        }

                        _logger.LogInformation("Fetching EstablishmentDetailId using RegistrationNumber: {RegNo}", factoryLicenseReg.FactoryRegistrationNumber);

                        var EstablishmentDetailId = await _db.Set<EstablishmentRegistration>()
                            .Where(m => m.RegistrationNumber == factoryLicenseReg.FactoryRegistrationNumber)
                            .Select(m => m.EstablishmentDetailId)
                            .FirstOrDefaultAsync();

                        if (EstablishmentDetailId == Guid.Empty)
                        {
                            _logger.LogWarning("EstablishmentDetailId not found for RegistrationNumber: {RegNo}", factoryLicenseReg.FactoryRegistrationNumber);
                            return false;
                        }

                        var establishmentDetail = await _db.Set<EstablishmentDetail>()
                            .FirstOrDefaultAsync(m => m.Id == EstablishmentDetailId);

                        if (establishmentDetail == null)
                        {
                            _logger.LogWarning("EstablishmentDetail not found for Id: {Id}", EstablishmentDetailId);
                            return false;
                        }

                        if (!Guid.TryParse(establishmentDetail.SubDivisionId, out Guid parsedSubDiv))
                        {
                            _logger.LogWarning("Invalid SubDivisionId: {SubDivisionId}", establishmentDetail.SubDivisionId);
                            return false;
                        }

                        totalWorkers =
                           (establishmentDetail?.TotalNumberOfEmployee ?? 0) +
                           (establishmentDetail?.TotalNumberOfContractEmployee ?? 0) +
                           (establishmentDetail?.TotalNumberOfInterstateWorker ?? 0);

                        factoryTypeId = establishmentDetail.FactoryTypeId;
                        subDivisionId = parsedSubDiv;

                        _logger.LogInformation(
                            "FactoryLicense Workers calculated: {Workers}, FactoryTypeId: {FactoryTypeId}, SubDivisionId: {SubDivisionId}",
                            totalWorkers,
                            factoryTypeId,
                            subDivisionId
                        );

                        factoryLicenseReg.IsESignCompletedOccupier = true;
                        factoryLicenseReg.UpdatedAt = DateTime.Now;
                        applicationUrl = factoryLicenseReg.ApplicationPDFUrl;

                        _logger.LogInformation("FactoryLicense ESign completed for ApplicationId: {ApplicationId}", appReg.ApplicationId);
                    }

                    else if (module.Name == ApplicationTypeNames.FactoryCommencementCessation)
                    {
                        _logger.LogInformation("Processing FactoryCommencementCessation workflow for ApplicationId: {ApplicationId}", appReg.ApplicationId);

                        var comReg = await _db.Set<CommencementCessationApplication>()
                            .FirstOrDefaultAsync(x => x.ApplicationId == appReg.ApplicationId);

                        if (comReg == null)
                        {
                            _logger.LogWarning("CommencementCessationApplication not found for ApplicationId: {ApplicationId}", appReg.ApplicationId);
                            return false;
                        }

                        _logger.LogInformation("Fetching EstablishmentDetailId using RegistrationNumber: {RegNo}", comReg.FactoryRegistrationNumber);

                        var EstablishmentDetailId = await _db.Set<EstablishmentRegistration>()
                            .Where(m => m.RegistrationNumber == comReg.FactoryRegistrationNumber)
                            .Select(m => m.EstablishmentDetailId)
                            .FirstOrDefaultAsync();

                        var establishmentDetail = await _db.Set<EstablishmentDetail>()
                            .FirstOrDefaultAsync(m => m.Id == EstablishmentDetailId);

                        if (establishmentDetail == null)
                        {
                            _logger.LogWarning("EstablishmentDetail not found for Id: {Id}", EstablishmentDetailId);
                            return false;
                        }

                        if (!Guid.TryParse(establishmentDetail.SubDivisionId, out Guid parsedSubDiv))
                        {
                            _logger.LogWarning("Invalid SubDivisionId: {SubDivisionId}", establishmentDetail.SubDivisionId);
                            return false;
                        }

                        totalWorkers =
                           (establishmentDetail?.TotalNumberOfEmployee ?? 0) +
                           (establishmentDetail?.TotalNumberOfContractEmployee ?? 0) +
                           (establishmentDetail?.TotalNumberOfInterstateWorker ?? 0);

                        factoryTypeId = establishmentDetail.FactoryTypeId;
                        subDivisionId = parsedSubDiv;

                        _logger.LogInformation(
                            "CommencementCessation Workers calculated: {Workers}, FactoryTypeId: {FactoryTypeId}, SubDivisionId: {SubDivisionId}",
                            totalWorkers,
                            factoryTypeId,
                            subDivisionId
                        );

                        comReg.IsESignCompleted = true;
                        comReg.UpdatedDate = DateTime.Now;
                        applicationUrl = comReg.ApplicationPDFUrl;

                        _logger.LogInformation("CommencementCessation ESign completed for ApplicationId: {ApplicationId}", appReg.ApplicationId);
                    }

                    else if (module.Name == ApplicationTypeNames.Appeal)
                    {
                        _logger.LogInformation("Processing Appeal workflow for ApplicationId: {ApplicationId}", appReg.ApplicationId);

                        var appealReg = await _db.Set<Appeal>()
                            .FirstOrDefaultAsync(x => x.AppealApplicationNumber == appReg.ApplicationId);

                        if (appealReg == null)
                        {
                            _logger.LogWarning("Appeal record not found for ApplicationId: {ApplicationId}", appReg.ApplicationId);
                            return false;
                        }

                        _logger.LogInformation("Fetching EstablishmentDetailId using RegistrationNumber: {RegNo}", appealReg.FactoryRegistrationNumber);

                        var EstablishmentDetailId = await _db.Set<EstablishmentRegistration>()
                            .Where(m => m.RegistrationNumber == appealReg.FactoryRegistrationNumber)
                            .Select(m => m.EstablishmentDetailId)
                            .FirstOrDefaultAsync();

                        var establishmentDetail = await _db.Set<EstablishmentDetail>()
                            .FirstOrDefaultAsync(m => m.Id == EstablishmentDetailId);

                        if (establishmentDetail == null)
                        {
                            _logger.LogWarning("EstablishmentDetail not found for Id: {Id}", EstablishmentDetailId);
                            return false;
                        }

                        if (!Guid.TryParse(establishmentDetail.SubDivisionId, out Guid parsedSubDiv))
                        {
                            _logger.LogWarning("Invalid SubDivisionId: {SubDivisionId}", establishmentDetail.SubDivisionId);
                            return false;
                        }

                        totalWorkers =
                           (establishmentDetail?.TotalNumberOfEmployee ?? 0) +
                           (establishmentDetail?.TotalNumberOfContractEmployee ?? 0) +
                           (establishmentDetail?.TotalNumberOfInterstateWorker ?? 0);

                        factoryTypeId = establishmentDetail.FactoryTypeId;
                        subDivisionId = parsedSubDiv;

                        _logger.LogInformation(
                            "Appeal Workers calculated: {Workers}, FactoryTypeId: {FactoryTypeId}, SubDivisionId: {SubDivisionId}",
                            totalWorkers,
                            factoryTypeId,
                            subDivisionId
                        );

                        appealReg.IsESignCompleted = true;
                        appealReg.UpdatedAt = DateTime.Now;
                        applicationUrl = appealReg.ApplicationPDFUrl;

                        _logger.LogInformation("Appeal ESign completed for ApplicationId: {ApplicationId}", appReg.ApplicationId);
                    }

                    else if (module.Name == ApplicationTypeNames.BoilerRegistration)
                    {
                        _logger.LogInformation("Processing BoilerRegistration workflow for ApplicationId: {ApplicationId}", appReg.ApplicationId);

                        var boilerReg = await _db.Set<BoilerRegistration>()
                            .Include(b => b.BoilerDetail)
                            .FirstOrDefaultAsync(x => x.ApplicationId == appReg.ApplicationId);

                        if (boilerReg == null)
                        {
                            _logger.LogWarning("BoilerRegistration not found for ApplicationId: {ApplicationId}", appReg.ApplicationId);
                            return false;
                        }

                        if (boilerReg.BoilerDetail?.SubDivisionId == null)
                        {
                            _logger.LogWarning("BoilerDetail SubDivisionId missing for ApplicationId: {ApplicationId}", appReg.ApplicationId);
                            return false;
                        }

                        subDivisionId = boilerReg.BoilerDetail.SubDivisionId.Value;
                        totalWorkers = 60;
                        factoryTypeId = Guid.Parse("C3AE3BD6-E45D-4D61-A09C-25365A7EA099");

                        _logger.LogInformation(
                            "BoilerRegistration Workers: {Workers}, FactoryTypeId: {FactoryTypeId}, SubDivisionId: {SubDivisionId}",
                            totalWorkers,
                            factoryTypeId,
                            subDivisionId
                        );

                        boilerReg.IsESignCompleted = true;
                        boilerReg.UpdatedAt = DateTime.Now;
                        applicationUrl = boilerReg.ApplicationPDFUrl;

                        _logger.LogInformation("BoilerRegistration ESign completed for ApplicationId: {ApplicationId}", appReg.ApplicationId);
                    }

                    _logger.LogInformation("Determining WorkerRange for workers: {Workers}", totalWorkers);

                    var workerRange = await _db.Set<WorkerRange>()
                        .FirstOrDefaultAsync(wr =>
                            totalWorkers >= wr.MinWorkers &&
                            totalWorkers <= wr.MaxWorkers);

                    if (workerRange != null)
                    {
                        factoryCategoryId = await _db.Set<FactoryCategory>()
                            .Where(fc =>
                                fc.WorkerRangeId == workerRange.Id &&
                                fc.FactoryTypeId == factoryTypeId)
                            .Select(fc => (Guid?)fc.Id)
                            .FirstOrDefaultAsync();
                    }

                    _logger.LogInformation("FactoryCategoryId resolved: {FactoryCategoryId}", factoryCategoryId);

                    var officeId = await _db.Set<OfficeApplicationArea>()
                        .Where(oaa => oaa.CityId == subDivisionId)
                        .Select(oaa => (Guid?)oaa.OfficeId)
                        .FirstOrDefaultAsync();

                    if (!officeId.HasValue)
                    {
                        _logger.LogWarning("Office not found for SubDivisionId: {SubDivisionId}", subDivisionId);
                        return false;
                    }

                    _logger.LogInformation("OfficeId resolved: {OfficeId}", officeId);

                    var workflow = await _db.Set<ApplicationWorkFlow>()
                        .FirstOrDefaultAsync(wf =>
                            wf.ModuleId == appReg.ModuleId &&
                            wf.FactoryCategoryId == factoryCategoryId &&
                            wf.OfficeId == officeId.Value);

                    if (workflow == null)
                    {
                        _logger.LogWarning("Workflow not found for ModuleId: {ModuleId}", appReg.ModuleId);
                        return false;
                    }

                    var workflowLevel = await _db.Set<ApplicationWorkFlowLevel>()
                        .Where(wfl => wfl.ApplicationWorkFlowId == workflow.Id)
                        .OrderBy(wfl => wfl.LevelNumber)
                        .FirstOrDefaultAsync();

                    if (workflowLevel == null)
                    {
                        _logger.LogWarning("Workflow level not found");
                        return false;
                    }

                    var roleInfo = await _db.Set<Role>()
                    .Where(r => r.Id == workflowLevel.RoleId)
                    .Select(r => new
                    {
                        Name = r.Post.Name + ", " + r.Office.City.Name
                    })
                    .FirstOrDefaultAsync();

                    _logger.LogInformation("Workflow Level determined: {LevelId}", workflowLevel.Id);

                    var approvalRequest = new ApplicationApprovalRequest
                    {
                        ModuleId = appReg.ModuleId,
                        ApplicationRegistrationId = appReg.Id,
                        ApplicationWorkFlowLevelId = workflowLevel.Id,
                        Status = "Pending",
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    var history = new ApplicationHistory
                    {
                        ApplicationId = appReg.ApplicationId,
                        ApplicationType = module.Name,
                        Action = "E-Sign Successful",
                        PreviousStatus = null,
                        NewStatus = "",
                        Comments = "Application E-Signed and sent to " + roleInfo.Name + " for Verification",
                        ActionBy = appReg.UserId.ToString(),
                        ActionByName = "Applicant",
                        ActionDate = DateTime.Now
                    };
                    _db.ApplicationHistories.Add(history);

                    _db.Set<ApplicationApprovalRequest>().Add(approvalRequest);

                    await _db.SaveChangesAsync();

                    _logger.LogInformation("Approval request created successfully");

                    await transaction.CommitAsync();

                    _logger.LogInformation("Transaction committed successfully");

                    if (pdfBytes != null)
                    {
                        _logger.LogInformation("Overwriting existing PDF at: {Url}", applicationUrl);
                        await OverwriteExistingPdfAsync(applicationUrl, pdfBytes);
                    }

                    _logger.LogInformation("===== UpdateApplicationESignData COMPLETED SUCCESSFULLY =====");

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating ESign data for PRN: {PRN}", prnNumber);
                    throw;
                }
            }
        }

        public async Task OverwriteExistingPdfAsync(string fileUrl, byte[] newPdfBytes)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                throw new ArgumentException("File URL is required.", nameof(fileUrl));

            if (newPdfBytes == null || newPdfBytes.Length == 0)
                throw new ArgumentException("PDF content is empty.", nameof(newPdfBytes));

            // Convert URL → Relative Path
            string relativePath;

            if (fileUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                var uri = new Uri(fileUrl);
                relativePath = uri.AbsolutePath.TrimStart('/');
            }
            else
            {
                // In case DB already stores relative path
                relativePath = fileUrl.TrimStart('/');
            }

            // Convert Relative Path → Physical Path
            var physicalPath = Path.Combine(_environment.WebRootPath, relativePath);

            if (!File.Exists(physicalPath))
                throw new FileNotFoundException("PDF file not found on server.", physicalPath);

            // Overwrite file
            await File.WriteAllBytesAsync(physicalPath, newPdfBytes);
        }

        public async Task<bool> SavePRNNumber(string registrationId, string prnNumber)
        {
            var appReg = await _db.Set<ApplicationRegistration>()
                .FirstOrDefaultAsync(r => r.ApplicationId == registrationId);
            if (appReg == null)
                return false;
            appReg.ESignPrnNumber = prnNumber;
            appReg.UpdatedDate = DateTime.Now;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}