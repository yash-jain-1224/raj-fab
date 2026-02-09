using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    public class FactoryRegistrationDto
    {
        public string Id { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string? MapApprovalAcknowledgementNumber { get; set; }
        
        // Period of License
        public DateTime LicenseFromDate { get; set; }
        public DateTime LicenseToDate { get; set; }
        public int LicenseYears { get; set; }
        
        // General Information
        public string FactoryName { get; set; } = string.Empty;
        public string? FactoryRegistrationNumber { get; set; }
        
        // Factory Address and Contact Information
        public string PlotNumber { get; set; } = string.Empty;
        public string StreetLocality { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string? DistrictName { get; set; }
        public string CityTown { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string? AreaName { get; set; }
        public string Pincode { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        
        // Nature of manufacturing process
        public string ManufacturingProcess { get; set; } = string.Empty;
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
        public string PowerUnit { get; set; } = "HP";
        public string? KNumber { get; set; }
        
        // Particulars of Factory Manager
        public string FactoryManagerName { get; set; } = string.Empty;
        public string FactoryManagerFatherName { get; set; } = string.Empty;
        public string FactoryManagerPlotNumber { get; set; } = string.Empty;
        public string FactoryManagerStreetLocality { get; set; } = string.Empty;
        public string FactoryManagerDistrict { get; set; } = string.Empty;
        public string? FactoryManagerDistrictName { get; set; }
        public string FactoryManagerArea { get; set; } = string.Empty;
        public string? FactoryManagerAreaName { get; set; }
        public string FactoryManagerCityTown { get; set; } = string.Empty;
        public string FactoryManagerPincode { get; set; } = string.Empty;
        public string FactoryManagerMobile { get; set; } = string.Empty;
        public string FactoryManagerEmail { get; set; } = string.Empty;
        public string? FactoryManagerPanCard { get; set; }
        
        // Particulars of Occupier
        public string OccupierType { get; set; } = string.Empty;
        public string OccupierName { get; set; } = string.Empty;
        public string OccupierFatherName { get; set; } = string.Empty;
        public string OccupierPlotNumber { get; set; } = string.Empty;
        public string OccupierStreetLocality { get; set; } = string.Empty;
        public string OccupierCityTown { get; set; } = string.Empty;
        public string OccupierDistrict { get; set; } = string.Empty;
        public string? OccupierDistrictName { get; set; }
        public string OccupierArea { get; set; } = string.Empty;
        public string? OccupierAreaName { get; set; }
        public string OccupierPincode { get; set; } = string.Empty;
        public string OccupierMobile { get; set; } = string.Empty;
        public string OccupierEmail { get; set; } = string.Empty;
        public string? OccupierPanCard { get; set; }
        
        // Land and Building
        public string LandOwnerName { get; set; } = string.Empty;
        public string LandOwnerPlotNumber { get; set; } = string.Empty;
        public string LandOwnerStreetLocality { get; set; } = string.Empty;
        public string LandOwnerDistrict { get; set; } = string.Empty;
        public string? LandOwnerDistrictName { get; set; }
        public string LandOwnerArea { get; set; } = string.Empty;
        public string? LandOwnerAreaName { get; set; }
        public string LandOwnerCityTown { get; set; } = string.Empty;
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
        public string Status { get; set; } = "Pending";
        public string? Comments { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        
        public int AmendmentCount { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public List<FactoryRegistrationDocumentDto> Documents { get; set; } = new List<FactoryRegistrationDocumentDto>();
        
        public FeeCalculationResultDto? CalculatedFee { get; set; }
    }
    
    public class FactoryRegistrationDocumentDto
    {
        public string Id { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FileExtension { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }
    
    public class CreateFactoryRegistrationRequest
    {
        public string? MapApprovalAcknowledgementNumber { get; set; }
        
        // Period of License
        [Required]
        public DateTime LicenseFromDate { get; set; }
        [Required]
        public DateTime LicenseToDate { get; set; }
        [Required]
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
        [Required]
        public string ManufacturingProcess { get; set; } = string.Empty;
        [Required]
        public DateTime ProductionStartDate { get; set; }
        [Required]
        public string ManufacturingProcessLast12Months { get; set; } = string.Empty;
        [Required]
        public string ManufacturingProcessNext12Months { get; set; } = string.Empty;
        
        // Workers Employed
        [Required]
        public int MaxWorkersMaleProposed { get; set; }
        [Required]
        public int MaxWorkersFemaleProposed { get; set; }
        [Required]
        public int MaxWorkersTransgenderProposed { get; set; }
        [Required]
        public int MaxWorkersMaleEmployed { get; set; }
        [Required]
        public int MaxWorkersFemaleEmployed { get; set; }
        [Required]
        public int MaxWorkersTransgenderEmployed { get; set; }
        [Required]
        public int WorkersMaleOrdinary { get; set; }
        [Required]
        public int WorkersFemaleOrdinary { get; set; }
        [Required]
        public int WorkersTransgenderOrdinary { get; set; }
        
        // Power Installed
        [Required]
        public decimal TotalRatedHorsePower { get; set; }
        public string PowerUnit { get; set; } = "HP";
        public string? KNumber { get; set; }
        
        // Particulars of Factory Manager
        [Required]
        public string FactoryManagerName { get; set; } = string.Empty;
        [Required]
        public string FactoryManagerFatherName { get; set; } = string.Empty;
        [Required]
        public string FactoryManagerPlotNumber { get; set; } = string.Empty;
        [Required]
        public string FactoryManagerStreetLocality { get; set; } = string.Empty;
        [Required]
        public string FactoryManagerDistrict { get; set; } = string.Empty;
        [Required]
        public string FactoryManagerArea { get; set; } = string.Empty;
        public string? FactoryManagerCityTown { get; set; }
        [Required]
        public string FactoryManagerPincode { get; set; } = string.Empty;
        [Required]
        public string FactoryManagerMobile { get; set; } = string.Empty;
        [Required]
        public string FactoryManagerEmail { get; set; } = string.Empty;
        
        [StringLength(10)]
        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Invalid PAN card format")]
        public string? FactoryManagerPanCard { get; set; }
        
        // Particulars of Occupier
        [Required]
        public string OccupierType { get; set; } = string.Empty;
        [Required]
        public string OccupierName { get; set; } = string.Empty;
        [Required]
        public string OccupierFatherName { get; set; } = string.Empty;
        [Required]
        public string OccupierPlotNumber { get; set; } = string.Empty;
        [Required]
        public string OccupierStreetLocality { get; set; } = string.Empty;
        public string? OccupierCityTown { get; set; }
        [Required]
        public string OccupierDistrict { get; set; } = string.Empty;
        [Required]
        public string OccupierArea { get; set; } = string.Empty;
        [Required]
        public string OccupierPincode { get; set; } = string.Empty;
        [Required]
        public string OccupierMobile { get; set; } = string.Empty;
        [Required]
        public string OccupierEmail { get; set; } = string.Empty;
        
        [StringLength(10)]
        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Invalid PAN card format")]
        public string? OccupierPanCard { get; set; }
        
        // Land and Building
        [Required]
        public string LandOwnerName { get; set; } = string.Empty;
        [Required]
        public string LandOwnerPlotNumber { get; set; } = string.Empty;
        [Required]
        public string LandOwnerStreetLocality { get; set; } = string.Empty;
        [Required]
        public string LandOwnerDistrict { get; set; } = string.Empty;
        [Required]
        public string LandOwnerArea { get; set; } = string.Empty;
        public string? LandOwnerCityTown { get; set; }
        [Required]
        public string LandOwnerPincode { get; set; } = string.Empty;
        [Required]
        public string LandOwnerMobile { get; set; } = string.Empty;
        [Required]
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
        [Required]
        public bool DeclarationAccepted { get; set; }
    }
    
    public class UpdateFactoryRegistrationStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty; // Pending, Under Review, Approved, Rejected
        public string? Comments { get; set; }
    }
}