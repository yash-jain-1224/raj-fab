using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Models.FactoryModels;
using RajFabAPI.Services.Interface;
using System.ComponentModel.DataAnnotations;
using static RajFabAPI.Constants.AppConstants;

namespace RajFabAPI.Services
{
    public class ApplicationWorkFlowService : IApplicationWorkFlowService
    {
        private readonly ApplicationDbContext _context;

        public ApplicationWorkFlowService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ApplicationWorkFlowResponseDto>> GetAllAsync()
        {
            return await _context.ApplicationWorkFlows
                .Include(w => w.Office)
                .Include(w => w.FactoryCategory)
                .Include(w => w.Module)
                    .ThenInclude(m => m.Rule)
                        .ThenInclude(r => r.Act)
                .Include(w => w.Levels)
                .Select(w => new ApplicationWorkFlowResponseDto
                {
                    Id = w.Id,

                    OfficeId = w.OfficeId,
                    OfficeName = w.Office.Name,

                    ActId = w.Module.Rule.Act.Id,
                    ActName = w.Module.Rule.Act.Name,

                    RuleId = w.Module.Rule.Id,
                    RuleName = w.Module.Rule.Name,

                    ModuleId = w.ModuleId,
                    ModuleName = w.Module.Name,

                    FactoryCategoryId = w.FactoryCategoryId,
                    FactoryCategoryName = w.FactoryCategory.Name,

                    LevelCount = w.LevelCount,
                    IsActive = w.IsActive,

                    Levels = w.Levels
                        .OrderBy(l => l.LevelNumber)
                        .Select(l => new ApplicationWorkFlowLevelResponseDto
                        {
                            Id = l.Id,
                            LevelNumber = l.LevelNumber,
                            RoleId = l.RoleId,
                            IsActive = l.IsActive,
                            OfficeId = _context.Roles
                                .Where(r => r.Id == l.RoleId)
                                .Select(r => r.OfficeId)
                                .FirstOrDefault() != w.OfficeId
                                    ? _context.Roles
                                        .Where(r => r.Id == l.RoleId)
                                        .Select(r => r.OfficeId)
                                        .FirstOrDefault()
                                    : null

                        }).ToList()
                })
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task<IEnumerable<ApplicationWorkFlowResponseDto>> GetByOfficeAsync(Guid officeId)
        {
            return await _context.ApplicationWorkFlows
                .Where(w => w.OfficeId == officeId && w.IsActive)
                .Include(w => w.Office)
                .Include(w => w.FactoryCategory)
                .Include(w => w.Module)
                    .ThenInclude(m => m.Rule)
                        .ThenInclude(r => r.Act)
                .Include(w => w.Levels)
                .Select(w => new ApplicationWorkFlowResponseDto
                {
                    Id = w.Id,

                    OfficeId = w.OfficeId,
                    OfficeName = w.Office.Name,

                    ActId = w.Module.Rule.Act.Id,
                    ActName = w.Module.Rule.Act.Name,

                    RuleId = w.Module.Rule.Id,
                    RuleName = w.Module.Rule.Name,

                    ModuleId = w.ModuleId,
                    ModuleName = w.Module.Name,

                    FactoryCategoryId = w.FactoryCategoryId,
                    FactoryCategoryName = w.FactoryCategory.Name,

                    LevelCount = w.LevelCount,
                    IsActive = w.IsActive,

                    Levels = w.Levels
                        .OrderBy(l => l.LevelNumber)
                        .Select(l => new ApplicationWorkFlowLevelResponseDto
                        {
                            Id = l.Id,
                            LevelNumber = l.LevelNumber,
                            RoleId = l.RoleId,
                            IsActive = l.IsActive
                        }).ToList()
                })
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task<ApplicationWorkFlowResponseDto?> GetByIdAsync(Guid id)
        {
            var w = await _context.ApplicationWorkFlows
                .Include(x => x.Levels)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (w == null) return null;

            return MapToDto(w);
        }

        public async Task<IEnumerable<ApplicationWorkFlowResponseDto>> CreateAsync(
            CreateApplicationWorkFlowDto dto)
        {
            var result = new List<ApplicationWorkFlowResponseDto>();

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var row in dto.Applications)
                {
                    // 🔒 Duplicate check
                    var exists = await _context.ApplicationWorkFlows.AnyAsync(w =>
                        w.OfficeId == dto.OfficeId &&
                        w.ModuleId == row.ModuleId &&
                        w.FactoryCategoryId == row.FactoryCategoryId &&
                        w.IsActive
                    );

                    if (exists)
                        throw new ValidationException(
                            "Application workflow already exists for this Office, Module and Factory Category."
                        );

                    // 🔢 Validate levels
                    ValidateLevels(
                        row.LevelCount,
                        row.Levels.Select(l => l.LevelNumber).ToList()
                    );

                    // 🔐 Validate roles
                    foreach (var level in row.Levels)
                    {
                        var role = await _context.Roles
                            .AsNoTracking()
                            .FirstOrDefaultAsync(r => r.Id == level.RoleId);

                        if (role == null)
                            throw new ValidationException("Invalid Role.");
                    }

                    var workflow = new ApplicationWorkFlow
                    {
                        OfficeId = dto.OfficeId,
                        ModuleId = row.ModuleId,
                        FactoryCategoryId = row.FactoryCategoryId,
                        LevelCount = row.LevelCount,
                        IsActive = true,
                        Levels = row.Levels.Select(l => new ApplicationWorkFlowLevel
                        {
                            LevelNumber = l.LevelNumber,
                            RoleId = l.RoleId,
                            IsActive = true
                        }).ToList()
                    };

                    _context.ApplicationWorkFlows.Add(workflow);
                    result.Add(MapToDto(workflow));
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return result;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<ApplicationWorkFlowResponseDto?> UpdateAsync(
            Guid id,
            UpdateApplicationWorkFlowDto dto)
        {
            var workflow = await _context.ApplicationWorkFlows
                .FirstOrDefaultAsync(w => w.Id == id);

            if (workflow == null) return null;

            workflow.LevelCount = dto.LevelCount;
            workflow.IsActive = dto.IsActive;
            workflow.FactoryCategoryId = dto.FactoryCategoryId;
            workflow.UpdatedAt = DateTime.Now;

            var existingLevels = await _context.ApplicationWorkFlowLevels
                .Where(l => l.ApplicationWorkFlowId == workflow.Id)
                .ToListAsync();

            var existingLevelsById = existingLevels
                .ToDictionary(l => l.Id);

            foreach (var levelDto in dto.Levels)
            {
                if (levelDto.Id != Guid.Empty &&
                    existingLevelsById.TryGetValue(levelDto.Id, out var existingLevel))
                {
                    existingLevel.LevelNumber = levelDto.LevelNumber;
                    existingLevel.RoleId = levelDto.RoleId;
                    existingLevel.IsActive = levelDto.IsActive;
                    existingLevel.UpdatedAt = DateTime.Now;
                }
                else
                {
                    var newLevel = new ApplicationWorkFlowLevel
                    {
                        ApplicationWorkFlowId = workflow.Id,
                        LevelNumber = levelDto.LevelNumber,
                        RoleId = levelDto.RoleId,
                        IsActive = levelDto.IsActive,
                        CreatedAt = DateTime.Now
                    };

                    _context.ApplicationWorkFlowLevels.Add(newLevel);
                }
            }

            var dtoLevelIds = dto.Levels
                .Where(l => l.Id != Guid.Empty)
                .Select(l => l.Id)
                .ToHashSet();

            var levelsToDelete = existingLevels
                .Where(l => !dtoLevelIds.Contains(l.Id))
                .ToList();

            if (levelsToDelete.Any())
            {
                _context.ApplicationWorkFlowLevels.RemoveRange(levelsToDelete);
            }

            await _context.SaveChangesAsync();
            return MapToDto(workflow);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var workflow = await _context.ApplicationWorkFlows
                .FirstOrDefaultAsync(w => w.Id == id);

            if (workflow == null)
                throw new Exception("Workflow not found");

            await _context.ApplicationWorkFlowLevels
                .Where(l => l.ApplicationWorkFlowId == workflow.Id)
                .ExecuteDeleteAsync();

            _context.ApplicationWorkFlows.Remove(workflow);

            await _context.SaveChangesAsync();
            return true;
        }

        private static ApplicationWorkFlowResponseDto MapToDto(ApplicationWorkFlow w)
        {
            return new ApplicationWorkFlowResponseDto
            {
                Id = w.Id,
                OfficeId = w.OfficeId,
                ModuleId = w.ModuleId,
                FactoryCategoryId = w.FactoryCategoryId,
                LevelCount = w.LevelCount,
                IsActive = w.IsActive,
                Levels = w.Levels
                    .OrderBy(l => l.LevelNumber)
                    .Select(l => new ApplicationWorkFlowLevelResponseDto
                    {
                        Id = l.Id,
                        LevelNumber = l.LevelNumber,
                        RoleId = l.RoleId,
                        IsActive = l.IsActive
                    }).ToList()
            };
        }

        private static void ValidateLevels(int levelCount, List<int> levelNumbers)
        {
            if (levelCount != levelNumbers.Count)
                throw new ValidationException("LevelCount must match number of workflow levels.");

            if (levelNumbers.Distinct().Count() != levelNumbers.Count)
                throw new ValidationException("Duplicate LevelNumber is not allowed.");

            if (!levelNumbers.OrderBy(x => x)
                .SequenceEqual(Enumerable.Range(1, levelCount)))
                throw new ValidationException("Levels must be sequential starting from 1.");
        }

        public async Task<bool> AddApplicationToWorkFlow(string applicationId)
        {
            // Load the application registration and module together
            var appReg = await _context.Set<ApplicationRegistration>()
                .FirstOrDefaultAsync(ar => ar.ApplicationId == applicationId);

            if (appReg == null || appReg.ModuleId == Guid.Empty)
                return false; // Early exit if application or module not found

            var module = await _context.Set<FormModule>()
                .FirstOrDefaultAsync(m => m.Id == appReg.ModuleId);

            int totalWorkers = 0;
            Guid factoryTypeIdGuid = Guid.Empty;
            Guid? subDivisionId = null;

            if (module?.Name == ApplicationTypeNames.NewEstablishment || module?.Name == ApplicationTypeNames.FactoryAmendment || module?.Name == ApplicationTypeNames.FactoryRenewal)
            {
                // Load establishment and details together
                var estReg = await _context.Set<EstablishmentRegistration>()
                    .FirstOrDefaultAsync(er => er.EstablishmentRegistrationId == applicationId);

                if (estReg != null)
                {
                    var estDetails = await _context.Set<EstablishmentDetail>()
                        .FirstOrDefaultAsync(ed => ed.Id == estReg.EstablishmentDetailId);

                    var entityData = await _context.Set<EstablishmentEntityMapping>()
                        .FirstOrDefaultAsync(ed => ed.EstablishmentRegistrationId == estReg.EstablishmentRegistrationId);

                    if (entityData == null)
                    {
                        return false;
                    }

                    FactoryDetail? factorydata = null;

                    if (entityData.EntityType == "Factory")
                    {
                        factorydata = await _context.Set<FactoryDetail>()
                            .FirstOrDefaultAsync(f => f.Id == entityData.EntityId);
                    }

                    if (factorydata != null)
                    {
                        totalWorkers = factorydata.NumberOfWorker ?? 0;
                        factoryTypeIdGuid = estDetails.FactoryTypeId ?? Guid.Empty;

                        if (Guid.TryParse(factorydata.SubDivisionId, out var parsedSubDivision))
                            subDivisionId = parsedSubDivision;
                    }
                }
            }

            // Early exit if subdivision not found
            if (!subDivisionId.HasValue)
                return false;

            // Get worker range in a single query
            var workerRange = await _context.Set<WorkerRange>()
                .FirstOrDefaultAsync(wr => totalWorkers >= wr.MinWorkers && totalWorkers <= wr.MaxWorkers);

            Guid? factoryCategoryId = null;

            if (workerRange != null)
            {
                factoryCategoryId = await _context.Set<FactoryCategory>()
                    .Where(fc => fc.WorkerRangeId == workerRange.Id && fc.FactoryTypeId == factoryTypeIdGuid)
                    .Select(fc => (Guid?)fc.Id)
                    .FirstOrDefaultAsync();
            }

            var officeApplicationArea = await _context.Set<OfficeApplicationArea>()
                .Where(oaa => oaa.CityId == subDivisionId.Value)
                .Select(oaa => new { oaa.OfficeId })
                .FirstOrDefaultAsync();

            if (officeApplicationArea == null)
                return false;

            // Get workflow and workflow level in a single query
            var workflow = await _context.Set<ApplicationWorkFlow>()
                .Where(wf => wf.ModuleId == appReg.ModuleId
                          && wf.FactoryCategoryId == factoryCategoryId
                          && wf.OfficeId == officeApplicationArea.OfficeId)
                .FirstOrDefaultAsync();

            if (workflow == null)
                return false;

            var workflowLevel = await _context.Set<ApplicationWorkFlowLevel>()
                .Where(wfl => wfl.ApplicationWorkFlowId == workflow.Id)
                .OrderBy(wfl => wfl.LevelNumber)
                .FirstOrDefaultAsync();

            if (workflowLevel == null)
                return false;

            // Create approval request
            var applicationApprovalRequest = new ApplicationApprovalRequest
            {
                ModuleId = appReg.ModuleId,
                ApplicationRegistrationId = appReg.Id,
                ApplicationWorkFlowLevelId = workflowLevel.Id,
                Status = "Pending",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            _context.Set<ApplicationApprovalRequest>().Add(applicationApprovalRequest);
            return true;
        }

    }
}
