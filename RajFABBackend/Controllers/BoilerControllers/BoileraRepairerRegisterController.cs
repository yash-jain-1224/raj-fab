using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services;
using RajFabAPI.Services.Interface;
using System.Net;
using static RajFabAPI.Constants.AppConstants;

namespace RajFabAPI.Controllers.BoilerControllers
{
    [Route("api/boiler-repairer")]
    [ApiController]
    [Authorize] 
    public class BoilerRepairerController : ControllerBase
    {
        private readonly IBoilerRepairerService _repairerService;

        public BoilerRepairerController(IBoilerRepairerService repairerService)
        {
            _repairerService = repairerService;
        }

        /* ============================================================
           ?? CREATE (NEW REGISTRATION)
           POST: api/boiler-repairer/create
        ============================================================ */

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] BoilerRepairerCreateDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

            if (userIdGuid == Guid.Empty)
                return Unauthorized("Invalid user.");

            var applicationId = await _repairerService
                .SaveRepairerAsync(dto, userIdGuid, "new", null);

            return Ok(new
            {
                message = "Boiler Repairer application created successfully",
                applicationId = applicationId
            });
        }

        /* ============================================================
           ?? AMEND (Based on RepairerRegistrationNo)
        ============================================================ */
        [HttpPost("amend")]
        public async Task<IActionResult> Amend(
            [FromQuery] string repairerRegistrationNo,
            [FromBody] BoilerRepairerCreateDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

            if (userIdGuid == Guid.Empty)
                return Unauthorized("Invalid user.");

            var applicationId = await _repairerService
                .SaveRepairerAsync(dto, userIdGuid, "amend", repairerRegistrationNo);

            return Ok(new
            {
                message = "Repairer amendment submitted successfully",
                applicationId = applicationId
            });
        }

        [HttpPost("renew")]
        public async Task<IActionResult> Renew([FromBody] BoilerRepairerRenewalDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

            if (userIdGuid == Guid.Empty)
                return Unauthorized("Invalid user.");

            var applicationId = await _repairerService.RenewRepairerAsync(dto, userIdGuid);

            return Ok(new
            {
                message = "Boiler Repairer renewal submitted successfully",
                applicationId = applicationId
            });
        }

        [HttpPost("close")]
        public async Task<IActionResult> Close([FromBody] BoilerRepairerClosureDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

            if (userIdGuid == Guid.Empty)
                return Unauthorized("Invalid user.");

            var applicationId = await _repairerService.CloseRepairerAsync(dto, userIdGuid);

            return Ok(new
            {
                message = "Boiler Repairer closure submitted successfully",
                applicationId = applicationId
            });
        }

        [HttpGet("application/{applicationId}")]
        public async Task<IActionResult> GetByApplicationId(string applicationId)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
                return BadRequest("ApplicationId is required.");

            var result = await _repairerService.GetByApplicationIdAsync(WebUtility.UrlDecode(applicationId));

            if (result == null)
                return NotFound("Record not found.");

            return Ok(result);
        }

        /* ==========================================================
           ? GET BY REGISTRATION NO (Latest + Approved Only)
        ========================================================== */

        [HttpGet("registration/{registrationNo}")]
        public async Task<IActionResult> GetByRegistrationNo(string registrationNo)
        {
            if (string.IsNullOrWhiteSpace(registrationNo))
                return BadRequest("RepairerRegistrationNo is required.");

            var result = await _repairerService
                .GetLatestApprovedByRegistrationNoAsync(WebUtility.UrlDecode(registrationNo));

            if (result == null)
                return NotFound("Latest approved record not found.");

            return Ok(result);
        }

        /* ==========================================================
           ? GET ALL
        ========================================================== */

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _repairerService.GetAllAsync();
            return Ok(result);
        }

    }
}
  


   