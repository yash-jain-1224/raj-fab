using RajFabAPI.DTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services.Interface
{
    public interface IBoilerManufactureService
    {
        Task<string> SaveManufactureAsync(  BoilerManufactureCreateDto dto,   Guid userId,  string? type,  string? manufactureApplicationId);

        Task<string> RenewManufactureAsync(    BoilerManufactureRenewalDto dto,   Guid userId);
        Task<string> CloseManufactureAsync(BoilerManufactureClosureDto dto, Guid userId);
        Task<BoilerManufactureDetailsDto?> GetByApplicationIdAsync(string applicationId);
        Task<BoilerManufactureDetailsDto?> GetLatestApprovedByRegistrationNoAsync(string manufactureRegistrationNo);
        Task<List<BoilerManufactureDetailsDto>> GetAllAsync();
        Task<string> GenerateManufacturePdfAsync(string applicationId);
        Task<string> GenerateObjectionLetter(BoilerObjectionLetterDto dto, string applicationId);
        Task<string> GenerateCertificatePdfAsync(string applicationId, string postName, string userName);
    }



}