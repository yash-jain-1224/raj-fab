using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IDocumentUploadService
    {
        Task<DocumentUploadDto> UploadAsync(IFormFile file, Guid userId);
    }
}


