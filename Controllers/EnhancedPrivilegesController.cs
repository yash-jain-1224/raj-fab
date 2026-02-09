using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/privileges")]
    public class EnhancedPrivilegesController : ControllerBase
    {
        private readonly IEnhancedPrivilegeService _service;

        public EnhancedPrivilegesController(IEnhancedPrivilegeService service)
        {
            _service = service;
        }

        [HttpGet("modules/{moduleId}/permissions")]
        public async Task<IActionResult> GetModulePermissions(Guid moduleId)
        {
            var permissions = await _service.GetModulePermissionsAsync(moduleId);
            return Ok(new { success = true, data = permissions });
        }

        [HttpGet("modules/permissions")]
        public async Task<IActionResult> GetAllModulePermissions()
        {
            var permissions = await _service.GetAllModulePermissionsAsync();
            return Ok(new { success = true, data = permissions });
        }

        [HttpGet("role/{roleId}")]
        public async Task<IActionResult> GetRolePrivileges(Guid roleId)
        {
            var privileges = await _service.GetRolePrivilegesAsync(roleId);
            if (privileges == null)
            {
                return NotFound(new { success = false, message = "Role not found" });
            }

            return Ok(new { success = true, data = privileges });
        }

        [HttpPost("role/assign")]
        public async Task<IActionResult> AssignRolePrivileges(
            [FromBody] AssignRolePrivilegesDto dto)
        {
           

            var success = await _service.AssignRolePrivilegesAsync(dto);
            if (!success)
            {
                return BadRequest(new { success = false, message = "Failed to assign role privileges" });
            }

            return Ok(new { success = true, message = "Role privileges assigned successfully" });
        }

        [HttpPost("role/{roleId}/modules/{moduleId}/remove")]
        public async Task<IActionResult> RemoveRoleModulePrivileges(
            Guid roleId,
            Guid moduleId)
        {
            var success = await _service.RemoveRoleModulePrivilegesAsync(roleId, moduleId);
            if (!success)
            {
                return NotFound(new { success = false, message = "Privileges not found" });
            }

            return Ok(new { success = true, message = "Module privileges removed from role" });
        }

        [HttpGet("role/{roleId}/check")]
        public async Task<IActionResult> CheckRolePermission(
            Guid roleId,
            [FromQuery] Guid moduleId,
            [FromQuery] string permission)
        {
            var hasPermission = await _service.CheckRolePermissionAsync(
                roleId,
                moduleId,
                permission);

            return Ok(new
            {
                success = true,
                data = new
                {
                    hasPermission
                }
            });
        }

        [HttpGet("user/{userId}/check")]
        public async Task<IActionResult> CheckUserPermission(
            Guid userId,
            [FromQuery] Guid moduleId,
            [FromQuery] string permission)
        {
            var result = await _service.CheckUserPermissionAsync(
                userId,
                moduleId,
                permission);

            return Ok(new { success = true, data = result });
        }

        [HttpPost("initialize-defaults")]
        public async Task<IActionResult> InitializeDefaultPermissions()
        {
            var success = await _service.InitializeDefaultPermissionsAsync();
            if (!success)
            {
                return BadRequest(new { success = false, message = "Failed to initialize default permissions" });
            }

            return Ok(new { success = true, message = "Default permissions initialized" });
        }
    }
}
