using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _db;

        public TransactionService(ApplicationDbContext db)
        {
            _db = db;
        }

        // Get all transactions
        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            return await _db.Transactions
                .AsNoTracking()
                .ToListAsync();
        }

        // Get transaction by ID
        public async Task<Transaction?> GetByIdAsync(int id)
        {
            return await _db.Transactions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        // Create a new transaction
        public async Task<Transaction> AddAsync(CreateTransactionDto dto)
        {
            var transaction = new Transaction
            {
                PrnNumber = dto.PrnNumber,
                ModuleId = dto.ModuleId,
                UserId = dto.UserId,
                Amount = dto.Amount,
                PaidAmount = dto.PaidAmount,
                Status = dto.Status ?? "Pending",
                ApplicationId = dto.ApplicationId,
                PaymentReq = dto.PaymentReq,
                PaymentRes = dto.PaymentRes,
                CreatedAt = DateTime.UtcNow,
                Remarks = "Payment Initiated"
            };

            _db.Transactions.Add(transaction);
            await _db.SaveChangesAsync();

            return transaction;
        }

        // Update a transaction
        public async Task<Transaction?> UpdateAsync(int id, UpdateTransactionDto dto)
        {
            var transaction = await _db.Transactions.FirstOrDefaultAsync(t => t.Id == id);
            if (transaction == null)
                return null;

            transaction.PrnNumber = dto.PrnNumber;
            transaction.ModuleId = dto.ModuleId;
            transaction.UserId = dto.UserId;
            transaction.Amount = dto.Amount;
            transaction.PaidAmount = dto.PaidAmount;
            transaction.Status = dto.Status;
            transaction.ApplicationId = dto.ApplicationId;
            transaction.PaymentReq = dto.PaymentReq;
            transaction.PaymentRes = dto.PaymentRes;
            transaction.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return transaction;
        }
        public async Task<Transaction?> GetByPrnAsync(string prn)
        {
            return await _db.Transactions
                .FirstOrDefaultAsync(t => t.PrnNumber == prn);
        }

    }
}
