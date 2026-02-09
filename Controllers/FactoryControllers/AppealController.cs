using Microsoft.AspNetCore.Mvc;
using RajFabAPI.Dtos;
using RajFabAPI.Services;
using Microsoft.AspNetCore.Authorization;

namespace RajFabAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AppealController : ControllerBase
    {
        private readonly IAppealService _service;

        public AppealController(IAppealService service)
        {
            _service = service;
        }

        // CREATE
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] AppealCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // GET ALL
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var appeals = await _service.GetAllAsync();
            return Ok(appeals);
        }

        // GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var appeal = await _service.GetByIdAsync(id);
            if (appeal == null) return NotFound();

            return Ok(appeal);
        }

        // UPDATE
        [HttpPost("update/{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] AppealUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = await _service.UpdateAsync(id, dto);
            if (!updated) return NotFound();

            return NoContent();
        }
    }
}
