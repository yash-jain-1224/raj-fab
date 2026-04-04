using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IPaymentService
    {
        string BuildPaymentRedirectForm(decimal amount, int serviceId, string factoryName, int sServiceType, string regNo, string? userEmail = null, string? userMobile = null, string? userName = null);
        Task<string> ActionRequestPaymentRPP(decimal AMOUNT, string ApplicantName, string ApplicantMobile, string ApplicantEmail, string SSOID, string EnDnKEY, string CHECKSUMKEY, string ApplicationId, string ModuleId, string UserId);
        bool VerifyChecksum(EmitraPaymentResponse response, string checksumKey);
        string AESDecrypt(string textToDecrypt, string AESENCKEY);
        Task<string?> PaymentByApplicationIdAsync(string applicationId);
    }
}