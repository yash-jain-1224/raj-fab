using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
  public interface IRoleInspectionPrivilegeService
  {
    Task<IEnumerable<RoleInspectionPrivilegeDto>> GetAllAsync();
    Task<IEnumerable<RoleInspectionPrivilegeDto>> GetByRoleAsync(Guid roleId);
    Task<RoleInspectionPrivilegeDto> CreateAsync(CreateRoleInspectionPrivilegeDto dto);
    Task<bool> DeleteAsync(Guid id);
  }
}