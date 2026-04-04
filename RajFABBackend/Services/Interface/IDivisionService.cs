// Services/IDivisionService.cs

// Services/IDivisionService.cs
using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IDivisionService
    {
        Task<IEnumerable<DivisionResponseDto>> GetAllAsync();
        Task<DivisionResponseDto?> GetByIdAsync(Guid id);
        Task<DivisionResponseDto> CreateAsync(CreateDivisionDto dto);
        Task<DivisionResponseDto?> UpdateAsync(Guid id, UpdateDivisionDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}


