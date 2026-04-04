using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IApplicationWorkFlowService
    {
        Task<IEnumerable<ApplicationWorkFlowResponseDto>> GetAllAsync();

        Task<IEnumerable<ApplicationWorkFlowResponseDto>> GetByOfficeAsync(Guid officeId);

        Task<ApplicationWorkFlowResponseDto?> GetByIdAsync(Guid id);

        Task<IEnumerable<ApplicationWorkFlowResponseDto>> CreateAsync(
            CreateApplicationWorkFlowDto dto);

        Task<ApplicationWorkFlowResponseDto?> UpdateAsync(
            Guid id, UpdateApplicationWorkFlowDto dto);

        Task<bool> DeleteAsync(Guid id);
        Task<bool> AddApplicationToWorkFlow(string applicationId);
    }

}
