using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class FactoryRegistrationFee
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public string FactoryRegistrationId { get; set; } = string.Empty;
        
        [Required]
        public int TotalWorkers { get; set; }
        
        [Required]
        public decimal TotalPowerHP { get; set; }
        
        [Required]
        public decimal TotalPowerKW { get; set; }
        
        [Required]
        public decimal FactoryFee { get; set; }
        
        [Required]
        public decimal ElectricityFee { get; set; }
        
        [Required]
        public decimal TotalFee { get; set; }
        
        public string? FeeBreakdown { get; set; } // JSON string with details
        
        [Required]
        public DateTime CalculatedAt { get; set; } = DateTime.Now;
        
        // Navigation Properties
        [ForeignKey("FactoryRegistrationId")]
        public virtual FactoryRegistration FactoryRegistration { get; set; } = null!;
    }
}
