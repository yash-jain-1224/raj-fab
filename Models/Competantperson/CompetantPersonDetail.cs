using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace RajFabAPI.Models.CompetentPerson
{


    public class CompetantPersonDetail
    {
        public Guid Id { get; set; }

        public Guid RegistrationId { get; set; }

        public string Name { get; set; } = null!;
        public string? FatherName { get; set; }

        public DateTime? DOB { get; set; }

        public string? Address { get; set; }

        public string? Email { get; set; }
        public string? Mobile { get; set; }

        public int? Experience { get; set; }

        public string? Qualification { get; set; }
        public string? Engineering { get; set; }

        public string? PhotoPath { get; set; }
        public string? SignPath { get; set; }
        public string? AttachmentPath { get; set; }

        public CompetentPersonRegistration? Registration { get; set; }
    }

}
