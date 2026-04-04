using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers.Common
{
    [ApiController]
    [Route("api/document")]
    public class DocumentUploadController : ControllerBase
    {
        private readonly IDocumentUploadService _service;

        public DocumentUploadController(IDocumentUploadService service)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFile([FromForm] DocumentUploadRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest(new { success = false, message = "File is required" });

            var userIdClaim = User.FindFirst("userId")?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { success = false, message = "Invalid user" });

            try
            {
                var result = await _service.UploadAsync(request.File, userId, request.ModuleId, request.ModuleDocType);

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("user-documents")]
        public async Task<IActionResult> GetUserDocuments()
        {
            try
            {
                var userIdClaim = User.FindFirst("userId")?.Value;

                if (!Guid.TryParse(userIdClaim, out var userId))
                    return Unauthorized(new { success = false, message = "Invalid user" });

                var result = await _service.GetDocumentsByUserAsync(userId);

                return Ok(new { success = true, data = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
