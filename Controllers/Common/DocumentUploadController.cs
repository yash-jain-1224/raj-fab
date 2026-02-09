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
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "File is required" });

            var userIdClaim = User.FindFirst("userId")?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { success = false, message = "Invalid user" });

            try
            {
                var result = await _service.UploadAsync(file, userId);
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
    }
}
