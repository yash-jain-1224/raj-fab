namespace RajFabAPI.Models
{
    public class FormField
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Type { get; set; } = "text"; // text, email, tel, number, date, select, radio, checkbox, textarea, file
        public bool Required { get; set; } = false;
        public string? Placeholder { get; set; }
        public List<string>? Options { get; set; } // For select, radio, checkbox
        public FieldValidation? Validation { get; set; }
        public int Order { get; set; }
        public string? SectionId { get; set; } // For organizing fields into sections
        public FormFieldApiConfig? ApiConfig { get; set; }
    }

    public class FieldValidation
    {
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string? Pattern { get; set; }
        public string? Message { get; set; }
    }

    public class FormFieldApiConfig
    {
        public string? Endpoint { get; set; }
        public string? Method { get; set; } = "GET";
        public string? TriggerOn { get; set; } = "change";
        public List<string>? DependsOn { get; set; }
    }
}