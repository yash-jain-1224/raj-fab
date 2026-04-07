using RajFabAPI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RajFabAPI.DTOs
{
    public class CreateFactoryLicenseDto
    {
        [Required]
        [MaxLength(100)]
        public string FactoryRegistrationNumber { get; set; } = string.Empty;

        [Required]
        public DateTime ValidFrom { get; set; }

        [Required]
        public DateTime ValidTo { get; set; }

        public int? NoOfYears { get; set; }

        // Workers employed
        public int? WorkersProposedMale { get; set; }
        public int? WorkersProposedFemale { get; set; }
        public int? WorkersProposedTransgender { get; set; }
        public int? WorkersLastYearMale { get; set; }
        public int? WorkersLastYearFemale { get; set; }
        public int? WorkersLastYearTransgender { get; set; }
        public int? WorkersOrdinaryMale { get; set; }
        public int? WorkersOrdinaryFemale { get; set; }
        public int? WorkersOrdinaryTransgender { get; set; }

        // Power details
        public decimal? SanctionedLoad { get; set; }
        public string? SanctionedLoadUnit { get; set; }

        // Manufacturing process
        public string? ManufacturingProcessLast12Months { get; set; }
        public string? ManufacturingProcessNext12Months { get; set; }
        public string? DateOfStartProduction { get; set; }

        // JSON snapshots — serialized on frontend, stored as-is, parsed on approval
        public string? FactoryData { get; set; }
        public string? MapApprovalData { get; set; }
    }
    public class FactoryLicenseData
    {
        public FactoryLicense FactoryLicense { get; set; }
        public EstablishmentRegistrationDetailsDto EstFullDetails { get; set; }
        public FactoryMapApproval MapApprovalDetails { get; set; }
        public List<ApplicationHistoryDto> ApplicationHistory { get; set; } = new();
        public string? CertificatePDFUrl { get; set; }
    }

    public class FactoryLicenseCertificateRequestDto
    {
        public string? Remarks { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string? Place { get; set; }
        public string? Signature { get; set; }
        public string? IssuedAt { get; set; }
    }

    // ─── JSON helper classes — used to deserialize FactoryData / MapApprovalData on approval ───

    /// <summary>
    /// Deserialised from FactoryLicense.FactoryData JSON.
    /// Matches the object built on the frontend from estData.
    /// </summary>
    public class FactoryDataJson
    {
        [JsonPropertyName("factoryName")]
        public string? FactoryName { get; set; }

        [JsonPropertyName("managerDetail")]
        public PersonShortJson? ManagerDetail { get; set; }

        [JsonPropertyName("employerDetail")]
        public PersonShortJson? EmployerDetail { get; set; }
    }

    /// <summary>
    /// Matches PersonShortDto shape sent from frontend.
    /// </summary>
    public class PersonShortJson
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("designation")]
        public string? Designation { get; set; }

        [JsonPropertyName("addressLine1")]
        public string? AddressLine1 { get; set; }

        [JsonPropertyName("addressLine2")]
        public string? AddressLine2 { get; set; }

        [JsonPropertyName("district")]
        public string? District { get; set; }

        [JsonPropertyName("tehsil")]
        public string? Tehsil { get; set; }

        [JsonPropertyName("area")]
        public string? Area { get; set; }

        [JsonPropertyName("pincode")]
        public string? Pincode { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("telephone")]
        public string? Telephone { get; set; }

        [JsonPropertyName("mobile")]
        public string? Mobile { get; set; }
    }

    /// <summary>
    /// Deserialised from FactoryLicense.MapApprovalData JSON.
    /// </summary>
    public class MapApprovalDataJson
    {
        /// <summary>
        /// The full premiseOwnerDetails object — re-serialised back into
        /// FactoryMapApproval.PremiseOwnerDetails on approval.
        /// </summary>
        [JsonPropertyName("premiseOwnerDetails")]
        public System.Text.Json.JsonElement? PremiseOwnerDetails { get; set; }
    }
}
