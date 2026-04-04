using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModulesController : ControllerBase
    {
        private readonly IModuleService _moduleService;

        public ModulesController(IModuleService moduleService)
        {
            _moduleService = moduleService;
        }

        /// <summary>
        /// Get all modules
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ModuleResponseDto>>> GetModules([FromQuery] Guid? ruleId = null)
        {
            var modules = await _moduleService.GetAllModulesAsync(ruleId);
            return Ok(modules);
        }

        /// <summary>
        /// Get module by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ModuleResponseDto>> GetModule(Guid id)
        {
            var module = await _moduleService.GetModuleByIdAsync(id);
            if (module == null)
                return NotFound($"Module with ID {id} not found.");

            return Ok(module);
        }

        /// <summary>
        /// Create a new module
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ModuleResponseDto>> CreateModule(CreateModuleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var module = await _moduleService.CreateModuleAsync(dto);
                return CreatedAtAction(nameof(GetModule), new { id = module.Id }, module);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing module
        /// </summary>
        [HttpPost("{id}/update")]
        public async Task<ActionResult<ModuleResponseDto>> UpdateModule(Guid id, UpdateModuleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var module = await _moduleService.UpdateModuleAsync(id, dto);
                if (module == null)
                    return NotFound($"Module with ID {id} not found.");
                return Ok(module);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a module
        /// </summary>
        [HttpPost("{id}/delete")]
        public async Task<IActionResult> DeleteModule(Guid id)
        {
            try
            {
                var deleted = await _moduleService.DeleteModuleAsync(id);
                if (!deleted)
                    return NotFound(new { success = false, message = $"Module with ID {id} not found." });

                return Ok(new { success = true, message = "Module deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}