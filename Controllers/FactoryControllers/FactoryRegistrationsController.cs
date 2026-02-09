using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers.FactoryControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FactoryRegistrationsController : ControllerBase
    {
        private readonly IFactoryRegistrationService _factoryRegistrationService;

        public FactoryRegistrationsController(IFactoryRegistrationService factoryRegistrationService)
        {
            _factoryRegistrationService = factoryRegistrationService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<List<FactoryRegistrationDto>>>> GetAllRegistrations()
        {
            var result = await _factoryRegistrationService.GetAllRegistrationsAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<FactoryRegistrationDto>>> GetRegistrationById(string id)
        {
            var result = await _factoryRegistrationService.GetRegistrationByIdAsync(id);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet("by-registration-number/{registrationNumber}")]
        public async Task<ActionResult<ApiResponseDto<FactoryRegistrationDto>>> GetRegistrationByRegistrationNumber(string registrationNumber)
        {
            var result = await _factoryRegistrationService.GetRegistrationByRegistrationNumberAsync(registrationNumber);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<FactoryRegistrationDto>>> CreateRegistration([FromBody] CreateFactoryRegistrationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto<FactoryRegistrationDto>
                {
                    Success = false,
                    Message = "Invalid model state",
                    Data = null
                });
            }

            var result = await _factoryRegistrationService.CreateRegistrationAsync(request);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetRegistrationById), new { id = result.Data!.Id }, result);
        }

        [HttpPost("{id}/status/update")]
        public async Task<ActionResult<ApiResponseDto<FactoryRegistrationDto>>> UpdateRegistrationStatus(string id, [FromBody] UpdateFactoryRegistrationStatusRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto<FactoryRegistrationDto>
                {
                    Success = false,
                    Message = "Invalid model state",
                    Data = null
                });
            }

            // For now, we'll use a placeholder reviewer. In a real application, this would come from authentication
            var reviewedBy = "System Admin";
            var result = await _factoryRegistrationService.UpdateRegistrationStatusAsync(id, request, reviewedBy);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("{id}/amend")]
        public async Task<ActionResult<ApiResponseDto<FactoryRegistrationDto>>> AmendRegistration(string id, [FromBody] CreateFactoryRegistrationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto<FactoryRegistrationDto>
                {
                    Success = false,
                    Message = "Invalid model state",
                    Data = null
                });
            }

            var result = await _factoryRegistrationService.AmendRegistrationAsync(id, request);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("{id}/delete")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteRegistration(string id)
        {
            var result = await _factoryRegistrationService.DeleteRegistrationAsync(id);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpPost("{id}/documents")]
        public async Task<ActionResult<ApiResponseDto<FactoryRegistrationDocumentDto>>> UploadDocument(string id, IFormFile file, [FromForm] string documentType)
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
                return BadRequest(new ApiResponseDto<FactoryRegistrationDocumentDto>
                {
                    Success = false,
                    Message = "Document type is required",
                    Data = null
                });
            }

            var result = await _factoryRegistrationService.UploadDocumentAsync(id, file, documentType);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("documents/{documentId}/delete")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteDocument(string documentId)
        {
            var result = await _factoryRegistrationService.DeleteDocumentAsync(documentId);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
    }
}