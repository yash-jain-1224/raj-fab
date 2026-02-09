using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models
{
    public class CommencementCessationApplication
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(50)]
        public string Type { get; set; }

        [Required]
        [StringLength(36)]
        public string ApplicationId { get; set; }

        [Required]
        [StringLength(100)]
        public string FactoryRegistrationNumber { get; set; }

        [Required]
        public DateTime CessationIntimationDate { get; set; }
        [Required]
        public DateTime CessationIntimationEffectiveDate { get; set; }

        [StringLength(50)]
        public string? ApproxDurationOfWork { get; set; }

        [Required]
        public string OccupierSignature { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        [Required]
        [Column(TypeName = "decimal(3,1)")]
        public decimal Version { get; set; } = 1.0m;

        [Required]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}