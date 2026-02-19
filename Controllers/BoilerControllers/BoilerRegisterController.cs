using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services;
using RajFabAPI.Services.Interface;
using static RajFabAPI.Constants.AppConstants;

namespace RajFabAPI.Controllers.BoilerControllers
{
    [ApiController]
    [Route("api/boilers")]
    public class BoilerRegisterController : ControllerBase
    {
        private readonly IBoilerRegistartionService _boilerService;

        public BoilerRegisterController(IBoilerRegistartionService boilerService)
        {
            _boilerService = boilerService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateBoilerRegistrationDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

            if (userIdGuid == Guid.Empty)
                return Unauthorized("Invalid user.");

            // ?? type = new
            var applicationId = await _boilerService.SaveBoilerAsync(dto, userIdGuid, "new", null);

            return Ok(new
            {
                message = "Boiler created successfully",
                applicationId = applicationId
            });
        }

        [HttpPost("renew")]
        public async Task<IActionResult> Renew([FromBody] RenewalBoilerDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

            if (userIdGuid == Guid.Empty)
                return Unauthorized("Invalid user.");

            var appId = await _boilerService.RenewBoilerAsync(dto, userIdGuid);

            return Ok(new
            {
                message = "Boiler renewed successfully",
                applicationId = appId
            });
        }


        [HttpPost("amend")]
        public async Task<IActionResult> AmendBoiler( [FromQuery] string boilerRegistrationNo,   [FromBody] CreateBoilerRegistrationDto dto) 
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                var userIdGuid = Guid.TryParse(userId, out var parsedGuid)
                    ? parsedGuid
                    : Guid.Empty;

                if (userIdGuid == Guid.Empty)
                    return Unauthorized("Invalid user.");

                if (string.IsNullOrWhiteSpace(boilerRegistrationNo))
                    return BadRequest("BoilerRegistrationNo is required for amendment.");

                var applicationId = await _boilerService.SaveBoilerAsync(
                    dto,
                    userIdGuid,
                    "amend",                 // ?? important
                    boilerRegistrationNo     // ?? existing boiler number
                );

                return Ok(new
                {
                    message = "Boiler Amendment Application Submitted Successfully",
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

       


        [HttpGet("get")]
        public async Task<IActionResult> GetByApplicationId(string applicationId)
        {
            var result = await _boilerService.GetByApplicationIdAsync(applicationId);

            if (result == null)
                return NotFound("Boiler not found.");

            return Ok(result);
        }


        [HttpPost("update")]
        public async Task<IActionResult> UpdateByApplicationId(string applicationId, [FromBody] CreateBoilerRegistrationDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid)
                ? parsedGuid
                : Guid.Empty;

            if (userIdGuid == Guid.Empty)
                return Unauthorized("Invalid user.");

            var result = await _boilerService
                .UpdateBoilerAsync(applicationId, dto);

            if (!result)
                return NotFound("Boiler not found.");

            return Ok(new
            {
                message = "Boiler updated successfully"
            });
        }

        [HttpPost("closure")]
        public async Task<IActionResult> CloseBoiler([FromBody] CreateBoilerClosureDto dto)
        {
            var userId = Guid.Parse(User.FindFirst("UserId")!.Value);

            var appId = await _boilerService.CreateClosureAsync(dto, userId);

            return Ok(new
            {
                Message = "Boiler closure application submitted.",
                ApplicationId = appId
            });
        }

        [HttpGet("closure/getbyid")]
        public async Task<IActionResult> GetClosureByApplicationId([FromQuery] string applicationId)
        {
            var result = await _boilerService.GetClosureByApplicationIdAsync(applicationId);

            if (result == null)
                return NotFound("Closure application not found.");

            return Ok(result);
        }

        [HttpGet("closures/getall")]
        public async Task<IActionResult> GetAllClosures()
        {
            var result = await _boilerService.GetAllClosuresAsync();
            return Ok(result);
        }


        [HttpPost("repair/modification")]
        public async Task<IActionResult> CreateRepair([FromBody] CreateBoilerRepairDto dto)
        {
            var userId = Guid.Parse(User.FindFirst("UserId")!.Value);

            var appId = await _boilerService.CreateRepairAsync(dto, userId);

            return Ok(new { ApplicationId = appId });
        }




    }
}
