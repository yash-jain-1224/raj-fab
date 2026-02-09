using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models
{
    public class LicenseRenewal
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string RenewalNumber { get; set; } = string.Empty;
        public string OriginalRegistrationId { get; set; } = string.Empty;
        public string OriginalRegistrationNumber { get; set; } = string.Empty;
        
        // Renewal Period
        public int RenewalYears { get; set; }
        public DateTime LicenseRenewalFrom { get; set; }
        public DateTime LicenseRenewalTo { get; set; }
        
        // Factory Information
        public string FactoryName { get; set; } = string.Empty;
        public string FactoryRegistrationNumber { get; set; } = string.Empty;
        public string PlotNumber { get; set; } = string.Empty;
        public string StreetLocality { get; set; } = string.Empty;
        public string CityTown { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string? Email { get; set; }
        
        // Manufacturing Details
        public string ManufacturingProcess { get; set; } = string.Empty;
        public DateTime ProductionStartDate { get; set; }
        public string ManufacturingProcessLast12Months { get; set; } = string.Empty;
        public string ManufacturingProcessNext12Months { get; set; } = string.Empty;
        public string ProductDetailsLast12Months { get; set; } = string.Empty;
        
        // Workers
        public int MaxWorkersMaleProposed { get; set; }
        public int MaxWorkersFemaleProposed { get; set; }
        public int MaxWorkersTransgenderProposed { get; set; }
        public int MaxWorkersMaleEmployed { get; set; }
        public int MaxWorkersFemaleEmployed { get; set; }
        public int MaxWorkersTransgenderEmployed { get; set; }
        public int WorkersMaleOrdinary { get; set; }
        public int WorkersFemaleOrdinary { get; set; }
        public int WorkersTransgenderOrdinary { get; set; }
        
        // Power
        public decimal TotalRatedHorsePower { get; set; }
        public decimal MaximumPowerToBeUsed { get; set; }
        
        // Manager
        public string FactoryManagerName { get; set; } = string.Empty;
        public string FactoryManagerFatherName { get; set; } = string.Empty;
        public string FactoryManagerAddress { get; set; } = string.Empty;
        
        // Occupier
        public string OccupierType { get; set; } = string.Empty;
        public string OccupierName { get; set; } = string.Empty;
        public string OccupierFatherName { get; set; } = string.Empty;
        public string OccupierAddress { get; set; } = string.Empty;
        
        // Land Owner
        public string LandOwnerName { get; set; } = string.Empty;
        public string LandOwnerAddress { get; set; } = string.Empty;
        
        // Building Plan
        public string BuildingPlanReferenceNumber { get; set; } = string.Empty;
        public DateTime? BuildingPlanApprovalDate { get; set; }
        
        // Waste Disposal
        public string? WasteDisposalReferenceNumber { get; set; }
        public DateTime? WasteDisposalApprovalDate { get; set; }
        public string? WasteDisposalAuthority { get; set; }
        
        // Declaration
        public string Place { get; set; } = string.Empty;
        public DateTime DeclarationDate { get; set; }
        public bool Declaration1Accepted { get; set; }
        public bool Declaration2Accepted { get; set; }
        public bool Declaration3Accepted { get; set; }
        
        // Payment
        public decimal PaymentAmount { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
        public string? PaymentTransactionId { get; set; }
        public DateTime? PaymentDate { get; set; }
        
        // Status
        public string Status { get; set; } = "Draft";
        public string? Comments { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public int AmendmentCount { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        // Navigation
        public virtual ICollection<LicenseRenewalDocument> Documents { get; set; } = new List<LicenseRenewalDocument>();
    }
    
    public class LicenseRenewalDocument
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string RenewalId { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FileExtension { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.Now;
        
        // Navigation
        public virtual LicenseRenewal? Renewal { get; set; }
    }
}
