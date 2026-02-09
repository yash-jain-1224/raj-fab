using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RailwayStationsController : ControllerBase
    {
        private readonly RailwayStationService _service;

        public RailwayStationsController(RailwayStationService service)
        {
            _service = service;
        }

        // GET: api/railwaystations
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(new { success = true, data = list });
        }

        // POST: api/railwaystations
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRailwayStationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid data" });

            var station = await _service.CreateAsync(dto);
            return Ok(new { success = true, data = station });
        }

        // POST: api/railwaystations/{id}/update
        [HttpPost("{id}/update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateRailwayStationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid data" });

            var station = await _service.UpdateAsync(id, dto);
            if (station == null)
                return NotFound(new { success = false, message = "Railway Station not found" });

            return Ok(new { success = true, data = station });
        }

        // POST: api/railwaystations/{id}/delete
        [HttpPost("{id}/delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound(new { success = false, message = "Railway Station not found" });

            return Ok(new { success = true });
        }
    }
}
