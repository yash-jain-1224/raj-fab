using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class AnnualReturn
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(255)]
        public string FactoryRegistrationNumber { get; set; }

        public bool IsActive { get; set; } = true;

        [Column(TypeName = "nvarchar(max)")]
        public string FormData { get; set; } = string.Empty;

        [Column(TypeName = "decimal(3,1)")]
        public decimal Version { get; set; } = 1.0m;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
