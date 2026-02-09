using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    public class CreateDistrictDto
    {
        [Required] public string Name { get; set; } = string.Empty;
        [Required] public Guid DivisionId { get; set; }
    }

    public class UpdateDistrictDto
    {
        [Required] public string Name { get; set; } = string.Empty;
        [Required] public Guid DivisionId { get; set; }
    }

    public class DistrictResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid DivisionId { get; set; }
        public string DivisionName { get; set; } = string.Empty;
    }
}
