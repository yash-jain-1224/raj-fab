using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    public class CreateDivisionDto
    {
        [Required] public string Name { get; set; } = string.Empty;
    }

    public class UpdateDivisionDto
    {
        [Required] public string Name { get; set; } = string.Empty;
    }

    public class DivisionResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
