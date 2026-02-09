

// Services/IAreaService.cs



// Services/IAreaService.cs
using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IAreaService
    {
        Task<IEnumerable<AreaResponseDto>> GetAllAsync();
        Task<IEnumerable<AreaResponseDto>> GetByDistrictAsync(Guid districtId);
        Task<IEnumerable<AreaResponseDto>> GetByCityAsync(Guid cityId);
        Task<AreaResponseDto?> GetByIdAsync(Guid id);
        Task<AreaResponseDto> CreateAsync(CreateAreaDto dto);
        Task<AreaResponseDto?> UpdateAsync(Guid id, UpdateAreaDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}