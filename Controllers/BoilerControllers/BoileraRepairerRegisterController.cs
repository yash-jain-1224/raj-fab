using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services;
using RajFabAPI.Services.Interface;
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

    }
}
  


   