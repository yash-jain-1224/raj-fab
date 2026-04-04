using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IFactoryMapApprovalService
    {
        Task<ApiResponseDto<List<FactoryMapApprovalDto>>> GetAllApplicationsAsync(Guid userId);
        Task<ApiResponseDto<FactoryMapApprovalDto>> GetApplicationByIdAsync(string id);
        Task<ApiResponseDto<FactoryMapApprovalDto>> GetApplicationByAcknowledgementNumberAsync(string acknowledgementNumber);
        Task<string> CreateApplicationAsync(CreateFactoryMapApprovalRequest request, Guid userId, bool? isNew = true, string? factoryMapApprovalId = "");
        Task<ApiResponseDto<FactoryMapApprovalDto>> UpdateApplicationStatusAsync(string id, UpdateFactoryMapApprovalStatusRequest request, string reviewedBy);
        Task<ApiResponseDto<bool>> DeleteApplicationAsync(string id);
        Task<ApiResponseDto<FactoryMapDocumentDto>> UploadDocumentAsync(string applicationId, IFormFile file, string documentType);
        Task<ApiResponseDto<bool>> DeleteDocumentAsync(string documentId);
        Task<ApiResponseDto<FactoryMapApprovalDto>> AmendApplicationAsync(string id, CreateFactoryMapApprovalRequest request);
        string GenerateAcknowledgementNumber();
        Task<bool> UpdateStatusAndRemark(string registrationId, string status);
        Task<string> GenerateFactoryMapApprovalPdf(FactoryMapApprovalDto dto);
        Task<string> GenerateCertificateAsync(MapApprovalCertificateRequestDto dto, Guid userId, string applicationId);
        Task<ApiResponseDto<FactoryMapApprovalDto>> UpdateApplicationAsync(string applicationId, CreateFactoryMapApprovalRequest request);
        Task<string> GenerateObjectionLetter(MapApprovalObjectionLetterDto dto, string applicationId);
    }
}