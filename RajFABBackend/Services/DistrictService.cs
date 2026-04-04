// Services/DistrictService.cs
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
    public class DistrictService : IDistrictService
    {
        private readonly ApplicationDbContext _context;

        public DistrictService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<DistrictResponseDto>> GetAllAsync()
        {
            return await _context.Districts
                .Include(d => d.Division)
                .Select(d => new DistrictResponseDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    DivisionId = d.DivisionId,
                    DivisionName = d.Division.Name
                })
                 .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<DistrictResponseDto>> GetByDivisionAsync(Guid divisionId)
        {
            return await _context.Districts
                .Where(d => d.DivisionId == divisionId)
                .Include(d => d.Division)
                .Select(d => new DistrictResponseDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    DivisionId = d.DivisionId,
                    DivisionName = d.Division.Name
                }).OrderBy(d => d.Name).ToListAsync();
        }

        public async Task<DistrictResponseDto?> GetByIdAsync(Guid id)
        {
            var d = await _context.Districts.Include(x => x.Division).FirstOrDefaultAsync(x => x.Id == id);
            if (d == null) return null;
            return new DistrictResponseDto
            {
                Id = d.Id,
                Name = d.Name,
                DivisionId = d.DivisionId,
                DivisionName = d.Division.Name
            };
        }

        public async Task<DistrictResponseDto> CreateAsync(CreateDistrictDto dto)
        {
            var d = new District { Name = dto.Name, DivisionId = dto.DivisionId };
            _context.Districts.Add(d);
            await _context.SaveChangesAsync();
            return new DistrictResponseDto { Id = d.Id, Name = d.Name, DivisionId = d.DivisionId, DivisionName = (await _context.Divisions.FindAsync(dto.DivisionId))?.Name ?? "" };
        }

        public async Task<DistrictResponseDto?> UpdateAsync(Guid id, UpdateDistrictDto dto)
        {
            var d = await _context.Districts.FindAsync(id);
            if (d == null) return null;
            d.Name = dto.Name;
            d.DivisionId = dto.DivisionId;
            await _context.SaveChangesAsync();
            return new DistrictResponseDto { Id = d.Id, Name = d.Name, DivisionId = d.DivisionId, DivisionName = (await _context.Divisions.FindAsync(dto.DivisionId))?.Name ?? "" };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var d = await _context.Districts.FindAsync(id);
            if (d == null) return false;

            // Check for dependent cities and areas
            var hasCities = await _context.Cities.AnyAsync(c => c.DistrictId == id);
            var hasAreas = await _context.Areas.AnyAsync(a => a.DistrictId == id);
            
            if (hasCities || hasAreas)
            {
                throw new InvalidOperationException(
                    "Cannot delete district - this district has associated cities or areas"
                );
            }

            _context.Districts.Remove(d);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
