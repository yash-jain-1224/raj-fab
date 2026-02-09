using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RulesController : ControllerBase
    {
        private readonly IRuleService _service;

        public RulesController(IRuleService service)
        {
            _service = service;
        }

        // GET: api/Rules
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _service.GetAllAsync());

        // GET: api/Rules/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { success = false, message = "Rule not found" });

            return Ok(result);
        }

        // GET: api/Rules/by-act/{actId}
        [HttpGet("by-act/{actId}")]
        public async Task<IActionResult> GetByAct(Guid actId)
            => Ok(await _service.GetByActAsync(actId));

        // POST: api/Rules
        [HttpPost]
        public async Task<IActionResult> Create(CreateRuleDto dto)
            => Ok(await _service.CreateAsync(dto));

        // POST: api/Rules/{id}/update
        [HttpPost("{id}/update")]
        public async Task<IActionResult> Update(Guid id, UpdateRuleDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            if (result == null)
                return NotFound(new { success = false, message = "Rule not found" });

            return Ok(result);
        }

        // POST: api/Rules/{id}/delete
        [HttpPost("{id}/delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _service.DeleteAsync(id);
                if (!success)
                    return NotFound(new { success = false, message = "Rule not found" });

                return Ok(new { success = true, data = success });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
