using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IFeeCalculationService
    {
        Task<FeeCalculationResultDto> CalculateFactoryRegistrationFee(
            int totalWorkers, 
            decimal totalPower, 
            string powerUnit
        );
    }
}
