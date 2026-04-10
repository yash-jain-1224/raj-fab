using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models.FactoryModels
{
    public class NonHazardousFactoryRegistration
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string FactoryRegistrationNumber { get; set; }

        // NEW FIELDS
        [MaxLength(100)]
        public string ApplicationNumber { get; set; }

        [MaxLength(500)]
        public string ApplicationPDFUrl { get; set; } = string.Empty;

        [MaxLength(500)]
        public string ObjectionLetterUrl { get; set; } = string.Empty;

        public decimal Version { get; set; } = 1.0m;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}