using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models.BoilerModels
{
   

    public class BoilerDetail
    {
        public Guid Id { get; set; }


        // ✅ FK to BoilerRegistration
        public Guid BoilerRegistrationId { get; set; }

        [ForeignKey(nameof(BoilerRegistrationId))]
       
       
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? SubDivisionId { get; set; }
        public Guid? TehsilId { get; set; }
        public String? Area { get; set; }
        public int? PinCode { get; set; }
        public int? RenewalYears { get; set; }
        public DateTime? ValidUpto { get; set; } // calculated expiry
        public string? Telephone { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public int? ErectionTypeId { get; set; }

        /* ===== BOILER TECHNICAL ===== */
        public string? MakerNumber { get; set; }
        public int? YearOfMake { get; set; }
        public decimal? HeatingSurfaceArea { get; set; }

        public decimal? EvaporationCapacity { get; set; }
        public string? EvaporationUnit { get; set; }

        public decimal? IntendedWorkingPressure { get; set; }
        public string? PressureUnit { get; set; }

        public int? BoilerType { get; set; }
        public int? BoilerCategory { get; set; }

        public bool? Superheater { get; set; }
        public decimal? SuperheaterOutletTemp { get; set; }

        public bool? Economiser { get; set; }
        public decimal? EconomiserOutletTemp { get; set; }

        public int? FurnaceType { get; set; }

        /* ===== DOCUMENTS ===== */
        public string? DrawingsPath { get; set; }
        public string? SpecificationPath { get; set; }
        public string? FormI_B_CPath { get; set; }
        public string? FormI_DPath { get; set; }
        public string? FormI_EPath { get; set; }
        public string? FormIV_APath { get; set; }
        public string? FormV_APath { get; set; }
        public string? TestCertificatesPath { get; set; }
        public string? WeldRepairChartsPath { get; set; }
        public string? PipesCertificatesPath { get; set; }
        public string? TubesCertificatesPath { get; set; }
        public string? CastingCertificatePath { get; set; }
        public string? ForgingCertificatePath { get; set; }
        public string? HeadersCertificatePath { get; set; }
        public string? DishedEndsInspectionPath { get; set; }
        public string? BoilerAttendantCertificatePath { get; set; }
        public string? BoilerOperationEngineerCertificatePath { get; set; }

       
    }

}
