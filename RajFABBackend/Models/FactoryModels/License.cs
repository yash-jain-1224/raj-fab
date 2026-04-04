using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    [Table("FactoryLicenses")]
    public class FactoryLicense
    {
        [Key]
        [MaxLength(36)]
        public string Id { get; set; } = Guid.NewGuid().ToString().ToUpper();

        [Required]
        [MaxLength(100)]
        public string FactoryLicenseNumber { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? FactoryRegistrationNumber { get; set; }

        public int NoOfYears { get; set; } = 1;

        [Required]
        public DateTime ValidFrom { get; set; }

        [Required]
        public DateTime ValidTo { get; set; }

        [Required]
        [MaxLength(255)]
        public string Place { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        public string? ManagerSignature { get; set; }
        public string? OccupierSignature { get; set; }
        public string? AuthorisedSignature { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        [Column(TypeName = "decimal(3,1)")]
        public decimal Version { get; set; } = 1.0m;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        [Required]
        [MaxLength(20)]
        public string Type { get; set; } = "New";
        
        public bool IsESignCompletedOccupier { get; set; } = false;
        public bool IsESignCompletedManager { get; set; } = false;
        [StringLength(100)]
        public string? ESignPrnNumberManager { get; set; }
        [StringLength(100)]
        public string? ESignPrnNumberOccupier { get; set; }
        public bool IsPaymentCompleted { get; set; } = false;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public string? ApplicationPDFUrl { get; set; }
        public string? ObjectionLetterUrl { get; set; } = "";

        [StringLength(100)]
        public string? ESignPrnNumber { get; set; }
        public bool IsESignCompleted { get; set; } = false;

        // Workers employed
        public int? WorkersProposedMale { get; set; }
        public int? WorkersProposedFemale { get; set; }
        public int? WorkersProposedTransgender { get; set; }
        public int? WorkersLastYearMale { get; set; }
        public int? WorkersLastYearFemale { get; set; }
        public int? WorkersLastYearTransgender { get; set; }
        public int? WorkersOrdinaryMale { get; set; }
        public int? WorkersOrdinaryFemale { get; set; }
        public int? WorkersOrdinaryTransgender { get; set; }

        // Power details
        [Column(TypeName = "decimal(18,2)")]
        public decimal? SanctionedLoad { get; set; }
        public string? SanctionedLoadUnit { get; set; }

        // Manufacturing process
        public string? ManufacturingProcessLast12Months { get; set; }
        public string? ManufacturingProcessNext12Months { get; set; }
        public string? DateOfStartProduction { get; set; }
    }
}
