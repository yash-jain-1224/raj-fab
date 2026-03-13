using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services;
using RajFabAPI.Services.Interface;
using System.Net;

namespace RajFabAPI.Controllers.SMTCRegisterController
{
    [ApiController]
    [Route("api/smtc")]
    [Authorize]
    public class SMTCRegisterController: ControllerBase
    {
        private readonly ISMTCRegistrationService _service;

        public SMTCRegisterController(ISMTCRegistrationService service)
        {
            _service = service;
        }


        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateSMTCRegistrationDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsed) ? parsed : Guid.Empty;

            if (userIdGuid == Guid.Empty)
                return Unauthorized("Invalid user.");

            var applicationId = await _service.SaveSMTCAsync(dto, userIdGuid, "new", null);

            return Ok(new
            {
                message = "SMTC application submitted successfully",
                applicationId
            });
        }

        [HttpPost("amend")]
        public async Task<IActionResult> Amend(  [FromQuery] string smtcRegistrationNo, [FromBody] CreateSMTCRegistrationDto dto) 
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsed) ? parsed : Guid.Empty;

            var applicationId = await _service.SaveSMTCAsync(
                dto,
                userIdGuid,
                "amend",
                smtcRegistrationNo);

            return Ok(new
            {
                message = "SMTC amendment submitted",
                applicationId
            });
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
