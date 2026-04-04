using System;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models
{
    public class Office
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Pincode { get; set; }
        public int LevelCount { get; set; } = 0;

        public bool IsActive { get; set; } = true;
        public bool IsHeadOffice { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public Guid DistrictId { get; set; }
        public District District { get; set; } = null!;

        public Guid CityId { get; set; }
        public City City { get; set; } = null!;

        public ICollection<OfficeApplicationArea> ApplicationArea { get; set; } = new List<OfficeApplicationArea>();

        public ICollection<OfficeInspectionArea> InspectionArea { get; set; } = new List<OfficeInspectionArea>();
    }
}
