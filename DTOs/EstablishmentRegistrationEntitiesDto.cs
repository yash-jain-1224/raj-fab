using System;
using RajFabAPI.Models;
using RajFabAPI.Models.FactoryModels;

namespace RajFabAPI.DTOs
{
    public class EstablishmentRegistrationEntitiesDto
    {
        public bool AutoRenewal { get; set; }
        public string EstablishmentRegistrationId { get; set; }
        public EstablishmentDetailsDto? EstablishmentDetail { get; set; }
        public int? ApplicationApprovalRequestId { get; set; }
        public string? Status { get; set; }
        public PersonDetailDto? MainOwnerDetail { get; set; }
        public PersonDetailDto? ManagerOrAgentDetail { get; set; }
        public List<ContractorDetailDto> ContractorDetail { get; set; }
        public FactoryDto? Factory { get; set; }
        public BeediCigarWorksDto? BeediCigarWork { get; set; }
        public MotorTransportServiceDto? MotorTransportService { get; set; }
        public BuildingAndConstructionWorkDto? BuildingAndConstructionWork { get; set; }
        public NewsPaperEstablishmentDto? NewsPaperEstablishment { get; set; }
        public AudioVisualWorkDto? AudioVisualWork { get; set; }
        public PlantationDto? Plantation { get; set; }
        public List<string> EstablishmentTypes { get; set; } = new List<string>();
        public EstablishmentRegistrationDto RegistrationDetail { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? SignatureBase64 { get; set; }
        public string? DeclarationPlace { get; set; }
    }
    public class EstablishmentApplicationDto 
    {
        public EstablishmentRegistrationEntitiesDto ApplicationDetails { get; set; }
        public List<ApplicationHistory> ApplicationHistory { get; set; }
        public List<Transaction> TransactionHistory { get; set; }
    }
}
