using RajFabAPI.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RajFabAPI.Services.Interface
{
    public interface INonHazardousFactoryRegistrationService
    {
        Task<List<NonHazardousFactoryRegistrationDto>> GetAllAsync();
        Task<NonHazardousApplicationResponseDto?> GetByIdAsync(Guid id);
        Task<string> CreateAsync(CreateNonHazardousFactoryRegistrationRequest request, Guid userId);
        Task<NonHazardousApplicationResponseDto> GetByApplicationNumberAsync(string applicationNumber);
        Task<string> UpdateAsync(Guid applicationId, CreateNonHazardousFactoryRegistrationRequest request, Guid userId);
        Task<bool> DeleteAsync(Guid id);
        Task<string> GenerateNonHazardousPdfAsync(Guid id);
        Task<string> GenerateNonHazardousObjectionLetter(Guid id, List<string> objections, string signatoryName, string designation, string location);
        Task<bool> UpdateStatusAndRemark(string Id, string status);
        Task<string> GenerateNonHazardousCertificateAsync(CertificateRequestDto dto, Guid userId, Guid id);
    }
}