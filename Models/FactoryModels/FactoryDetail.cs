using System;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models
{
    public class FactoryDetail
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? ManufacturingDetail { get; set; }
        public string? Situation { get; set; }
        public string? AreaId { get; set; }
        public string? Address { get; set; }
        public string? PinCode { get; set; }
        public int? NumberOfWorker { get; set; }
        public decimal? SanctionedLoad { get; set; }
        public Guid EmployerId { get; set; }
        public Guid ManagerId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}