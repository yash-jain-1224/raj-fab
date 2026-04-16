using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.DTOs.CompetantpersonDtos;
using RajFabAPI.Services.Interface;
using System.Net;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompetantPersonController : ControllerBase
    {
        private readonly ICompetantPersonRegistartionService _service;

        public CompetantPersonController(ICompetantPersonRegistartionService service)
        {
            _service = service;
        }



        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateCompetentRegistrationDto dto)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                var userIdGuid = Guid.TryParse(userId, out var parsedGuid)
                    ? parsedGuid
                    : Guid.Empty;

                if (userIdGuid == Guid.Empty)
                    return Unauthorized("Invalid user.");

                var applicationId = await _service.SaveCompetentPersonAsync(  dto, userIdGuid, "new", null );

                return Ok(new
                {
                    message = "Competent Person registration submitted successfully",
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
             

        [HttpPost("amend")]
        public async Task<IActionResult> Amend(
            [FromQuery] string competentRegistrationNo,
            [FromBody] CreateCompetentRegistrationDto dto)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                var userIdGuid = Guid.TryParse(userId, out var parsedGuid)
                    ? parsedGuid
                    : Guid.Empty;

                if (userIdGuid == Guid.Empty)
                    return Unauthorized("Invalid user.");

                if (string.IsNullOrWhiteSpace(competentRegistrationNo))
                    return BadRequest("CompetentRegistrationNo is required.");

                var applicationId = await _service.SaveCompetentPersonAsync(    dto,  userIdGuid,   "amend",  competentRegistrationNo );

                return Ok(new
                {
                    message = "Competent Person amendment submitted successfully",
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
        public async Task<IActionResult> GetByApplication(string applicationId)
        {
            var data = await _service.GetByApplicationIdAsync(WebUtility.UrlDecode(applicationId));

            if (data == null)
                return NotFound("Application not found");

            return Ok(data);
        }

        [HttpGet("registration/{registrationNo}")]
        public async Task<IActionResult> GetByRegistration(string registrationNo)
        {
            var data = await _service.GetLatestApprovedByRegistrationNoAsync(WebUtility.UrlDecode(registrationNo));

            if (data == null)
                return NotFound("Registration not found");

            return Ok(data);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();

            return Ok(result);
        }

    }
}
