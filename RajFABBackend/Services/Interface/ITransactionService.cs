// Services/ITransactionService.cs
using RajFabAPI.DTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services.Interface
{
    public interface ITransactionService
    {
        Task<IEnumerable<Transaction>> GetAllAsync();
        Task<Transaction?> GetByIdAsync(int id);
        Task<Transaction> AddAsync(CreateTransactionDto dto);
        Task<Transaction?> UpdateAsync(int id, UpdateTransactionDto dto);
        Task<Transaction?> GetByPrnAsync(string prn);
    }
}
