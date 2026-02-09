using System;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    public class CreateOfficeDto
    {
        [Required]
        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }
        public string? Pincode { get; set; }
        public bool IsHeadOffice { get; set; }

        public int LevelCount { get; set; }

        [Required]
        public Guid DistrictId { get; set; }

        [Required]
        public Guid CityId { get; set; }

        public List<Guid> OfficeApplicationAreaIds { get; set; } = new();
        public List<Guid> OfficeInspectionAreaIds { get; set; } = new();
    }
    public class OfficeResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Pincode { get; set; }
        public bool IsHeadOffice { get; set; }
        public int LevelCount { get; set; }
        public Guid DistrictId { get; set; }
        public string DistrictName { get; set; } = string.Empty;
        public Guid CityId { get; set; }
        public string CityName { get; set; } = string.Empty;

        public List<Guid> OfficeApplicationArea { get; set; } = new();
        public List<Guid> OfficeInspectionArea { get; set; } = new();
    }

    public class UpdateOfficeDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }
        public string? Pincode { get; set; }
        public bool IsHeadOffice { get; set; }

        [Required]
        public Guid DistrictId { get; set; }

        [Required]
        public Guid CityId { get; set; }
        public List<Guid> OfficeApplicationAreaIds { get; set; } = new();
        public List<Guid> OfficeInspectionAreaIds { get; set; } = new();
    }

    public class UpdateOfficeLevelCountDto
    {
        public int LevelCount { get; set; }
    }

}
