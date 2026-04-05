namespace RajFabAPI.DTOs
{
    public class DocumentUploadDto
    {
        public string DocumentUrl { get; set; } = string.Empty;
    }
    public class DocumentUploadRequest
    {
        public Guid ModuleId { get; set; }
        public string? ModuleDocType { get; set; } = "";
        public IFormFile File { get; set; }
    }
    public class UserDocumentsByModuleDto
    {
        public string ModuleName { get; set; } = string.Empty; // actual module name
        public List<DocumentListDto> Documents { get; set; } = new();
    }

    public class DocumentListDto
    {
        public string DocumentName { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;
        public string ModuleDocType { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public decimal Version { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class UserDocumentResultDto
{
    public List<UserDocumentsByModuleDto> CurrentDocuments { get; set; } = new();
    public List<UserDocumentsByModuleDto> OldDocuments { get; set; } = new();
}

}
