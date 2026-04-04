using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers.FactoryControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NonHazardousFactoryRegistrationController : ControllerBase
    {
        private readonly INonHazardousFactoryRegistrationService _service;

        public NonHazardousFactoryRegistrationController(INonHazardousFactoryRegistrationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<NonHazardousFactoryRegistrationDto>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NonHazardousFactoryRegistrationDto>> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<NonHazardousFactoryRegistrationDto>> Create([FromBody] CreateNonHazardousFactoryRegistrationRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}