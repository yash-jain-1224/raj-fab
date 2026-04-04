using RajFabAPI.Dtos;
using RajFabAPI.Models;

namespace RajFabAPI.Services
{
    public interface IAppealService
    {
        Task<string> CreateAsync(AppealCreateDto dto);
        Task<IEnumerable<AppealListDto>> GetAllAsync();
        Task<AppealResDto?> GetByIdAsync(string id);
        Task<bool> UpdateAsync(string id, AppealUpdateDto dto);
        Task<string> GenerateAppealPdf(AppealResDto dto);
    }
}
