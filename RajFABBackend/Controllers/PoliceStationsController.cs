using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PoliceStationsController : ControllerBase
    {
        private readonly PoliceStationService _service;

        public PoliceStationsController(PoliceStationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(new { success = true, data = list });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePoliceStationDto dto)
        {
            var ps = await _service.CreateAsync(dto);
            return Ok(new { success = true, data = ps });
        }

        [HttpPost("{id}/update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreatePoliceStationDto dto)
        {
            var ps = await _service.UpdateAsync(id, dto);
            if (ps == null) return NotFound(new { success = false, message = "Not found" });
            return Ok(new { success = true, data = ps });
        }

        [HttpPost("{id}/delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success) return NotFound(new { success = false, message = "Not found" });
            return Ok(new { success = true });
        }
    }
}
