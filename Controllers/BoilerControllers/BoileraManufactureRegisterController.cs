using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services;
using RajFabAPI.Services.Interface;
using System.Net;
using static RajFabAPI.Constants.AppConstants;

namespace RajFabAPI.Controllers.BoilerControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BoilerManufactureController : ControllerBase
    {
        private readonly IBoilerManufactureService _manufactureService;

        public BoilerManufactureController(IBoilerManufactureService manufactureService)
        {
            _manufactureService = manufactureService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] BoilerManufactureCreateDto dto)
        {
            // ?? Get UserId from token (same as your boiler module)
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

            if (userIdGuid == Guid.Empty)
                return Unauthorized("Invalid user.");

            // ?? Always NEW for this API
            var applicationId = await _manufactureService.SaveManufactureAsync( dto, userIdGuid, "new", null);

            return Ok(new
            {
                message = "Boiler Manufacture application created successfully",
                applicationId = applicationId
            });
        }

        [HttpPost("amend")]
        public async Task<IActionResult> Amend(  string manufactureRegistrationNo,  [FromBody] BoilerManufactureCreateDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

            if (userIdGuid == Guid.Empty)
                return Unauthorized("Invalid user.");

            var result = await _manufactureService
                .SaveManufactureAsync(dto, userIdGuid, "amend", manufactureRegistrationNo);

            return Ok(new
            {
                message = "Amendment submitted successfully",
                applicationId = result
            });
        }


        [HttpPost("renew")]
        public async Task<IActionResult> Renew([FromBody] BoilerManufactureRenewalDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

            if (userIdGuid == Guid.Empty)
                return Unauthorized("Invalid user.");

            var result = await _manufactureService.RenewManufactureAsync(dto, userIdGuid);

            return Ok(new
            {
                message = "Boiler Manufacture Renewal Submitted Successfully",
                applicationId = result
            });
        }

        [HttpPost("closer")]
        public async Task<IActionResult> CloseManufacture([FromBody] BoilerManufactureClosureDto dto)
        {
            try
            {
                // ?? Get Logged-in UserId from Token
                var userIdClaim = User.FindFirst("userId")?.Value;

                if (!Guid.TryParse(userIdClaim, out Guid userId))
                    return Unauthorized("Invalid user.");

                // ?? Call Service
                var applicationId = await _manufactureService.CloseManufactureAsync(dto, userId);

                return Ok(new
                {
                    message = "Closure application submitted successfully.",
                    applicationId = applicationId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }


        [HttpGet("applicationId")]
        public async Task<IActionResult> GetByApplication([FromQuery] string applicationId)
        {
            var data = await _manufactureService.GetByApplicationIdAsync(WebUtility.UrlDecode(applicationId));

            if (data == null)
                return NotFound("Application not found");

            return Ok(data);
        }


        [HttpGet("manufactureRegistrationNo")]
        public async Task<IActionResult> GetByRegistration([FromQuery]  string manufactureRegistrationNo)
        {
            var data = await _manufactureService.GetLatestApprovedByRegistrationNoAsync(manufactureRegistrationNo);

            if (data == null)
                return NotFound("Registration not found");

            return Ok(data);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _manufactureService.GetAllAsync();

            return Ok(result);
        }

    }
}
