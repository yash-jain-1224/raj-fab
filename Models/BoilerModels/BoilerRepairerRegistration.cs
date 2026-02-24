using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace RajFabAPI.Models.BoilerModels
{
    

    public class BoilerRepairerRegistration
    {
        public Guid Id { get; set; }

        public string? FactoryRegistrationNo { get; set; }
        public string? ApplicationId { get; set; }
        public string RepairerRegistrationNo { get; set; } = null!;
        public string? BrClassification { get; set; }

        public string? EstablishmentJson { get; set; }
        // 🔥 RENEWAL TRACKING
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUpto { get; set; }



        public string? JobsExecutedJson { get; set; }
        public string? DocumentEvidence { get; set; }

        public int? ApprovalHistoryJson { get; set; }
        public int? RejectedHistoryJson { get; set; }

        public bool? ToolsAvailable { get; set; }

        public int? SimultaneousSites { get; set; }

        public bool? AcceptsRegulations { get; set; }
        public bool? AcceptsResponsibility { get; set; }
        public bool? CanSupplyMaterial { get; set; }

        public string? QualityControlType { get; set; }
        public string? QualityControlDetailsjson { get; set; }

        public string Status { get; set; } = "Pending";
        public string? Type { get; set; } = "new";

        public decimal Version { get; set; } = 1.0m;
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public ICollection<BoilerRepairerEngineer> BoilerRepairerEngineers { get; set; } = new List<BoilerRepairerEngineer>();
        public ICollection<BoilerRepairerWelder> BoilerRepairerWelders { get; set; } = new List<BoilerRepairerWelder>();
       

    }

}
