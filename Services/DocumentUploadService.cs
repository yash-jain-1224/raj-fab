using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
    public class DocumentUploadService : IDocumentUploadService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _environment;

        public DocumentUploadService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration config,
            IWebHostEnvironment environment)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _config = config;
            _environment = environment;
        }

        public async Task<DocumentUploadDto> UploadAsync(IFormFile file, Guid userId)
        {
            if (userId == Guid.Empty)
                throw new InvalidOperationException("Invalid user");

            if (file == null || file.Length == 0)
                throw new InvalidOperationException("Invalid file");

            var allowedTypes = new[] { "application/pdf", "image/jpeg", "image/png" };

            if (!allowedTypes.Contains(file.ContentType))
                throw new InvalidOperationException("Unsupported file type");

            if (file.Length > 10 * 1024 * 1024)
                throw new InvalidOperationException("File size exceeds limit");

            var documentId = Guid.NewGuid();
            var originalFileName = Path.GetFileName(file.FileName);
            var safeFileName = $"{documentId}_{originalFileName}";

            var uploadPath = Path.Combine(_environment.WebRootPath, "documents");
            Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, safeFileName);

            await using var stream = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                81920,
                useAsync: true);

            await file.CopyToAsync(stream);

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");

            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host.Value}";
            var fileUrl = $"{baseUrl}/documents/{safeFileName}";

            var entity = new DocumentUpload
            {
                Id = documentId.ToString(),
                UserId = userId.ToString(),
                DocumentName = originalFileName,
                DocumentType = file.ContentType,
                DocumentSize = file.Length,
                DocumentUrl = fileUrl,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.DocumentUploads.Add(entity);
            await _context.SaveChangesAsync();

            return new DocumentUploadDto
            {
                DocumentUrl = fileUrl
            };
        }

    }
}
