using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace RajFabAPI.Models.BoilerModels
{


    public class BoilerManufactureClosure
    {
        public Guid Id { get; set; }

        public string ManufactureRegistrationNo { get; set; } = null!;
        public string? ApplicationId { get; set; }

        public string ClosureReason { get; set; } = null!;
        public DateTime ClosureDate { get; set; }

        public string? Remarks { get; set; }
        public string? DocumentPath { get; set; }

        public string? Type { get; set; } = "close";
        public bool IsActive { get; set; } = true;

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }


}
