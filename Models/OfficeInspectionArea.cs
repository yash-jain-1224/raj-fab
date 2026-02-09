using System;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models
{
    public class OfficeInspectionArea
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid OfficeId { get; set; }
        public Office Office { get; set; } = null!;

        public Guid CityId { get; set; }
        public City City { get; set; } = null!;
    }
}