using RajFabAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    public class CreateBoilerRegistrationDto
    {

        public string? TransferType { get; set; } // SameState / OtherState

        public string? OldRegistrationNo { get; set; }
        public string? OldStateName { get; set; }

        public PersonDetailDto? OwnerDetail { get; set; }
        public PersonDetailDto? MakerDetail { get; set; }

        public BoilerTechnicalDto? BoilerDetail { get; set; }
    }

    public class RenewalBoilerDto
    {
        public string BoilerRegistrationNo { get; set; } = string.Empty;

        public int RenewalYears { get; set; }

        public string? BoilerAttendantCertificatePath { get; set; }
        public string? BoilerOperationEngineerCertificatePath { get; set; }
    }

    public class CertificateResult
    {
        public string RegistrationNumber { get; set; }
        public string CertificateUrl { get; set; }
    }

    public class GetBoilerResponseDto
    {
        public Guid Id { get; set; }
        public string? ApplicationId { get; set; }
        public string? BoilerRegistrationNo { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
        public decimal Version { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? ApplicationPDFUrl { get; set; }
        public string? CertificateUrl { get; set; }

        public BoilerTechnicalDto? BoilerDetail { get; set; }

        public PersonDetailDto? Owner { get; set; }
        public PersonDetailDto? Maker { get; set; }

        public List<ApplicationHistory> ApplicationHistory { get; set; } = new();
    }

    public class BoilerTechnicalDto
    {
        /* ===== FACTORY ===== */
      

        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? SubDivisionId { get; set; }
        public Guid? TehsilId { get; set; }
        public String? Area { get; set; }
        public int? PinCode { get; set; }
        public int? RenewalYears { get; set; }
        public string? Telephone { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public int? ErectionTypeId { get; set; }

        /* ===== BOILER ===== */
        public string? MakerNumber { get; set; }
        public int? YearOfMake { get; set; }
        public decimal? HeatingSurfaceArea { get; set; }

        public decimal? EvaporationCapacity { get; set; }
        public string? EvaporationUnit { get; set; }

        public decimal? IntendedWorkingPressure { get; set; }
        public string? PressureUnit { get; set; }

        public int? BoilerTypeID { get; set; }
        public int? BoilerCategoryID { get; set; }

        public bool? Superheater { get; set; }
        public decimal? SuperheaterOutletTemp { get; set; }

        public bool? Economiser { get; set; }
        public decimal? EconomiserOutletTemp { get; set; }

        public int? FurnaceTypeID { get; set; }

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
    public class CreateBoilerClosureDto
    {
        public string BoilerRegistrationNo { get; set; } = null!;

        public string ClosureType { get; set; } = null!;
        public DateTime ClosureDate { get; set; }

        public string? ToStateName { get; set; }
        public string? Reasons { get; set; }
        public string? Remarks { get; set; }

        public string? ClosureReportPath { get; set; }
    }

    public class UpdateBoilerClosureDto
    {
        public string? ClosureType { get; set; }
        public DateTime? ClosureDate { get; set; }
        public string? ToStateName { get; set; }
        public string? Reasons { get; set; }
        public string? Remarks { get; set; }
        public string? ClosureReportPath { get; set; }
    }
    
    public class BoilerClosureResponseDto
    {
        public string ApplicationId { get; set; } = null!;
        public string BoilerRegistrationNo { get; set; } = null!;

        public string ClosureType { get; set; } = null!;
        public DateTime ClosureDate { get; set; }
        public string? ToStateName { get; set; }

        public string? Reasons { get; set; }
        public string? Remarks { get; set; }
        public string? ClosureReportPath { get; set; }

        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
    public class CreateBoilerRepairDto
{
    public string BoilerRegistrationNo { get; set; } = null!;
    public string? RenewalApplicationId { get; set; } = null;
    public string RepairType { get; set; } = null!; // Repair / Modification / Both

    public PersonDetailDto ?RepairerDetail { get; set; } = null!;

    public string? AttendantCertificatePath { get; set; }
    public string? OperationEngineerCertificatePath { get; set; }
    public string? RepairDocumentPath { get; set; }
}

public class BoilerRepairResponseDto
{
    public string ApplicationId { get; set; } = null!;
    public string BoilerRegistrationNo { get; set; } = null!;
    public string RepairType { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public PersonDetailDto Repairer { get; set; } = null!;
}

    public class GetBoilerRepairDto
    {
       
        public string? ApplicationId { get; set; }
        public string? BoilerRegistrationNo { get; set; }
        public string? RenewalApplicationId { get; set; }
        public string? RepairType { get; set; }
        public string? Status { get; set; }

        public string? AttendantCertificatePath { get; set; }
        public string? OperationEngineerCertificatePath { get; set; }
        public string? RepairDocumentPath { get; set; }

        public DateTime CreatedAt { get; set; }

        public PersonDetailDto? Repairer { get; set; }
    }

    public class UpdateBoilerRepairDto
    {
        public string? RepairType { get; set; }

        public string? AttendantCertificatePath { get; set; }
        public string? OperationEngineerCertificatePath { get; set; }
        public string? RepairDocumentPath { get; set; }

        public PersonDetailDto? RepairerDetail { get; set; }
    }

    public class BoilerCertificateDto
    {
        public string? ApplicationId { get; set; }
        public string? BoilerRegistrationNo { get; set; }

        public string? BoilerType { get; set; }
        public decimal? HeatingSurfaceArea { get; set; }
        public string? YearOfMake { get; set; }

        public string? EvaporationCapacity { get; set; }

        public string? OwnerName { get; set; }
        public string? Address { get; set; }

        public string? Repairs { get; set; }
        public string? Remarks { get; set; }

        public decimal? MaxPressure { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public decimal? SafetyValvePressure { get; set; }

        public decimal? Fee { get; set; }
    }
    public class BoilerCertificateRequestDto
    {
        public string? Remarks { get; set; }
    }
}
