
using RajFabAPI.DTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services.Interface
{
    public interface ICertificateService
    {
        Task<Certificate> GenerateCertificateAsync(GenerateCertificateDto dto, Guid issuerUserId);
        Task<List<Certificate>> GetCertificatesByRegistrationNumberAsync(string registrationNumber);
        Task<Certificate?> GetLatestCertificateAsync(string registrationNumber);
    }
}