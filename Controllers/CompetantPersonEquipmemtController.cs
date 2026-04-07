using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.DTOs.CompetantpersonDtos;
using RajFabAPI.Services;
using RajFabAPI.Services.Interface;
using System.Net;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompetantPersonEquipmentController : ControllerBase
    {
        private readonly ICompetantPersonEquipmentRegistartionService _service;

        public CompetantPersonEquipmentController(ICompetantPersonEquipmentRegistartionService service)
        {
            _service = service;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateCompetentEquipmentDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

            if (userIdGuid == Guid.Empty)
                return Unauthorized("Invalid user.");

            var applicationId = await _service.SaveCompetentEquipmentAsync(
                dto,
                userIdGuid,
                "new",
                null
            );

            return Ok(new
            {
                message = "Equipment registration submitted successfully",
                applicationId = applicationId
            });
        }

        [HttpPost("amend")]
        public async Task<IActionResult> Amend(  [FromQuery] string equipmentRegistrationNo, [FromBody] CreateCompetentEquipmentDto dto)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                var userIdGuid = Guid.TryParse(userId, out var parsedGuid)
                    ? parsedGuid
                    : Guid.Empty;

                if (userIdGuid == Guid.Empty)
                    return Unauthorized("Invalid user.");

                if (string.IsNullOrWhiteSpace(equipmentRegistrationNo))
                    return BadRequest("EquipmentRegistrationNo is required.");

                var applicationId = await _service.SaveCompetentEquipmentAsync(   dto, userIdGuid, "amend", equipmentRegistrationNo  );

                return Ok(new
                {
                    message = "Equipment amendment submitted successfully",
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


        [HttpGet("application/{applicationId}")]
        public async Task<IActionResult> GetByApplicationId(string applicationId)
        {
            var result = await _service.GetByApplicationIdAsync(WebUtility.UrlDecode(applicationId));

            if (result == null)
                return NotFound("Record not found.");

            return Ok(result);
        }

        [HttpGet("registration/{registrationNo}")]
        public async Task<IActionResult> GetLatest(string registrationNo)
        {
            var result = await _service.GetLatestApprovedByRegistrationNoAsync(WebUtility.UrlDecode(registrationNo));

            if (result == null)
                return NotFound("Approved record not found.");

            return Ok(result);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();

            return Ok(result);
        }

    }
}
