using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace RajFabAPI.Models.CompetentPerson
{

    public class CompetentEquipmentRegistration
    {
        public Guid Id { get; set; }

        public string ApplicationId { get; set; } = null!;

        // Link to Competent Person module
        public string CompetentRegistrationNo { get; set; } = null!;

        public string? CompetentEquipmentRegistrationNo { get; set; }

        public string Type { get; set; } = "new";

        public string Status { get; set; } = "Pending";

        public decimal Version { get; set; } = 1.0m;

        public bool IsActive { get; set; } = true;

        public int? RenewalYears { get; set; }

        public DateTime? ValidUpto { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<CompetentPersonEquipment>? Equipments { get; set; }
    }

}
