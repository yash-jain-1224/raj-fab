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

        public string? ManufacturingProcess { get; set; }
        public string? FactoryType { get; set; }
        public string? Category { get; set; }
        public int? WorkerCount { get; set; }

        public List<string> Objections { get; set; } = new();

        public string? SignatureBase64 { get; set; }
        public string? SignatoryName { get; set; }
        public string? SignatoryDesignation { get; set; }
        public string? SignatoryLocation { get; set; }
    }

    // ── Factory Map Approval Objection Letter ─────────────────────────────────
    public class MapApprovalObjectionLetterDto
    {
        public string? ApplicationId { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;

        public string? EstablishmentName { get; set; }
        public string? EstablishmentAddress { get; set; }

        public string? Subject { get; set; }

        public string? PlantParticulars { get; set; }
        public string? ProductName { get; set; }
        public string? ManufacturingProcess { get; set; }
        public int? MaxWorkers { get; set; }
        public string? AreaOfPremise { get; set; }

        public List<string> Objections { get; set; } = new();

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
        public string? EstablishmentAddress { get; set; }

        public string? Subject { get; set; }

        public string? LicenseNumber { get; set; }
        public string? RegistrationNumber { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public int? NoOfYears { get; set; }

        public List<string> Objections { get; set; } = new();

        public string? SignatureBase64 { get; set; }
        public string? SignatoryName { get; set; }
        public string? SignatoryDesignation { get; set; }
        public string? SignatoryLocation { get; set; }
    }
}
