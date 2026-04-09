using RajFabAPI.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RajFabAPI.Services.Interface
{
    public interface INonHazardousFactoryRegistrationService
    {
        Task<List<NonHazardousFactoryRegistrationDto>> GetAllAsync();
        Task<NonHazardousFactoryRegistrationDto?> GetByIdAsync(Guid id);
        Task<NonHazardousFactoryRegistrationDto> CreateAsync(CreateNonHazardousFactoryRegistrationRequest request, Guid userId);

        Task<NonHazardousApplicationResponseDto> GetByApplicationIdAsync(string applicationId);
        Task<NonHazardousFactoryRegistrationDto> UpdateAsync(string applicationId, CreateNonHazardousFactoryRegistrationRequest request, Guid userId);
        Task<bool> DeleteAsync(Guid id);
    }
}