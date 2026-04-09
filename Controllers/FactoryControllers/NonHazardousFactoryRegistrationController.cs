using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;
using System.Net;

namespace RajFabAPI.Controllers.FactoryControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NonHazardousFactoryRegistrationController : ControllerBase
    {
        private readonly INonHazardousFactoryRegistrationService _service;

        public NonHazardousFactoryRegistrationController(INonHazardousFactoryRegistrationService service)
            => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAllAsync();
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                if (result == null)
                    return NotFound(new { success = false, message = "Record not found" });

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateNonHazardousFactoryRegistrationRequest dto)
        {
            try
            {
               
                var userIdClaim = User.FindFirst("UserId")?.Value;

                if (!Guid.TryParse(userIdClaim, out var userId))
                    return Unauthorized("Invalid UserId");

                var result = await _service.CreateAsync(dto, userId);

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("details/{applicationId}")]
        public async Task<IActionResult> GetDetails(string applicationId)
        {
            var result = await _service.GetByApplicationIdAsync(WebUtility.UrlDecode(applicationId));

            return Ok(result);
        }

        [HttpPost("{id}/delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _service.DeleteAsync(id);
                if (!success)
                    return NotFound(new { success = false, message = "Record not found" });

                return Ok(new { success = true, data = success });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("update/{applicationId}")]
        public async Task<IActionResult> Update(string applicationId, CreateNonHazardousFactoryRegistrationRequest dto)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;

                if (!Guid.TryParse(userIdClaim, out var userId))
                    return Unauthorized("Invalid User");

                var result = await _service.UpdateAsync(WebUtility.UrlDecode(applicationId), dto, userId);

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}