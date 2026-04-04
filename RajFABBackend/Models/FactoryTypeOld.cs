namespace RajFabAPI.Models
{
    public class FactoryTypeOld
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public List<FactoryTypeDocument> RequiredDocuments { get; set; } = new List<FactoryTypeDocument>();
        public List<ManufacturingProcessType> AllowedProcessTypes { get; set; } = new List<ManufacturingProcessType>();
    }

    public class FactoryTypeDocument
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FactoryTypeId { get; set; } = string.Empty;
        public string DocumentTypeId { get; set; } = string.Empty;
        public bool IsRequired { get; set; } = true;
        public int Order { get; set; }
        public DocumentType? DocumentType { get; set; }
    }

    public class DocumentType
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FileTypes { get; set; } = ".pdf,.doc,.docx,.jpg,.jpeg,.png"; // Allowed file types
        public int MaxSizeMB { get; set; } = 25;
        public string Module { get; set; } = string.Empty; // Factory, Boiler, License, etc.
        public string ServiceType { get; set; } = string.Empty; // Registration, Renewal, Modification, Transfer
        public bool IsConditional { get; set; } = false; // Whether document requirement is conditional
        public string? ConditionalField { get; set; } // Field that determines if this document is required
        public string? ConditionalValue { get; set; } // Value that makes this document required
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    public class ManufacturingProcessType
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FactoryTypeId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool HasHazardousChemicals { get; set; } = false;
        public bool HasDangerousOperations { get; set; } = false;
        public int WorkerLimit { get; set; } = 50; // Default limit
        public List<ProcessDocument> RequiredDocuments { get; set; } = new List<ProcessDocument>();
    }

    public class ProcessDocument
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ManufacturingProcessTypeId { get; set; } = string.Empty;
        public string DocumentTypeId { get; set; } = string.Empty;
        public bool IsRequired { get; set; } = true;
        public string? ConditionalField { get; set; } // Field that determines if this document is required
        public string? ConditionalValue { get; set; } // Value that makes this document required
    }

    // Boiler specific document requirements
    public class BoilerDocumentType
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string BoilerServiceType { get; set; } = string.Empty; // registration, renewal, modification, transfer
        public string DocumentTypeId { get; set; } = string.Empty;
        public bool IsRequired { get; set; } = true;
        public string? ConditionalField { get; set; } // Field that determines if this document is required
        public string? ConditionalValue { get; set; } // Value that makes this document required
        public int OrderIndex { get; set; } = 0; // Display order
        public DocumentType? DocumentType { get; set; }
    }
}