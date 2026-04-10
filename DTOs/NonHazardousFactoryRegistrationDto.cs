using RajFabAPI.Models;

namespace RajFabAPI.DTOs
{
    public class NonHazardousFactoryRegistrationDto
    {
        public Guid Id { get; set; }
        public string FactoryRegistrationNumber { get; set; }
        public FactoryBasicDto FactoryDetails { get; set; }
        public string ApplicationNumber { get; set; }
        public string? ApplicationPDFUrl { get; set; }
        public string? ObjectionLetterUrl { get; set; }
        public decimal Version { get; set; } = 1.0m;
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateNonHazardousFactoryRegistrationRequest
    {
        public string FactoryRegistrationNumber { get; set; }
    }

    public class NonHazardousApplicationResponseDto
    {
        public NonHazardousFactoryRegistrationDto ApplicationDetails { get; set; }
        public List<ApplicationHistory> ApplicationHistory { get; set; }

    }
}