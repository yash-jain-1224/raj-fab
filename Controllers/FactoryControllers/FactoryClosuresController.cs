using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers.FactoryControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FactoryClosuresController : ControllerBase
    {
        private readonly IFactoryClosureService _factoryClosureService;
        private readonly ILogger<FactoryClosuresController> _logger;

        public FactoryClosuresController(
            IFactoryClosureService factoryClosureService,
            ILogger<FactoryClosuresController> logger)
        {
            _factoryClosureService = factoryClosureService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<List<FactoryClosureDto>>>> GetAllClosures()
        {
            var result = await _factoryClosureService.GetAllClosuresAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<FactoryClosureDto>>> GetClosureById(string id)
        {
            var result = await _factoryClosureService.GetClosureByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet("factory/{factoryRegistrationId}")]
        public async Task<ActionResult<ApiResponseDto<List<FactoryClosureDto>>>> GetClosuresByFactory(string factoryRegistrationId)
        {
            var result = await _factoryClosureService.GetClosuresByFactoryRegistrationIdAsync(factoryRegistrationId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<FactoryClosureDto>>> CreateClosure([FromBody] CreateFactoryClosureRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _factoryClosureService.CreateClosureAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetClosureById), new { id = result.Data!.Id }, result);
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult<ApiResponseDto<FactoryClosureDto>>> UpdateClosureStatus(
            string id,
            [FromBody] UpdateFactoryClosureStatusRequest request,
            [FromQuery] string reviewedBy = "Admin")
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _factoryClosureService.UpdateStatusAsync(id, request, reviewedBy);
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpPost("{id}/documents")]
        public async Task<ActionResult<ApiResponseDto<FactoryClosureDocumentDto>>> UploadDocument(
            string id,
            IFormFile file,
            [FromForm] string documentType)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponseDto<FactoryClosureDocumentDto>
                {
                    Success = false,
                    Message = "No file provided"
                });
            }

            // Validate file size (25MB limit)
            if (file.Length > 25 * 1024 * 1024)
            {
                return BadRequest(new ApiResponseDto<FactoryClosureDocumentDto>
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
                return BadRequest(new ApiResponseDto<FactoryClosureDocumentDto>
                {
                    Success = false,
                    Message = "Invalid file type. Allowed: PDF, DOC, DOCX, JPG, JPEG, PNG"
                });
            }

            var result = await _factoryClosureService.UploadDocumentAsync(id, file, documentType);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteClosure(string id)
        {
            var result = await _factoryClosureService.DeleteClosureAsync(id);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
    }
}
