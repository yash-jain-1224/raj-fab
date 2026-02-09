using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models
{
    public class Occupier
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string FatherName { get; set; } = string.Empty;
        
        [Required]
        public DateTime DateOfBirth { get; set; }
        
        [Required]
        [StringLength(10)]
        public string Gender { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(15)]
        public string MobileNo { get; set; } = string.Empty;
        
        [StringLength(10)]
        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Invalid PAN card format")]
        public string? PanCard { get; set; }
        
        [Required]
        [StringLength(50)]
        public string PlotNo { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string StreetLocality { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string VillageTownCity { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string District { get; set; } = string.Empty;
        
        [Required]
        [StringLength(10)]
        public string Pincode { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? Designation { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}