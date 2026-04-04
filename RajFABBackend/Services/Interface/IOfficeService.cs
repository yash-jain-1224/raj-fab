using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IOfficeService
    {
        Task<IEnumerable<OfficeResponseDto>> GetAllAsync();
        Task<OfficeResponseDto?> GetByIdAsync(System.Guid id);
        Task<OfficeResponseDto> CreateAsync(CreateOfficeDto dto);
        Task<OfficeResponseDto?> UpdateAsync(System.Guid id, UpdateOfficeDto dto);
        Task<bool> DeleteAsync(System.Guid id);
        Task<bool> UpdateLevelCountAsync(Guid officeId, int levelCount);

    }
}