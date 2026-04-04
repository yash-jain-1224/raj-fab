using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class FactoryRegistrationFinishGood
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string FactoryRegistrationId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal QuantityPerDay { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Unit { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxStorageCapacity { get; set; }
        
        [StringLength(100)]
        public string? StorageMethod { get; set; }
        
        [StringLength(500)]
        public string? Remarks { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation property
        public FactoryRegistration FactoryRegistration { get; set; } = null!;
    }
}
