using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    public class CreateAreaDto
    {
        [Required] public string Name { get; set; } = string.Empty;
        [Required] public Guid CityId { get; set; }
    }

    public class UpdateAreaDto
    {
        [Required] public string Name { get; set; } = string.Empty;
        [Required] public Guid CityId { get; set; }
    }

    public class AreaResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid CityId { get; set; }
        public string CityName { get; set; } = string.Empty;
        public Guid DistrictId { get; set; }
        public string DistrictName { get; set; } = string.Empty;
    }

    public class AreaHierarchyDto
    {
        public Guid AreaId { get; set; }
        public string? AreaName { get; set; }
        public Guid DistrictId { get; set; }
        public string? DistrictName { get; set; }
        public Guid DivisionId { get; set; }
        public string? DivisionName { get; set; }
    }
}
