
namespace RajFabAPI.Models.FactoryModels
{
    public class FactoryContractorMapping
    {
        public string EstablishmentRegistrationId { get; set; }
        public Guid ContractorDetailId { get; set; } // id from PersonDetail table
    }
}
