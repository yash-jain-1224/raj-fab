using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface ILicenseRenewalService
    {
        Task<ApiResponseDto<List<LicenseRenewalDto>>> GetAllRenewalsAsync();
        Task<ApiResponseDto<LicenseRenewalDto>> GetRenewalByIdAsync(string id);
        Task<ApiResponseDto<LicenseRenewalDto>> GetRenewalByNumberAsync(string renewalNumber);
        Task<ApiResponseDto<List<LicenseRenewalDto>>> GetRenewalsByRegistrationIdAsync(string registrationId);
        Task<ApiResponseDto<LicenseRenewalDto>> CreateRenewalAsync(CreateLicenseRenewalRequest request);
        Task<ApiResponseDto<LicenseRenewalDto>> UpdateRenewalStatusAsync(string id, UpdateLicenseRenewalStatusRequest request, string reviewedBy);
        Task<ApiResponseDto<bool>> DeleteRenewalAsync(string id);
        Task<ApiResponseDto<LicenseRenewalDocumentDto>> UploadDocumentAsync(string renewalId, IFormFile file, string documentType);
        Task<ApiResponseDto<bool>> DeleteDocumentAsync(string documentId);
        Task<ApiResponseDto<PaymentResponseDto>> InitiatePaymentAsync(InitiatePaymentRequest request);
        Task<ApiResponseDto<LicenseRenewalDto>> CompletePaymentAsync(CompletePaymentRequest request);
        Task<ApiResponseDto<decimal>> CalculatePaymentAmountAsync(string renewalId);
        Task<ApiResponseDto<LicenseRenewalDto>> AmendRenewalAsync(string id, CreateLicenseRenewalRequest request);
        string GenerateRenewalNumber();
    }
}
