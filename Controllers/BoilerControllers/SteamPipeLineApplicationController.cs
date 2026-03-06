using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services;
using RajFabAPI.Services.Interface;
using System.Net;

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

        [HttpPost("renew")]
        public async Task<IActionResult> Renew( [FromBody] RenewSteamPipeLineDto dto)
        {
            var appId = await _service.RenewSteamPipeLineAsync(dto);

            return Ok(new
            {
                Message = "Steam Pipe Line renewed successfully.",
                ApplicationId = appId
            });
        }

        [HttpGet("by-application/{applicationId}")]
        public async Task<IActionResult> GetByApplicationId(string applicationId)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
                return BadRequest("ApplicationId is required.");

            var result = await _service
                .GetSteamPipeLineByApplicationIdAsync(WebUtility.UrlDecode(applicationId));

            if (result == null)
                return NotFound("Application not found.");

            return Ok(result);
        }

        /* ============================================
           GET BY REGISTRATION NO (ALL VERSIONS)
        ============================================ */

        [HttpGet("by-registration/{registrationNo}")]
        
        public async Task<IActionResult> GetLatestApproved(string registrationNo)
        {
            var result = await _service.GetLatestApprovedByRegistrationNoAsync(WebUtility.UrlDecode(registrationNo));

            if (result == null)
                return NotFound("Latest version is not approved.");

            return Ok(result);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllSteamPipeLinesAsync();
            return Ok(result);
        }

        [HttpPost("update/{applicationId}")]
        public async Task<IActionResult> Update(  string applicationId,   [FromBody] CreateSteamPipeLineDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service
                .UpdateSteamPipeLineAsync(WebUtility.UrlDecode(applicationId), dto);

            return Ok(new
            {
                Message = "Application updated successfully.",
                ApplicationId = result
            });
        }

        [HttpPost("close")]
        public async Task<IActionResult> Close(  [FromBody] CreateSteamPipeLineCloseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var applicationId =
                await _service.CloseSteamPipeLineAsync(dto);

            return Ok(new
            {
                Message = "Steam Pipe Line closure request submitted successfully.",
                ApplicationId = applicationId
            });
        }
    }
}
