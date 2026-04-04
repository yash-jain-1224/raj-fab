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
    }
}
