using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RajFabAPI.Controllers.Common;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RajFabAPI.Controllers.FactoryControllers
{
    [ApiController]
    [Route("api/establishment")]
    public class EstablishmentRegistrationController : ControllerBase
    {
        private readonly IEstablishmentRegistrationService _service;
        private readonly IValidator<CreateEstablishmentRegistrationDto> _validator;
        private readonly ILogger<ApplicationRegistrationController> _logger;

        public EstablishmentRegistrationController(
            IEstablishmentRegistrationService service,
            IValidator<CreateEstablishmentRegistrationDto> validator,
            ILogger<ApplicationRegistrationController> logger)
        {
            _service = service;
            _validator = validator;
            _logger = logger;
        }

        // POST: api/factoryobject/complete
        // Accepts the full JSON payload, persists across normalized tables and returns registration id when successful.
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateEstablishmentRegistrationDto dto)
        {
            if (dto == null) return BadRequest("Payload required.");
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;
            //var userIdGuid = Guid.Parse("831EDD9B-1546-4B92-82D9-1328E35D7A0A");
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => string.IsNullOrWhiteSpace(g.Key) ? "Request" : g.Key,
                        g => g.Select(
                            x => x.ErrorMessage).ToArray()
                    );

                var details = new ValidationProblemDetails(errors)
                {
                    Title = "Validation failed",
                    Status = StatusCodes.Status400BadRequest
                };
                return ValidationProblem(details);
            }

            try
            {
                var html = await _service.SaveEstablishmentAsync(dto, userIdGuid);
                //return Content(html, "text/html");

                return CreatedAtAction(null, new { html }, new { html });
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception)
            {
                // log exception if you have logging; return generic error to client
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while saving the registration.");
            }
        }

        // GET: api/establishment/{id}
        // Retrieves the registration details for the given id.
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRegistrationDetails(string id)
        {
            try
            {
                var details = await _service.GetRegistrationDetailsAsync(id);
                if (details == null)
                    return NotFound("Registration not found.");

                return Ok(new { success = true, data = details });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the registration details.");
            }
        }

        [HttpGet("establishmentDetails/{registrationId}")]
        public async Task<IActionResult> GetAllEntitiesByRegistrationId(string registrationId)
        {
            if (string.IsNullOrWhiteSpace(registrationId))
                return BadRequest("registrationId is required.");
            var data = await _service.GetAllEntitiesByRegistrationIdAsync(registrationId);
            if (data == null)
                return NotFound();
            return Ok(new { success = true, data });
        }

        [HttpGet("factoryDetails/{factoryRegistrationNumber}")]
        public async Task<IActionResult> GetFactoryDetailsByFactoryRegistrationNumber(string factoryRegistrationNumber)
        {
            if (string.IsNullOrWhiteSpace(factoryRegistrationNumber))
                return BadRequest("Factory Registration Number is required.");

            try
            {
                var data = await _service
                    .GetFactoryDetailsByFactoryRegistrationNumberAsync(factoryRegistrationNumber);

                if (data == null)
                    return NotFound(new { success = false, message = "Factory not found." });

                return Ok(new { success = true, data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // GET: api/establishment/all
        [Authorize]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllEstablishmentDetails()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("Invalid userId in token");
                return BadRequest(new { success = false, message = "Invalid user ID in token." });
            }
            var details = await _service.GetAllEstablishmentDetailsAsync(userId);
            return Ok(new { success = true, data = details });
        }

        [Authorize]
        [HttpGet("getfactoryregistrationnumber")]
        public async Task<IActionResult> GetFactoryRegistrationNumberByUser()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("Invalid userId in token");
                return BadRequest(new { success = false, message = "Invalid user ID in token." });
            }
            var FactoryRegistrationNumber = await _service.GetFactoryRegistrationNumber(userId);
            return Ok(new { success = true, FactoryRegistrationNumber });
        }

        // PUT: api/establishment/{id}
        // Updates the establishment registration with the given id.
        [Authorize]
        [HttpPost("update/{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CreateEstablishmentRegistrationDto dto)
        {
            if (dto == null) return BadRequest("Payload required.");
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;
            //var userIdGuid = Guid.Parse("831EDD9B-1546-4B92-82D9-1328E35D7A0A");
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => string.IsNullOrWhiteSpace(g.Key) ? "Request" : g.Key,
                        g => g.Select(x => x.ErrorMessage).ToArray()
                    );

                var details = new ValidationProblemDetails(errors)
                {
                    Title = "Validation failed",
                    Status = StatusCodes.Status400BadRequest
                };
                return ValidationProblem(details);
            }

            try
            {
                var registrationId = await _service.UpdateEstablishmentAsync(id, dto, userIdGuid);
                return Ok(new { registrationId });
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception)
            {
                // log exception if you have logging; return generic error to client
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the registration.");
            }
        }

        [HttpPost("{id}/documents")]
        public async Task<ActionResult<ApiResponseDto<EstablishmentRegistrationDocumentDto>>> UploadDocument(string id, IFormFile file, [FromForm] string documentType)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponseDto<FactoryRegistrationDocumentDto>
                {
                    Success = false,
                    Message = "No file provided",
                    Data = null
                });
            }

            if (string.IsNullOrEmpty(documentType))
            {
                return BadRequest(new ApiResponseDto<EstablishmentRegistrationDocumentDto>
                {
                    Success = false,
                    Message = "Document type is required",
                    Data = null
                });
            }

            var result = await _service.UploadDocumentAsync(id, file, documentType);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("documents/{documentId}/delete")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteDocument(string documentId)
        {
            var result = await _service.DeleteDocumentAsync(documentId);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [Authorize]
        [HttpPost("amendmend/{registrationId}")]
        public async Task<IActionResult> Amendment(string registrationId, [FromBody] CreateEstablishmentRegistrationDto dto)
        {
            if (dto == null) return BadRequest("Payload required.");
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;
            //var userIdGuid = Guid.Parse("831EDD9B-1546-4B92-82D9-1328E35D7A0A");
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => string.IsNullOrWhiteSpace(g.Key) ? "Request" : g.Key,
                        g => g.Select(
                            x => x.ErrorMessage).ToArray()
                    );

                var details = new ValidationProblemDetails(errors)
                {
                    Title = "Validation failed",
                    Status = StatusCodes.Status400BadRequest
                };
                return ValidationProblem(details);
            }

            try
            {
                var registrationNumber = await _service.SaveEstablishmentAsync(dto, userIdGuid, "amendment", registrationId);
                return CreatedAtAction(null, new { id = registrationNumber }, new { registrationNumber });
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception)
            {
                // log exception if you have logging; return generic error to client
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while saving the registration.");
            }
        }

        [Authorize]
        [HttpPost("renew/{registrationId}")]
        public async Task<IActionResult> Renew(string registrationId, [FromBody] RenewEstablishmentDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

            try
            {
                var registrationNumber = await _service.RenewEstablishmentAsync(dto, userIdGuid, registrationId);
                return CreatedAtAction(null, new { id = registrationNumber }, new { registrationNumber });
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while saving the registration.");
            }
        }

        [Authorize]
        [HttpPost("generateCertificate/{registrationId}")]
        public async Task<IActionResult> GenerateCertificate(
            [FromBody] EstablishmentCertificateRequestDto dto, string registrationId)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

            try
            {
                var certificateUrl = await _service.GenerateCertificateAsync(dto, userIdGuid, registrationId);
                return CreatedAtAction(null, new { id = certificateUrl }, new { certificateUrl });
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while saving the registration.");
            }
        }
    }
}