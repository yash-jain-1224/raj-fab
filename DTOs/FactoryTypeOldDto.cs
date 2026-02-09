namespace RajFabAPI.DTOs
{
    public class FactoryTypeOldDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<FactoryTypeDocumentDto> RequiredDocuments { get; set; } = new List<FactoryTypeDocumentDto>();
        public List<ManufacturingProcessTypeDto> AllowedProcessTypes { get; set; } = new List<ManufacturingProcessTypeDto>();
    }

    public class FactoryTypeDocumentDto
    {
        public string Id { get; set; } = string.Empty;
        public string DocumentTypeId { get; set; } = string.Empty;
        public string DocumentTypeName { get; set; } = string.Empty;
        public string DocumentTypeDescription { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public int Order { get; set; }
        public string FileTypes { get; set; } = string.Empty;
        public int MaxSizeMB { get; set; }
    }

    public class DocumentTypeDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FileTypes { get; set; } = string.Empty;
        public int MaxSizeMB { get; set; }
        public string Module { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public bool IsConditional { get; set; } = false;
        public string? ConditionalField { get; set; }
        public string? ConditionalValue { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ManufacturingProcessTypeDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool HasHazardousChemicals { get; set; }
        public bool HasDangerousOperations { get; set; }
        public int WorkerLimit { get; set; }
        public List<ProcessDocumentDto> RequiredDocuments { get; set; } = new List<ProcessDocumentDto>();
    }

    public class ProcessDocumentDto
    {
        public string Id { get; set; } = string.Empty;
        public string DocumentTypeId { get; set; } = string.Empty;
        public string DocumentTypeName { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public string? ConditionalField { get; set; }
        public string? ConditionalValue { get; set; }
    }

    public class CreateFactoryTypeRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> DocumentTypeIds { get; set; } = new List<string>();
        public List<CreateManufacturingProcessTypeRequest> ProcessTypes { get; set; } = new List<CreateManufacturingProcessTypeRequest>();
    }

    public class CreateDocumentTypeRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FileTypes { get; set; } = ".pdf,.doc,.docx,.jpg,.jpeg,.png";
        public int MaxSizeMB { get; set; } = 25;
        public string Module { get; set; } = string.Empty; // Factory, Boiler, License, etc.
        public string ServiceType { get; set; } = string.Empty; // Registration, Renewal, Modification, Transfer
        public bool IsConditional { get; set; } = false;
        public string? ConditionalField { get; set; }
        public string? ConditionalValue { get; set; }
    }

    // Boiler specific DTOs
    public class BoilerDocumentTypeDto
    {
        public string Id { get; set; } = string.Empty;
        public string BoilerServiceType { get; set; } = string.Empty;
        public string DocumentTypeId { get; set; } = string.Empty;
        public string DocumentTypeName { get; set; } = string.Empty;
        public string DocumentTypeDescription { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public string? ConditionalField { get; set; }
        public string? ConditionalValue { get; set; }
        public int OrderIndex { get; set; }
        public string FileTypes { get; set; } = string.Empty;
        public int MaxSizeMB { get; set; }
    }

    public class CreateBoilerDocumentTypeRequest
    {
        public string BoilerServiceType { get; set; } = string.Empty;
        public string DocumentTypeId { get; set; } = string.Empty;
        public bool IsRequired { get; set; } = true;
        public string? ConditionalField { get; set; }
        public string? ConditionalValue { get; set; }
        public int OrderIndex { get; set; } = 0;
    }

    public class CreateManufacturingProcessTypeRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool HasHazardousChemicals { get; set; }
        public bool HasDangerousOperations { get; set; }
        public int WorkerLimit { get; set; } = 50;
        public List<CreateProcessDocumentRequest> RequiredDocuments { get; set; } = new List<CreateProcessDocumentRequest>();
    }

    public class CreateProcessDocumentRequest
    {
        public string DocumentTypeId { get; set; } = string.Empty;
        public bool IsRequired { get; set; } = true;
        public string? ConditionalField { get; set; }
        public string? ConditionalValue { get; set; }
    }
}