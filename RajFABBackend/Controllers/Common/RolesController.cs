using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _service;
        public RolesController(IRoleService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? officeId = null)
        {
            var roles = officeId.HasValue
                ? await _service.GetByOfficeAsync(officeId.Value)
                : await _service.GetAllAsync();

            return Ok(new { success = true, data = roles });
        }

        // GET: api/roles/with-privileges
        [HttpGet("with-privileges")]
        public async Task<IActionResult> GetAllWithPrivileges([FromQuery] Guid? officeId)
        {
            var roles = await _service.GetAllWithPrivilegesAsync(officeId);
            return Ok(new { success = true, data = roles });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
        {
            try
            {
                var role = await _service.CreateAsync(dto);
                return Ok(new { success = true, data = role });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var msg = dbEx.InnerException?.Message ?? dbEx.Message;

                if (msg.Contains("IX_Roles_PostId_OfficeId") || msg.Contains("duplicate"))
                    return BadRequest(new { success = false, message = "A role with this post already exists in this office." });

                return StatusCode(500, new { success = false, message = "Database error while creating role." });
            }
        }

        [HttpPost("{id}/update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateRoleDto dto)
        {
            try
            {
                var role = await _service.UpdateAsync(id, dto);
                if (role == null)
                    return NotFound(new { success = false, message = "Role not found" });

                return Ok(new { success = true, data = role });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var msg = dbEx.InnerException?.Message ?? dbEx.Message;

                if (msg.Contains("IX_Roles_PostId_OfficeId") || msg.Contains("duplicate"))
                    return BadRequest(new { success = false, message = "A role with this post already exists in this office." });

                return StatusCode(500, new { success = false, message = "Database error while updating role." });
            }
        }

        [HttpPost("{id}/delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _service.DeleteAsync(id);

                if (!success)
                    return NotFound(new { success = false, message = "Role not found" });

                return Ok(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
