using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace RajFabAPI.Models.BoilerModels
{


    public class BoilerRepairerClosure
    {
        public Guid Id { get; set; }

        public string RepairerRegistrationNo { get; set; } = null!;
        public string? ApplicationId { get; set; }

        public string ClosureReason { get; set; } = null!;
        public DateTime ClosureDate { get; set; }

        public string? Remarks { get; set; }
        public string? DocumentPath { get; set; }

        public string? Type { get; set; }
        public string Status { get; set; } = "Pending";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }


}
