using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
  public interface IOfficeLevelService
  {
    Task<IEnumerable<OfficeLevelDto>> GetAllAsync();
    Task<OfficeLevelDto?> GetByIdAsync(Guid id);
    Task<OfficeLevelDto> CreateAsync(CreateOfficeLevelDto dto);
    Task<OfficeLevelDto?> UpdateAsync(Guid id, CreateOfficeLevelDto dto);
    Task<bool> DeleteAsync(Guid id);
  }
}
