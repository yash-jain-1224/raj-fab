using System;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models
{
    public class FactoryDetail
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? ManufacturingType { get; set; }
        public string? ManufacturingDetail { get; set; }
        public string? Situation { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? SubDivisionId { get; set; }
        public string? TehsilId { get; set; }
        public string? Area { get; set; }
        public string? Pincode { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? Mobile { get; set; }
        public int? NumberOfWorker { get; set; }
        public decimal? SanctionedLoad { get; set; }
        public string? SanctionedLoadUnit { get; set; }
        public Guid EmployerId { get; set; }
        public Guid ManagerId { get; set; }
        public string? OwnershipType { get; internal set; }
        public string? OwnershipSector { get; internal set; }
        public string? ActivityAsPerNIC { get; internal set; }
        public string? NICCodeDetail { get; internal set; }
        public string? IdentificationOfEstablishment { get; internal set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}