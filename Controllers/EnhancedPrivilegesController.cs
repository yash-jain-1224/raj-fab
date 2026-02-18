using iTextSharp.text.log;
using Microsoft.AspNetCore.Mvc;
using RajFabAPI.Controllers.Common;
using RajFabAPI.DTOs;
using RajFabAPI.Services;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/privileges")]
    public class EnhancedPrivilegesController : ControllerBase
    {
        private readonly IEnhancedPrivilegeService _service;
        private readonly ILogger<ApplicationApprovalRequestsController> _logger;

        public EnhancedPrivilegesController(IEnhancedPrivilegeService service, ILogger<ApplicationApprovalRequestsController> logger)
        {
            _logger = logger;
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
        public async Task<IActionResult> AssignRolePrivileges([FromBody] AssignRolePrivilegesDto dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, message = "Request body is required" });

            if (dto.RoleId == Guid.Empty)
                return BadRequest(new { success = false, message = "Valid RoleId is required" });

            if (dto.ModulePermissions == null || !dto.ModulePermissions.Any())
                return BadRequest(new { success = false, message = "At least one module permission is required" });

            try
            {
                var success = await _service.AssignRolePrivilegesAsync(dto);

                if (!success)
                    return NotFound(new { success = false, message = "Role not found" });

                return Ok(new { success = true, message = "Role privileges assigned successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning privileges to role {RoleId}", dto.RoleId);

                return StatusCode(500, new { success = false, message = "An error occurred while assigning privileges" });
            }
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
