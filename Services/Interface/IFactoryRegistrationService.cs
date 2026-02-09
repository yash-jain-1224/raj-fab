using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IFactoryRegistrationService
    {
        Task<ApiResponseDto<List<FactoryRegistrationDto>>> GetAllRegistrationsAsync();
        Task<ApiResponseDto<FactoryRegistrationDto>> GetRegistrationByIdAsync(string id);
        Task<ApiResponseDto<FactoryRegistrationDto>> GetRegistrationByRegistrationNumberAsync(string registrationNumber);
        Task<ApiResponseDto<FactoryRegistrationDto>> CreateRegistrationAsync(CreateFactoryRegistrationRequest request);
        Task<ApiResponseDto<FactoryRegistrationDto>> UpdateRegistrationStatusAsync(string id, UpdateFactoryRegistrationStatusRequest request, string reviewedBy);
        Task<ApiResponseDto<bool>> DeleteRegistrationAsync(string id);
        Task<ApiResponseDto<FactoryRegistrationDocumentDto>> UploadDocumentAsync(string registrationId, IFormFile file, string documentType);
        Task<ApiResponseDto<bool>> DeleteDocumentAsync(string documentId);
        Task<ApiResponseDto<FactoryRegistrationDto>> AmendRegistrationAsync(string id, CreateFactoryRegistrationRequest request);
        string GenerateRegistrationNumber();
    }
}