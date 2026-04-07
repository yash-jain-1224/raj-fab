using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace RajFabAPI.Models.CompetentPerson
{


    public class CompetantOccupierDetail
    {
        public Guid Id { get; set; }

        public Guid RegistrationId { get; set; }

        public string Name { get; set; } = null!;
        public string? Designation { get; set; }
        public string? Relation { get; set; }

        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }

        public Guid? DistrictId { get; set; }
        public Guid? TehsilId { get; set; }
        public Guid? SdoId { get; set; }

        public string? City { get; set; }
        public string? Pincode { get; set; }

        public string? Email { get; set; }
        public string? Mobile { get; set; }
        public string? Telephone { get; set; }

        public CompetentPersonRegistration? Registration { get; set; }
    }

}
