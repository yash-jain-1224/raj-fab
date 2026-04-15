namespace RajFabAPI.DTOs
{
    // ── Establishment Registration Objection Letter ────────────────────────────
    public class EstablishmentObjectionLetterDto
    {
        public string? ApplicationId { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;

        public string? EstablishmentName { get; set; }
        public string? EstablishmentAddress { get; set; }

        public string? Subject { get; set; }

        public string? ManufacturingType { get; set; }
        public string? ManufacturingProcess { get; set; }
        public string? FactoryType { get; set; }
        public string? Category { get; set; }
        public int? WorkerCount { get; set; }

        //public string Objections { get; set; }
        public string Objections { get; set; }

        public string? SignatureBase64 { get; set; }
        public string? SignatoryName { get; set; }
        public string? SignatoryDesignation { get; set; }
        public string? SignatoryLocation { get; set; }
    }

    public class DocumentStateDto
    {
        public bool Checked { get; set; }
        public string Remark { get; set; }
    }

    // ── Factory Plan Approval Objection Letter ─────────────────────────────────
    public class MapApprovalObjectionLetterDto
    {
        public string? ApplicationId { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;

        public string? EstablishmentName { get; set; }
        public string? FactoryDetails { get; set; }
        public string? EstablishmentAddress { get; set; }

        public string? Subject { get; set; }

        public string? PlantParticulars { get; set; }
        public string? ProductName { get; set; }
        public string? ManufacturingProcess { get; set; }
        public int? MaxWorkers { get; set; }
        public string? AreaOfPremise { get; set; }

        public string Objections { get; set; }

        public string? SignatureBase64 { get; set; }
        public string? SignatoryName { get; set; }
        public string? SignatoryDesignation { get; set; }
        public string? SignatoryLocation { get; set; }
    }

    // ── Factory License Objection Letter ──────────────────────────────────────
    public class LicenseObjectionLetterDto
    {
        public string? ApplicationId { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;

        public string? EstablishmentName { get; set; }
        public string? FactoryAddress { get; set; }

        public string? Subject { get; set; }

        public string? LicenseNumber { get; set; }
        public string? RegistrationNumber { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public int? NoOfYears { get; set; }
        public decimal? SanctionLoad { get; set; }
        public string? SanctionLoadUnit { get; set; }

        // Factory details
        public string? ManufacturingProcess { get; set; }   // from FactoryDetail table
        public int? MaxWorkers { get; set; }                // from FactoryMapApproval table
        public string? FactoryTypeName { get; set; }        // from FactoryMapApproval table

        public string Objections { get; set; }

        public string? SignatureBase64 { get; set; }
        public string? SignatoryName { get; set; }
        public string? SignatoryDesignation { get; set; }
        public string? SignatoryLocation { get; set; }
    }

    public class ManagerChangeObjectionLetterDto
    {
        public string Objections { get; set; }
        public string? SignatoryName { get; set; }
        public string? SignatoryDesignation { get; set; }
        public string? SignatoryLocation { get; set; }
        public ManagerChangeGetResponseDto ManagerChangeData { get; set; }
    }

    public class BoilerObjectionLetterDto
    {
        public string? ApplicationId { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;

        public string? OwnerName { get; set; }   // optional (if available)
        public string? Address { get; set; }

        public string? BoilerRegistrationNo { get; set; }

        // Boiler details
        public string? BoilerType { get; set; }
        public string? BoilerCategory { get; set; }
        public decimal? HeatingSurfaceArea { get; set; }
        public decimal? EvaporationCapacity { get; set; }
        public decimal? WorkingPressure { get; set; }
        public int? YearOfMake { get; set; }

        public string Objections { get; set; }
        // Signature
        public string? SignatureBase64 { get; set; }
        public string? SignatoryName { get; set; }
        public string? SignatoryDesignation { get; set; }
        public string? SignatoryLocation { get; set; }
    }
}
