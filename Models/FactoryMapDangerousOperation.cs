using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models
{
    public class FactoryMapDangerousOperation
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string FactoryMapApprovalId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string ChemicalName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string OrganicInorganicDetails { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Comments { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation property
        public FactoryMapApproval FactoryMapApproval { get; set; } = null!;
    }
}
