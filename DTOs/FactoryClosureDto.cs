using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    public class FactoryClosureDto
    {
        public string Id { get; set; } = string.Empty;
        public string ClosureNumber { get; set; } = string.Empty;
        public string FactoryRegistrationId { get; set; } = string.Empty;
        public string FactoryName { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string OccupierName { get; set; } = string.Empty;
        public string FactoryAddress { get; set; } = string.Empty;
        public decimal FeesDue { get; set; }
        public DateTime LastRenewalDate { get; set; }
        public DateTime ClosureDate { get; set; }
        public string ReasonForClosure { get; set; } = string.Empty;
        public string InspectingOfficerName { get; set; } = string.Empty;
        public string InspectionRemarks { get; set; } = string.Empty;
        public DateTime? InspectionDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CurrentStage { get; set; }
        public string? AssignedTo { get; set; }
        public string? AssignedToName { get; set; }
        public string? Comments { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        
        public List<FactoryClosureDocumentDto> Documents { get; set; } = new List<FactoryClosureDocumentDto>();
    }

    public class FactoryClosureDocumentDto
    {
        public string Id { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FileExtension { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }

    public class CreateFactoryClosureRequest
    {
        [Required]
        public string FactoryRegistrationId { get; set; } = string.Empty;
        
        [Required]
        public decimal FeesDue { get; set; }
        
        [Required]
        public DateTime LastRenewalDate { get; set; }
        
        [Required]
        public DateTime ClosureDate { get; set; }
        
        [Required]
        public string ReasonForClosure { get; set; } = string.Empty;
        
        [Required]
        public string InspectingOfficerName { get; set; } = string.Empty;
        
        [Required]
        public string InspectionRemarks { get; set; } = string.Empty;
        
        public DateTime? InspectionDate { get; set; }
    }

    public class UpdateFactoryClosureStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;
        
        public string? Comments { get; set; }
    }
}
