using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class ManagerChange
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string FactoryRegistrationNumber { get; set; }

        [Required]
        [MaxLength(50)]
        public string ApplicationNumber { get; set; }

        [Required]
        public Guid OldManagerId { get; set; }

        [Required]
        public Guid NewManagerId { get; set; }

        [Required]
        public DateTime DateOfAppointment { get; set; }

        [Required]
        public string? Status { get; set; } = "Pending";

        // Version column
        [Required]
        [Column(TypeName = "decimal(3,1)")]
        public decimal Version { get; set; } = 1.0m;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public string? ApplicationPDFUrl { get; set; }
    }
}
