using RajFabAPI.DTOs;
namespace RajFabAPI.Services.Interface
{
  public interface IFactoryCategoryService
  {
    Task<IEnumerable<FactoryCategoryDto>> GetAllAsync();
    Task<FactoryCategoryDto?> GetByIdAsync(Guid id);
    Task<FactoryCategoryDto> CreateAsync(CreateFactoryCategoryDto dto);
    Task<FactoryCategoryDto?> UpdateAsync(Guid id, CreateFactoryCategoryDto dto);
    Task<bool> DeleteAsync(Guid id);
  }
}
