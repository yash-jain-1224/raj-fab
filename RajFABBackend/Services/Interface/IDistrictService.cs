// Services/IDistrictService.cs

// Services/IDistrictService.cs
using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IDistrictService
    {
        Task<IEnumerable<DistrictResponseDto>> GetAllAsync();
        Task<IEnumerable<DistrictResponseDto>> GetByDivisionAsync(Guid divisionId);
        Task<DistrictResponseDto?> GetByIdAsync(Guid id);
        Task<DistrictResponseDto> CreateAsync(CreateDistrictDto dto);
        Task<DistrictResponseDto?> UpdateAsync(Guid id, UpdateDistrictDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}