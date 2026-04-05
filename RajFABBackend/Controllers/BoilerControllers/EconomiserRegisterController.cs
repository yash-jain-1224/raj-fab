using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RajFabAPI.DTOs;
using RajFabAPI.DTOs.RajFabAPI.DTOs.EconomiserDTOs;
using RajFabAPI.Services;
using RajFabAPI.Services.Interface;
using System.Net;
using System.Security.Claims;
using static RajFabAPI.Constants.AppConstants;

namespace RajFabAPI.Controllers.BoilerControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EconomiserController : ControllerBase
    {
        private readonly IEconomiserService _economiserService;

        public EconomiserController(IEconomiserService economiserService)
        {
            _economiserService = economiserService;
        }

        [HttpPost("create")]
  
        public async Task<IActionResult> CreateEconomiser([FromBody] EconomiserCreateDto dto)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

                var applicationId = await _economiserService.SaveEconomiserAsync(dto, userIdGuid, "new", null);

                // If result contains HTML (payment gateway redirect), return it
                if (applicationId != null && applicationId.Contains("<html", StringComparison.OrdinalIgnoreCase))
                {
                    return Ok(new { success = true, html = applicationId });
                }

                return Ok(new
                {
                    success = true,
                    applicationId,
                    message = "Economiser registration created successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }



        [HttpPost("amend/{registrationNo}")]
        
        public async Task<IActionResult> AmendEconomiser(string registrationNo, [FromBody] EconomiserCreateDto dto)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

                var applicationId = await _economiserService.SaveEconomiserAsync(dto, userIdGuid, "amend", registrationNo);

                return Ok(new
                {
                    success = true,
                    applicationId,
                    message = "Economiser amendment submitted successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("renew")]
       
        public async Task<IActionResult> RenewEconomiser([FromBody] EconomiserRenewalDto dto)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

                var applicationId = await _economiserService.RenewEconomiserAsync(dto, userIdGuid);

                return Ok(new
                {
                    success = true,
                    applicationId,
                    message = "Economiser renewal submitted successfully."
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
       
        public async Task<IActionResult> UpdateEconomiser(   string applicationId,  [FromBody] EconomiserCreateDto dto)
        {
            try
            {
                var result = await _economiserService.UpdateEconomiserAsync(WebUtility.UrlDecode(applicationId), dto);

                if (!result)
                    return NotFound("Application not found.");

                return Ok(new
                {
                    success = true,
                    message = "Economiser application updated successfully."
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
            var result = await _economiserService.GetByApplicationIdAsync(WebUtility.UrlDecode(applicationId));

            if (result == null)
                return NotFound();

            return Ok(result);
        }

       

        [HttpGet("registration/{registrationNo}")]
        public async Task<IActionResult> GetLatestApproved(string registrationNo)
        {
            var result = await _economiserService.GetLatestApprovedByRegistrationNoAsync(WebUtility.UrlDecode(registrationNo));

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _economiserService.GetAllAsync();
            return Ok(result);
        }


        [HttpPost("generate-pdf/{applicationId}")]
        public async Task<IActionResult> GeneratePdf(string applicationId)
        {
            var filePath = await _economiserService.GenerateEconomiserPdfAsync(WebUtility.UrlDecode(applicationId));
            return Ok(new { success = true, message = "PDF generated successfully" });
        }

        [HttpPost("close")]

        public async Task<IActionResult> CloseEconomiser([FromBody] EconomiserClosureDto dto)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

                var applicationId = await _economiserService.CloseEconomiserAsync(dto, userIdGuid);

                return Ok(new
                {
                    success = true,
                    applicationId,
                    message = "Economiser closure request submitted successfully."
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