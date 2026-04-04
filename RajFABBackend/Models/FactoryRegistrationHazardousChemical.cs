using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models
{
    public class FactoryRegistrationHazardousChemical
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string FactoryRegistrationId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string ChemicalName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string ChemicalType { get; set; } = string.Empty; // Raw Materials, Intermediate Products, Final Products, Hazardous chemicals, Toxic, Inflammable, Corrosive, Highly Reactive
        
        [StringLength(1000)]
        public string? Comments { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation property
        public FactoryRegistration FactoryRegistration { get; set; } = null!;
    }
}
