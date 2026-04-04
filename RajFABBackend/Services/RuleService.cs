using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
    public class RuleService : IRuleService
    {
        private readonly ApplicationDbContext _context;

        public RuleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RuleResponseDto>> GetAllAsync()
        {
            return await _context.Rules
                .Select(r => new RuleResponseDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Category = r.Category,
                    IsActive = r.IsActive,
                    ImplementationYear = r.ImplementationYear,
                    ActId = r.ActId
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<RuleResponseDto>> GetByActAsync(Guid actId)
        {
            return await _context.Rules
                .Where(r => r.ActId == actId)
                .Select(r => new RuleResponseDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Category = r.Category,
                    IsActive = r.IsActive,
                    ImplementationYear = r.ImplementationYear,
                    ActId = r.ActId
                })
                .ToListAsync();
        }

        public async Task<RuleResponseDto?> GetByIdAsync(Guid id)
        {
            var r = await _context.Rules.FindAsync(id);
            if (r == null) return null;

            return new RuleResponseDto
            {
                Id = r.Id,
                Name = r.Name,
                Category = r.Category,
                IsActive = r.IsActive,
                ImplementationYear = r.ImplementationYear,
                ActId = r.ActId
            };
        }

        public async Task<RuleResponseDto> CreateAsync(CreateRuleDto dto)
        {
            var rule = new Rule
            {
                Name = dto.Name,
                Category = dto.Category,
                ActId = dto.ActId,
                ImplementationYear = dto.ImplementationYear
            };

            _context.Rules.Add(rule);
            await _context.SaveChangesAsync();

            return new RuleResponseDto
            {
                Id = rule.Id,
                Name = rule.Name,
                Category = rule.Category,
                IsActive = rule.IsActive,
                ImplementationYear = rule.ImplementationYear,
                ActId = rule.ActId
            };
        }

        public async Task<RuleResponseDto?> UpdateAsync(Guid id, UpdateRuleDto dto)
        {
            var rule = await _context.Rules.FindAsync(id);
            if (rule == null) return null;

            rule.Name = dto.Name;
            rule.Category = dto.Category;
            rule.IsActive = dto.IsActive;
            rule.ImplementationYear = dto.ImplementationYear;

            await _context.SaveChangesAsync();

            return new RuleResponseDto
            {
                Id = rule.Id,
                Name = rule.Name,
                Category = rule.Category,
                IsActive = rule.IsActive,
                ImplementationYear = rule.ImplementationYear,
                ActId = rule.ActId
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var rule = await _context.Rules.FindAsync(id);
            if (rule == null) return false;

            _context.Rules.Remove(rule);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
