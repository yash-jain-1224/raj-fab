using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace RajFabAPI.Models.BoilerModels
{

    public class WelderEmployer
    {
        public Guid Id { get; set; }

        public Guid WelderApplicationId { get; set; }

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

        // Navigation
        public WelderApplication? WelderApplication { get; set; }
    }

}


