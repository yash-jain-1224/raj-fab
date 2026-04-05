using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class SMTCRegistration
    {
        public Guid Id { get; set; }

        public string ApplicationId { get; set; } = null!;

        public string? SMTCRegistrationNo { get; set; }

        public string FactoryRegistrationNo { get; set; } = null!;

        public bool TrainingCenterAvailable { get; set; }

        public int? SeatingCapacity { get; set; }

        public string? TrainingCenterPhotoPath { get; set; }

        public bool? AudioVideoFacility { get; set; }

        public string? Comments { get; set; }

        public decimal Amount { get; set; } = 0;
        public bool IsPaymentCompleted { get; set; } = false;
        public bool IsESignCompleted { get; set; } = false;
        [MaxLength(500)]
        public string? ApplicationPDFUrl { get; set; }

        public string Type { get; set; } = "new";

        public string Status { get; set; } = "Pending";

        public decimal Version { get; set; } = 1.0m;

        public bool IsActive { get; set; } = true;

        public DateTime? ValidUpto { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<SMTCTrainerDetail> Trainers { get; set; } = new List<SMTCTrainerDetail>();
    }
}