using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class Master
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string ComboName { get; set; } = null!;

        [Required]
        public int OptionId { get; set; }

        [Required]
        [MaxLength(500)]
        public string OptionValue { get; set; } = null!;

        public bool IsActive { get; set; } = true;
    }
}