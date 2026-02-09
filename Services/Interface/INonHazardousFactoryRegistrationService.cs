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
        Task<NonHazardousFactoryRegistrationDto> CreateAsync(CreateNonHazardousFactoryRegistrationRequest request);
        Task<bool> DeleteAsync(Guid id);
    }
}