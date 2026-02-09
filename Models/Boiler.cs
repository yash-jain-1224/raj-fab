using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class BoilerSpecifications
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string BoilerType { get; set; } = string.Empty; // fire-tube, water-tube, electric, waste-heat, other
        
        [Required]
        [MaxLength(100)]
        public string Manufacturer { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string SerialNumber { get; set; } = string.Empty;
        
        public int YearOfManufacture { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal WorkingPressure { get; set; } // in kg/cm²
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal DesignPressure { get; set; } // in kg/cm²
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal SteamCapacity { get; set; } // in tonnes/hour
        
        [Required]
        [MaxLength(50)]
        public string FuelType { get; set; } = string.Empty; // coal, oil, gas, biomass, electric, multi-fuel
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal HeatingArea { get; set; } // in m²
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal? SuperheaterArea { get; set; } // in m²
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal? EconomiserArea { get; set; } // in m²
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal? AirPreheaterArea { get; set; } // in m²
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    public class BoilerLocation
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string FactoryName { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string? FactoryLicenseNumber { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string PlotNumber { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string Street { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Locality { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(10)]
        public string Pincode { get; set; } = string.Empty;
        
        public Guid AreaId { get; set; }
        public Guid CityId { get; set; }
        public Guid DistrictId { get; set; }
        public Guid DivisionId { get; set; }
        
        [Column(TypeName = "decimal(10,8)")]
        public decimal? Latitude { get; set; }
        
        [Column(TypeName = "decimal(11,8)")]
        public decimal? Longitude { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        [ForeignKey("AreaId")]
        public Area? Area { get; set; }
        
        [ForeignKey("CityId")]
        public City? City { get; set; }
        
        [ForeignKey("DistrictId")]
        public District? District { get; set; }
        
        [ForeignKey("DivisionId")]
        public Division? Division { get; set; }
    }

    public class BoilerSafetyFeatures
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string SafetyValves { get; set; } = "[]"; // JSON array of safety valve objects
        
        public int WaterGauges { get; set; }
        public int PressureGauges { get; set; }
        public int? FusiblePlugs { get; set; }
        public int BlowdownValves { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string FeedwaterSystem { get; set; } = string.Empty;
        
        public bool EmergencyShutoff { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    public class BoilerCertificate
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string CertificateNumber { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string CertificateType { get; set; } = string.Empty; // initial, renewal, transfer, modification
        
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string InspectorName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string InspectorId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "active"; // active, expired, suspended, cancelled
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    public class BoilerInspectionHistory
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string InspectionId { get; set; } = string.Empty;
        
        public DateTime InspectionDate { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string InspectionType { get; set; } = string.Empty; // annual, biennial, special, modification
        
        [Required]
        [MaxLength(200)]
        public string InspectorName { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Findings { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Recommendations { get; set; } = string.Empty;
        
        public DateTime NextInspectionDue { get; set; }
        public bool CertificateIssued { get; set; }
        
        public Guid BoilerId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        // Navigation property
        [ForeignKey("BoilerId")]
        public RegisteredBoiler? Boiler { get; set; }
    }

    public class RegisteredBoiler
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string RegistrationNumber { get; set; } = string.Empty;
        
        public Guid SpecificationsId { get; set; }
        public Guid LocationId { get; set; }
        public Guid SafetyFeaturesId { get; set; }
        public Guid CurrentCertificateId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string OwnerId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string OwnerName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string OperatorName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string OperatorCertificateNumber { get; set; } = string.Empty;
        
        public DateTime OperatorCertificateExpiry { get; set; }
        
        public DateTime RegistrationDate { get; set; }
        public DateTime LastModified { get; set; } = DateTime.Now;
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "active"; // active, inactive, transferred, scrapped
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        [ForeignKey("SpecificationsId")]
        public BoilerSpecifications? Specifications { get; set; }
        
        [ForeignKey("AreaId")]
        public BoilerLocation? Location { get; set; }
        
        [ForeignKey("SafetyFeaturesId")]
        public BoilerSafetyFeatures? SafetyFeatures { get; set; }
        
        [ForeignKey("CurrentCertificateId")]
        public BoilerCertificate? CurrentCertificate { get; set; }
        
        // Collection navigation properties
        public ICollection<BoilerInspectionHistory> InspectionHistory { get; set; } = new List<BoilerInspectionHistory>();
        public ICollection<BoilerApplication> Applications { get; set; } = new List<BoilerApplication>();
    }

    public class BoilerApplication
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string ApplicationNumber { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string ApplicationType { get; set; } = string.Empty; // registration, renewal, modification, transfer
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "pending"; // pending, approved, rejected, under-review
        
        public Guid? BoilerId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string ApplicantName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string OrganizationName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string ContactPerson { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(15)]
        public string Mobile { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Address { get; set; } = string.Empty;
        
        [Column(TypeName = "nvarchar(max)")]
        public string ApplicationData { get; set; } = "{}"; // JSON data for specific application type
        
        [Column(TypeName = "nvarchar(max)")]
        public string DocumentPaths { get; set; } = "[]"; // JSON array of document file paths
        
        public DateTime SubmissionDate { get; set; } = DateTime.Now;
        public DateTime? ProcessingDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        
        [MaxLength(200)]
        public string? ProcessedBy { get; set; }
        
        [Column(TypeName = "nvarchar(max)")]
        public string? Comments { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        // Navigation property
        [ForeignKey("BoilerId")]
        public RegisteredBoiler? Boiler { get; set; }
    }
}