// Services/IActService.cs
using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IActService
    {
        Task<IEnumerable<ActResponseDto>> GetAllAsync();
        Task<ActResponseDto?> GetByIdAsync(Guid id);
        Task<ActResponseDto> CreateAsync(CreateActDto dto);
        Task<ActResponseDto?> UpdateAsync(Guid id, UpdateActDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
