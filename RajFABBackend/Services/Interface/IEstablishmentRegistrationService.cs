using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IEstablishmentRegistrationService
    {
        Task<string> SaveEstablishmentAsync(CreateEstablishmentRegistrationDto dto, Guid userId, string? type = "new", string? establishmentRegistrationId = "");
        Task<string> UpdateEstablishmentAsync(string registrationId, CreateEstablishmentRegistrationDto dto, Guid userIdGuid);
        Task<EstablishmentRegistrationDetailsDto?> GetRegistrationDetailsAsync(string id);
        Task<EstablishmentApplicationDto?> GetAllEntitiesByRegistrationIdAsync(string registrationId);
        Task<List<EstablishmentDetailsDto>> GetAllEstablishmentDetailsAsync(Guid userId);
        Task<ApiResponseDto<EstablishmentRegistrationDocumentDto>> UploadDocumentAsync(string registrationId, IFormFile file, string documentType);
        Task<ApiResponseDto<bool>> DeleteDocumentAsync(string documentId);
        Task<bool> UpdateStatusAndRemark(string registrationId, string status);
        Task<EstablishmentRegistrationDetailsDto?> GetFactoryDetailsByFactoryRegistrationNumberAsync(string factoryRegistrationNumber);
        Task<string> RenewEstablishmentAsync( RenewEstablishmentDto dto, Guid userId,  string registrationId);
        Task<string> GenerateCertificateAsync(EstablishmentCertificateRequestDto  dto, Guid userId, string registrationId);
        Task<string?> GetFactoryRegistrationNumber(Guid userId);
        Task<string> GenerateEstablishmentPdf(EstablishmentApplicationDto dto);
        Task<string?> getFilePathByPrn(string prnNumber);
        Task<string> GenerateObjectionLetter(EstablishmentObjectionLetterDto dto, string registrationId);
    }
}