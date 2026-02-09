using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class FactoryClosure
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string ClosureNumber { get; set; } = string.Empty;
        
        // Reference to Factory Registration
        [Required]
        public string FactoryRegistrationId { get; set; } = string.Empty;
        
        // Factory Details (denormalized for quick access)
        public string FactoryName { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string OccupierName { get; set; } = string.Empty;
        public string FactoryAddress { get; set; } = string.Empty;
        
        // Closure Details
        public decimal FeesDue { get; set; }
        public DateTime LastRenewalDate { get; set; }
        public DateTime ClosureDate { get; set; }
        
        [Required]
        public string ReasonForClosure { get; set; } = string.Empty;
        
        // Inspection Details
        public string InspectingOfficerName { get; set; } = string.Empty;
        public string InspectionRemarks { get; set; } = string.Empty;
        public DateTime? InspectionDate { get; set; }
        
        // Status and Workflow
        public string Status { get; set; } = "Pending"; // Pending, Under Review, Approved, Rejected, Closed
        public string? CurrentStage { get; set; }
        public string? AssignedTo { get; set; } // User ID
        public string? AssignedToName { get; set; }
        
        // Review Information
        public string? Comments { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        
        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public DateTime? ClosedAt { get; set; }
        
        // Navigation Properties
        [ForeignKey("FactoryRegistrationId")]
        public virtual FactoryRegistration FactoryRegistration { get; set; } = null!;
        
        public virtual List<FactoryClosureDocument> Documents { get; set; } = new List<FactoryClosureDocument>();
    }
    
    public class FactoryClosureDocument
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string FactoryClosureId { get; set; } = string.Empty;
        
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
        [ForeignKey("FactoryClosureId")]
        public virtual FactoryClosure FactoryClosure { get; set; } = null!;
    }
}
