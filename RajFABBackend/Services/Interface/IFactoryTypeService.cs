using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IFactoryTypeService
    {
        Task<ApiResponseDto<List<FactoryTypeOldDto>>> GetAllFactoryTypesAsync();
        Task<ApiResponseDto<FactoryTypeOldDto>> GetFactoryTypeByIdAsync(string id);
        Task<ApiResponseDto<FactoryTypeOldDto>> CreateFactoryTypeAsync(CreateFactoryTypeRequest request);
        Task<ApiResponseDto<FactoryTypeOldDto>> UpdateFactoryTypeAsync(string id, CreateFactoryTypeRequest request);
        Task<ApiResponseDto<bool>> DeleteFactoryTypeAsync(string id);
        
        // Document Type management
        Task<ApiResponseDto<List<DocumentTypeDto>>> GetAllDocumentTypesAsync();
        Task<ApiResponseDto<List<DocumentTypeDto>>> GetDocumentTypesByModuleAsync(string module);
        Task<ApiResponseDto<List<DocumentTypeDto>>> GetDocumentTypesByModuleAndServiceAsync(string module, string serviceType);
        Task<ApiResponseDto<DocumentTypeDto>> CreateDocumentTypeAsync(CreateDocumentTypeRequest request);
        Task<ApiResponseDto<DocumentTypeDto>> UpdateDocumentTypeAsync(string id, CreateDocumentTypeRequest request);
        Task<ApiResponseDto<bool>> DeleteDocumentTypeAsync(string id);

        // Boiler Document Type management
        Task<ApiResponseDto<List<BoilerDocumentTypeDto>>> GetBoilerDocumentTypesAsync(string serviceType);
        Task<ApiResponseDto<BoilerDocumentTypeDto>> CreateBoilerDocumentTypeAsync(CreateBoilerDocumentTypeRequest request);
        Task<ApiResponseDto<bool>> DeleteBoilerDocumentTypeAsync(string id);
    }
}