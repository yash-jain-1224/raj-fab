using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services;
using RajFabAPI.Services.Interface;
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
            var applicationId = await _manufactureService.SaveManufactureAsync(
                dto,
                userIdGuid,
                "new",
                null);

            return Ok(new
            {
                message = "Boiler Manufacture application created successfully",
                applicationId = applicationId
            });
        }
    }
}
