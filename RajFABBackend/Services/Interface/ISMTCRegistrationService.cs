using RajFabAPI.DTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services.Interface
{

    public interface ISMTCRegistrationService
    {

        Task<string> SaveSMTCAsync(CreateSMTCRegistrationDto dto, Guid userId, string? type, string? smtcRegistrationNo);

        Task<List<SMTCRegistrationDetailsDto>> GetAllAsync();
        Task<SMTCRegistrationDetailsDto?> GetLatestApprovedByRegistrationNoAsync(string registrationNo);
        Task<SMTCRegistrationDetailsDto?> GetByApplicationIdAsync(string applicationId);
        Task<string> GenerateSmtcPdfAsync(string applicationId);
        Task<string> GenerateObjectionLetter(BoilerObjectionLetterDto dto, string applicationId);
        Task<string> GenerateCertificatePdfAsync(string applicationId, string postName, string userName);



    }


}