// Services/IActService.cs
using RajFabAPI.DTOs;
using RajFabAPI.DTOs.CompetantpersonDtos;

namespace RajFabAPI.Services.Interface
{
    public interface ICompetantPersonEquipmentRegistartionService
    {
        Task<string> SaveCompetentEquipmentAsync(CreateCompetentEquipmentDto dto, Guid userId, string? type, string? equipmentRegistrationNo);

        Task<List<CompetentEquipmentDetailsDto>> GetAllAsync();

        Task<CompetentEquipmentDetailsDto?> GetLatestApprovedByRegistrationNoAsync(string registrationNo);
        Task<CompetentEquipmentDetailsDto?> GetByApplicationIdAsync(string applicationId);
    }
}
