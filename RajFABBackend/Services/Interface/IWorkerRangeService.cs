using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
  public interface IWorkerRangeService
  {
    Task<IEnumerable<WorkerRangeDto>> GetAllAsync();
    Task<WorkerRangeDto?> GetByIdAsync(Guid id);
    Task<WorkerRangeDto> CreateAsync(CreateWorkerRangeDto dto);
    Task<WorkerRangeDto?> UpdateAsync(Guid id, CreateWorkerRangeDto dto);
    Task<bool> DeleteAsync(Guid id);
  }
}