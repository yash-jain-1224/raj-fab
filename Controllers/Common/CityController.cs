using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.Models;

namespace RajFabAPI.Controllers.Common
{
    [ApiController]
    [Route("api/[controller]")]
    public class CitiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public CitiesController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public IActionResult GetAll([FromQuery] Guid? districtId = null)
        {
            var query = _context.Cities.AsQueryable();
            
            if (districtId.HasValue && districtId.Value != Guid.Empty)
            {
                query = query.Where(c => c.DistrictId == districtId.Value);
            }

            var list = query
                .Select(c => new { 
                    c.Id, 
                    c.Name, 
                    c.DistrictId,
                    DistrictName = c.District != null ? c.District.Name : "Unknown District"
                })
                 .OrderBy(c => c.Name)
                .ToList();

            return Ok(new { success = true, data = list });
        }

        public class CreateCityDto
        {
            public string Name { get; set; } = string.Empty;
            public Guid DistrictId { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCityDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.DistrictId == Guid.Empty)
                return BadRequest(new { success = false, message = "Name and DistrictId are required." });

            var existsDistrict = await _context.Districts.AnyAsync(d => d.Id == dto.DistrictId);
            if (!existsDistrict) return NotFound(new { success = false, message = "District not found." });

            var city = new City { Name = dto.Name.Trim(), DistrictId = dto.DistrictId };
            _context.Cities.Add(city);
            await _context.SaveChangesAsync();

            var district = await _context.Districts.FindAsync(city.DistrictId);
            return Ok(new { success = true, data = new { city.Id, city.Name, city.DistrictId, DistrictName = district?.Name ?? "Unknown District" } });
        }

        [HttpPost("{id:guid}/update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateCityDto dto)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city == null) return NotFound(new { success = false, message = "City not found." });

            if (string.IsNullOrWhiteSpace(dto.Name) || dto.DistrictId == Guid.Empty)
                return BadRequest(new { success = false, message = "Name and DistrictId are required." });

            var existsDistrict = await _context.Districts.AnyAsync(d => d.Id == dto.DistrictId);
            if (!existsDistrict) return NotFound(new { success = false, message = "District not found." });

            city.Name = dto.Name.Trim();
            city.DistrictId = dto.DistrictId;
            await _context.SaveChangesAsync();

            var district = await _context.Districts.FindAsync(city.DistrictId);
            return Ok(new { success = true, data = new { city.Id, city.Name, city.DistrictId, DistrictName = district?.Name ?? "Unknown District" } });
        }

        [HttpPost("{id:guid}/delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var city = await _context.Cities.FindAsync(id);
                if (city == null) return NotFound(new { success = false, message = "City not found." });

                // Check for dependent areas
                var hasAreas = await _context.Areas.AnyAsync(a => a.CityId == id);
                
                if (hasAreas)
                {
                    return BadRequest(new { success = false, message = "Cannot delete city - this city has associated areas" });
                }

                _context.Cities.Remove(city);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
