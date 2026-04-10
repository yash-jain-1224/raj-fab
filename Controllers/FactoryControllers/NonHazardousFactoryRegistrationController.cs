using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;
using System.Net;

namespace RajFabAPI.Controllers.FactoryControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NonHazardousFactoryRegistrationController : ControllerBase
    {
        private readonly INonHazardousFactoryRegistrationService _service;
        private readonly IESignService _eSignService;
        private readonly ILogger<NonHazardousFactoryRegistrationController> _logger;

        public NonHazardousFactoryRegistrationController(
            INonHazardousFactoryRegistrationService service,
            IESignService eSignService,
            ILogger<NonHazardousFactoryRegistrationController> logger)
        {
            _service = service;
            _eSignService = eSignService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("[GET] GetAll called");

            try
            {
                var result = await _service.GetAllAsync();

                _logger.LogInformation("[GET] GetAll success, count: {Count}", result?.Count());

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GET] GetAll failed");

                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            _logger.LogInformation("[GET] GetById called, Id: {Id}", id);

            try
            {
                var result = await _service.GetByIdAsync(id);

                if (result == null)
                {
                    _logger.LogWarning("[GET] GetById - Not found, Id: {Id}", id);
                    return NotFound(new { success = false, message = "Record not found" });
                }

                _logger.LogInformation("[GET] GetById success, Id: {Id}", id);

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GET] GetById failed, Id: {Id}", id);

                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CreateNonHazardousFactoryRegistrationRequest dto)
        {
            _logger.LogInformation("[POST] Create called");

            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;

                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("[POST] Create - Invalid UserId");
                    return Unauthorized("Invalid UserId");
                }

                var applicationId = await _service.CreateAsync(dto, userId);

                if (applicationId == null)
                {
                    _logger.LogError("[POST] Create failed - applicationId null, userId: {UserId}", userId);
                    throw new Exception("Application not created");
                }

                _logger.LogInformation("[POST] Create success, AppId: {AppId}, UserId: {UserId}", applicationId, userId);

                var html = await _eSignService.GenerateESignHtmlAsync(applicationId);

                _logger.LogInformation("[POST] ESign generated, AppId: {AppId}", applicationId);

                return Ok(new { success = true, data = html });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[POST] Create failed");

                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("details/{*applicationNumber}")]
        public async Task<IActionResult> GetDetails(string applicationNumber)
        {
            _logger.LogInformation("[GET] GetDetails called, AppId: {AppId}", applicationNumber);

            try
            {
                var decodedId = WebUtility.UrlDecode(applicationNumber);

                var result = await _service.GetByApplicationNumberAsync(decodedId);

                _logger.LogInformation("[GET] GetDetails success, AppId: {AppId}", decodedId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GET] GetDetails failed, AppId: {AppId}", applicationNumber);

                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("{id}/delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogInformation("[POST] Delete called, Id: {Id}", id);

            try
            {
                var success = await _service.DeleteAsync(id);

                if (!success)
                {
                    _logger.LogWarning("[POST] Delete - Not found, Id: {Id}", id);
                    return NotFound(new { success = false, message = "Record not found" });
                }

                _logger.LogInformation("[POST] Delete success, Id: {Id}", id);

                return Ok(new { success = true, data = success });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "[POST] Delete validation error, Id: {Id}", id);

                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[POST] Delete failed, Id: {Id}", id);

                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("update/{applicationId}")]
        public async Task<IActionResult> Update(Guid applicationId, CreateNonHazardousFactoryRegistrationRequest dto)
        {
            _logger.LogInformation("[POST] Update called, AppId: {AppId}", applicationId);

            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;

                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("[POST] Update - Invalid User");
                    return Unauthorized("Invalid User");
                }

                await _service.UpdateAsync(applicationId, dto, userId);

                _logger.LogInformation("[POST] Update success, AppId: {AppId}, UserId: {UserId}", applicationId, userId);

                var html = await _eSignService.GenerateESignHtmlAsync(applicationId.ToString());

                _logger.LogInformation("[POST] ESign generated after update, AppId: {AppId}", applicationId);

                return Ok(new { success = true, data = html });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[POST] Update failed, AppId: {AppId}", applicationId);

                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // POST: api/CommencementCessations/generateCertificate/{registrationId}
        [Authorize]
        [HttpPost("generateCertificate/{registrationId}")]
        public async Task<IActionResult> GenerateCertificate(
            [FromBody] CertificateRequestDto dto,
            string registrationId)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

            try
            {
                var certificateId = await _service.GenerateNonHazardousCertificateAsync(dto, userIdGuid, Guid.Parse(registrationId));
                var html = await _eSignService.GenerateCertificateESignHtmlAsync(certificateId);

                return Ok(new { success = true, data = html });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error while generating certificate.");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Data not found while generating certificate.");
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating certificate.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { success = false, message = "An error occurred while generating the certificate." });
            }
        }
    }
}