using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services;
using RajFabAPI.Services.Interface;
using System.Net;
using static RajFabAPI.Constants.AppConstants;

namespace RajFabAPI.Controllers.BoilerControllers
{
    [Route("api/boiler-drawing")]
    [ApiController]
    [Authorize]
    public class BoilerDrawingRegisterController : ControllerBase
    {
        private readonly IBoilerDrawingService _boilerDrawingService;

        public BoilerDrawingRegisterController(IBoilerDrawingService boilerDrawingService)
        {
            _boilerDrawingService = boilerDrawingService;
        }


        [HttpPost("create")]
       
        public async Task<IActionResult> CreateBoilerDrawing([FromBody] BoilerDrawingCreateDto dto)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                var userIdGuid = Guid.TryParse(userId, out var parsedGuid)
                    ? parsedGuid
                    : Guid.Empty;

                var applicationId = await _boilerDrawingService.SaveBoilerDrawingAsync(dto, userIdGuid, "new", null);

                return Ok(new
                {
                    success = true,
                    applicationId,
                    message = "Boiler drawing application submitted successfully."
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

    

        [HttpPost("amend/{registrationNo}")]
        
        public async Task<IActionResult> AmendBoilerDrawing(string registrationNo, [FromBody] BoilerDrawingCreateDto dto)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                var userIdGuid = Guid.TryParse(userId, out var parsedGuid)
                    ? parsedGuid
                    : Guid.Empty;

                var applicationId = await _boilerDrawingService.SaveBoilerDrawingAsync( dto,  userIdGuid, "amend",registrationNo);

                return Ok(new
                {
                    success = true,
                    applicationId,
                    message = "Boiler drawing amendment submitted successfully."
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


        [HttpPost("renew")]
       
        public async Task<IActionResult> RenewBoilerDrawing(  [FromBody] BoilerDrawingRenewalDto dto)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;

                var userIdGuid = Guid.TryParse(userId, out var parsedGuid)
                    ? parsedGuid
                    : Guid.Empty;

                var applicationId =  await _boilerDrawingService.RenewBoilerDrawingAsync(dto, userIdGuid);

                return Ok(new
                {
                    success = true,
                    applicationId,
                    message = "Boiler drawing renewal submitted successfully."
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
        
        public async Task<IActionResult> UpdateBoilerDrawing( string applicationId,  [FromBody] BoilerDrawingCreateDto dto)
        {
            try
            {
                var result = await _boilerDrawingService.UpdateBoilerDrawingAsync(WebUtility.UrlDecode(applicationId), dto);

                if (!result)
                    return NotFound("Application not found.");

                return Ok(new
                {
                    success = true,
                    message = "Boiler drawing application updated successfully."
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
            var result = await _boilerDrawingService.GetByApplicationIdAsync(WebUtility.UrlDecode(applicationId));

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("registration/{registrationNo}")]
        public async Task<IActionResult> GetLatestApproved(string registrationNo)
        {
            var result = await _boilerDrawingService.GetLatestApprovedByRegistrationNoAsync(WebUtility.UrlDecode(registrationNo));

            if (result == null)
                return NotFound("Latest version not approved.");

            return Ok(result);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _boilerDrawingService.GetAllAsync();

            return Ok(result);
        }

        [HttpPost("close")]         
        public async Task<IActionResult> CloseBoilerDrawing(  [FromBody] BoilerDrawingClosureDto dto)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;

                var userIdGuid = Guid.TryParse(userId, out var parsedGuid)
                    ? parsedGuid
                    : Guid.Empty;

                var applicationId =  await _boilerDrawingService.CloseBoilerDrawingAsync(dto, userIdGuid);

                return Ok(new
                {
                    success = true,
                    applicationId,
                    message = "Boiler drawing closure submitted successfully."
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
  


   