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

        // Get transaction by PRN
        public async Task<Transaction?> GetByPrnAsync(string prn)
        {
            return await _db.Transactions
                .FirstOrDefaultAsync(t => t.PrnNumber == prn);
        }

        // Create a new transaction
        public async Task<Transaction> AddAsync(CreateTransactionDto dto)
        {
            if (!Guid.TryParse(dto.ModuleId, out Guid moduleGuid))
                throw new ArgumentException("Invalid ModuleId");

            if (!Guid.TryParse(dto.UserId, out Guid userGuid))
                throw new ArgumentException("Invalid UserId");

            var transaction = new Transaction
            {
                PrnNumber = dto.PrnNumber,
                ModuleId = moduleGuid,
                UserId = userGuid,
                Amount = dto.Amount,
                PaidAmount = dto.PaidAmount ?? 0,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Pending" : dto.Status,
                ApplicationId = dto.ApplicationId,
                PaymentReq = dto.PaymentReq,
                PaymentRes = dto.PaymentRes,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Remarks = dto.Remarks ?? "Payment Initiated",
                Message = dto.Message ?? string.Empty
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

            if (!Guid.TryParse(dto.ModuleId, out Guid moduleGuid))
                throw new ArgumentException("Invalid ModuleId");

            if (!Guid.TryParse(dto.UserId, out Guid userGuid))
                throw new ArgumentException("Invalid UserId");

            transaction.PrnNumber = dto.PrnNumber ?? transaction.PrnNumber;
            transaction.ModuleId = moduleGuid;
            transaction.UserId = userGuid;
            transaction.Amount = dto.Amount;
            transaction.PaidAmount = dto.PaidAmount ?? transaction.PaidAmount;
            transaction.Status = string.IsNullOrWhiteSpace(dto.Status) ? transaction.Status : dto.Status;
            transaction.ApplicationId = dto.ApplicationId ?? transaction.ApplicationId;
            transaction.PaymentReq = dto.PaymentReq ?? transaction.PaymentReq;
            transaction.PaymentRes = dto.PaymentRes ?? transaction.PaymentRes;
            transaction.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            return transaction;
        }
    }
}
