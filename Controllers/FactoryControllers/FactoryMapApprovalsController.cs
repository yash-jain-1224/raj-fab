using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;
using static System.Net.Mime.MediaTypeNames;

namespace RajFabAPI.Controllers.FactoryControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FactoryMapApprovalsController : ControllerBase
    {
        private readonly IFactoryMapApprovalService _factoryMapApprovalService;
        private readonly IESignService _eSignService;

        public FactoryMapApprovalsController(IFactoryMapApprovalService factoryMapApprovalService, IESignService eSignService)
        {
            _factoryMapApprovalService = factoryMapApprovalService;
            _eSignService = eSignService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<List<FactoryMapApprovalDto>>>> GetAllApplications()
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;
            var result = await _factoryMapApprovalService.GetAllApplicationsAsync(userIdGuid);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<FactoryMapApprovalDto>>> GetApplicationById(string id)
        {
            var result = await _factoryMapApprovalService.GetApplicationByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet("by-acknowledgement/{acknowledgementNumber}")]
        public async Task<ActionResult<ApiResponseDto<FactoryMapApprovalDto>>> GetApplicationByAcknowledgementNumber(string acknowledgementNumber)
        {
            var result = await _factoryMapApprovalService.GetApplicationByAcknowledgementNumberAsync(acknowledgementNumber);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<string>>> CreateApplication([FromBody] CreateFactoryMapApprovalRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var userId = User.FindFirst("userId")?.Value;
                var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

                var applicationId = await _factoryMapApprovalService.CreateApplicationAsync(request, userIdGuid);
                if (applicationId == null)
                    throw new Exception("Application not created");
                var html = await _eSignService.GenerateESignHtmlAsync(applicationId);
                return CreatedAtAction(null, new { html }, new { html });
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetFeeAmountAsync exception: " + ex);
                // log exception if you have logging; return generic error to client
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("{id}/status/update")]
        public async Task<ActionResult<ApiResponseDto<FactoryMapApprovalDto>>> UpdateApplicationStatus(
            string id,
            [FromBody] UpdateFactoryMapApprovalStatusRequest request,
            [FromQuery] string reviewedBy = "Admin")
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _factoryMapApprovalService.UpdateApplicationStatusAsync(id, request, reviewedBy);
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpPost("{id}/amend")]
        public async Task<ActionResult<ApiResponseDto<FactoryMapApprovalDto>>> AmendApplication(string id, [FromBody] CreateFactoryMapApprovalRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirst("userId")?.Value;
                var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;
                var applicationId = await _factoryMapApprovalService.CreateApplicationAsync(request, userIdGuid, false, id);

                if (applicationId == null)
                    throw new Exception("Application not created");
                var html = await _eSignService.GenerateESignHtmlAsync(applicationId);
                return CreatedAtAction(null, new { html }, new { html });

            }
            catch (Exception ex)
            {
                Console.WriteLine("GetFeeAmountAsync exception: " + ex);
                // log exception if you have logging; return generic error to client
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while saving the registration.");
            }
        }

        [Authorize]
        [HttpPost("update/{applicationId}")]
        public async Task<ActionResult<ApiResponseDto<FactoryMapApprovalDto>>> UpdateApplication(
            string applicationId, [FromBody] CreateFactoryMapApprovalRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _factoryMapApprovalService.UpdateApplicationAsync(applicationId, request);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("{id}/delete")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteApplication(string id)
        {
            var result = await _factoryMapApprovalService.DeleteApplicationAsync(id);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpPost("{id}/documents")]
        public async Task<ActionResult<ApiResponseDto<FactoryMapDocumentDto>>> UploadDocument(
            string id,
            IFormFile file,
            [FromForm] string documentType)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponseDto<FactoryMapDocumentDto>
                {
                    Success = false,
                    Message = "No file provided"
                });
            }

            // Validate file size (25MB limit)
            if (file.Length > 25 * 1024 * 1024)
            {
                return BadRequest(new ApiResponseDto<FactoryMapDocumentDto>
                {
                    Success = false,
                    Message = "File size exceeds 25MB limit"
                });
            }

            // Validate file extension
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new ApiResponseDto<FactoryMapDocumentDto>
                {
                    Success = false,
                    Message = "Invalid file type. Allowed: PDF, DOC, DOCX, JPG, JPEG, PNG"
                });
            }

            var result = await _factoryMapApprovalService.UploadDocumentAsync(id, file, documentType);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("documents/{documentId}/delete")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteDocument(string documentId)
        {
            var result = await _factoryMapApprovalService.DeleteDocumentAsync(documentId);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet("generate-acknowledgement")]
        public ActionResult<string> GenerateAcknowledgementNumber()
        {
            var acknowledgementNumber = _factoryMapApprovalService.GenerateAcknowledgementNumber();
            return Ok(acknowledgementNumber);
        }

        [Authorize]
        [HttpPost("{id}/generateCertificate")]
        public async Task<IActionResult> GenerateCertificate(
            [FromBody] MapApprovalCertificateRequestDto dto, string id)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;
            try
            {
                var certificateId = await _factoryMapApprovalService.GenerateCertificateAsync(dto, userIdGuid, id);
                //var html = await _eSignService.GenerateCertificateESignHtmlAsync(certificateId);
                // return Ok(new { html });
                return Ok(new ApiResponseDto<FactoryMapDocumentDto>
                {
                    Success = true,
                    Message = "Certificate Generated successfully"
                });
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception) { return StatusCode(500, "An error occurred while generating the certificate."); }
        }
    }
}