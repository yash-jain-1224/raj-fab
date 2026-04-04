using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IFactoryClosureService
    {
        Task<ApiResponseDto<List<FactoryClosureDto>>> GetAllClosuresAsync();
        Task<ApiResponseDto<FactoryClosureDto>> GetClosureByIdAsync(string id);
        Task<ApiResponseDto<List<FactoryClosureDto>>> GetClosuresByFactoryRegistrationIdAsync(string factoryRegistrationId);
        Task<ApiResponseDto<FactoryClosureDto>> CreateClosureAsync(CreateFactoryClosureRequest request);
        Task<ApiResponseDto<FactoryClosureDto>> UpdateStatusAsync(string id, UpdateFactoryClosureStatusRequest request, string reviewedBy);
        Task<ApiResponseDto<FactoryClosureDocumentDto>> UploadDocumentAsync(string closureId, IFormFile file, string documentType);
        Task<ApiResponseDto<bool>> DeleteClosureAsync(string id);
    }
}
