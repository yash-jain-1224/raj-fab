using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class EstablishmentRegistration
    {
        // Business registration identifier (human/business friendly)
        public string EstablishmentRegistrationId { get; set; }

        // Optional link to the primary detail row created for this registration
        public Guid? EstablishmentDetailId { get; set; }

        // New: links to PersonDetail rows stored for registration-level persons
        public Guid? MainOwnerDetailId { get; set; }
        public Guid? ManagerOrAgentDetailId { get; set; }
        public Guid? ContractorDetailId { get; set; }

        // Workflow status e.g. Pending, Approved, Rejected
        public string? Status { get; set; }
        public string Type { get; set; }
        public string? RegistrationNumber { get; set; }
        
        // Version column
        [Required]
        [Column(TypeName = "decimal(3,1)")]
        public decimal Version { get; set; } = 1.0m;

        public string? Place { get; set; }
        public string? Signature { get; set; }
        public DateTime? Date { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }

    public class EstablishmentRegistrationDocument
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string EstablishmentRegistrationId { get; set; } = string.Empty;

        [Required]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string FilePath { get; set; } = string.Empty;

        public long FileSize { get; set; }
        public string FileExtension { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("EstablishmentRegistrationId")]
        public virtual EstablishmentRegistration EstablishmentRegistration { get; set; } = null!;

    }
}