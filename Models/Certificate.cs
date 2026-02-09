using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class Certificate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string RegistrationNumber { get; set; }
        
        [Column(TypeName = "decimal(3,1)")]
        public decimal CertificateVersion { get; set; } = 1.0m;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CertificateUrl { get; set; }
        public Guid IssuedByUserId { get; set; }
        public DateTime IssuedAt { get; set; }
        public string Place { get; set; }
        public string Signature { get; set; }
        public string Status { get; set; }
        public Guid ModuleId { get; set; }
        public string Remarks { get; set; }
    }
}
