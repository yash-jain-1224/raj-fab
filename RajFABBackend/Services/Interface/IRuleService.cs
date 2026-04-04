// Services/IRuleService.cs
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services.Interface
{
    public interface IRuleService
    {
        Task<IEnumerable<RuleResponseDto>> GetAllAsync();
        Task<IEnumerable<RuleResponseDto>> GetByActAsync(Guid actId);
        Task<RuleResponseDto?> GetByIdAsync(Guid id);
        Task<RuleResponseDto> CreateAsync(CreateRuleDto dto);
        Task<RuleResponseDto?> UpdateAsync(Guid id, UpdateRuleDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}