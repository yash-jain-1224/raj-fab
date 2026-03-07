using RajFabAPI.DTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services.Interface
{
    public interface IWelderApplicationService
    {
        Task<string> SaveWelderAsync(CreateWelderRegistrationDto dto, Guid userId, string? type, string? welderRegistrationNo);

        Task<string> RenewWelderAsync(WelderRenewalDto dto, Guid userId);
        Task<GetWelderResponseDto?> GetByApplicationIdAsync(string applicationId);
        Task<GetWelderResponseDto?> GetLatestApprovedByRegistrationNoAsync(string registrationNo);
        Task<List<GetWelderResponseDto>> GetAllAsync();
    }



}