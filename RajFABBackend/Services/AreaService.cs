// Services/AreaService.cs
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
    public class AreaService : IAreaService
    {
        private readonly ApplicationDbContext _context;

        public AreaService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<AreaResponseDto>> GetAllAsync()
        {
            return await _context.Areas
                .Include(a => a.District)
                .Include(a => a.City)
                .Select(a => new AreaResponseDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    CityId = a.CityId ?? Guid.Empty,
                    CityName = a.City != null ? a.City.Name : "",
                    DistrictId = a.DistrictId,
                    DistrictName = a.District.Name
                }).ToListAsync();
        }

        public async Task<IEnumerable<AreaResponseDto>> GetByDistrictAsync(Guid districtId)
        {
            return await _context.Areas
                .Where(a => a.DistrictId == districtId)
                .Include(a => a.District)
                .Include(a => a.City)
                .Select(a => new AreaResponseDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    CityId = a.CityId ?? Guid.Empty,
                    CityName = a.City != null ? a.City.Name : "",
                    DistrictId = a.DistrictId,
                    DistrictName = a.District.Name
                }).ToListAsync();
        }

        public async Task<IEnumerable<AreaResponseDto>> GetByCityAsync(Guid cityId)
        {
            return await _context.Areas
                .Where(a => a.CityId == cityId)
                .Include(a => a.District)
                .Include(a => a.City)
                .Select(a => new AreaResponseDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    CityId = a.CityId ?? Guid.Empty,
                    CityName = a.City != null ? a.City.Name : "",
                    DistrictId = a.DistrictId,
                    DistrictName = a.District.Name
                }).ToListAsync();
        }

        public async Task<AreaResponseDto?> GetByIdAsync(Guid id)
        {
            var a = await _context.Areas
                .Include(x => x.District)
                .Include(x => x.City)
                .FirstOrDefaultAsync(x => x.Id == id);
            
            if (a == null) return null;
            
            return new AreaResponseDto
            {
                Id = a.Id,
                Name = a.Name,
                CityId = a.CityId ?? Guid.Empty,
                CityName = a.City != null ? a.City.Name : "",
                DistrictId = a.DistrictId,
                DistrictName = a.District.Name
            };
        }

        public async Task<AreaResponseDto> CreateAsync(CreateAreaDto dto)
        {
            // Get city and its district
            var city = await _context.Cities
                .Include(c => c.District)
                .FirstOrDefaultAsync(c => c.Id == dto.CityId);
            
            if (city == null)
                throw new ArgumentException($"City with ID {dto.CityId} not found");

            var area = new Area 
            { 
                Name = dto.Name, 
                CityId = dto.CityId,
                DistrictId = city.DistrictId 
            };
            
            _context.Areas.Add(area);
            await _context.SaveChangesAsync();
            
            return new AreaResponseDto 
            { 
                Id = area.Id, 
                Name = area.Name,
                CityId = area.CityId ?? Guid.Empty,
                CityName = city.Name,
                DistrictId = area.DistrictId,
                DistrictName = city.District.Name
            };
        }

        public async Task<AreaResponseDto?> UpdateAsync(Guid id, UpdateAreaDto dto)
        {
            var a = await _context.Areas.FindAsync(id);
            if (a == null) return null;
            
            // Get city and its district
            var city = await _context.Cities
                .Include(c => c.District)
                .FirstOrDefaultAsync(c => c.Id == dto.CityId);
            
            if (city == null)
                throw new ArgumentException($"City with ID {dto.CityId} not found");

            a.Name = dto.Name;
            a.CityId = dto.CityId;
            a.DistrictId = city.DistrictId;
            
            await _context.SaveChangesAsync();
            
            return new AreaResponseDto 
            { 
                Id = a.Id, 
                Name = a.Name,
                CityId = a.CityId ?? Guid.Empty,
                CityName = city.Name,
                DistrictId = a.DistrictId,
                DistrictName = city.District.Name
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var a = await _context.Areas.FindAsync(id);
            if (a == null) return false;
            _context.Areas.Remove(a);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
