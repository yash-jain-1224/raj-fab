using RajFabAPI.DTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services
{
    public interface IFactoryLicenseService
    {
        Task<IEnumerable<FactoryLicense>> GetAllAsync(Guid userId);
        Task<FactoryLicenseData?> GetByIdAsync(string id);
        Task<string?> CreateAsync(CreateFactoryLicenseDto dto, Guid userId, string? type = "New", string FactoryLicenseNumber = "");
        Task<FactoryLicense?> UpdateAsync(Guid id, CreateFactoryLicenseDto dto, Guid userId);
        // Task<bool> DeleteAsync(Guid id);
        Task<string> GenerateFactoryLicensePdf(FactoryLicenseData dto);
    }
}
