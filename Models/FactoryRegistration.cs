using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class FactoryRegistration
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string RegistrationNumber { get; set; } = string.Empty;
        
        // Plan Approval Reference
        public string? MapApprovalAcknowledgementNumber { get; set; }
        
        // Period of License
        public DateTime LicenseFromDate { get; set; }
        public DateTime LicenseToDate { get; set; }
        public int LicenseYears { get; set; }
        
        // General Information
        [Required]
        public string FactoryName { get; set; } = string.Empty;
        public string? FactoryRegistrationNumber { get; set; }
        
        // Factory Address and Contact Information
        [Required]
        public string PlotNumber { get; set; } = string.Empty;
        [Required]
        public string StreetLocality { get; set; } = string.Empty;
        [Required]
        public string District { get; set; } = string.Empty;
        public string? CityTown { get; set; }
        [Required]
        public string Area { get; set; } = string.Empty;
        [Required]
        public string Pincode { get; set; } = string.Empty;
        [Required]
        public string Mobile { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        
        // Nature of manufacturing process
        public string ManufacturingProcess { get; set; } = string.Empty; // Others, Electricity generating Station, etc.
        public DateTime ProductionStartDate { get; set; }
        public string ManufacturingProcessLast12Months { get; set; } = string.Empty;
        public string ManufacturingProcessNext12Months { get; set; } = string.Empty;
        
        // Workers Employed
        public int MaxWorkersMaleProposed { get; set; }
        public int MaxWorkersFemaleProposed { get; set; }
        public int MaxWorkersTransgenderProposed { get; set; }
        public int MaxWorkersMaleEmployed { get; set; }
        public int MaxWorkersFemaleEmployed { get; set; }
        public int MaxWorkersTransgenderEmployed { get; set; }
        public int WorkersMaleOrdinary { get; set; }
        public int WorkersFemaleOrdinary { get; set; }
        public int WorkersTransgenderOrdinary { get; set; }
        
        // Power Installed
        public decimal TotalRatedHorsePower { get; set; }
        public string PowerUnit { get; set; } = "HP"; // HP, KW, etc.
        public string? KNumber { get; set; }
        
        // Particulars of Factory Manager
        public string FactoryManagerName { get; set; } = string.Empty;
        public string FactoryManagerFatherName { get; set; } = string.Empty;
        public string FactoryManagerPlotNumber { get; set; } = string.Empty;
        public string FactoryManagerStreetLocality { get; set; } = string.Empty;
        public string FactoryManagerDistrict { get; set; } = string.Empty;
        public string FactoryManagerArea { get; set; } = string.Empty;
        public string? FactoryManagerCityTown { get; set; }
        public string FactoryManagerPincode { get; set; } = string.Empty;
        public string FactoryManagerMobile { get; set; } = string.Empty;
        public string FactoryManagerEmail { get; set; } = string.Empty;
        
        [StringLength(10)]
        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Invalid PAN card format")]
        public string? FactoryManagerPanCard { get; set; }
        
        // Particulars of Occupier
        public string OccupierType { get; set; } = string.Empty; // Director, Partner, etc.
        public string OccupierName { get; set; } = string.Empty;
        public string OccupierFatherName { get; set; } = string.Empty;
        public string OccupierPlotNumber { get; set; } = string.Empty;
        public string OccupierStreetLocality { get; set; } = string.Empty;
        public string? OccupierCityTown { get; set; }
        public string OccupierDistrict { get; set; } = string.Empty;
        public string OccupierArea { get; set; } = string.Empty;
        public string OccupierPincode { get; set; } = string.Empty;
        public string OccupierMobile { get; set; } = string.Empty;
        public string OccupierEmail { get; set; } = string.Empty;
        
        [StringLength(10)]
        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Invalid PAN card format")]
        public string? OccupierPanCard { get; set; }
        
        // Land and Building - Owner Details
        public string LandOwnerName { get; set; } = string.Empty;
        public string LandOwnerPlotNumber { get; set; } = string.Empty;
        public string LandOwnerStreetLocality { get; set; } = string.Empty;
        public string LandOwnerDistrict { get; set; } = string.Empty;
        public string LandOwnerArea { get; set; } = string.Empty;
        public string? LandOwnerCityTown { get; set; }
        public string LandOwnerPincode { get; set; } = string.Empty;
        public string LandOwnerMobile { get; set; } = string.Empty;
        public string LandOwnerEmail { get; set; } = string.Empty;
        
        // Building Plan Approval
        public string? BuildingPlanReferenceNumber { get; set; }
        public DateTime? BuildingPlanApprovalDate { get; set; }
        
        // Disposal of wastes and effluents
        public string? WasteDisposalReferenceNumber { get; set; }
        public DateTime? WasteDisposalApprovalDate { get; set; }
        public string? WasteDisposalAuthority { get; set; }
        
        // Payment
        public bool WantToMakePaymentNow { get; set; }
        
        // Declaration
        public bool DeclarationAccepted { get; set; }
        
        // Status and tracking
        public string Status { get; set; } = "Pending"; // Pending, Under Review, Approved, Rejected
        
        public string? CurrentStage { get; set; }
        
        public string? AssignedTo { get; set; } // User ID
        
        public string? AssignedToName { get; set; }
        
        public string? Comments { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        
        public int AmendmentCount { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        // Navigation Properties
        public virtual List<FactoryRegistrationDocument> Documents { get; set; } = new List<FactoryRegistrationDocument>();
    }
    
    public class FactoryRegistrationDocument
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string FactoryRegistrationId { get; set; } = string.Empty;
        
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
        [ForeignKey("FactoryRegistrationId")]
        public virtual FactoryRegistration FactoryRegistration { get; set; } = null!;
    }
}