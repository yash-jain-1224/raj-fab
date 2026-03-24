using System.ComponentModel.DataAnnotations;
using RajFabAPI.Models;

namespace RajFabAPI.DTOs
{
    public class FactoryMapApprovalDto
    {
        public string Id { get; set; } = string.Empty;
        public string AcknowledgementNumber { get; set; } = string.Empty;
        public string FactoryDetails { get; set; }
        public string OccupierDetails { get; set; }
        public string? CertificatePDFUrl { get; set; }
        public List<ApplicationHistory> ApplicationHistory { get; set; } = new List<ApplicationHistory>();

        [Required]
        [StringLength(200)]
        public string PlantParticulars { get; set; }

        [Required]
        public string ProductName { get; set; }

        [Required]
        public string ManufacturingProcess { get; set; }

        [Required]
        public int MaxWorkerMale { get; set; }

        [Required]
        public int MaxWorkerFemale { get; set; }
        public decimal AreaFactoryPremise { get; set; }
        public int? NoOfFactoriesIfCommonPremise { get; set; }
        public string? PremiseOwnerName { get; set; }
        public string? PremiseOwnerContactNo { get; set; }
        public string? PremiseOwnerAddressPlotNo { get; set; }
        public string? PremiseOwnerAddressStreet { get; set; }
        public string? PremiseOwnerAddressCity { get; set; }
        public string? PremiseOwnerAddressDistrict { get; set; }
        public string? PremiseOwnerAddressState { get; set; }
        public string? PremiseOwnerAddressPinCode { get; set; }
        public string? Place { get; set; }
        public string Status { get; set; }
        public decimal Version { get; set; }
        public string ApplicationPDFUrl { get; set; }
        public bool IsNew { get; set; }
        public DateTime? Date { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<FactoryMapRawMaterialDto> RawMaterials { get; set; } = new List<FactoryMapRawMaterialDto>();
        public List<FactoryMapIntermediateProductDto> IntermediateProducts { get; set; } = new List<FactoryMapIntermediateProductDto>();
        public List<FactoryMapFinishGoodDto> FinishGoods { get; set; } = new List<FactoryMapFinishGoodDto>();
        public List<ChemicalDto> Chemicals { get; set; } = new List<ChemicalDto>();
        public FactoryMapApprovalFileDto? File { get; set; }
        // public List<FactoryMapDocumentDto> FactoryMapDocuments { get; set; } = new List<FactoryMapDocumentDto>();
    }

    public class FactoryMapApprovalFileDto
    {
        public string? LandOwnershipDocumentUrl { get; set; }
        public string? ApprovedLandPlanUrl { get; set; }
        public string? ManufacturingProcessDescriptionUrl { get; set; }
        public string? ProcessFlowChartUrl { get; set; }
        public string? RawMaterialsListUrl { get; set; }
        public string? HazardousProcessesListUrl { get; set; }
        public string? EmergencyPlanUrl { get; set; }
        public string? SafetyHealthPolicyUrl { get; set; }
        public string? FactoryPlanDrawingUrl { get; set; }
        public string? SafetyPolicyApplicableUrl { get; set; }
        public string? OccupierPhotoIdProofUrl { get; set; }
        public string? OccupierAddressProofUrl { get; set; }
    }

    public class FactoryMapDocumentDto
    {
        public string Id { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? FileSize { get; set; }
        public string? FileExtension { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class CreateFactoryMapApprovalRequest
    {
        [Required]
        public string FactoryDetails { get; set; }
        
        [Required]
        public string OccupierDetails { get; set; }

        [Required]
        [StringLength(200)]
        public string PlantParticulars { get; set; }

        [Required]
        public string ProductName { get; set; }

        [Required]
        public string ManufacturingProcess { get; set; }

        [Required]
        public int MaxWorkerMale { get; set; }

        [Required]
        public int MaxWorkerFemale { get; set; }
        public decimal AreaFactoryPremise { get; set; }
        public int? NoOfFactoriesIfCommonPremise { get; set; }
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
        public List<CreateRawMaterialRequest>? RawMaterials { get; set; }
        public List<CreateIntermediateProductRequest>? IntermediateProducts { get; set; }
        public List<CreateFinishGoodRequest>? FinishGoods { get; set; }
        //public List<CreateDangerousOperationRequest>? DangerousOperations { get; set; }
        public List<CreateChemicalRequest>? Chemicals { get; set; }

        public CreateFactoryMapApprovalFileRequest? File { get; set; }

        //public List<FactoryMapDocumentDto> FactoryMapDocuments { get; set; } = new List<FactoryMapDocumentDto>();
    }

    public class OccupierDetailsRequest
    {
        [Required]
        public string Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(100)]
        public string Designation { get; set; }

        [StringLength(50)]
        public string Type { get; set; } // e.g., Individual, Company

        [StringLength(10)]
        public string RelationType { get; set; } // e.g., S/o, D/o

        [StringLength(200)]
        public string RelativeName { get; set; }

        // Address
        [StringLength(200)]
        public string AddressLine1 { get; set; } // Plot / Street

        [StringLength(200)]
        public string AddressLine2 { get; set; } // Area / Locality

        [StringLength(100)]
        public string District { get; set; }

        [StringLength(100)]
        public string Tehsil { get; set; }

        [StringLength(100)]
        public string Area { get; set; }

        [StringLength(10)]
        public string Pincode { get; set; }

        // Contact
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; }

        [Phone]
        [StringLength(15)]
        public string Mobile { get; set; }

        [Phone]
        [StringLength(15)]
        public string Telephone { get; set; }
    }

    public class MapApprovalFactoryDetailsRequest
    {
        [Required]
        public string Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Situation { get; set; } // Location / Situation description

        // Address
        [StringLength(200)]
        public string AddressLine1 { get; set; } // Plot / Street

        [StringLength(200)]
        public string AddressLine2 { get; set; } // Area / Locality

        [Required]
        public Guid DistrictId { get; set; }

        [Required]
        public Guid SubDivisionId { get; set; }

        [Required]
        public Guid TehsilId { get; set; }

        [StringLength(200)]
        public string Area { get; set; }

        [StringLength(10)]
        public string Pincode { get; set; }

        // Contact
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; }

        [Phone]
        [StringLength(15)]
        public string Mobile { get; set; }

        [Phone]
        [StringLength(15)]
        public string Telephone { get; set; }

        [StringLength(200)]
        [Url]
        public string Website { get; set; }
    }

    public class MapApprovalFactoryDetailsDto
    {
        public string FactoryName { get; set; }
        public string FactorySituation { get; set; }
        public string FactoryPlotNo { get; set; }
        public string DivisionId { get; set; }
        public string DivisionName { get; set; }
        public string DistrictId { get; set; }
        public string DistrictName { get; set; }
        public string AreaId { get; set; }
        public string AreaName { get; set; }
        public string FactoryPincode { get; set; }
        public string ContactNo { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UpdateFactoryMapApprovalStatusRequest
    {
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty; // Pending, Approved, Rejected

        [StringLength(1000)]
        public string? Comments { get; set; }
    }

    public class FactoryMapApprovalResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AcknowledgementNumber { get; set; }
        public FactoryMapApprovalDto? Data { get; set; }
    }


    public class CreateFactoryMapApprovalFileRequest
    {
        public string? LandOwnershipDocumentUrl { get; set; }
        public string? ApprovedLandPlanUrl { get; set; }
        public string? ManufacturingProcessDescriptionUrl { get; set; }
        public string? ProcessFlowChartUrl { get; set; }
        public string? RawMaterialsListUrl { get; set; }
        public string? HazardousProcessesListUrl { get; set; }
        public string? EmergencyPlanUrl { get; set; }
        public string? SafetyHealthPolicyUrl { get; set; }
        public string? FactoryPlanDrawingUrl { get; set; }
        public string? SafetyPolicyApplicableUrl { get; set; }
        public string? OccupierPhotoIdProofUrl { get; set; }
        public string? OccupierAddressProofUrl { get; set; }
    }
    public class FactoryMapRawMaterialDto
    {
        public string Id { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public string? MaxStorageQuantity { get; set; }
    }

    public class FactoryMapIntermediateProductDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string MaxStorageQuantity { get; set; }
    }

    public class CreateRawMaterialRequest
    {
        [Required]
        [StringLength(200)]
        public string MaterialName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? MaxStorageQuantity { get; set; }
    }

    public class CreateIntermediateProductRequest
    {
        [Required]
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        public string? MaxStorageQuantity { get; set; }

    }

    public class OccupierDetailsModel
    {
        public string? name { get; set; }
        public string? designation { get; set; }
        public string? relationType { get; set; }
        public string? relativeName { get; set; }
        public string? addressLine1 { get; set; }
        public string? addressLine2 { get; set; }
        public string? district { get; set; }
        public string? tehsil { get; set; }
        public string? area { get; set; }
        public string? pincode { get; set; }
        public string? email { get; set; }
        public string? mobile { get; set; }
        public string? telephone { get; set; }
    }

    public class FactoryDetailsModel
    {
        public string? name { get; set; }
        public string? situation { get; set; }
        public string? addressLine1 { get; set; }
        public string? addressLine2 { get; set; }
        public string? subDivisionId { get; set; }
        public string? area { get; set; }
        public string? pincode { get; set; }
        public string? email { get; set; }
        public string? mobile { get; set; }
        public string? telephone { get; set; }
        public string? website { get; set; }
    }

    public class MapApprovalCertificateRequestDto
    {
        public string? Remarks { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string? Place { get; set; }
        public string? Signature { get; set; }
        public string? IssuedAt { get; set; }
    }

}