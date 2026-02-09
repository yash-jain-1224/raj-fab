using Microsoft.AspNetCore.Mvc;
using RajFabAPI.Services;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommencementCessationsController : ControllerBase
    {
        private readonly ICommencementCessationService _service;

        public CommencementCessationsController(ICommencementCessationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<CommencementCessationDto>>> GetAll()
        {
            return await _service.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CommencementCessationDto>> GetById(string id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null) return NotFound();
            return dto;
        }

        [HttpPost]
        public async Task<ActionResult<CommencementCessationDto>> Create([FromBody] CommencementCessationRequestDto request)
        {
            var dto = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }
    }
}