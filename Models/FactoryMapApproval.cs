using RajFabAPI.DTOs;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class FactoryMapApproval
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(255)]
        public string AcknowledgementNumber { get; set; }

        [Required]
        [StringLength(200)]
        public string PlantParticulars { get; set; }

        [Required]
        public string ProductName { get; set; }

        [Required]
        public string ManufacturingProcess { get; set; }

        [Required]
        public int MaxWorkerMale { get; set; }

        [Column(TypeName = "decimal(3,1)")]
        public decimal Version { get; set; } = 1.0m;
        public bool IsNew { get; set; } = true;

        [Required]
        public int MaxWorkerFemale { get; set; }
        public int MaxWorkerTransgender { get; set; }
        public decimal AreaFactoryPremise { get; set; }
        public int? NoOfFactoriesIfCommonPremise { get; set; }
        public string? PremiseOwnerDetails { get; set; }
        public string? PremiseOwnerName { get; set; }
        public string? PremiseOwnerContactNo { get; set; }
        public string? PremiseOwnerAddressPlotNo { get; set; }
        public string? PremiseOwnerAddressStreet { get; set; }
        public string? PremiseOwnerAddressCity { get; set; }
        public string? PremiseOwnerAddressDistrict { get; set; }
        public string? PremiseOwnerAddressState { get; set; }
        public string? PremiseOwnerAddressPinCode { get; set; }
        public string? Place { get; set; }
        public DateTime? Date { get; set; }
        public bool IsESignCompleted { get; set; } = false;
        public string? ApplicationPDFUrl { get; set; } = string.Empty;
        public string? ObjectionLetterUrl { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        //public List<FactoryMapDocument> Documents { get; set; } = new List<FactoryMapDocument>();

        public FactoryMapApprovalFile File { get; set; }
        public List<FactoryMapRawMaterial> RawMaterials { get; set; } = new List<FactoryMapRawMaterial>();
        public List<FactoryMapIntermediateProduct> IntermediateProducts { get; set; } = new List<FactoryMapIntermediateProduct>();
        public List<FactoryMapFinishGood> FinishGoods { get; set; } = new List<FactoryMapFinishGood>();
        //public List<FactoryMapDangerousOperation> DangerousOperations { get; set; } = new List<FactoryMapDangerousOperation>();
        public List<FactoryMapApprovalChemical> Chemicals { get; set; } = new List<FactoryMapApprovalChemical>();
        public string FactoryDetails { get; set; }
        public string OccupierDetails { get; set; }
    }

    public class MapApprovalFactoryDetail
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FactoryMapApprovalId { get; set; }
        public FactoryMapApproval FactoryMapApproval { get; set; }
        public string FactoryName { get; set; }
        public string FactorySituation { get; set; }
        public string FactoryPlotNo { get; set; }
        public string DivisionId { get; set; }
        public string DistrictId { get; set; }
        public string AreaId { get; set; }
        public string FactoryPincode { get; set; }
        public string ContactNo { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }

    }

    public class MapApprovalOccupierDetail
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FactoryMapApprovalId { get; set; }
        public FactoryMapApproval FactoryMapApproval { get; set; }

        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        public int? RelationTypeId { get; set; }

        [Required]
        public string RelativeName { get; set; }

        [StringLength(100)]
        public string OfficeAddressPlotno { get; set; }

        [StringLength(200)]
        public string OfficeAddressStreet { get; set; }

        [StringLength(100)]
        public string OfficeAddressCity { get; set; }

        [StringLength(100)]
        public string? OfficeAddressDistrict { get; set; }

        [StringLength(100)]
        public string? OfficeAddressState { get; set; }

        [StringLength(100)]
        public string? OfficeAddressPinCode { get; set; }

        [StringLength(100)]
        public string? ResidentialAddressPlotno { get; set; }

        [StringLength(200)]
        public string ResidentialAddressStreet { get; set; }

        [StringLength(100)]
        public string ResidentialAddressCity { get; set; }

        [StringLength(100)]
        public string? ResidentialAddressDistrict { get; set; }

        [StringLength(100)]
        public string? ResidentialAddressState { get; set; }

        [StringLength(10)]
        public string? ResidentialAddressPinCode { get; set; }

        [StringLength(15)]
        public string? OccupierMobile { get; set; }

        [StringLength(200)]
        [EmailAddress]
        public string? OccupierEmail { get; set; }
    }

    public class FactoryMapDocument
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string FactoryMapApprovalId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [StringLength(100)]
        public string? FileSize { get; set; }

        [StringLength(50)]
        public string? FileExtension { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        // Navigation property
        public FactoryMapApproval FactoryMapApproval { get; set; } = null!;
    }

    public class FactoryMapRawMaterial
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string FactoryMapApprovalId { get; set; }

        [Required]
        [StringLength(200)]
        public string MaterialName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? MaxStorageQuantity { get; set; }

        [StringLength(50)]
        public string? Unit { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class FactoryMapIntermediateProduct
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string FactoryMapApprovalId { get; set; }
        [Required]
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;

        public string? MaxStorageQuantity { get; set; }

        [StringLength(50)]
        public string? Unit { get; set; }

        
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }

    public class FactoryMapApprovalChemical
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string FactoryMapApprovalId { get; set; }
        [Required]
        [StringLength(200)]
        public string TradeName { get; set; }

        [Required]
        [StringLength(200)]
        public string ChemicalName { get; set; }

        [Required]
        [StringLength(200)]
        public string MaxStorageQuantity { get; set; }

        [StringLength(50)]
        public string? Unit { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    public class FactoryMapApprovalFile
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string FactoryMapApprovalId { get; set; }

        [StringLength(500)]
        public string? LandOwnershipDocumentUrl { get; set; }

        [StringLength(500)]
        public string? ApprovedLandPlanUrl { get; set; }

        [StringLength(500)]
        public string? ManufacturingProcessDescriptionUrl { get; set; }

        [StringLength(500)]
        public string? ProcessFlowChartUrl { get; set; }

        [StringLength(500)]
        public string? RawMaterialsListUrl { get; set; }

        [StringLength(500)]
        public string? HazardousProcessesListUrl { get; set; }

        [StringLength(500)]
        public string? EmergencyPlanUrl { get; set; }

        [StringLength(500)]
        public string? SafetyHealthPolicyUrl { get; set; }

        [StringLength(500)]
        public string? FactoryPlanDrawingUrl { get; set; }

        [StringLength(500)]
        public string? SafetyPolicyApplicableUrl { get; set; }

        [StringLength(500)]
        public string? OccupierPhotoIdProofUrl { get; set; }

        [StringLength(500)]
        public string? OccupierAddressProofUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public FactoryMapApproval FactoryMapApproval { get; set; }
    }
}