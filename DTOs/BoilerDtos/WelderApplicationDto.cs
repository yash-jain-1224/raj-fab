using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    public class CreateWelderRegistrationDto
    {
        public WelderDetailDto? WelderDetail { get; set; }

        public WelderEmployerDto? EmployerDetail { get; set; }
    }

    public class WelderDetailDto
    {
        // Personal Info
        public string? Name { get; set; }

        public string? FatherName { get; set; }

        public DateTime? DOB { get; set; }

        public string? IdentificationMark { get; set; }

        public string? Weight { get; set; }

        public string? Height { get; set; }

        // Address
        public string? AddressLine1 { get; set; }

        public string? AddressLine2 { get; set; }

        public string? District { get; set; }

        public string? Tehsil { get; set; }

        public string? Area { get; set; }

        public string? Pincode { get; set; }

        public string? Telephone { get; set; }

        public string? Mobile { get; set; }

        public string? Email { get; set; }

        // Experience
        public string? ExperienceYears { get; set; }

        public string? ExperienceDetails { get; set; }

        public string? ExperienceCertificate { get; set; }

        // Test
        public string? TestType { get; set; }

        public string? Radiography { get; set; }

        public string? Materials { get; set; }

        // Qualification
        public DateTime? DateOfTest { get; set; }

        public string? TypePosition { get; set; }

        public string? MaterialType { get; set; }

        public string? MaterialGrouping { get; set; }

        public string? ProcessOfWelding { get; set; }

        public string? WeldWithBacking { get; set; }

        public string? ElectrodeGrouping { get; set; }

        public string? TestPieceXrayed { get; set; }

        // Documents
        public string? Photo { get; set; }

        public string? Thumb { get; set; }

        public string? WelderSign { get; set; }

        public string? EmployerSign { get; set; }
    }

    public class WelderEmployerDto
    {
        public string? EmployerType { get; set; }

        public string? EmployerName { get; set; }

        public string? FirmName { get; set; }

        // Address
        public string? AddressLine1 { get; set; }

        public string? AddressLine2 { get; set; }

        public string? District { get; set; }

        public string? Tehsil { get; set; }

        public string? Area { get; set; }

        public string? Pincode { get; set; }

        public string? Telephone { get; set; }

        public string? Mobile { get; set; }

        public string? Email { get; set; }

        public DateTime? EmployedFrom { get; set; }

        public DateTime? EmployedTo { get; set; }
    }

    public class WelderRenewalDto
    {
        public string WelderRegistrationNo { get; set; } = null!;

        public int RenewalYears { get; set; }
    }

    public class GetWelderResponseDto
    {
        

        public string? ApplicationId { get; set; }

        public string? WelderRegistrationNo { get; set; }

        public string? Status { get; set; }

        public string? Type { get; set; }

        public decimal Version { get; set; }

        public DateTime? ValidFrom { get; set; }

        public DateTime? ValidUpto { get; set; }

        public WelderDetailDto? WelderDetail { get; set; }

        public WelderEmployerDto? EmployerDetail { get; set; }
    }



}
