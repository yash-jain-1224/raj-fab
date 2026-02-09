using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    public class FactoryMapApprovalDto
    {
        public string Id { get; set; } = string.Empty;
        public string AcknowledgementNumber { get; set; } = string.Empty; 
        public OccupierDetailsDto OccupierDetail { get; set; }

        public MapApprovalFactoryDetailsDto MapApprovalFactoryDetail { get; set; }

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
        public bool IsNew { get; set; }
        public DateTime? Date { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<FactoryMapRawMaterialDto> RawMaterials { get; set; } = new List<FactoryMapRawMaterialDto>();
        public List<FactoryMapIntermediateProductDto> IntermediateProducts { get; set; } = new List<FactoryMapIntermediateProductDto>();
        public List<FactoryMapFinishGoodDto> FinishGoods { get; set; } = new List<FactoryMapFinishGoodDto>();
        public List<ChemicalDto> Chemicals { get; set; } = new List<ChemicalDto>();
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
        public OccupierDetailsRequest OccupierDetail { get; set; }

        public MapApprovalFactoryDetailsRequest MapApprovalFactoryDetail { get; set; }
                
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
    }

    public class OccupierDetailsDto
    {
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
        public string? OfficeAddressDistrictName { get; set; }

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
        public string? ResidentialAddressDistrictName { get; set; }

        [StringLength(100)]
        public string? ResidentialAddressState { get; set; }

        [StringLength(10)]
        public string? ResidentialAddressPinCode { get; set; }

        [StringLength(15)]
        public string? OccupierMobile { get; set; }

        [StringLength(200)]
        [EmailAddress]
        public string? OccupierEmail { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class OccupierDetailsRequest
    {

        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        public int RelationTypeId { get; set; }

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

    public class MapApprovalFactoryDetailsRequest
    {
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
}