using iText.Commons.Actions.Contexts;
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;
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
                    var mapApproval = _db.FactoryMapApprovals.FirstOrDefault(x => x.Id == appRegistration.ApplicationId);
                    if (mapApproval != null)
                    {
                        applicationUserDashboardDtos.Add(new ApplicationUserDashboardDto
                        {
                            ApplicationRegistrationId = appRegistration.Id,
                            ApplicationType = ApplicationTypeNames.MapApproval,
                            Status = mapApproval.Status,
                            CreatedDate = appRegistration.CreatedDate,
                            ApplicationId = Guid.Parse(appRegistration.ApplicationId),
                            ApplicationTitle = mapApproval != null ? "Map Approval" : "",
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
            if (string.IsNullOrWhiteSpace(prnNumber) || string.IsNullOrWhiteSpace(signedPDFBase64))
                return false;

            var appReg = await _db.Set<ApplicationRegistration>()
                .FirstOrDefaultAsync(r => r.ESignPrnNumber == prnNumber);

            if (appReg == null || appReg.ModuleId == Guid.Empty)
                return false;

            var module = await _db.Set<FormModule>()
                .FirstOrDefaultAsync(m => m.Id == appReg.ModuleId);

            if (module == null)
                return false;

            if (signedPDFBase64.Contains(","))
                signedPDFBase64 = signedPDFBase64.Split(',')[1];

            byte[] pdfBytes = Convert.FromBase64String(signedPDFBase64);

            int totalWorkers = 0;
            Guid? factoryTypeId = null;
            Guid? subDivisionId = null;
            string applicationUrl = null;

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                if (module.Name == ApplicationTypeNames.NewEstablishment)
                {
                    var estReg = await _db.Set<EstablishmentRegistration>()
                        .FirstOrDefaultAsync(x => x.EstablishmentRegistrationId == appReg.ApplicationId);

                    if (estReg == null)
                        return false;

                    var estDetails = await _db.Set<EstablishmentDetail>()
                        .FirstOrDefaultAsync(ed => ed.Id == estReg.EstablishmentDetailId);

                    if (estDetails == null)
                        return false;
                    if (!Guid.TryParse(estDetails.SubDivisionId, out Guid parsedSubDiv))
                        return false;

                    totalWorkers =
                        (estDetails.TotalNumberOfEmployee ?? 0) +
                        (estDetails.TotalNumberOfContractEmployee ?? 0) +
                        (estDetails.TotalNumberOfInterstateWorker ?? 0);

                    factoryTypeId = estDetails.FactoryTypeId ?? Guid.Empty;
                    subDivisionId = parsedSubDiv;

                    estReg.IsESignCompleted = true;
                    estReg.UpdatedDate = DateTime.Now;

                    applicationUrl = estReg.ApplicationPDFUrl;
                }
                else if (module.Name == ApplicationTypeNames.MapApproval)
                {
                    var mapReg = await _db.Set<FactoryMapApproval>()
                        .FirstOrDefaultAsync(x => x.Id == appReg.ApplicationId);

                    if (mapReg == null)
                        return false;
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var factoryDetails = JsonSerializer.Deserialize<FactoryDetailsModel>(
                        mapReg.FactoryDetails,
                        options
                    );

                    var factoryType = _db.FactoryTypes.FirstOrDefault(x => x.Name == "Not Applicable");

                    if (!Guid.TryParse(factoryDetails.subDivisionId, out Guid parsedSubDiv))
                        return false;

                    totalWorkers =
                        (mapReg?.MaxWorkerMale ?? 0) +
                        (mapReg?.MaxWorkerFemale ?? 0);
                    factoryTypeId = factoryType?.Id;
                    subDivisionId = parsedSubDiv;
                    mapReg.IsESignCompleted = true;
                    mapReg.UpdatedAt = DateTime.Now;
                    applicationUrl = mapReg.ApplicationPDFUrl;
                }
                else if (module.Name == ApplicationTypeNames.FactoryCommencementCessation)
                {
                    var comReg = await _db.Set<CommencementCessationApplication>()
                        .FirstOrDefaultAsync(x => x.ApplicationId == appReg.ApplicationId);

                    if (comReg == null)
                        return false;

                    var EstablishmentDetailId = await _db.Set<EstablishmentRegistration>()
                                   .Where(m => m.RegistrationNumber == comReg.FactoryRegistrationNumber)
                                   .Select(m => m.EstablishmentDetailId)
                                   .FirstOrDefaultAsync();

                    var establishmentDetail = await _db.Set<EstablishmentDetail>()
                        .FirstOrDefaultAsync(m => m.Id == EstablishmentDetailId);


                    if (!Guid.TryParse(establishmentDetail.SubDivisionId, out Guid parsedSubDiv))
                        return false;

                    totalWorkers =
                       (establishmentDetail?.TotalNumberOfEmployee ?? 0) +
                       (establishmentDetail?.TotalNumberOfContractEmployee ?? 0) +
                       (establishmentDetail?.TotalNumberOfInterstateWorker ?? 0);
                    
                    factoryTypeId = establishmentDetail.FactoryTypeId;
                    subDivisionId = parsedSubDiv;
                    comReg.IsESignCompleted = true;
                    comReg.UpdatedDate = DateTime.Now;
                    applicationUrl = comReg.ApplicationPDFUrl;
                }

                var workerRange = await _db.Set<WorkerRange>()
                    .FirstOrDefaultAsync(wr =>
                        totalWorkers >= wr.MinWorkers &&
                        totalWorkers <= wr.MaxWorkers);

                Guid? factoryCategoryId = null;

                if (workerRange != null)
                {
                    factoryCategoryId = await _db.Set<FactoryCategory>()
                        .Where(fc =>
                            fc.WorkerRangeId == workerRange.Id &&
                            fc.FactoryTypeId == factoryTypeId)
                        .Select(fc => (Guid?)fc.Id)
                        .FirstOrDefaultAsync();
                }

                var officeId = await _db.Set<OfficeApplicationArea>()
                    .Where(oaa => oaa.CityId == subDivisionId)
                    .Select(oaa => (Guid?)oaa.OfficeId)
                    .FirstOrDefaultAsync();

                if (!officeId.HasValue)
                    return false;

                var workflow = await _db.Set<ApplicationWorkFlow>()
                    .FirstOrDefaultAsync(wf =>
                        wf.ModuleId == appReg.ModuleId &&
                        wf.FactoryCategoryId == factoryCategoryId &&
                        wf.OfficeId == officeId.Value);

                if (workflow == null)
                    return false;

                var workflowLevel = await _db.Set<ApplicationWorkFlowLevel>()
                    .Where(wfl => wfl.ApplicationWorkFlowId == workflow.Id)
                    .OrderBy(wfl => wfl.LevelNumber)
                    .FirstOrDefaultAsync();

                if (workflowLevel == null)
                    return false;

                var approvalRequest = new ApplicationApprovalRequest
                {
                    ModuleId = appReg.ModuleId,
                    ApplicationRegistrationId = appReg.Id,
                    ApplicationWorkFlowLevelId = workflowLevel.Id,
                    Status = "Pending",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                _db.Set<ApplicationApprovalRequest>().Add(approvalRequest);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                await OverwriteExistingPdfAsync(applicationUrl, pdfBytes);
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
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