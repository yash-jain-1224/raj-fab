using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IMasterService
    {
        Task<IEnumerable<MasterResponseDto>> GetAllAsync();
        Task<IEnumerable<MasterResponseDto>> GetByOptionIdAsync(int optionId);
        Task<MasterResponseDto?> GetByIdAsync(int id);
        Task<MasterResponseDto> CreateAsync(CreateMasterDto dto);
        Task<MasterResponseDto?> UpdateAsync(int id, UpdateMasterDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<MasterResponseDto>> GetByMasterNameAsync(string masterName);
    }
}