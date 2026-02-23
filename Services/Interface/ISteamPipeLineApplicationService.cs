using RajFabAPI.DTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services.Interface
{
    public interface ISteamPipeLineApplicationService
    {
        Task<Guid> SaveAsync(
            CreateSteamPipeLineDto dto,
            Guid userId,
            string type,
            Guid? applicationId = null
        );

        Task<SteamPipeLineResponseDto?> GetByIdAsync(Guid id);

        Task<List<SteamPipeLineResponseDto>> GetByUserIdAsync(Guid userId);
    }



}