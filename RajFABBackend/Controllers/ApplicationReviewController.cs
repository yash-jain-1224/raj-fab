using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationReviewController : ControllerBase
    {
        private readonly IApplicationReviewService _service;

        public ApplicationReviewController(IApplicationReviewService service)
        {
            _service = service;
        }

        [HttpGet("assigned/{userId}")]
        public async Task<ActionResult<ApiResponseDto<List<ApplicationSummaryDto>>>> GetAssignedApplications(string userId, [FromQuery] Guid? moduleId = null)
        {
            var applications = await _service.GetAssignedApplicationsAsync(userId, moduleId);
            return Ok(new ApiResponseDto<List<ApplicationSummaryDto>>
            {
                Success = true,
                Data = applications
            });
        }

        [HttpGet("area/{areaId}")]
        public async Task<ActionResult<ApiResponseDto<List<ApplicationSummaryDto>>>> GetApplicationsByArea(Guid areaId)
        {
            var applications = await _service.GetApplicationsByAreaAsync(areaId);
            return Ok(new ApiResponseDto<List<ApplicationSummaryDto>>
            {
                Success = true,
                Data = applications
            });
        }

        [HttpGet("all")]
        public async Task<ActionResult<ApiResponseDto<List<ApplicationSummaryDto>>>> GetAllApplications()
        {
            var applications = await _service.GetAllApplicationsAsync();
            return Ok(new ApiResponseDto<List<ApplicationSummaryDto>>
            {
                Success = true,
                Data = applications
            });
        }

        [HttpGet("{applicationType}/{applicationId}")]
        public async Task<ActionResult<ApiResponseDto<ApplicationDetailDto>>> GetApplicationDetail(
            string applicationType, 
            string applicationId,
            [FromQuery] string userId)
        {
            var detail = await _service.GetApplicationDetailAsync(applicationType, applicationId, userId);
            if (detail == null)
            {
                return NotFound(new ApiResponseDto<ApplicationDetailDto>
                {
                    Success = false,
                    Message = "Application not found"
                });
            }

            return Ok(new ApiResponseDto<ApplicationDetailDto>
            {
                Success = true,
                Data = detail
            });
        }

        [HttpPost("{applicationType}/{applicationId}/forward")]
        public async Task<ActionResult<ApiResponseDto<bool>>> ForwardApplication(
            string applicationType, 
            string applicationId, 
            [FromBody] ForwardApplicationRequest request,
            [FromQuery] string userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Invalid request data"
                });
            }

            var result = await _service.ForwardApplicationAsync(applicationType, applicationId, request, userId);
            return Ok(new ApiResponseDto<bool>
            {
                Success = result,
                Data = result,
                Message = result ? "Application forwarded successfully" : "Failed to forward application"
            });
        }

        [HttpPost("{applicationType}/{applicationId}/remark")]
        public async Task<ActionResult<ApiResponseDto<bool>>> AddRemark(
            string applicationType, 
            string applicationId, 
            [FromBody] AddRemarkRequest request,
            [FromQuery] string userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Invalid request data"
                });
            }

            var result = await _service.AddRemarkAsync(applicationType, applicationId, request, userId);
            return Ok(new ApiResponseDto<bool>
            {
                Success = result,
                Data = result,
                Message = result ? "Remark added successfully" : "Failed to add remark"
            });
        }

        [HttpPost("{applicationType}/{applicationId}/approve")]
        public async Task<ActionResult<ApiResponseDto<bool>>> ApproveApplication(
            string applicationType, 
            string applicationId, 
            [FromBody] ApproveApplicationRequest request,
            [FromQuery] string userId)
        {
            var result = await _service.ApproveApplicationAsync(applicationType, applicationId, request, userId);
            return Ok(new ApiResponseDto<bool>
            {
                Success = result,
                Data = result,
                Message = result ? "Application approved successfully" : "Failed to approve application"
            });
        }

        [HttpPost("{applicationType}/{applicationId}/reject")]
        public async Task<ActionResult<ApiResponseDto<bool>>> RejectApplication(
            string applicationType, 
            string applicationId, 
            [FromBody] RejectApplicationRequest request,
            [FromQuery] string userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Invalid request data"
                });
            }

            var result = await _service.RejectApplicationAsync(applicationType, applicationId, request, userId);
            return Ok(new ApiResponseDto<bool>
            {
                Success = result,
                Data = result,
                Message = result ? "Application rejected" : "Failed to reject application"
            });
        }

        [HttpPost("{applicationType}/{applicationId}/return")]
        public async Task<ActionResult<ApiResponseDto<bool>>> ReturnToApplicant(
            string applicationType, 
            string applicationId, 
            [FromBody] ReturnApplicationRequest request,
            [FromQuery] string userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Invalid request data"
                });
            }

            var result = await _service.ReturnToApplicantAsync(applicationType, applicationId, request, userId);
            return Ok(new ApiResponseDto<bool>
            {
                Success = result,
                Data = result,
                Message = result ? "Application returned to applicant" : "Failed to return application"
            });
        }

        [HttpGet("{applicationType}/{applicationId}/history")]
        public async Task<ActionResult<ApiResponseDto<List<ApplicationHistoryDto>>>> GetApplicationHistory(
            string applicationType, 
            string applicationId)
        {
            var history = await _service.GetApplicationHistoryAsync(applicationType, applicationId);
            return Ok(new ApiResponseDto<List<ApplicationHistoryDto>>
            {
                Success = true,
                Data = history
            });
        }

        [HttpGet("{applicationType}/{applicationId}/eligible-reviewers")]
        public async Task<ActionResult<ApiResponseDto<List<EligibleReviewerDto>>>> GetEligibleReviewers(
            string applicationType, 
            string applicationId)
        {
            var reviewers = await _service.GetEligibleReviewersAsync(applicationType, applicationId);
            return Ok(new ApiResponseDto<List<EligibleReviewerDto>>
            {
                Success = true,
                Data = reviewers
            });
        }
    }
}
