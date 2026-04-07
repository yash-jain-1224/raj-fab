using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers.FactoryControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FactoryMapApprovalsController : ControllerBase
    {
        private readonly IFactoryMapApprovalService _factoryMapApprovalService;
        private readonly IESignService _eSignService;
        private readonly ILogger<FactoryMapApprovalsController> _logger;

        public FactoryMapApprovalsController(
            IFactoryMapApprovalService factoryMapApprovalService,
            ILogger<FactoryMapApprovalsController> logger,
            IESignService eSignService)
        {
            _factoryMapApprovalService = factoryMapApprovalService;
            _eSignService = eSignService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<List<FactoryMapApprovalDto>>>> GetAllApplications()
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;
            _logger.LogInformation("[GET] GetAllApplications — userId: {UserId}", userIdGuid);

            try
            {
                var result = await _factoryMapApprovalService.GetAllApplicationsAsync(userIdGuid);
                _logger.LogInformation("[GET] GetAllApplications — completed, success: {Success}", result.Success);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GET] GetAllApplications — unhandled exception for userId: {UserId}", userIdGuid);
                return StatusCode(500, "An error occurred while fetching applications.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<FactoryMapApprovalDto>>> GetApplicationById(string id)
        {
            _logger.LogInformation("[GET] GetApplicationById — id: {Id}", id);

            try
            {
                var result = await _factoryMapApprovalService.GetApplicationByIdAsync(id);
                if (!result.Success)
                {
                    _logger.LogWarning("[GET] GetApplicationById — not found, id: {Id}", id);
                    return NotFound(result);
                }
                _logger.LogInformation("[GET] GetApplicationById — found, id: {Id}", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GET] GetApplicationById — unhandled exception, id: {Id}", id);
                return StatusCode(500, "An error occurred while fetching the application.");
            }
        }

        [HttpGet("by-acknowledgement/{acknowledgementNumber}")]
        public async Task<ActionResult<ApiResponseDto<FactoryMapApprovalDto>>> GetApplicationByAcknowledgementNumber(string acknowledgementNumber)
        {
            _logger.LogInformation("[GET] GetApplicationByAcknowledgementNumber — ackNo: {AckNo}", acknowledgementNumber);

            try
            {
                var result = await _factoryMapApprovalService.GetApplicationByAcknowledgementNumberAsync(acknowledgementNumber);
                if (!result.Success)
                {
                    _logger.LogWarning("[GET] GetApplicationByAcknowledgementNumber — not found, ackNo: {AckNo}", acknowledgementNumber);
                    return NotFound(result);
                }
                _logger.LogInformation("[GET] GetApplicationByAcknowledgementNumber — found, ackNo: {AckNo}", acknowledgementNumber);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GET] GetApplicationByAcknowledgementNumber — unhandled exception, ackNo: {AckNo}", acknowledgementNumber);
                return StatusCode(500, "An error occurred while fetching the application.");
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<string>>> CreateApplication([FromBody] CreateFactoryMapApprovalRequest request)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;
            _logger.LogInformation("[POST] CreateApplication — userId: {UserId}", userIdGuid);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("[POST] CreateApplication — invalid model state, userId: {UserId}", userIdGuid);
                return BadRequest(ModelState);
            }

            try
            {
                var applicationId = await _factoryMapApprovalService.CreateApplicationAsync(request, userIdGuid);
                if (applicationId == null)
                {
                    _logger.LogError("[POST] CreateApplication — service returned null applicationId, userId: {UserId}", userIdGuid);
                    throw new Exception("Application not created");
                }

                _logger.LogInformation("[POST] CreateApplication — created, applicationId: {AppId}, userId: {UserId}", applicationId, userIdGuid);

                var html = await _eSignService.GenerateESignHtmlAsync(applicationId);
                _logger.LogInformation("[POST] CreateApplication — e-sign HTML generated, applicationId: {AppId}", applicationId);

                return CreatedAtAction(null, new { html }, new { html });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[POST] CreateApplication — unhandled exception, userId: {UserId}", userIdGuid);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{id}/status/update")]
        public async Task<ActionResult<ApiResponseDto<FactoryMapApprovalDto>>> UpdateApplicationStatus(
            string id,
            [FromBody] UpdateFactoryMapApprovalStatusRequest request,
            [FromQuery] string reviewedBy = "Admin")
        {
            _logger.LogInformation("[POST] UpdateApplicationStatus — id: {Id}, reviewedBy: {ReviewedBy}", id, reviewedBy);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("[POST] UpdateApplicationStatus — invalid model state, id: {Id}", id);
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _factoryMapApprovalService.UpdateApplicationStatusAsync(id, request, reviewedBy);
                if (!result.Success)
                {
                    _logger.LogWarning("[POST] UpdateApplicationStatus — not found or failed, id: {Id}", id);
                    return NotFound(result);
                }
                _logger.LogInformation("[POST] UpdateApplicationStatus — success, id: {Id}", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[POST] UpdateApplicationStatus — unhandled exception, id: {Id}", id);
                return StatusCode(500, "An error occurred while updating application status.");
            }
        }

        [HttpPost("{id}/amend")]
        public async Task<ActionResult<ApiResponseDto<FactoryMapApprovalDto>>> AmendApplication(
            string id, [FromBody] CreateFactoryMapApprovalRequest request)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;
            _logger.LogInformation("[POST] AmendApplication — id: {Id}, userId: {UserId}", id, userIdGuid);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("[POST] AmendApplication — invalid model state, id: {Id}", id);
                return BadRequest(ModelState);
            }

            try
            {
                var applicationId = await _factoryMapApprovalService.CreateApplicationAsync(request, userIdGuid, false, id);
                if (applicationId == null)
                {
                    _logger.LogError("[POST] AmendApplication — service returned null applicationId, id: {Id}", id);
                    throw new Exception("Application not created");
                }

                _logger.LogInformation("[POST] AmendApplication — amended, newApplicationId: {AppId}, originalId: {Id}", applicationId, id);

                var html = await _eSignService.GenerateESignHtmlAsync(applicationId);
                _logger.LogInformation("[POST] AmendApplication — e-sign HTML generated, applicationId: {AppId}", applicationId);

                return CreatedAtAction(null, new { html }, new { html });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[POST] AmendApplication — unhandled exception, id: {Id}", id);
                return StatusCode(500, "An error occurred while saving the registration.");
            }
        }

        [Authorize]
        [HttpPost("update/{applicationId}")]
        public async Task<ActionResult<ApiResponseDto<FactoryMapApprovalDto>>> UpdateApplication(
            string applicationId, [FromBody] CreateFactoryMapApprovalRequest request)
        {
            _logger.LogInformation("[POST] UpdateApplication — applicationId: {AppId}", applicationId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("[POST] UpdateApplication — invalid model state, applicationId: {AppId}", applicationId);
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _factoryMapApprovalService.UpdateApplicationAsync(applicationId, request);
                if (!result.Success)
                {
                    _logger.LogWarning("[POST] UpdateApplication — not found or failed, applicationId: {AppId}", applicationId);
                    return NotFound(result);
                }
                _logger.LogInformation("[POST] UpdateApplication — success, applicationId: {AppId}", applicationId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[POST] UpdateApplication — unhandled exception, applicationId: {AppId}", applicationId);
                return StatusCode(500, "An error occurred while updating the application.");
            }
        }

        [HttpPost("{id}/delete")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteApplication(string id)
        {
            _logger.LogInformation("[POST] DeleteApplication — id: {Id}", id);

            try
            {
                var result = await _factoryMapApprovalService.DeleteApplicationAsync(id);
                if (!result.Success)
                {
                    _logger.LogWarning("[POST] DeleteApplication — not found, id: {Id}", id);
                    return NotFound(result);
                }
                _logger.LogInformation("[POST] DeleteApplication — deleted, id: {Id}", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[POST] DeleteApplication — unhandled exception, id: {Id}", id);
                return StatusCode(500, "An error occurred while deleting the application.");
            }
        }

        [HttpPost("{id}/documents")]
        public async Task<ActionResult<ApiResponseDto<FactoryMapDocumentDto>>> UploadDocument(
            string id, IFormFile file, [FromForm] string documentType)
        {
            _logger.LogInformation("[POST] UploadDocument — id: {Id}, documentType: {DocType}, fileName: {FileName}, size: {Size}b",
                id, documentType, file?.FileName, file?.Length);

            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("[POST] UploadDocument — no file provided, id: {Id}", id);
                return BadRequest(new ApiResponseDto<FactoryMapDocumentDto> { Success = false, Message = "No file provided" });
            }

            if (file.Length > 25 * 1024 * 1024)
            {
                _logger.LogWarning("[POST] UploadDocument — file too large ({Size}b), id: {Id}", file.Length, id);
                return BadRequest(new ApiResponseDto<FactoryMapDocumentDto> { Success = false, Message = "File size exceeds 25MB limit" });
            }

            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                _logger.LogWarning("[POST] UploadDocument — invalid extension '{Ext}', id: {Id}", fileExtension, id);
                return BadRequest(new ApiResponseDto<FactoryMapDocumentDto> { Success = false, Message = "Invalid file type. Allowed: PDF, DOC, DOCX, JPG, JPEG, PNG" });
            }

            try
            {
                var result = await _factoryMapApprovalService.UploadDocumentAsync(id, file, documentType);
                if (!result.Success)
                {
                    _logger.LogWarning("[POST] UploadDocument — upload failed, id: {Id}", id);
                    return BadRequest(result);
                }
                _logger.LogInformation("[POST] UploadDocument — success, id: {Id}, docType: {DocType}", id, documentType);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[POST] UploadDocument — unhandled exception, id: {Id}", id);
                return StatusCode(500, "An error occurred while uploading the document.");
            }
        }

        [HttpPost("documents/{documentId}/delete")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteDocument(string documentId)
        {
            _logger.LogInformation("[POST] DeleteDocument — documentId: {DocId}", documentId);

            try
            {
                var result = await _factoryMapApprovalService.DeleteDocumentAsync(documentId);
                if (!result.Success)
                {
                    _logger.LogWarning("[POST] DeleteDocument — not found, documentId: {DocId}", documentId);
                    return NotFound(result);
                }
                _logger.LogInformation("[POST] DeleteDocument — deleted, documentId: {DocId}", documentId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[POST] DeleteDocument — unhandled exception, documentId: {DocId}", documentId);
                return StatusCode(500, "An error occurred while deleting the document.");
            }
        }

        [HttpGet("generate-acknowledgement")]
        public ActionResult<string> GenerateAcknowledgementNumber()
        {
            _logger.LogInformation("[GET] GenerateAcknowledgementNumber — requested");

            try
            {
                var acknowledgementNumber = _factoryMapApprovalService.GenerateAcknowledgementNumber();
                _logger.LogInformation("[GET] GenerateAcknowledgementNumber — generated: {AckNo}", acknowledgementNumber);
                return Ok(acknowledgementNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GET] GenerateAcknowledgementNumber — unhandled exception");
                return StatusCode(500, "An error occurred while generating acknowledgement number.");
            }
        }

        [Authorize]
        [HttpPost("{id}/generateCertificate")]
        public async Task<IActionResult> GenerateCertificate(
            [FromBody] MapApprovalCertificateRequestDto dto, string id)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;
            _logger.LogInformation("[POST] GenerateCertificate — id: {Id}, userId: {UserId}", id, userIdGuid);

            try
            {
                var certificateId = await _factoryMapApprovalService.GenerateCertificateAsync(dto, userIdGuid, id);
                _logger.LogInformation("[POST] GenerateCertificate — success, certificateId: {CertId}, id: {Id}", certificateId, id);

                return Ok(new ApiResponseDto<FactoryMapDocumentDto>
                {
                    Success = true,
                    Message = "Certificate Generated successfully"
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "[POST] GenerateCertificate — not found, id: {Id}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[POST] GenerateCertificate — unhandled exception, id: {Id}, userId: {UserId}", id, userIdGuid);
                return StatusCode(500, "An error occurred while generating the certificate.");
            }
        }
    }
}