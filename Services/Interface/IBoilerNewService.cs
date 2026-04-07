using RajFabAPI.DTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services.Interface
{

    public interface IBoilerNewService
    {


        Task<Guid> SaveBoilerAsync(CreateRegisteredBoilerRequestDto dto, Guid userId, string type, Guid? boilerId = null);

        
        Task<RegisteredBoilerResponseDto?> GetByIdAsync(Guid id);

        Task<List<RegisteredBoilerResponseDto>> GetByUserIdAsync(Guid userId);
        Task<List<RegisteredBoilerResponseDto>> GetAllAsync();
        Task<RegisteredBoilerResponseDto?> UpdateAsync( Guid id, CreateRegisteredBoilerRequestDto dto, Guid userId);

        //Task<Guid> RenewBoilerAsync( RenewBoilerDto dto, Guid userId,Guid boilerId);

    }


}