using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    // Registration DTOs
    public class BoilerRegistrationDto
    {
        public required ApplicantInfoDto ApplicantInfo { get; set; }
        public required BoilerSpecificationsDto Specifications { get; set; }
        public required BoilerLocationDto Location { get; set; }
        public required BoilerSafetyFeaturesDto SafetyFeatures { get; set; }
        public required OperatorDetailsDto OperatorDetails { get; set; }
    }

    public class ApplicantInfoDto
    {
        [Required]
        public string OwnerName { get; set; } = string.Empty;
        
        [Required]
        public string OrganizationName { get; set; } = string.Empty;
        
        [Required]
        public string ContactPerson { get; set; } = string.Empty;
        
        [Required]
        [Phone]
        public string Mobile { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Address { get; set; } = string.Empty;
    }

    public class BoilerSpecificationsDto
    {
        [Required]
        public string BoilerType { get; set; } = string.Empty;
        
        [Required]
        public string Manufacturer { get; set; } = string.Empty;
        
        [Required]
        public string SerialNumber { get; set; } = string.Empty;
        
        [Required]
        public int YearOfManufacture { get; set; }
        
        [Required]
        public decimal WorkingPressure { get; set; }
        
        [Required]
        public decimal DesignPressure { get; set; }
        
        [Required]
        public decimal SteamCapacity { get; set; }
        
        [Required]
        public string FuelType { get; set; } = string.Empty;
        
        [Required]
        public decimal HeatingArea { get; set; }
        
        public decimal? SuperheaterArea { get; set; }
        public decimal? EconomiserArea { get; set; }
        public decimal? AirPreheaterArea { get; set; }
    }

    public class BoilerLocationDto
    {
        [Required]
        public string FactoryName { get; set; } = string.Empty;
        
        public string? FactoryLicenseNumber { get; set; }
        
        [Required]
        public string PlotNumber { get; set; } = string.Empty;
        
        [Required]
        public string Street { get; set; } = string.Empty;
        
        [Required]
        public string Locality { get; set; } = string.Empty;
        
        [Required]
        public string Pincode { get; set; } = string.Empty;
        
        [Required]
        public Guid AreaId { get; set; }
        
        [Required]
        public Guid CityId { get; set; }
        
        [Required]
        public Guid DistrictId { get; set; }
        
        [Required]
        public Guid DivisionId { get; set; }
        
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }

    public class BoilerSafetyFeaturesDto
    {
        [Required]
        public List<SafetyValveDto> SafetyValves { get; set; } = new();
        
        [Required]
        public int WaterGauges { get; set; }
        
        [Required]
        public int PressureGauges { get; set; }
        
        public int? FusiblePlugs { get; set; }
        
        [Required]
        public int BlowdownValves { get; set; }
        
        [Required]
        public string FeedwaterSystem { get; set; } = string.Empty;
        
        [Required]
        public bool EmergencyShutoff { get; set; }
    }

    public class SafetyValveDto
    {
        public int Count { get; set; }
        public decimal SettingPressure { get; set; }
        public decimal Capacity { get; set; }
    }

    public class OperatorDetailsDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string CertificateNumber { get; set; } = string.Empty;
        
        [Required]
        public DateTime CertificateExpiry { get; set; }
    }

    // Renewal DTOs
    public class BoilerRenewalDto
    {
        [Required]
        public string BoilerRegistrationNumber { get; set; } = string.Empty;
        
        [Required]
        public string CurrentCertificateNumber { get; set; } = string.Empty;
        
        [Required]
        public DateTime LastInspectionDate { get; set; }
        
        [Required]
        public string RenewalReason { get; set; } = string.Empty;
        
        [Required]
        public string ChangesFromLastInspection { get; set; } = string.Empty;
        
        [Required]
        public OperatorDetailsDto OperatorDetails { get; set; } = new();
    }

    // Modification DTOs
    public class BoilerModificationDto
    {
        [Required]
        public string BoilerRegistrationNumber { get; set; } = string.Empty;
        
        [Required]
        public string ModificationType { get; set; } = string.Empty;
        
        [Required]
        public string ModificationDetails { get; set; } = string.Empty;
        
        [Required]
        public string EngineeringJustification { get; set; } = string.Empty;
        
        [Required]
        public string SafetyImpactAssessment { get; set; } = string.Empty;
        
        public ProposedChangesDto? ProposedChanges { get; set; }
    }

    public class ProposedChangesDto
    {
        public BoilerSpecificationsDto? CurrentSpecs { get; set; }
        public BoilerSpecificationsDto? ProposedSpecs { get; set; }
    }

    // Transfer DTOs
    public class BoilerTransferDto
    {
        [Required]
        public string BoilerRegistrationNumber { get; set; } = string.Empty;
        
        [Required]
        public string TransferType { get; set; } = string.Empty;
        
        [Required]
        public CurrentOwnerDto CurrentOwner { get; set; } = new();
        
        [Required]
        public NewOwnerDto NewOwner { get; set; } = new();
        
        public BoilerLocationDto? NewLocation { get; set; }
        
        [Required]
        public string TransferReason { get; set; } = string.Empty;
        
        [Required]
        public OperatorDetailsDto OperatorDetails { get; set; } = new();
    }

    public class CurrentOwnerDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string OrganizationName { get; set; } = string.Empty;
        
        [Required]
        public string ContactInfo { get; set; } = string.Empty;
    }

    public class NewOwnerDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string OrganizationName { get; set; } = string.Empty;
        
        [Required]
        public string ContactPerson { get; set; } = string.Empty;
        
        [Required]
        [Phone]
        public string Mobile { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Address { get; set; } = string.Empty;
    }

    // Inspection DTOs
    public class BoilerInspectionDto
    {
        [Required]
        public DateTime InspectionDate { get; set; }
        
        [Required]
        public string InspectionType { get; set; } = string.Empty;
        
        [Required]
        public string InspectorName { get; set; } = string.Empty;
        
        [Required]
        public string Findings { get; set; } = string.Empty;
        
        [Required]
        public string Recommendations { get; set; } = string.Empty;
        
        [Required]
        public DateTime NextInspectionDue { get; set; }
        
        [Required]
        public bool CertificateIssued { get; set; }
    }

    // Status update DTO
    public class UpdateApplicationStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
        
        public string? Comments { get; set; }
        
        public string? ProcessedBy { get; set; }
    }
}