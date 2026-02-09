using RajFabAPI.Models;

namespace RajFabAPI.Services.Interface
{
    public interface IUserRoleAssignmentService
    {
        Task<IEnumerable<UserRoleResponseDto>> GetAllAsync();
        Task<IEnumerable<UserRoleResponseDto>> GetByUserIdAsync(Guid userId);
        Task<UserRoleResponseDto> CreateAsync(CreateUserRoleRequest dto);
        Task<UserRoleResponseDto?> UpdateAsync(Guid id, CreateUserRoleRequest dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
