using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly IPostService _service;
        public PostController(IPostService service) => _service = service;

        [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());
        [HttpGet("{id}")] public async Task<IActionResult> Get(Guid id) => Ok(await _service.GetByIdAsync(id));
        [HttpPost] public async Task<IActionResult> Create(CreatePostDto dto) => Ok(await _service.CreateAsync(dto));
        [HttpPost("{id}/update")] public async Task<IActionResult> Update(Guid id, UpdatePostDto dto) => Ok(await _service.UpdateAsync(id, dto));
        [HttpPost("{id}/delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _service.DeleteAsync(id);
                if (!success) return NotFound(new { success = false, message = "Post not found" });
                return Ok(new { success = true, data = success });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
