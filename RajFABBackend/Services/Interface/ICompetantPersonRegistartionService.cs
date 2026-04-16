// Services/IActService.cs
using RajFabAPI.DTOs;
using RajFabAPI.DTOs.CompetantpersonDtos;

namespace RajFabAPI.Services.Interface
{
    public interface ICompetantPersonRegistartionService
    {
        Task<string> SaveCompetentPersonAsync(CreateCompetentRegistrationDto dto, Guid userId, string? type, string? competentRegistrationNo);
        Task<CompetentRegistrationDetailsDto?> GetByApplicationIdAsync(string applicationId);

        Task<CompetentRegistrationDetailsDto?> GetLatestApprovedByRegistrationNoAsync(string registrationNo);

        Task<List<CompetentRegistrationDetailsDto>> GetAllAsync();

        Task<string> GenerateCompetentPersonPdfAsync(string applicationId);
        Task<string> GenerateObjectionLetter(BoilerObjectionLetterDto dto, string applicationId);
        Task<string> GenerateCertificatePdfAsync(string applicationId, string postName, string userName);
    }
}
