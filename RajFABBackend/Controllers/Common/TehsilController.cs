using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.Models;

namespace RajFabAPI.Controllers.Common
{
    [ApiController]
    [Route("api/[controller]")]
    public class TehsilsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public TehsilsController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public IActionResult GetAll([FromQuery] Guid? districtId = null)
        {
            var query = _context.Tehsils.AsQueryable();

            if (districtId.HasValue && districtId.Value != Guid.Empty)
            {
                query = query.Where(t => t.DistrictId == districtId.Value);
            }

            var list = query
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.NameHindi,
                    t.DistrictId
                })
                .OrderBy(t => t.Name)
                .ToList();

            return Ok(new { success = true, data = list });
        }

        public class CreateTehsilDto
        {
            public string Name { get; set; } = string.Empty;
            public string NameHindi { get; set; } = string.Empty;
            public Guid DistrictId { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTehsilDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.DistrictId == Guid.Empty)
                return BadRequest(new { success = false, message = "Name and DistrictId are required." });

            var existsDistrict = await _context.Districts.AnyAsync(d => d.Id == dto.DistrictId);
            if (!existsDistrict)
                return NotFound(new { success = false, message = "District not found." });

            var tehsil = new Tehsil
            {
                Name = dto.Name.Trim(),
                NameHindi = dto.NameHindi?.Trim(),
                DistrictId = dto.DistrictId
            };

            _context.Tehsils.Add(tehsil);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                data = new
                {
                    tehsil.Id,
                    tehsil.Name,
                    tehsil.NameHindi,
                    tehsil.DistrictId
                }
            });
        }

        [HttpPost("{id:guid}/update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateTehsilDto dto)
        {
            var tehsil = await _context.Tehsils.FindAsync(id);
            if (tehsil == null)
                return NotFound(new { success = false, message = "Tehsil not found." });

            if (string.IsNullOrWhiteSpace(dto.Name) || dto.DistrictId == Guid.Empty)
                return BadRequest(new { success = false, message = "Name and DistrictId are required." });

            var existsDistrict = await _context.Districts.AnyAsync(d => d.Id == dto.DistrictId);
            if (!existsDistrict)
                return NotFound(new { success = false, message = "District not found." });

            tehsil.Name = dto.Name.Trim();
            tehsil.NameHindi = dto.NameHindi?.Trim();
            tehsil.DistrictId = dto.DistrictId;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                data = new
                {
                    tehsil.Id,
                    tehsil.Name,
                    tehsil.NameHindi,
                    tehsil.DistrictId
                }
            });
        }

        [HttpPost("{id:guid}/delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var tehsil = await _context.Tehsils.FindAsync(id);
                if (tehsil == null)
                    return NotFound(new { success = false, message = "Tehsil not found." });

                _context.Tehsils.Remove(tehsil);
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
