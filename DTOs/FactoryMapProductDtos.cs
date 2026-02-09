using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    // Finish Good DTOs
    public class FactoryMapFinishGoodDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal QuantityPerDay { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal? MaxStorageCapacity { get; set; }
        public string? StorageMethod { get; set; }
        public string? Remarks { get; set; }
    }
    
    public class CreateFinishGoodRequest
    {
        [Required]
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;
        
        [Required]
        public decimal QuantityPerDay { get; set; }
        
        [StringLength(50)]
        public string Unit { get; set; } = string.Empty;
        
        public string? MaxStorageCapacity { get; set; }
        
        [StringLength(100)]
        public string? StorageMethod { get; set; }
        
        [StringLength(500)]
        public string? Remarks { get; set; }
    }

    // Dangerous Operation DTOs
    public class FactoryMapDangerousOperationDto
    {
        public string Id { get; set; } = string.Empty;
        public string ChemicalName { get; set; } = string.Empty;
        public string OrganicInorganicDetails { get; set; } = string.Empty;
        public string? Comments { get; set; }
    }
    
    public class CreateDangerousOperationRequest
    {
        [Required]
        [StringLength(500)]
        public string ChemicalName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string OrganicInorganicDetails { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Comments { get; set; }
    }

    // Hazardous Chemical DTOs
    public class ChemicalDto
    {
        public string Id { get; set; }
        public string ChemicalName { get; set; } = string.Empty;
        public string TradeName { get; set; } = string.Empty;
        public string MaxStorageQuantity { get; set; } = string.Empty;
    }
    
    public class CreateChemicalRequest
    {
        [Required]
        [StringLength(200)]
        public string TradeName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string ChemicalName { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string MaxStorageQuantity { get; set; }
    }
}
