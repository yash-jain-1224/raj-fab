using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommencementCessationsController : ControllerBase
    {
        private readonly ICommencementCessationService _service;
        private readonly IESignService _eSignService;

        public CommencementCessationsController(ICommencementCessationService service, IESignService eSignService)
        {
            _service = service;
            _eSignService = eSignService;
        }

        [HttpGet]
        public async Task<ActionResult<List<CommencementCessationDto>>> GetAll()
        {
            return await _service.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CommencementCessationResDto>> GetById(string id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null) return NotFound();
            // var filePath = await _service.GenerateCommencementCessationPdf(dto);
            return dto;
        }

        [HttpPost]
        public async Task<ActionResult<CommencementCessationDto>> Create([FromBody] CommencementCessationRequestDto request)
        {
            var applicationId = await _service.CreateAsync(request);
            if (applicationId == null)
                throw new Exception("Application not created");
            var html = await _eSignService.GenerateESignHtmlAsync(applicationId);
            return CreatedAtAction(null, new { html }, new { html });
            //return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }
    }
}