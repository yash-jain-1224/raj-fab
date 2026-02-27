using RajFabAPI.DTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services.Interface
{
    public interface IBoilerRegistartionService
    {
        Task<string> SaveBoilerAsync(      CreateBoilerRegistrationDto dto,      Guid userId,      string? type,      string? boilerRegistrationNo);
        Task<string> RenewBoilerAsync(RenewalBoilerDto dto, Guid userId);

        Task<GetBoilerResponseDto?> GetByApplicationIdAsync(string applicationId);
        Task<List<GetBoilerResponseDto>> GetAllFullAsync();
        Task<GetBoilerResponseDto?> GetLatestApprovedByRegistrationNoAsync(string registrationNo);
        Task<bool> UpdateBoilerAsync(string applicationId, CreateBoilerRegistrationDto dto);

        Task<string> CreateClosureAsync(CreateBoilerClosureDto dto, Guid userId);
        Task<bool> UpdateClosureAsync(string applicationId, UpdateBoilerClosureDto dto, Guid userId);

        Task<BoilerClosureResponseDto?> GetClosureByApplicationIdAsync(string applicationId);
        Task<List<BoilerClosureResponseDto>> GetAllClosuresAsync();

        Task<string> CreateRepairAsync(CreateBoilerRepairDto dto, Guid userId);
        //Task<BoilerRepairResponseDto?> GetRepairByApplicationIdAsync(string applicationId);

        Task<List<GetBoilerRepairDto>> GetAllRepairsAsync();
        Task<GetBoilerRepairDto?> GetRepairByApplicationIdAsync(string applicationId);
        Task<bool> UpdateRepairAsync(string applicationId, UpdateBoilerRepairDto dto, Guid userId);
    }
}