using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IFormService
    {
        Task<IEnumerable<FormResponseDto>> GetAllFormsAsync();
        Task<IEnumerable<FormResponseDto>> GetFormsByModuleAsync(Guid moduleId);
        Task<FormResponseDto?> GetFormByIdAsync(Guid id);
        Task<FormResponseDto> CreateFormAsync(CreateFormDto dto);
        Task<FormResponseDto?> UpdateFormAsync(Guid id, UpdateFormDto dto);
        Task<bool> DeleteFormAsync(Guid id);
        Task<bool> FormExistsAsync(Guid id);
    }
}