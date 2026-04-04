using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class SMTCTrainerEducationDetail
    {
        public Guid Id { get; set; }

        public Guid TrainerId { get; set; }

        public string EducationType { get; set; } = null!;
        // Qualification / Engineering

        public string? Course { get; set; }

        public string? Degree { get; set; }

        public string? UniversityCollege { get; set; }

        public int? PassingYear { get; set; }

        public string? Specialization { get; set; }

        public SMTCTrainerDetail? Trainer { get; set; }
    }
}