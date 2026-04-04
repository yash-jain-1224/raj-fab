using RajFabAPI.DTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services.Interface
{
    public interface IModuleService
    {
        Task<IEnumerable<ModuleResponseDto>> GetAllModulesAsync(Guid? ruleId);
        Task<ModuleResponseDto?> GetModuleByIdAsync(Guid id);
        Task<ModuleResponseDto> CreateModuleAsync(CreateModuleDto dto);
        Task<ModuleResponseDto?> UpdateModuleAsync(Guid id, UpdateModuleDto dto);
        Task<bool> DeleteModuleAsync(Guid id);
        Task<bool> ModuleExistsAsync(Guid id);
    }
}