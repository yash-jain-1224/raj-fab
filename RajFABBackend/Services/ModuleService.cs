using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
    public class ModuleService : IModuleService
    {
        private static readonly string[] DefaultActions =
            {
                "VIEW",
                "EDIT",
                "FORWARD",
                "FORWARD_TO_APPLIER",
                "APPROVE",
                "REJECT"
            };

        private readonly ApplicationDbContext _context;

        public ModuleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ModuleResponseDto>> GetAllModulesAsync(Guid? ruleId = null)
        {
            var query = _context.Modules
                .Include(m => m.Act)
                .Include(m => m.Rule)
                .AsQueryable();

            if (ruleId.HasValue)
            {
                query = query.Where(m => m.RuleId == ruleId.Value);
            }

            var modules = await query
                .OrderBy(m => m.Name)
                .ToListAsync();

            return modules.Select(m => MapToResponseDto(m));
        }

        public async Task<ModuleResponseDto?> GetModuleByIdAsync(Guid id)
        {
            var module = await _context.Modules
                .Include(m => m.Act)
                .Include(m => m.Rule)
                .FirstOrDefaultAsync(m => m.Id == id);

            return module == null ? null : MapToResponseDto(module);
        }

        public async Task<ModuleResponseDto> CreateModuleAsync(CreateModuleDto dto)
        {
            // Validate Act & Rule
            if (!await _context.Acts.AnyAsync(a => a.Id == dto.ActId))
                throw new ArgumentException("Invalid ActId");

            if (!await _context.Rules.AnyAsync(r => r.Id == dto.RuleId))
                throw new ArgumentException("Invalid RuleId");

            var nameExists = await _context.Modules
                            .AnyAsync(m => m.Name.ToLower() == dto.Name.ToLower());

            if (nameExists)
                throw new InvalidOperationException("Module with same name already exists");

            var module = new FormModule
            {
                Name = dto.Name,
                Description = dto.Description,
                Category = dto.Category,
                ActId = dto.ActId,
                RuleId = dto.RuleId
            };

            _context.Modules.Add(module);
            await _context.SaveChangesAsync();
            await SeedDefaultPrivilegesAsync(module.Id);

            return await GetModuleByIdAsync(module.Id)
                   ?? throw new InvalidOperationException("Module creation failed");
        }

        public async Task<ModuleResponseDto?> UpdateModuleAsync(Guid id, UpdateModuleDto dto)
        {
            var module = await _context.Modules.FindAsync(id);
            if (module == null)
                return null;

            if (!string.IsNullOrWhiteSpace(dto.Name))
                module.Name = dto.Name;

            if (dto.Description != null)
                module.Description = dto.Description;

            if (!string.IsNullOrWhiteSpace(dto.Category))
                module.Category = dto.Category;

            if (dto.IsActive.HasValue)
                module.IsActive = dto.IsActive.Value;

            if (dto.ActId.HasValue)
            {
                if (!await _context.Acts.AnyAsync(a => a.Id == dto.ActId))
                    throw new ArgumentException("Invalid ActId");

                module.ActId = dto.ActId.Value;
            }

            if (dto.RuleId.HasValue)
            {
                if (!await _context.Rules.AnyAsync(r => r.Id == dto.RuleId))
                    throw new ArgumentException("Invalid RuleId");

                module.RuleId = dto.RuleId.Value;
            }

            module.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return await GetModuleByIdAsync(module.Id);
        }

        public async Task<bool> DeleteModuleAsync(Guid id)
        {
            var module = await _context.Modules.FindAsync(id);
            if (module == null)
                return false;

            var hasForms = await _context.Forms.AnyAsync(f => f.ModuleId == id);
            var hasPermissions = await _context.ModulePermissions.AnyAsync(mp => mp.ModuleId == id);

            if (hasForms || hasPermissions)
            {
                throw new InvalidOperationException(
                    "Cannot delete module - this module has associated forms or user privileges"
                );
            }

            _context.Modules.Remove(module);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ModuleExistsAsync(Guid id)
        {
            return await _context.Modules.AnyAsync(m => m.Id == id);
        }

        private async Task SeedDefaultPrivilegesAsync(Guid moduleId)
        {
            var exists = await _context.Modules.AnyAsync(m => m.Id == moduleId);
            if (!exists)
                throw new InvalidOperationException("Module not found while seeding privileges");

            foreach (var action in DefaultActions)
            {
                var alreadyExists = await _context.Privileges
                    .AnyAsync(p => p.ModuleId == moduleId && p.Action == action);

                if (!alreadyExists)
                {
                    _context.Privileges.Add(new Privilege
                    {
                        ModuleId = moduleId,
                        Action = action
                    });
                }
            }

            await _context.SaveChangesAsync();
        }


        private static ModuleResponseDto MapToResponseDto(FormModule m)
        {
            return new ModuleResponseDto
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Category = m.Category,
                IsActive = m.IsActive,
                ActId = m.ActId,
                ActName = m.Act?.Name,
                RuleId = m.RuleId,
                RuleName = m.Rule?.Name,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt
            };
        }
    }
}