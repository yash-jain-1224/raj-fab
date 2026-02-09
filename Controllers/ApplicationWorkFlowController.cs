using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationWorkFlowController : ControllerBase
    {
        private readonly IApplicationWorkFlowService _service;

        public ApplicationWorkFlowController(IApplicationWorkFlowService service)
        {
            _service = service;
        }

        // -------------------- GET ALL --------------------
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        // -------------------- GET BY OFFICE --------------------
        [HttpGet("office/{officeId}")]
        public async Task<IActionResult> GetByOffice(Guid officeId)
        {
            return Ok(await _service.GetByOfficeAsync(officeId));
        }

        // -------------------- GET BY ID --------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null)
                return NotFound(new { success = false, message = "Application workflow not found" });

            return Ok(data);
        }

        // -------------------- CREATE --------------------
        [HttpPost]
        public async Task<IActionResult> Create(CreateApplicationWorkFlowDto dto)
        {
            try
            {
                return Ok(await _service.CreateAsync(dto));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // -------------------- UPDATE --------------------
        [HttpPost("{id}/update")]
        public async Task<IActionResult> Update(Guid id, UpdateApplicationWorkFlowDto dto)
        {
            try
            {
                var result = await _service.UpdateAsync(id, dto);
                if (result == null)
                    return NotFound(new { success = false, message = "Application workflow not found" });

                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // -------------------- DELETE --------------------
        [HttpPost("{id}/delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound(new { success = false, message = "Application workflow not found" });

            return Ok(new { success = true, data = success });
        }
    }
}
