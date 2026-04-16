using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace RajFabAPI.Models.CompetentPerson
{
    public class CompetentPersonEquipment
    {
        public Guid Id { get; set; }

        public Guid EquipmentRegistrationId { get; set; }

        public Guid CompetentPersonId { get; set; }

        public string? EquipmentType { get; set; }

        public string? EquipmentName { get; set; }

        public string? IdentificationNumber { get; set; }

        public string? CalibrationCertificateNumber { get; set; }

        public DateTime? DateOfCalibration { get; set; }

        public DateTime? CalibrationValidity { get; set; }

        public string? CalibrationCertificatePath { get; set; }

        // Navigation
        public CompetentEquipmentRegistration? EquipmentRegistration { get; set; }
    }

}
