using System.Collections.Generic;
using System.Threading.Tasks;
using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IApplicationRegistrationService
    {
        Task<List<ApplicationRegistrationDto>> GetAllAsync();
        Task<ApplicationRegistrationDto?> GetByIdAsync(Guid id);
        Task<Guid> CreateAsync(CreateApplicationRegistrationDto dto);
        Task<ApplicationRegistrationDto?> UpdateAsync(Guid id, ApplicationRegistrationDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<List<ApplicationUserDashboardDto>> GetByUserIdAsync(Guid userId);
        Task<EstablishmentRegistrationDetailsDto?> GetRegistrationDetailsAsync(string registrationNumber);
    }
}