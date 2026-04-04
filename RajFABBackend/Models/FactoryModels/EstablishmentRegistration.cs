using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class EstablishmentRegistration
    {
        [Key]
        [MaxLength(100)]
        public string EstablishmentRegistrationId { get; set; } = Guid.NewGuid().ToString();

        public Guid? EstablishmentDetailId { get; set; }
        public Guid? MainOwnerDetailId { get; set; }
        public Guid? ManagerOrAgentDetailId { get; set; }
        public Guid? FactoryCategoryId { get; set; } = null;

        public string? Status { get; set; }
        [Required]
        public string Type { get; set; } = string.Empty;

        [StringLength(50)]
        public string ApplicationId { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string OccupierIdProof { get; set; } = string.Empty;
        [StringLength(500)]

        public string PartnershipDeed { get; set; } = string.Empty;
        [StringLength(500)]

        public string ManagerIdProof { get; set; } = string.Empty;
        [StringLength(500)]

        public string LoadSanctionCopy { get; set; } = string.Empty;

        public string? RegistrationNumber { get; set; }

        [Required]
        [Column(TypeName = "decimal(3,1)")]
        public decimal Version { get; set; } = 1.0m;

        public string? Place { get; set; }
        public string? Signature { get; set; }
        public DateTime? Date { get; set; }
        public bool AutoRenewal { get; set; } = false;
        public bool IsESignCompleted { get; set; } = false;
        public bool IsPaymentCompleted { get; set; } = false;
        public string? ApplicationPDFUrl { get; set; }
        public string? ObjectionLetterUrl { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ESignPrnNumber { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        // Initialize collections to avoid null references
        public virtual ICollection<EstablishmentRegistrationDocument> Documents { get; set; } = new List<EstablishmentRegistrationDocument>();
    }

    public class EstablishmentRegistrationDocument
    {
        [Key]
        [MaxLength(100)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(100)]
        public string EstablishmentRegistrationId { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? FileExtension { get; set; }

        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("EstablishmentRegistrationId")]
        public virtual EstablishmentRegistration? EstablishmentRegistration { get; set; }
    }

}