using RajFabAPI.DTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services
{
    public interface IFactoryLicenseService
    {
        Task<IEnumerable<FactoryLicense>> GetAllAsync(Guid userId);
        Task<FactoryLicense?> GetByIdAsync(Guid id, Guid userId);
        Task<string?> CreateAsync(CreateFactoryLicenseDto dto, Guid userId, string? type = "New", string FactoryLicenseNumber = "");
        Task<FactoryLicense?> UpdateAsync(Guid id, CreateFactoryLicenseDto dto, Guid userId);
        // Task<bool> DeleteAsync(Guid id);
    }
}
