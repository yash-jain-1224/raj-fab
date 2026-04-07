using Microsoft.EntityFrameworkCore;
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

        public async Task<UserDocumentResultDto> GetDocumentsByUserAsync(Guid userId)
{
    if (userId == Guid.Empty)
        throw new InvalidOperationException("Invalid user");

    var documents = await _context.DocumentUploads
        .Where(d => d.UserId == userId)
        .Join(
            _context.Modules,
            doc => doc.ModuleId,
            mod => mod.Id,
            (doc, mod) => new
            {
                ModuleName = mod.Name,
                doc.DocumentName,
                doc.DocumentUrl,
                doc.DocumentType,
                doc.ModuleDocType,
                doc.Version,
                doc.CreatedAt
            })
        .ToListAsync();

    var currentDocs = documents
        .GroupBy(d => new { d.ModuleName, d.ModuleDocType })
        .Select(g => g.OrderByDescending(x => x.Version).First())
        .ToList();

    var oldDocs = documents
        .Except(currentDocs)
        .ToList();

    return new UserDocumentResultDto
    {
        CurrentDocuments = currentDocs
            .GroupBy(d => d.ModuleName)
            .Select(g => new UserDocumentsByModuleDto
            {
                ModuleName = g.Key,
                Documents = g.Select(d => new DocumentListDto
                {
                    DocumentName = d.DocumentName,
                    DocumentUrl = d.DocumentUrl,
                    DocumentType = d.DocumentType,
                    ModuleDocType = d.ModuleDocType,
                    Version = d.Version,
                    CreatedAt = d.CreatedAt
                }).ToList()
            }).ToList(),

        OldDocuments = oldDocs
            .GroupBy(d => d.ModuleName)
            .Select(g => new UserDocumentsByModuleDto
            {
                ModuleName = g.Key,
                Documents = g.Select(d => new DocumentListDto
                {
                    DocumentName = d.DocumentName,
                    DocumentUrl = d.DocumentUrl,
                    DocumentType = d.DocumentType,
                    ModuleDocType = d.ModuleDocType,
                    Version = d.Version,
                    CreatedAt = d.CreatedAt
                }).ToList()
            }).ToList()
    };
}

        public async Task<DocumentUploadDto> UploadAsync(IFormFile file, Guid userId, Guid moduleId, string moduleDocType)
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

            // --- Determine version ---
            var lastDoc = await _context.DocumentUploads
                .Where(d =>
                    d.UserId == userId &&
                    d.ModuleId == moduleId &&
                    d.ModuleDocType == moduleDocType)
                .OrderByDescending(d => d.Version)
                .FirstOrDefaultAsync();

            var newVersion = lastDoc != null ? lastDoc.Version + 1 : 1.0m;

            var entity = new DocumentUpload
            {
                UserId = userId,
                ModuleId = moduleId,
                ModuleDocType = moduleDocType,
                Version = newVersion,
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
