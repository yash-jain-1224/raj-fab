using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    public class ModulePermissionDto
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public string PermissionName { get; set; } = string.Empty;
        public string PermissionCode { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class RoleModulePermissionDto
    {
        public Guid ModuleId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
        public Guid ActId { get; set; }

        public Guid RuleId { get; set; }
    }

    public class RolePrivilegeDataDto
    {
        public Guid RoleId { get; set; }
        public string PostName { get; set; } = string.Empty;
        public List<RoleModulePermissionDto> ModulePermissions { get; set; } = new();
    }

    public class AssignRolePrivilegesDto
    {
        [Required]
        public Guid RoleId { get; set; }

        [Required]
        public List<RoleModulePermissionAssignmentDto> ModulePermissions { get; set; } = new();
    }

    public class RoleModulePermissionAssignmentDto
    {
        [Required]
        public Guid ModuleId { get; set; }

        [Required]
        public List<string> Permissions { get; set; } = new();
    }

    public class PermissionCheckResultDto
    {
        public bool HasPermission { get; set; }
        public string? Reason { get; set; }
    }
}
