using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IDocumentUploadService
    {
        Task<DocumentUploadDto> UploadAsync(IFormFile file, Guid userId, Guid moduleId, string moduleDocType);
        Task<UserDocumentResultDto> GetDocumentsByUserAsync(Guid userId);
    }
}


