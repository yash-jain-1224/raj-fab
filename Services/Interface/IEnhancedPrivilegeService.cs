using RajFabAPI.DTOs;

namespace RajFabAPI.Services
{
    public interface IEnhancedPrivilegeService
    {
        Task<IEnumerable<ModulePermissionDto>> GetModulePermissionsAsync(Guid moduleId);
        Task<IEnumerable<ModulePermissionDto>> GetAllModulePermissionsAsync();

        Task<RolePrivilegeDataDto?> GetRolePrivilegesAsync(Guid roleId);

        Task<bool> AssignRolePrivilegesAsync(AssignRolePrivilegesDto dto);

        Task<bool> RemoveRoleModulePrivilegesAsync(Guid roleId, Guid moduleId);

        Task<bool> CheckRolePermissionAsync(Guid roleId, Guid moduleId, string permission);

        Task<PermissionCheckResultDto> CheckUserPermissionAsync(Guid userId, Guid moduleId, string permission);

        Task<bool> InitializeDefaultPermissionsAsync();
    }
}
