using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace RajFabAPI.Controllers.Common
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationRegistrationController : ControllerBase
    {
        private readonly IApplicationRegistrationService _service;
        private readonly ILogger<ApplicationRegistrationController> _logger;

        public ApplicationRegistrationController(IApplicationRegistrationService service, ILogger<ApplicationRegistrationController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // GET: api/applicationregistration
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAllAsync();
                _logger.LogInformation("Retrieved {Count} application registrations", result.Count);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving application registrations");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // GET: api/applicationregistration/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                if (result == null)
                {
                    _logger.LogWarning("Application registration with Id {Id} not found", id);
                    return NotFound(new { success = false, message = "Application registration not found" });
                }
                _logger.LogInformation("Retrieved application registration with Id {Id}", id);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving application registration with Id {Id}", id);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // GET: api/applicationregistration/byuser
        [HttpGet("byuser")]
        public async Task<IActionResult> GetByUserId()
        {
            try
            {
                var userIdClaim = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("Invalid userId in token");
                    return BadRequest(new { success = false, message = "Invalid user ID in token." });
                }
                var result = await _service.GetByUserIdAsync(userId);
                _logger.LogInformation("Retrieved application registrations for UserId {UserId}", userId);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving application registrations for user");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("registrationNumber/{number}")]
        public async Task<IActionResult> GetDetailsByRegistrationNumber(string number)
        {
            try
            {
                var details = await _service.GetRegistrationDetailsAsync(number);
                if (details == null)
                    return NotFound("Registration not found.");

                return Ok(new { success = true, data = details });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the registration details.");
            }
        }
    }
}