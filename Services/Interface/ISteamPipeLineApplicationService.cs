using RajFabAPI.DTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services.Interface
{
    public interface ISteamPipeLineApplicationService
    {
        Task<string> SaveSteamPipeLineAsync(CreateSteamPipeLineDto dto, string? type, string? steamPipeLineRegistrationNo);
        Task<string> RenewSteamPipeLineAsync(RenewSteamPipeLineDto dto);
        Task<List<SteamPipeLineFullResponseDto>> GetSteamPipeLineByRegistrationNoAsync(string registrationNo);
        Task<SteamPipeLineFullResponseDto?> GetSteamPipeLineByApplicationIdAsync(string applicationId);
        Task<string> UpdateSteamPipeLineAsync(string applicationId, CreateSteamPipeLineDto dto);
        Task<string> CloseSteamPipeLineAsync(CreateSteamPipeLineCloseDto dto);
    }



}