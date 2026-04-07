using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{




    public class BoilerRepairerCreateDto
    {
        public string? FactoryRegistrationNo { get; set; }
        public string? BrClassification { get; set; }

        // JSON coming from UI
        public string? EstablishmentJson { get; set; }

        public string? JobsExecutedJson { get; set; }
        public string? DocumentEvidence { get; set; }

        public string? ApprovalHistoryJson { get; set; }
        public string? RejectedHistoryJson { get; set; }

        public bool? ToolsAvailable { get; set; }
        public int? SimultaneousSites { get; set; }

        public bool? AcceptsRegulations { get; set; }
        public bool? AcceptsResponsibility { get; set; }
        public bool? CanSupplyMaterial { get; set; }

        // Inhouse / Outsourced / Not-Available
        public string? QualityControlType { get; set; }

        // Address/details if outsourced
        public string? QualityControlDetailsjson { get; set; }

        public List<BoilerRepairerEngineerDto>? Engineers { get; set; }
        public List<BoilerRepairerWelderDto>? Welders { get; set; }
    }
    public class BoilerRepairerEngineerDto
    {
        public string Name { get; set; } = null!;
        public string Designation { get; set; } = null!;
        public string Qualification { get; set; } = null!;
        public int ExperienceYears { get; set; }

        public string? DocumentPath { get; set; }
    }

    public class BoilerRepairerWelderDto
    {
        public string Name { get; set; } = null!;
        public string Designation { get; set; } = null!;
        public int ExperienceYears { get; set; }

        public string? CertificatePath { get; set; }
    }

    public class BoilerRepairerDetailsDto
    {
        public string? ApplicationId { get; set; }
        public string? RepairerRegistrationNo { get; set; }

        public string? FactoryRegistrationNo { get; set; }
        public string? BrClassification { get; set; }

        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUpto { get; set; }

        public string? EstablishmentJson { get; set; }
        public string? JobsExecutedJson { get; set; }
        public string? DocumentEvidence { get; set; }

        public int? ApprovalHistoryJson { get; set; }
        public int? RejectedHistoryJson { get; set; }

        public bool? ToolsAvailable { get; set; }
        public int? SimultaneousSites { get; set; }

        public bool? AcceptsRegulations { get; set; }
        public bool? AcceptsResponsibility { get; set; }
        public bool? CanSupplyMaterial { get; set; }

        public string? QualityControlType { get; set; }
        public string? QualityControlDetailsjson { get; set; }

        public string? Status { get; set; }
        public string? Type { get; set; }
        public decimal Version { get; set; }

        public List<BoilerRepairerEngineerDto>? Engineers { get; set; }
        public List<BoilerRepairerWelderDto>? Welders { get; set; }
    }

    public class BoilerRepairerResponseDto
    {
  

        public string ApplicationId { get; set; } = string.Empty;
        public string RepairerRegistrationNo { get; set; } = string.Empty;

        public string? FactoryRegistrationNo { get; set; }
        public string? BrClassification { get; set; }

        public string? EstablishmentJson { get; set; }
        public string? JobsExecutedJson { get; set; }
        public string? DocumentEvidence { get; set; }

        public string? ApprovalHistoryJson { get; set; }
        public string? RejectedHistoryJson { get; set; }

        public bool? ToolsAvailable { get; set; }
        public int? SimultaneousSites { get; set; }

        public bool? AcceptsRegulations { get; set; }
        public bool? AcceptsResponsibility { get; set; }
        public bool? CanSupplyMaterial { get; set; }

        public string? QualityControlType { get; set; }
        public string? QualityControlDetailsjson { get; set; }

        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUpto { get; set; }

        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Version { get; set; }

        public List<BoilerRepairerEngineerDto>? Engineers { get; set; }
        public List<BoilerRepairerWelderDto>? Welders { get; set; }
    }

    public class BoilerRepairerRenewalDto
    {
        public string RepairerRegistrationNo { get; set; } = null!;
        public int RenewalYears { get; set; }
    }

    public class BoilerRepairerClosureDto
    {
        public string RepairerRegistrationNo { get; set; } = null!;
        public string ClosureReason { get; set; } = null!;
        public DateTime ClosureDate { get; set; }

        public string? Remarks { get; set; }
        public string? DocumentPath { get; set; }
    }

}
