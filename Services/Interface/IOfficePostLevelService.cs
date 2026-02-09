using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
  public interface IOfficePostLevelService
  {
    Task<IEnumerable<OfficePostLevelResponseDto>> GetByOfficeAsync(Guid officeId);
    Task<bool> AssignAsync(AssignOfficePostLevelDto dto);
    Task<bool> DeleteAsync(Guid id);
  }
}
