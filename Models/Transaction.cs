using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string PrnNumber { get; set; }

        [Required]
        public string ModuleId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; } = DateTime.Now;

        public string PaymentReq { get; set; }  // Can store JSON/XML

        public string? PaymentRes { get; set; }  // Can store JSON/XML

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PaidAmount { get; set; }

        public string ApplicationId { get; set; }
        public string? Remarks { get; set; } = string.Empty;
    }
}
