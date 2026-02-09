// Services/IPostService.cs
using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IPostService
    {
        Task<IEnumerable<PostResponseDto>> GetAllAsync();
        Task<PostResponseDto?> GetByIdAsync(Guid id);
        Task<PostResponseDto> CreateAsync(CreatePostDto dto);
        Task<PostResponseDto?> UpdateAsync(Guid id, UpdatePostDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}


