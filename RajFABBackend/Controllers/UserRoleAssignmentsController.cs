using Microsoft.AspNetCore.Mvc;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserRoleAssignmentsController : ControllerBase
    {
        private readonly IUserRoleAssignmentService _service;

        public UserRoleAssignmentsController(IUserRoleAssignmentService service)
        {
            _service = service;
        }

        // GET: api/userroleassignments
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(new { success = true, data });
        }

        // GET: api/userroleassignments/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(Guid userId)
        {
            var data = await _service.GetByUserIdAsync(userId);
            return Ok(new { success = true, data });
        }

        // POST: api/userroleassignments
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRoleRequest dto)
        {
            try
            {
                var data = await _service.CreateAsync(dto);
                return Ok(new { success = true, data });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // PUT: api/userroleassignments/{id}/update
        [HttpPost("{id}/update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateUserRoleRequest dto)
        {
            try
            {
                var data = await _service.UpdateAsync(id, dto);
                return Ok(new { success = true, data });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return NotFound(new { success = false, message = "Role assignment not found" });
            }
        }

        // PUT: api/userroleassignments/{id}/delete
        [HttpPost("{id}/delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound(new { success = false, message = "Role assignment not found" });

            return Ok(new { success = true });
        }
    }
}
