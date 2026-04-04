using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models
{
    public class ApplicationObjectionLetter
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The primary key of the specific application record
        /// (EstablishmentRegistrationId / FactoryMapApprovalId / FactoryLicenseId)
        /// </summary>
        [Required, MaxLength(100)]
        public string ApplicationId { get; set; } = string.Empty;

        /// <summary>Module name — matches ApplicationTypeNames constants</summary>
        [Required, MaxLength(100)]
        public string ModuleName { get; set; } = string.Empty;

        /// <summary>URL to the generated PDF file</summary>
        [Required]
        public string FileUrl { get; set; } = string.Empty;

        /// <summary>Subject line used in the letter (entity.Remarks at time of generation)</summary>
        public string? Subject { get; set; }

        /// <summary>UserId (CreatedBy) of the authority who triggered the return</summary>
        [MaxLength(100)]
        public string? GeneratedBy { get; set; }

        /// <summary>Full name of the authority who generated the letter</summary>
        [MaxLength(200)]
        public string? GeneratedByName { get; set; }

        /// <summary>Designation used in the letter signature (Post.Name)</summary>
        [MaxLength(200)]
        public string? SignatoryDesignation { get; set; }

        /// <summary>Location used in the letter signature (Office.City.Name)</summary>
        [MaxLength(200)]
        public string? SignatoryLocation { get; set; }

        /// <summary>Auto-incremented letter count per application (1st, 2nd, 3rd…)</summary>
        public int Version { get; set; } = 1;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
