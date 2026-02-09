using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ManagerChangeController : ControllerBase
    {
        private readonly IManagerChangeService _managerChangeService;

        public ManagerChangeController(IManagerChangeService managerChangeService)
        {
            _managerChangeService = managerChangeService;
        }

        // GET: api/ManagerChange
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid or missing userId claim");

            var list = await _managerChangeService.GetAllAsync(userId);
            return Ok(new { success = true, data = list });
        }

        // GET: api/ManagerChange/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _managerChangeService.GetByIdAsync(id);
                return Ok(new ApiResponseDto<ManagerChangeGetResponseDto>
                {
                    Success = true,
                    Message = "Manager change details fetched successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { success = false, message = "Manager change not found" });
            }
        }
        
        [Authorize]
        // POST: api/ManagerChange
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateManagerChangeRequestDto dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, message = "Payload required" });

            var userIdClaim = User.FindFirst("userId")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { success = false, message = "Invalid or missing userId claim" });

            var result = await _managerChangeService.CreateAsync(dto, userId);
            return Ok(new { success = true, data = result });
        }

        // POST: api/ManagerChange/update/{managerChangeId}
        [HttpPost("update/{managerChangeId}")]
        public async Task<IActionResult> Update(
            [FromRoute] Guid managerChangeId,
            [FromBody] UpdateManagerChangeRequestDto dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, message = "Payload required" });

            var userIdClaim = User.FindFirst("userId")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { success = false, message = "Invalid or missing userId claim" });

            try
            {
                var result = await _managerChangeService.UpdateAsync(managerChangeId, dto);
                return Ok(new { success = true, data = result });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { success = false, message = "Manager change application not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
