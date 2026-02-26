using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers.SteamPipeLineApplicationControllers
{
    [ApiController]
    [Route("api/stpl")]
    [Authorize]
    public class SteamPipeLineApplicationController : ControllerBase
    {
        private readonly ISteamPipeLineApplicationService _service;

        public SteamPipeLineApplicationController( ISteamPipeLineApplicationService service)
        {
            _service = service;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create( [FromBody] CreateSteamPipeLineDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ?? Get UserId from JWT
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid)
                ? parsedGuid
                : Guid.Empty;

            if (userIdGuid == Guid.Empty)
                return Unauthorized("Invalid user.");

            var applicationId = await _service.SaveSteamPipeLineAsync(  dto,  "new",  null);

            return Ok(new
            {
                Message = "Steam Pipe Line application created successfully.",
                ApplicationId = applicationId
            });
        }

        /* ==========================================================
           ? AMEND (BASED ON REGISTRATION NO)
        ========================================================== */

        [HttpPost("amend/{steamPipeLineRegistrationNo}")]
        public async Task<IActionResult> Amend(  string steamPipeLineRegistrationNo,  [FromBody] CreateSteamPipeLineDto dto)
        {
            if (string.IsNullOrWhiteSpace(steamPipeLineRegistrationNo))
                return BadRequest("SteamPipeLineRegistrationNo is required.");

            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid)
                ? parsedGuid
                : Guid.Empty;

            if (userIdGuid == Guid.Empty)
                return Unauthorized("Invalid user.");

            var newAppId = await _service.SaveSteamPipeLineAsync(  dto,  "amend",  steamPipeLineRegistrationNo);

            return Ok(new
            {
                Message = "Amendment submitted successfully.",
                ApplicationId = newAppId
            });
        }

    }
}
