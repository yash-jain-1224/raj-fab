using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IFactoryTypeNewService
    {
        Task<IEnumerable<FactoryTypeNewDto>> GetAllAsync();
        Task<FactoryTypeNewDto?> GetByIdAsync(Guid id);
        Task<FactoryTypeNewDto> CreateAsync(CreateFactoryTypeNewRequest dto);
        Task<FactoryTypeNewDto?> UpdateAsync(Guid id, CreateFactoryTypeNewRequest dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
