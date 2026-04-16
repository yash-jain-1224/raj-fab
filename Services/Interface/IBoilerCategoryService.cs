using RajFabAPI.DTOs;
namespace RajFabAPI.Services.Interface
{
  public interface IBoilerCategoryService
  {
        Task<BoilerCategoryDto> CreateAsync(CreateBoilerCategoryDto dto);

        Task<BoilerCategoryDto?> GetByIdAsync(int id);
        Task<BoilerCategoryDto?> UpdateAsync(int id, CreateBoilerCategoryDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<BoilerCategoryDto>> GetAllAsync();
  }
}
