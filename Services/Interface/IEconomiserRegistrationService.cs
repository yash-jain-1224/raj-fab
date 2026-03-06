using RajFabAPI.DTOs;
using RajFabAPI.DTOs.RajFabAPI.DTOs.EconomiserDTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services.Interface
{
    public interface IEconomiserService
    {
        Task<string> SaveEconomiserAsync(EconomiserCreateDto dto, Guid userId, string? type, string? economiserRegistrationNo);
        Task<string> RenewEconomiserAsync(EconomiserRenewalDto dto, Guid userId);
        Task<List<EconomiserDetailsDto>> GetAllAsync();
        Task<EconomiserDetailsDto?> GetLatestApprovedByRegistrationNoAsync(string economiserRegistrationNo);
        Task<EconomiserDetailsDto?> GetByApplicationIdAsync(string applicationId);
        Task<string> CloseEconomiserAsync(EconomiserClosureDto dto, Guid userId);


    }



}