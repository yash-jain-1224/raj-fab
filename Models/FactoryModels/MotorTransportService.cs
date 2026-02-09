using System;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models
{
    public class MotorTransportService
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string? NatureOfService { get; set; }
        public string? Situation { get; set; }
        public string? AreaId { get; set; }
        public string? Address { get; set; }
        public Guid? EmployerId { get; set; }
        public Guid? ManagerId { get; set; }
        public int? MaxNumberOfWorkerDuringRegistration { get; set; }
        public int? TotalNumberOfVehicles { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}