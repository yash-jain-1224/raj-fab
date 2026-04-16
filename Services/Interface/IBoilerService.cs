using RajFabAPI.DTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services.Interface
{
    public interface IBoilerService
    {
        // Application methods
        Task<BoilerApplication> RegisterBoilerAsync(BoilerRegistrationDto dto);
        Task<BoilerApplication> RenewCertificateAsync(BoilerRenewalDto dto);
        Task<BoilerApplication> ModifyBoilerAsync(BoilerModificationDto dto);
        Task<BoilerApplication> TransferBoilerAsync(BoilerTransferDto dto);
        
        // Query methods
        Task<PagedResult<RegisteredBoiler>> GetAllBoilersAsync(int page, int pageSize);
        Task<RegisteredBoiler?> GetBoilerByRegistrationNumberAsync(string registrationNumber);
        Task<PagedResult<BoilerApplication>> GetApplicationsAsync(string? status, int page, int pageSize);
        Task<BoilerApplication?> GetApplicationByNumberAsync(string applicationNumber);
        
        // Status management
        Task<BoilerApplication> UpdateApplicationStatusAsync(string applicationNumber, string status, string? comments, string? processedBy);
        
        // Document management
        Task<string> UploadDocumentAsync(string applicationNumber, IFormFile file, string documentType);
        
        // Inspection methods
        Task<List<BoilerInspectionHistory>> GetInspectionHistoryAsync(Guid boilerId);
        Task<BoilerInspectionHistory> AddInspectionRecordAsync(Guid boilerId, BoilerInspectionDto dto);
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }
}