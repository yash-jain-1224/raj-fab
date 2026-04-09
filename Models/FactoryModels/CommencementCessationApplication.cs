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
        [StringLength(50)]
        public string ApplicationNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string FactoryRegistrationNumber { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; }

        [Required]
        public DateTime OnDate { get; set; }
        [Required]
        public DateTime FromDate { get; set; }

        public DateTime? DateOfCessation { get; set; }

        [StringLength(50)]
        public string? ApproxDurationOfWork { get; set; }

        public string? ApplicationPDFUrl { get; set; }
        public string? ObjectionLetterUrl { get; set; } = string.Empty;

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