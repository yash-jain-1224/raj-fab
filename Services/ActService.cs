using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
    public class ActService : IActService
    {
        private readonly ApplicationDbContext _context;

        public ActService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<ActResponseDto>> GetAllAsync()
        {
            return await _context.Acts
                .Select(d => new ActResponseDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    ImplementationYear = d.ImplementationYear,
                    IsActive = d.IsActive
                })
                .ToListAsync();
        }

        public async Task<ActResponseDto?> GetByIdAsync(Guid id)
        {
            var d = await _context.Acts.FindAsync(id);
            if (d == null) return null;

            return new ActResponseDto
            {
                Id = d.Id,
                Name = d.Name,
                ImplementationYear = d.ImplementationYear,
                IsActive = d.IsActive
            };
        }

        public async Task<ActResponseDto> CreateAsync(CreateActDto dto)
        {
            var d = new Act
            {
                Name = dto.Name,
                ImplementationYear = dto.ImplementationYear,
                IsActive = true
            };

            _context.Acts.Add(d);
            await _context.SaveChangesAsync();

            return new ActResponseDto
            {
                Id = d.Id,
                Name = d.Name,
                ImplementationYear = d.ImplementationYear,
                IsActive = d.IsActive
            };
        }

        public async Task<ActResponseDto?> UpdateAsync(Guid id, UpdateActDto dto)
        {
            var d = await _context.Acts.FindAsync(id);
            if (d == null) return null;

            d.Name = dto.Name;
            d.ImplementationYear = dto.ImplementationYear;

            await _context.SaveChangesAsync();

            return new ActResponseDto
            {
                Id = d.Id,
                Name = d.Name,
                ImplementationYear = d.ImplementationYear,
                IsActive = d.IsActive
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var d = await _context.Acts.FindAsync(id);
            if (d == null) return false;

            var hasRules = await _context.Rules.AnyAsync(r => r.ActId == id);
            if (hasRules)
                throw new InvalidOperationException("Cannot delete Act because Rules exist");

            _context.Acts.Remove(d);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
