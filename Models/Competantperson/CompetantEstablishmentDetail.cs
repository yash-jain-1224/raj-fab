using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace RajFabAPI.Models.CompetentPerson
{


    public class CompetantEstablishmentDetail
    {
        public Guid Id { get; set; }

        public Guid RegistrationId { get; set; }

        public string EstablishmentName { get; set; } = null!;

        public string? Email { get; set; }
        public string? Mobile { get; set; }
        public string? Telephone { get; set; }

        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }

        public Guid? DistrictId { get; set; }
        public Guid? TehsilId { get; set; }
        public Guid? SdoId { get; set; }

        public string? Area { get; set; }
        public string? Pincode { get; set; }

        public CompetentPersonRegistration? Registration { get; set; }
    }

}
