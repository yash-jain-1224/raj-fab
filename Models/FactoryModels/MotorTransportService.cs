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
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string SubDivisionId { get; set; }
        public string TehsilId { get; set; }
        public string? Area { get; set; }
        public string? Pincode { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? Mobile { get; set; }
        public Guid? EmployerId { get; set; }
        public Guid? ManagerId { get; set; }
        public int? MaxNumberOfWorkerDuringRegistration { get; set; }
        public int? TotalNumberOfVehicles { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}