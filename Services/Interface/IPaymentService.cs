using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IPaymentService
    {
        string BuildPaymentRedirectForm(decimal amount, int serviceId, string factoryName, int sServiceType, string regNo, string? userEmail = null, string? userMobile = null, string? userName = null);
    }
}