using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IManagerChangeService

    {
        Task<IEnumerable<ManagerChangeGetResponseDto>> GetAllAsync(Guid userId);
        Task<ManagerChangeResponseDto> CreateAsync( CreateManagerChangeRequestDto dto,  Guid userId);
        Task<ManagerChangeGetResponseDto> GetByIdAsync(Guid managerChangeId);
        Task<ManagerChangeResponseDto> UpdateAsync(Guid managerChangeId, UpdateManagerChangeRequestDto dto);
    }
}

