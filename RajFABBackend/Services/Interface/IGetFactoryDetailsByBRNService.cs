// Services/IActService.cs
using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IGetFactoryDetailsByBRNService
    {
        Task<BRNResponse?> GetFactoryDetailsByBRNNo(string BRNNumber);
    }
}
