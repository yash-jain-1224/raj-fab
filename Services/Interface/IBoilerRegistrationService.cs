using RajFabAPI.DTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services.Interface
{
    public interface IBoilerRegistartionService
    {
        Task<string> SaveBoilerAsync(      CreateBoilerRegistrationDto dto,      Guid userId,      string? type,      string? boilerRegistrationNo);
        Task<string> RenewBoilerAsync(RenewalBoilerDto dto, Guid userId);

        Task<GetBoilerResponseDto?> GetByApplicationIdAsync(string applicationId);
        Task<bool> UpdateBoilerAsync(string applicationId, CreateBoilerRegistrationDto dto);

        Task<string> CreateClosureAsync(CreateBoilerClosureDto dto, Guid userId);

        Task<BoilerClosureResponseDto?> GetClosureByApplicationIdAsync(string applicationId);
        Task<List<BoilerClosureResponseDto>> GetAllClosuresAsync();

        Task<string> CreateRepairAsync(CreateBoilerRepairDto dto, Guid userId);
        //Task<BoilerRepairResponseDto?> GetRepairByApplicationIdAsync(string applicationId);

    }


}