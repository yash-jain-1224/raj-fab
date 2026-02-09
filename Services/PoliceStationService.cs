using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services
{
    public class PoliceStationService
    {
        private readonly ApplicationDbContext _context;

        public PoliceStationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PoliceStationResponseDto>> GetAllAsync()
        {
            return await _context.PoliceStations
                .Include(p => p.District)
                .Include(p => p.City)
                .Select(p => new PoliceStationResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Address = p.Address,
                    DistrictId = p.DistrictId,
                    DistrictName = p.District.Name,
                    CityId = p.CityId,
                    CityName = p.City.Name
                })
                .ToListAsync();
        }

        public async Task<PoliceStation> CreateAsync(CreatePoliceStationDto dto)
        {
            var ps = new PoliceStation
            {
                Name = dto.Name,
                Address = dto.Address,
                DistrictId = dto.DistrictId,
                CityId = dto.CityId
            };
            _context.PoliceStations.Add(ps);
            await _context.SaveChangesAsync();
            return ps;
        }

        public async Task<PoliceStation?> UpdateAsync(Guid id, CreatePoliceStationDto dto)
        {
            var ps = await _context.PoliceStations.FindAsync(id);
            if (ps == null) return null;
            ps.Name = dto.Name;
            ps.Address = dto.Address;
            ps.DistrictId = dto.DistrictId;
            ps.CityId = dto.CityId;
            await _context.SaveChangesAsync();
            return ps;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var ps = await _context.PoliceStations.FindAsync(id);
            if (ps == null) return false;
            _context.PoliceStations.Remove(ps);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
