using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    public class CreateMasterDto
    {
        [Required]
        [MaxLength(255)]
        public string ComboName { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string OptionValue { get; set; } = null!;
    }

    public class UpdateMasterDto
    {
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

    public class MasterResponseDto
    {
        public int Id { get; set; }
        public string ComboName { get; set; } = null!;
        public int OptionId { get; set; }
        public string OptionValue { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}