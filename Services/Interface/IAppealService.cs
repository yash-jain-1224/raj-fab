using RajFabAPI.Dtos;
using RajFabAPI.Models;

namespace RajFabAPI.Services
{
    public interface IAppealService
    {
        Task<string> CreateAsync(AppealCreateDto dto);
        Task<IEnumerable<AppealListDto>> GetAllAsync();
        Task<AppealDetailDto?> GetByIdAsync(string id);
        Task<bool> UpdateAsync(string id, AppealUpdateDto dto);
    }
}
