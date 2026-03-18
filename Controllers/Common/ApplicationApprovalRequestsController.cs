using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;
using System.Threading.Tasks;
using static RajFabAPI.Constants.AppConstants;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace RajFabAPI.Controllers.Common
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationApprovalRequestsController : ControllerBase
    {
        private readonly IApplicationApprovalRequestService _service;
        private readonly ILogger<ApplicationApprovalRequestsController> _logger;

        public ApplicationApprovalRequestsController(IApplicationApprovalRequestService service, ILogger<ApplicationApprovalRequestsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // POST: api/applicationapprovalrequests/{requestId}
        [HttpPost("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateApplicationApprovalRequestDto dto)
        {
            _logger.LogInformation("Update request received for application approval request Id {Id}", id);
            try
            {
                var result = await _service.UpdateAsync(id, dto);
                if (result == null)
                {
                    _logger.LogWarning("Application approval request with Id {Id} not found", id);
                    return NotFound(new { success = false, message = "Application approval request not found" });
                }
                _logger.LogInformation("Application approval request with Id {Id} updated successfully", id);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating application approval request with Id {Id}", id);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // POST: api/applicationapprovalrequests/forwardapplication/{requestId}
        [HttpPost("forwardapplication/{id}")]
        public async Task<IActionResult> ForwardApplication(int id, [FromBody] ForwardApplicationRequestDto dto)
        {
            _logger.LogInformation("Forward application request received for Id {Id}", id);
            try
            {
                var updateDto = new UpdateApplicationApprovalRequestDto
                {
                    Status = ApplicationStatus.Forwarded,
                    Remarks = dto.Remarks
                };

                var result = await _service.UpdateAsync(id, updateDto);
                if (result == null)
                {
                    _logger.LogWarning("Application approval request with Id {Id} not found for forwarding", id);
                    return NotFound(new { success = false, message = "Application approval request not found" });
                }
                _logger.LogInformation("Application approval request with Id {Id} forwarded successfully", id);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding application approval request with Id {Id}", id);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // GET: api/applicationapprovalrequests/byoffice/{officeId}
        [Authorize]
        [HttpGet("office/dashboard")]
        public async Task<IActionResult> GetApplicationsByOffice()
        {
            try
            {
                var officePostId = User.FindFirst("officePostId")?.Value;
                var officePostGuid = Guid.TryParse(officePostId, out var parsedGuid) ? parsedGuid : Guid.Empty;
                if (officePostGuid == Guid.Empty)
                {
                    _logger.LogWarning("Invalid office post Id in token for office post requestId");
                    return BadRequest(new { success = false, message = "Invalid/No office post configured for user." });
                }
                var result = await _service.GetApplicationsByOfficePostIdAsync(officePostGuid);
                _logger.LogInformation("Returned {Count} applications for office", result.Count);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving applications for office");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // api to get if current workflowlevel is the last one by applicationapprovalrequestId
        [HttpGet("ispending/{requestId}")]
        public async Task<IActionResult> IsLastWorkflowLevel(int requestId)
        {
            try
            {
                var isPending = await _service.IsLastWorkflowLevelAsync(requestId);
                _logger.LogInformation("Checked if workflow level is last for request Id {Id}: {IsLast}", requestId, isPending);
                return Ok(new { success = true, isPending });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if workflow level is last for request Id {Id}", requestId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // GET: api/applicationapprovalrequests/objectionletters/{applicationId}
        // Returns all objection letters issued for this application — accessible by both authority and citizen
        [Authorize]
        [HttpGet("objectionletters/{applicationId}")]
        public async Task<IActionResult> GetObjectionLetters(string applicationId)
        {
            try
            {
                var letters = await _service.GetObjectionLettersByApplicationIdAsync(applicationId);
                return Ok(new { success = true, data = letters });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving objection letters for application {ApplicationId}", applicationId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("remarks/{registrationId}")]
        public async Task<IActionResult> GetRemarksByApplicationId(string registrationId)
        {
            try
            {
                if (String.IsNullOrEmpty(registrationId))
                {
                    _logger.LogWarning("Invalid registrationId");
                    return BadRequest(new { success = false, message = "Invalid/No registration id provided." });
                }
                var result = await _service.GetRemarksByApplicationId(registrationId);
                _logger.LogInformation("Returned remark details");
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving applications for office");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

    }
}
