using RajFabAPI.DTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services.Interface
{
    public interface IBoilerRepairerService
    {
        Task<string> SaveRepairerAsync(BoilerRepairerCreateDto dto, Guid userId, string? type, string? repairerRegistrationNo);
        Task<string> RenewRepairerAsync(BoilerRepairerRenewalDto dto, Guid userId);
        Task<string> CloseRepairerAsync(BoilerRepairerClosureDto dto, Guid userId);
        Task<BoilerRepairerResponseDto?> GetByApplicationIdAsync(string applicationId);
        Task<BoilerRepairerResponseDto?> GetLatestApprovedByRegistrationNoAsync(string registrationNo);
        Task<List<BoilerRepairerResponseDto>> GetAllAsync();
        Task<string> GenerateRepairerPdfAsync(string applicationId);
        Task<string> GenerateObjectionLetter(BoilerObjectionLetterDto dto, string applicationId);
        Task<string> GenerateCertificatePdfAsync(string applicationId, string postName, string userName);
    }



}