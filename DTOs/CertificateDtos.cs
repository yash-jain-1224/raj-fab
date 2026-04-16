using System;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    public class GenerateCertificateDto
    {
        public string RegistrationNumber { get; set; }
        public string Place { get; set; }
        public string Signature { get; set; }
        public Guid ModuleId { get; set; }
        public string Remarks { get; set; }
        public string Status { get; set; }
        public string CertificateUrl { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? IssuedAt { get; set; }
    }
}