using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommencementCessationsController : ControllerBase
    {
        private readonly ICommencementCessationService _service;
        private readonly IESignService _eSignService;
        private readonly ILogger<CommencementCessationsController> _logger;

        public CommencementCessationsController(
            ICommencementCessationService service,
            IESignService eSignService,
            ILogger<CommencementCessationsController> logger)
        {
            _service = service;
            _eSignService = eSignService;
            _logger = logger;
        }

        // GET: api/CommencementCessations
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _service.GetAllAsync();
                return Ok(new { success = true, data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all records.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { success = false, message = "An error occurred while fetching records." });
            }
        }

        // GET: api/CommencementCessations/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var data = await _service.GetByIdAsync(id);

                if (data == null)
                    return NotFound(new { success = false, message = "Record not found" });

                return Ok(new { success = true, data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching record with Id: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { success = false, message = "An error occurred while fetching the record." });
            }
        }

        // POST: api/CommencementCessations
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CommencementCessationRequestDto request)
        {
            try
            {
                var applicationId = await _service.CreateAsync(request);

                if (applicationId == null)
                    return BadRequest(new { success = false, message = "Application not created" });

                var html = await _eSignService.GenerateESignHtmlAsync(applicationId);

                return Ok(new { success = true, data = html });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error while creating application.");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Dependency not found while creating application.");
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while submitting the application.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { success = false, message = "An error occurred while submitting the application." });
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
                var certificateId = await _service.GenerateCertificateAsync(dto, userIdGuid, registrationId);
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