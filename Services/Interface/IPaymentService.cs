using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IPaymentService
    {
        string BuildPaymentRedirectForm(decimal amount, int serviceId, string factoryName, int sServiceType, string regNo, string? userEmail = null, string? userMobile = null, string? userName = null);
        string ActionRequestPaymentRPP(string MERCHANTID, double AMOUNT, string RU, string APPID, string ApplicantName, string ApplicantMobile, string ApplicantEmail, string SSOID, string TOKEN, string IPADDRESS, string EnDnKEY, string CHECKSUMKEY, long ID, int ServiceId, int AppStatus);
    }
}