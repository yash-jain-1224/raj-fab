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
        Task<List<BoilerManufactureDetailsDto>> GetAllByRegistrationNoAsync(string manufactureRegistrationNo);


    }



}