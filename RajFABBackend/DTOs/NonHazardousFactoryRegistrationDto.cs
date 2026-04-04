using System;

namespace RajFabAPI.DTOs
{
    public class NonHazardousFactoryRegistrationDto
    {
        public Guid Id { get; set; }
        public string RegistrationNo { get; set; }
        public string FactoryName { get; set; }
        public string ApplicantName { get; set; }
        public string ApplicantRelation { get; set; }
        public string RelationType { get; set; }
        public string RelationName { get; set; }
        public string ApplicantAddress { get; set; }
        public string AreaId { get; set; }
        public string DistrictId { get; set; }
        public string DivisionId { get; set; }
        public string Address { get; set; }
        public string Pincode { get; set; }
        public bool DeclarationAccepted { get; set; }
        public bool RequiredInfoAccepted { get; set; }
        public bool VerifyAccepted { get; set; }
        public bool WorkersLimitAccepted { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string ApplicationPlace { get; set; }
        public string ApplicantSignature { get; set; }
        public DateTime VerifyDate { get; set; }
        public string VerifyPlace { get; set; }
        public string VerifierSignature { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateNonHazardousFactoryRegistrationRequest
    {
        public string RegistrationNo { get; set; }
        public string FactoryName { get; set; }
        public string ApplicantName { get; set; }
        public string RelationType { get; set; }
        public string RelationName { get; set; }
        public string ApplicantAddress { get; set; }
        public string AreaId { get; set; }
        public string DistrictId { get; set; }
        public string DivisionId { get; set; }
        public string Address { get; set; }
        public string Pincode { get; set; }
        public bool DeclarationAccepted { get; set; }
        public bool RequiredInfoAccepted { get; set; }
        public bool VerifyAccepted { get; set; }
        public bool WorkersLimitAccepted { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string ApplicationPlace { get; set; }
        public string ApplicantSignature { get; set; }
        public DateTime VerifyDate { get; set; }
        public string VerifyPlace { get; set; }
        public string VerifierSignature { get; set; }
    }
}