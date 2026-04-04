using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services;
using RajFabAPI.Services.Interface;
using System.Net;

namespace RajFabAPI.Controllers.WelderApplicationControllers
{
    [ApiController]
    [Route("api/WelderApplication")]
    [Authorize]
    public class WelderApplicationController : ControllerBase
    {
        private readonly IWelderApplicationService _service;

        public WelderApplicationController(IWelderApplicationService service)
        {
            _service = service;
        }


        [HttpPost("Create")]
        [Authorize]
        public async Task<IActionResult> CreateWelder([FromBody] CreateWelderRegistrationDto dto)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

                var applicationId = await _service.SaveWelderAsync(dto, userIdGuid, "new", null);

                return Ok(new
                {
                    success = true,
                    applicationId,
                    message = "Welder registration submitted successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


        [HttpPost("amend/{registrationNo}")]
        [Authorize]
        public async Task<IActionResult> AmendWelder(string registrationNo, [FromBody] CreateWelderRegistrationDto dto)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

                var applicationId = await _service.SaveWelderAsync(dto, userIdGuid, "amend", registrationNo);

                return Ok(new
                {
                    success = true,
                    applicationId,
                    message = "Welder amendment submitted successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("renew")]
      
        public async Task<IActionResult> RenewWelder([FromBody] WelderRenewalDto dto)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

                var applicationId = await _service.RenewWelderAsync(dto, userIdGuid);

                return Ok(new
                {
                    success = true,
                    applicationId,
                    message = "Welder renewal submitted successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        [HttpPost("update/{applicationId}")]
        [Authorize]
        public async Task<IActionResult> UpdateWelder( string applicationId,  [FromBody] CreateWelderRegistrationDto dto)
        {
            try
            {
                var result = await _service.UpdateWelderAsync(WebUtility.UrlDecode(applicationId), dto);

                if (!result)
                    return NotFound("Application not found.");

                return Ok(new
                {
                    success = true,
                    message = "Welder application updated successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        [HttpGet("application/{applicationId}")]
        public async Task<IActionResult> GetByApplicationId(string applicationId)
        {
            var result = await _service.GetByApplicationIdAsync(WebUtility.UrlDecode(applicationId));

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("registration/{registrationNo}")]
        public async Task<IActionResult> GetLatestApprovedByRegistrationNo(string registrationNo)
        {
            var result = await _service.GetLatestApprovedByRegistrationNoAsync(WebUtility.UrlDecode(registrationNo));

            if (result == null)
                return NotFound("Latest version is not approved.");

            return Ok(result);
        }


        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpPost("close")]
      
        public async Task<IActionResult> CloseWelder([FromBody] WelderClosureDto dto)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;

                var userIdGuid = Guid.TryParse(userId, out var parsedGuid)
                    ? parsedGuid
                    : Guid.Empty;

                var applicationId = await _service.CloseWelderAsync(dto, userIdGuid);

                return Ok(new
                {
                    success = true,
                    applicationId,
                    message = "Welder closure submitted successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

    }
}
