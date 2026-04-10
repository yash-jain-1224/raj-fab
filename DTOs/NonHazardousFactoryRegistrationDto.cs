using RajFabAPI.Models;
using System;

namespace RajFabAPI.DTOs
{
    public class NonHazardousFactoryRegistrationDto
    {
        public Guid Id { get; set; }
        public string ApplicationId { get; set; }
        public string RegistrationNo { get; set; }
        public string FactoryName { get; set; }
        public string ApplicantName { get; set; }

        public string RelationType { get; set; }
        public string RelationName { get; set; }

        public string ApplicantAddressLine1 { get; set; }
        public string ApplicantAddressLine2 { get; set; }

        public string SubdivisionName { get; set; }
        public string TehsilName { get; set; }
        public string DistrictName { get; set; }
        public string Area { get; set; }
        public string Pincode { get; set; }

        public bool DeclarationAccepted { get; set; }
        public bool RequiredInfoAccepted { get; set; }
        public bool VerifyAccepted { get; set; }
        public bool WorkersLimitAccepted { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateNonHazardousFactoryRegistrationRequest
    {
      
        public string FactoryName { get; set; }
        public string ApplicantName { get; set; }

        public string RelationType { get; set; }
        public string RelationName { get; set; }

        public string ApplicantAddressLine1 { get; set; }
        public string ApplicantAddressLine2 { get; set; }

        public string SubdivisionName { get; set; }
        public string TehsilName { get; set; }
        public string DistrictName { get; set; }
        public string Area { get; set; }
        public string Pincode { get; set; }

        public bool DeclarationAccepted { get; set; }
        public bool RequiredInfoAccepted { get; set; }
        public bool VerifyAccepted { get; set; }
        public bool WorkersLimitAccepted { get; set; }
    }

    public class NonHazardousApplicationResponseDto
    {
        public NonHazardousFactoryRegistrationDto ApplicationDetails { get; set; }
        public List<ApplicationHistory> ApplicationHistory { get; set; }
        
    }
}