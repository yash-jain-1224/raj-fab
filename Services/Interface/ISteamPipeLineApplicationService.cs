using RajFabAPI.DTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services.Interface
{
    public interface ISteamPipeLineApplicationService
    {
        Task<string> SaveSteamPipeLineAsync(CreateSteamPipeLineDto dto, string? type, string? steamPipeLineRegistrationNo);



    }



}