// Services/DivisionService.cs
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
    public class DivisionService : IDivisionService
    {
        private readonly ApplicationDbContext _context;

        public DivisionService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<DivisionResponseDto>> GetAllAsync()
        {
            return await _context.Divisions
                .Select(d => new DivisionResponseDto { Id = d.Id, Name = d.Name })
                 .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<DivisionResponseDto?> GetByIdAsync(Guid id)
        {
            var d = await _context.Divisions.FindAsync(id);
            return d == null ? null : new DivisionResponseDto { Id = d.Id, Name = d.Name };
        }

        public async Task<DivisionResponseDto> CreateAsync(CreateDivisionDto dto)
        {
            var d = new Division { Name = dto.Name };
            _context.Divisions.Add(d);
            await _context.SaveChangesAsync();
            return new DivisionResponseDto { Id = d.Id, Name = d.Name };
        }

        public async Task<DivisionResponseDto?> UpdateAsync(Guid id, UpdateDivisionDto dto)
        {
            var d = await _context.Divisions.FindAsync(id);
            if (d == null) return null;
            d.Name = dto.Name;
            await _context.SaveChangesAsync();
            return new DivisionResponseDto { Id = d.Id, Name = d.Name };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var d = await _context.Divisions.FindAsync(id);
            if (d == null) return false;

            // Check for dependent districts
            var hasDistricts = await _context.Districts.AnyAsync(district => district.DivisionId == id);
            
            if (hasDistricts)
            {
                throw new InvalidOperationException(
                    "Cannot delete division - this division has associated districts"
                );
            }

            _context.Divisions.Remove(d);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}


