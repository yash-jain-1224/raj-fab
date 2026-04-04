using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    public class LicenseRenewalDto
    {
        public string Id { get; set; } = string.Empty;
        public string RenewalNumber { get; set; } = string.Empty;
        public string OriginalRegistrationId { get; set; } = string.Empty;
        public string OriginalRegistrationNumber { get; set; } = string.Empty;
        
        // Renewal Period
        public int RenewalYears { get; set; }
        public DateTime LicenseRenewalFrom { get; set; }
        public DateTime LicenseRenewalTo { get; set; }
        
        // Factory Information (pre-filled from original)
        public string FactoryName { get; set; } = string.Empty;
        public string FactoryRegistrationNumber { get; set; } = string.Empty;
        public string PlotNumber { get; set; } = string.Empty;
        public string StreetLocality { get; set; } = string.Empty;
        public string CityTown { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string? DistrictName { get; set; }
        public string Area { get; set; } = string.Empty;
        public string? AreaName { get; set; }
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
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Failed
        public string? PaymentTransactionId { get; set; }
        public DateTime? PaymentDate { get; set; }
        
        // Status
        public string Status { get; set; } = "Draft"; // Draft, Submitted, Under Review, Approved, Rejected
        public string? Comments { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public int AmendmentCount { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public List<LicenseRenewalDocumentDto> Documents { get; set; } = new List<LicenseRenewalDocumentDto>();
    }
    
    public class LicenseRenewalDocumentDto
    {
        public string Id { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FileExtension { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }
    
    public class CreateLicenseRenewalRequest
    {
        [Required]
        public string OriginalRegistrationId { get; set; } = string.Empty;
        
        [Required]
        public int RenewalYears { get; set; }
        
        [Required]
        public DateTime LicenseRenewalFrom { get; set; }
        
        [Required]
        public DateTime LicenseRenewalTo { get; set; }
        
        [Required]
        public string FactoryName { get; set; } = string.Empty;
        
        [Required]
        public string FactoryRegistrationNumber { get; set; } = string.Empty;
        
        [Required]
        public string PlotNumber { get; set; } = string.Empty;
        
        [Required]
        public string StreetLocality { get; set; } = string.Empty;
        
        [Required]
        public string CityTown { get; set; } = string.Empty;
        
        [Required]
        public string District { get; set; } = string.Empty;
        
        [Required]
        public string Area { get; set; } = string.Empty;
        
        [Required]
        public string Pincode { get; set; } = string.Empty;
        
        [Required]
        public string Mobile { get; set; } = string.Empty;
        
        public string? Email { get; set; }
        
        [Required]
        public string ManufacturingProcess { get; set; } = string.Empty;
        
        [Required]
        public DateTime ProductionStartDate { get; set; }
        
        [Required]
        public string ManufacturingProcessLast12Months { get; set; } = string.Empty;
        
        [Required]
        public string ManufacturingProcessNext12Months { get; set; } = string.Empty;
        
        [Required]
        public string ProductDetailsLast12Months { get; set; } = string.Empty;
        
        public int MaxWorkersMaleProposed { get; set; }
        public int MaxWorkersFemaleProposed { get; set; }
        public int MaxWorkersTransgenderProposed { get; set; }
        public int MaxWorkersMaleEmployed { get; set; }
        public int MaxWorkersFemaleEmployed { get; set; }
        public int MaxWorkersTransgenderEmployed { get; set; }
        public int WorkersMaleOrdinary { get; set; }
        public int WorkersFemaleOrdinary { get; set; }
        public int WorkersTransgenderOrdinary { get; set; }
        
        [Required]
        public decimal TotalRatedHorsePower { get; set; }
        
        [Required]
        public decimal MaximumPowerToBeUsed { get; set; }
        
        [Required]
        public string FactoryManagerName { get; set; } = string.Empty;
        
        [Required]
        public string FactoryManagerFatherName { get; set; } = string.Empty;
        
        [Required]
        public string FactoryManagerAddress { get; set; } = string.Empty;
        
        [Required]
        public string OccupierType { get; set; } = string.Empty;
        
        [Required]
        public string OccupierName { get; set; } = string.Empty;
        
        [Required]
        public string OccupierFatherName { get; set; } = string.Empty;
        
        [Required]
        public string OccupierAddress { get; set; } = string.Empty;
        
        [Required]
        public string LandOwnerName { get; set; } = string.Empty;
        
        [Required]
        public string LandOwnerAddress { get; set; } = string.Empty;
        
        [Required]
        public string BuildingPlanReferenceNumber { get; set; } = string.Empty;
        
        public DateTime? BuildingPlanApprovalDate { get; set; }
        
        public string? WasteDisposalReferenceNumber { get; set; }
        public DateTime? WasteDisposalApprovalDate { get; set; }
        public string? WasteDisposalAuthority { get; set; }
        
        [Required]
        public string Place { get; set; } = string.Empty;
        
        [Required]
        public DateTime DeclarationDate { get; set; }
        
        [Required]
        public bool Declaration1Accepted { get; set; }
        
        [Required]
        public bool Declaration2Accepted { get; set; }
        
        [Required]
        public bool Declaration3Accepted { get; set; }
    }
    
    public class UpdateLicenseRenewalStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;
        public string? Comments { get; set; }
    }
    
    public class InitiatePaymentRequest
    {
        [Required]
        public string RenewalId { get; set; } = string.Empty;
        
        [Required]
        public decimal Amount { get; set; }
        
        [Required]
        public string PaymentMethod { get; set; } = "Online"; // Online, Offline
    }
    
    public class CompletePaymentRequest
    {
        [Required]
        public string RenewalId { get; set; } = string.Empty;
        
        [Required]
        public string TransactionId { get; set; } = string.Empty;
        
        [Required]
        public string PaymentStatus { get; set; } = string.Empty; // Success, Failed
    }
    
    public class PaymentResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentUrl { get; set; }
    }
}
