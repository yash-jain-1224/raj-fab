using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FormsController : ControllerBase
    {
        private readonly IFormService _formService;
        private readonly IModuleService _moduleService;

        public FormsController(IFormService formService, IModuleService moduleService)
        {
            _formService = formService;
            _moduleService = moduleService;
        }

        /// <summary>
        /// Get all forms
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FormResponseDto>>> GetForms()
        {
            var forms = await _formService.GetAllFormsAsync();
            return Ok(forms);
        }

        /// <summary>
        /// Get forms by module ID
        /// </summary>
        [HttpGet("module/{moduleId}")]
        public async Task<ActionResult<IEnumerable<FormResponseDto>>> GetFormsByModule(Guid moduleId)
        {
            if (!await _moduleService.ModuleExistsAsync(moduleId))
                return NotFound($"Module with ID {moduleId} not found.");

            var forms = await _formService.GetFormsByModuleAsync(moduleId);
            return Ok(forms);
        }

        /// <summary>
        /// Get form by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<FormResponseDto>> GetForm(Guid id)
        {
            var form = await _formService.GetFormByIdAsync(id);
            if (form == null)
                return NotFound($"Form with ID {id} not found.");

            return Ok(form);
        }

        /// <summary>
        /// Create a new form
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<FormResponseDto>> CreateForm(CreateFormDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _moduleService.ModuleExistsAsync(dto.ModuleId))
                return BadRequest($"Module with ID {dto.ModuleId} does not exist.");

            var form = await _formService.CreateFormAsync(dto);
            return CreatedAtAction(nameof(GetForm), new { id = form.Id }, form);
        }

        /// <summary>
        /// Update an existing form
        /// </summary>
        [HttpPost("{id}/update")]
        public async Task<ActionResult<FormResponseDto>> UpdateForm(Guid id, UpdateFormDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var form = await _formService.UpdateFormAsync(id, dto);
            if (form == null)
                return NotFound($"Form with ID {id} not found.");

            return Ok(form);
        }

        /// <summary>
        /// Delete a form
        /// </summary>
        [HttpPost("{id}/delete")]
        public async Task<IActionResult> DeleteForm(Guid id)
        {
            var deleted = await _formService.DeleteFormAsync(id);
            if (!deleted)
                return NotFound($"Form with ID {id} not found.");

            return NoContent();
        }
    }
}