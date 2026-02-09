using System;
using RajFabAPI.Models;
using RajFabAPI.Models.FactoryModels;

namespace RajFabAPI.DTOs
{
    public class EstablishmentRegistrationEntitiesDto
    {
        public EstablishmentDetailsDto? EstablishmentDetail { get; set; }
        public int? ApplicationApprovalRequestId { get; set; }
        public string? Status { get; set; }
        public PersonDetailDto? MainOwnerDetail { get; set; }
        public PersonDetailDto? ManagerOrAgentDetail { get; set; }
        public ContractorDetailDto? ContractorDetail { get; set; }
        public FactoryDto? Factory { get; set; }
        public BeediCigarWorksDto? BeediCigarWork { get; set; }
        public MotorTransportServiceDto? MotorTransportService { get; set; }
        public BuildingAndConstructionWorkDto? BuildingAndConstructionWork { get; set; }
        public NewsPaperEstablishmentDto? NewsPaperEstablishment { get; set; }
        public AudioVisualWorkDto? AudioVisualWork { get; set; }
        public PlantationDto? Plantation { get; set; }
        public List<string> EstablishmentTypes { get; set; } = new List<string>();
        public EstablishmentRegistrationDto RegistrationDetail { get; set; }
    }
}
