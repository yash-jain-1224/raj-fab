using RajFabAPI.DTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services.Interface
{
    public interface ISteamPipeLineApplicationService
    {
        Task<string> SaveSteamPipeLineAsync(  CreateSteamPipeLineDto dto, Guid userId, string? type,  string? steamPipeLineRegistrationNo);
        Task<string> RenewSteamPipeLineAsync(  RenewSteamPipeLineDto dto, Guid userId);
        Task<SteamPipeLineFullResponseDto?> GetLatestApprovedByRegistrationNoAsync(string registrationNo);
        Task<List<SteamPipeLineFullResponseDto>> GetAllSteamPipeLinesAsync();
        Task<SteamPipeLineFullResponseDto?> GetSteamPipeLineByApplicationIdAsync(string applicationId);
        Task<string> UpdateSteamPipeLineAsync(string applicationId, CreateSteamPipeLineDto dto);
        Task<string> CloseSteamPipeLineAsync(CreateSteamPipeLineCloseDto dto);
        //Task<string> GenerateSteamPipeLinePdfAsync(string applicationId);
    }



}