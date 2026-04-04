using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    public class CreateSMTCRegistrationDto
    {
        [Required]
        public string FactoryRegistrationNo { get; set; } = null!;

        public bool TrainingCenterAvailable { get; set; }

        public int? SeatingCapacity { get; set; }

        public string? TrainingCenterPhotoPath { get; set; }

        public bool? AudioVideoFacility { get; set; }

        public string? Comments { get; set; }

        public List<SMTCTrainerDto> Trainers { get; set; } = new();
    }

    public class SMTCTrainerDto
    {
        [Required]
        public string TrainerName { get; set; } = null!;

        public int? TotalYearsExperience { get; set; }

        public string? Mobile { get; set; }

        public string? PhotoPath { get; set; }

        public string? DegreeDocumentPath { get; set; }

        public List<SMTCEducationDto> EducationDetails { get; set; } = new();
    }

    public class SMTCEducationDto
    {
        [Required]
        public string EducationType { get; set; } = null!;
        // Qualification / Engineering

        public string? Course { get; set; }

        public string? Degree { get; set; }

        public string? UniversityCollege { get; set; }

        public int? PassingYear { get; set; }

        public string? Specialization { get; set; }
    }
    public class SMTCRegistrationDetailsDto
    {
        public string? ApplicationId { get; set; }

        public string? SMTCRegistrationNo { get; set; }

        public string? FactoryRegistrationNo { get; set; }

        public bool TrainingCenterAvailable { get; set; }

        public int? SeatingCapacity { get; set; }

        public string? TrainingCenterPhotoPath { get; set; }

        public bool? AudioVideoFacility { get; set; }

        public string? Comments { get; set; }

        public string? Type { get; set; }

        public string? Status { get; set; }

        public decimal Version { get; set; }

        public DateTime? ValidUpto { get; set; }

        public List<SMTCTrainerDto> Trainers { get; set; } = new();
    }
}
