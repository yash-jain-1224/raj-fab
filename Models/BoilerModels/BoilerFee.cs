using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models.BoilerModels
{
    public class BoilerFee
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string ServiceType { get; set; } = string.Empty;

        public string? Category { get; set; }

        public decimal Amount { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
