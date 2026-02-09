using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;
using static RajFabAPI.Constants.AppConstants;

namespace RajFabAPI.Services
{
    public class FactoryLicenseService : IFactoryLicenseService
    {
        private readonly ApplicationDbContext _context;

        public FactoryLicenseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FactoryLicense>> GetAllAsync(Guid userId)
        {
            return await _context.FactoryLicenses
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<FactoryLicense?> GetByIdAsync(Guid id, Guid userId)
        {
            return await _context.FactoryLicenses
                .FirstOrDefaultAsync(x => x.Id == id.ToString() && x.IsActive);
        }

        public async Task<string> CreateAsync(CreateFactoryLicenseDto dto, Guid userId, string type, string FactoryLicenseNumber = "")
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.ValidTo <= dto.ValidFrom)
                throw new ArgumentException("ValidTo must be greater than ValidFrom.");

            decimal newVersion;
            string finalFactoryLicenseNumber;

            if (type == "New" && string.IsNullOrEmpty(FactoryLicenseNumber))
            {
                finalFactoryLicenseNumber = GenerateLicenseNumber();
                newVersion = 1.0m;
            }
            else if (type == "Amendment" || type == "Renewal")
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

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var license = new FactoryLicense
                {
                    Id = Guid.NewGuid().ToString().ToUpper(),
                    FactoryRegistrationNumber = dto.FactoryRegistrationNumber.Trim(),
                    FactoryLicenseNumber = finalFactoryLicenseNumber,
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
                    Type = string.IsNullOrEmpty(type) ? "New" : type
                };
                _context.FactoryLicenses.Add(license);

                string applicationTypeName = type switch
                {
                    "New" => ApplicationTypeNames.FactoryLicense,
                    "Amendment" => ApplicationTypeNames.FactoryLicenseAmendment,
                    "Renewal" => ApplicationTypeNames.FactoryLicenseRenewal,
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

                var estReg = await _context.Set<EstablishmentRegistration>()
                    .Where(er => er.RegistrationNumber == dto.FactoryRegistrationNumber && er.Status == "Approved")
                    .OrderByDescending(er => er.Version)
                    .FirstOrDefaultAsync();
                if (estReg == null) throw new Exception("Establishment details not found for the user.");

                var estDetail = await _context.Set<EstablishmentDetail>()
                    .FirstOrDefaultAsync(ed => ed.Id == estReg.EstablishmentDetailId);
                if (estDetail == null) throw new Exception("Establishment details not found for the user.");

                int totalWorkers = (estDetail.TotalNumberOfEmployee ?? 0) +
                                   (estDetail.TotalNumberOfContractEmployee ?? 0) +
                                   (estDetail.TotalNumberOfInterstateWorker ?? 0);

                var workerRange = await _context.Set<WorkerRange>()
                    .FirstOrDefaultAsync(wr => totalWorkers >= wr.MinWorkers && totalWorkers <= wr.MaxWorkers);

                var factoryType = _context.FactoryTypes.FirstOrDefault(x => x.Name == "Not Applicable");
                Guid? factoryTypeIdGuid = factoryType?.Id;

                Guid? workerRangeId = workerRange?.Id;
                var factoryCategory = await _context.Set<FactoryCategory>()
                    .FirstOrDefaultAsync(fc => fc.WorkerRangeId == workerRangeId && fc.FactoryTypeId == factoryTypeIdGuid);
                Guid? factoryCategoryId = factoryCategory?.Id;

                var officeApplicationArea = await _context.Set<OfficeApplicationArea>()
                    .FirstOrDefaultAsync(oaa => oaa.CityId == Guid.Parse(estDetail.AreaId));

                if (officeApplicationArea != null)
                {
                    var officeId = officeApplicationArea?.OfficeId;
                    var workflow = await _context.Set<ApplicationWorkFlow>()
                        .FirstOrDefaultAsync(wf => wf.ModuleId == module.Id && wf.FactoryCategoryId == factoryCategoryId && wf.OfficeId == officeId);
                    
                    if (workflow == null) throw new Exception("workflow not found for this module and factory category.");

                    Guid workflowId = workflow != null ? workflow.Id : Guid.Empty;
                    var workflowLevel = await _context.Set<ApplicationWorkFlowLevel>()
                        .Where(wfl => wfl.ApplicationWorkFlowId == workflowId)
                        .OrderBy(wfl => wfl.LevelNumber)
                        .FirstOrDefaultAsync();

                    if (workflow != null && workflowLevel != null)
                    {
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
                    }
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return finalFactoryLicenseNumber;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<FactoryLicense?> UpdateAsync(Guid id, CreateFactoryLicenseDto dto, Guid userId)
        {
            var license = await _context.FactoryLicenses.FindAsync(id);
            if (license == null || !license.IsActive)
                return null;

            license.FactoryRegistrationNumber = dto.FactoryRegistrationNumber;
            license.ValidFrom = dto.ValidFrom;
            license.ValidTo = dto.ValidTo;
            license.Place = dto.Place;
            license.Status = "Pending";
            license.Date = dto.Date;
            license.ManagerSignature = dto.ManagerSignature;
            license.OccupierSignature = dto.OccupierSignature;
            license.AuthorisedSignature = dto.AuthorisedSignature;
            license.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            var module = await _context.Set<FormModule>().FirstOrDefaultAsync(m => m.Name == ApplicationTypeNames.FactoryLicense);
            var appReg = await _context.ApplicationRegistrations.OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync(x => x.ApplicationId == license.Id && x.ModuleId == module.Id);

                // Fetch EstablishmentDetail for the user
                var estReg = await _context.Set<EstablishmentRegistration>()
                    .Where(er =>
                        er.RegistrationNumber == dto.FactoryRegistrationNumber &&
                        er.Status == "Approved")
                    .OrderByDescending(er => er.Version)
                    .FirstOrDefaultAsync();

                if (estReg == null)
                    throw new Exception("Establishment details not found for the user.");
                var estDetail = await _context.Set<EstablishmentDetail>()
                    .FirstOrDefaultAsync(ed => ed.Id == estReg.EstablishmentDetailId);
                if (estDetail == null)
                    throw new Exception("Establishment details not found for the user.");

                // Calculate total workers
                int totalWorkers = estDetail.TotalNumberOfEmployee ?? 0 + estDetail.TotalNumberOfContractEmployee ?? 0 + estDetail.TotalNumberOfInterstateWorker ?? 0;

                // Get WorkerRange and FactoryCategoryId
                var workerRange = await _context.Set<WorkerRange>()
                    .FirstOrDefaultAsync(wr => totalWorkers >= wr.MinWorkers && totalWorkers <= wr.MaxWorkers);

                var factoryType = _context.FactoryTypes.FirstOrDefault(x => x.Name == "Not Applicable");
                var factoryTypeIdGuid = factoryType?.Id;

                Guid? workerRangeId = workerRange?.Id;
                var factoryCategory = await _context.Set<FactoryCategory>()
                    .FirstOrDefaultAsync(fc => fc.WorkerRangeId == workerRangeId && fc.FactoryTypeId == factoryTypeIdGuid);
                Guid? factoryCategoryId = factoryCategory?.Id;

                var officeApplicationArea = await _context.Set<OfficeApplicationArea>()
                    .FirstOrDefaultAsync(oaa => oaa.CityId == Guid.Parse(estDetail.AreaId));
                if (officeApplicationArea != null)
                {
                    var officeId = officeApplicationArea?.OfficeId;
                    var workflow = await _context.Set<ApplicationWorkFlow>()
                        .FirstOrDefaultAsync(wf => wf.ModuleId == module.Id && wf.FactoryCategoryId == factoryCategoryId && wf.OfficeId == officeId);
                    var workflowLevel = await _context.Set<ApplicationWorkFlowLevel>()
                        .Where(wfl => wfl.ApplicationWorkFlowId == (workflow != null ? workflow.Id : Guid.Empty))
                        .OrderBy(wfl => wfl.LevelNumber)
                        .FirstOrDefaultAsync();

                    if (workflow != null)
                    {
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
                        await _context.SaveChangesAsync();
                    }
                }
            
            return license;
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

        public string GenerateLicenseNumber()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"FLN{timestamp}{random}";
        }
    }
}