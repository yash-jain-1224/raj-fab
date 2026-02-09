using RajFabAPI.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RajFabAPI.Services.Interface
{
    public interface ICommencementCessationService
    {
        Task<List<CommencementCessationDto>> GetAllAsync();
        Task<CommencementCessationDto?> GetByIdAsync(string id);
        Task<CommencementCessationDto> CreateAsync(CommencementCessationRequestDto request);
        Task<bool> UpdateStatusAndRemark(string registrationId, string status);
    }
}