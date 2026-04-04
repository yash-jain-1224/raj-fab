using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services
{
    public class RailwayStationService
    {
        private readonly ApplicationDbContext _context;

        public RailwayStationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<RailwayStationResponseDto>> GetAllAsync()
        {
            return await _context.RailwayStations
                .Include(r => r.District)
                .Include(r => r.City)
                .Select(r => new RailwayStationResponseDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code,
                    DistrictId = r.DistrictId,
                    DistrictName = r.District.Name,
                    CityId = r.CityId,
                    CityName = r.City.Name
                })
                .ToListAsync();
        }

        public async Task<RailwayStation> CreateAsync(CreateRailwayStationDto dto)
        {
            var station = new RailwayStation
            {
                Name = dto.Name,
                Code = dto.Code,
                DistrictId = dto.DistrictId,
                CityId = dto.CityId
            };

            _context.RailwayStations.Add(station);
            await _context.SaveChangesAsync();
            return station;
        }

        public async Task<RailwayStation?> UpdateAsync(Guid id, CreateRailwayStationDto dto)
        {
            var station = await _context.RailwayStations.FindAsync(id);
            if (station == null) return null;

            station.Name = dto.Name;
            station.Code = dto.Code;
            station.DistrictId = dto.DistrictId;
            station.CityId = dto.CityId;

            await _context.SaveChangesAsync();
            return station;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var station = await _context.RailwayStations.FindAsync(id);
            if (station == null) return false;

            _context.RailwayStations.Remove(station);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
