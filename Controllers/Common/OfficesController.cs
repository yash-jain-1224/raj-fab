using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OfficesController : ControllerBase
    {
        private readonly IOfficeService _service;

        public OfficesController(IOfficeService service)
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
        public async Task<IActionResult> Create([FromBody] CreateOfficeDto dto)
        {

            var ps = await _service.CreateAsync(dto);
            return Ok(new { success = true, data = ps });
            // if (!ModelState.IsValid) return BadRequest(ModelState);
            // var created = await _service.CreateAsync(dto);
            // return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPost("{id}/update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOfficeDto dto)
        {
            var ps = await _service.UpdateAsync(id, dto);
            if (ps == null) return NotFound(new { success = false, message = "Not found" });
            return Ok(new { success = true, data = ps });
            // if (!ModelState.IsValid) return BadRequest(ModelState);
            // var updated = await _service.UpdateAsync(id, dto);
            // if (updated == null) return NotFound();
            // return Ok(updated);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var ps = await _service.GetByIdAsync(id);
            return Ok(new { success = true, data = ps });
        }

        [HttpPost("{id}/delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _service.DeleteAsync(id);

                if (!success)
                    return NotFound(new { success = false, message = "Office not found" });

                return Ok(new { success = true, message = "Office deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        
        [HttpPost("{id}/level-count/update")]
        public async Task<IActionResult> UpdateLevelCount(
            Guid id,
            [FromBody] UpdateOfficeLevelCountDto dto
        )
        {
            try
            {
                await _service.UpdateLevelCountAsync(id, dto.LevelCount);

                return Ok(new
                {
                    success = true,
                    message = "Office level count updated successfully",
                    data = dto.LevelCount
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

    }
}
