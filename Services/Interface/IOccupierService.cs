using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IOccupierService
    {
        Task<ApiResponseDto<List<OccupierDto>>> GetAllOccupiersAsync();
        Task<ApiResponseDto<OccupierDto>> GetOccupierByIdAsync(string id);
        Task<ApiResponseDto<OccupierDto>> GetOccupierByEmailAsync(string email);
        Task<ApiResponseDto<OccupierDto>> CreateOccupierAsync(CreateOccupierRequest request);
        Task<ApiResponseDto<OccupierDto>> UpdateOccupierAsync(string id, CreateOccupierRequest request);
        Task<ApiResponseDto<bool>> DeleteOccupierAsync(string id);
    }
}