using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IManagerChangeService

    {
        Task<IEnumerable<ManagerChangeGetResponseDto>> GetAllAsync(Guid userId);
        Task<ManagerChangeResponseDto> CreateAsync( CreateManagerChangeRequestDto dto,  Guid userId);
        Task<ManagerChangeApplicationDto> GetByIdAsync(Guid managerChangeId);
        Task<ManagerChangeResponseDto> UpdateAsync(Guid managerChangeId, UpdateManagerChangeRequestDto dto);
        Task<string> GenerateManagerChangePdfAsync(Guid managerChangeId);
        Task<bool> UpdateStatusAndRemark(string applicationId, string status);
        Task<string> GenerateObjectionLetter(ManagerChangeObjectionLetterDto dto, string applicationId);
    }
}

