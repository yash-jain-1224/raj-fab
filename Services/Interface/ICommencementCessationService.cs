using RajFabAPI.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RajFabAPI.Services.Interface
{
    public interface ICommencementCessationService
    {
        Task<List<CommencementCessationDto>> GetAllAsync();
        Task<CommencementCessationResDto?> GetByIdAsync(string id);
        Task<string> CreateAsync(CommencementCessationRequestDto request);
        Task<bool> UpdateStatusAndRemark(string registrationId, string status);
        Task<string> GenerateCommencementCessationPdf(CommencementCessationResDto dto);
        Task<string> GenerateCertificateAsync(CertificateRequestDto dto, Guid userId, string applicationId);
    }
}