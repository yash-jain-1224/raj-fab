using RajFabAPI.DTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services
{
    public interface IFactoryLicenseService
    {
        Task<IEnumerable<FactoryLicense>> GetAllAsync(Guid userId);
        Task<FactoryLicenseData?> GetByIdAsync(string id);
        Task<string?> CreateAsync(CreateFactoryLicenseDto dto, Guid userId, string? type = "new", string FactoryLicenseNumber = "");
        Task<FactoryLicense?> UpdateAsync(string id, CreateFactoryLicenseDto dto, Guid userId);
        // Task<bool> DeleteAsync(Guid id);
        Task<string> GenerateFactoryLicensePdf(FactoryLicenseData dto, bool isCertificate = false);
        Task<string> GenerateCertificateAsync(FactoryLicenseCertificateRequestDto dto, Guid userId, string licenseId);
        Task<bool> UpdateStatusAndRemark(string applicationId, string status);
        Task<string> GenerateObjectionLetter(LicenseObjectionLetterDto dto, string licenseId);
    }
}
