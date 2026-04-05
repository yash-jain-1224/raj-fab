using RajFabAPI.DTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services.Interface
{
    public interface IBoilerDrawingService
    {
        Task<string> SaveBoilerDrawingAsync(BoilerDrawingCreateDto dto, Guid userId, string? type, string? boilerDrawingRegistrationNo);

        Task<string> RenewBoilerDrawingAsync(BoilerDrawingRenewalDto dto, Guid userId);
        Task<bool> UpdateBoilerDrawingAsync(string applicationId, BoilerDrawingCreateDto dto);
        Task<BoilerDrawingDetailsDto?> GetByApplicationIdAsync(string applicationId);
        Task<BoilerDrawingDetailsDto?> GetLatestApprovedByRegistrationNoAsync(string registrationNo);
        Task<string> CloseBoilerDrawingAsync(BoilerDrawingClosureDto dto, Guid userId);
        Task<List<BoilerDrawingDetailsDto>> GetAllAsync();
        Task<string> GenerateDrawingPdfAsync(string applicationId);

    }



}