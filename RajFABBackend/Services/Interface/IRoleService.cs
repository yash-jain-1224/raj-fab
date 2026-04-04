using RajFabAPI.DTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services.Interface
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleResponseDto>> GetAllAsync();
        Task<IEnumerable<RoleResponseDto>> GetByOfficeAsync(Guid officeId);
        Task<List<RoleWithPrivilegesDto>> GetAllWithPrivilegesAsync(Guid? officeId = null);
        Task<Role> CreateAsync(CreateRoleDto dto);
        Task<Role?> UpdateAsync(Guid id, CreateRoleDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
