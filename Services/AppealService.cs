using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.Dtos;
using RajFabAPI.Models;

namespace RajFabAPI.Services
{
    public class AppealService : IAppealService
    {
        private readonly ApplicationDbContext _context;

        public AppealService(ApplicationDbContext context)
        {
            _context = context;
        }

        // CREATE
        public async Task<AppealDetailDto> CreateAsync(AppealCreateDto dto)
        {
            // 1. Check if any appeals exist for this factory
            var existingAppeal = await _context.Appeals
                .Where(a => a.FactoryRegistrationNumber == dto.FactoryRegistrationNumber)
                .OrderByDescending(a => a.Version)
                .FirstOrDefaultAsync();

            string appealRegistrationNumber;
            decimal newVersion = 1.0m;

            if (existingAppeal != null)
            {
                // If factory exists, use same AppealRegistrationNumber
                appealRegistrationNumber = existingAppeal.AppealRegistrationNumber;
                newVersion = existingAppeal.Version + 1; // increment version
            }
            else
            {
                // New factory, generate new AppealRegistrationNumber
                appealRegistrationNumber = GenerateRegistrationNumber();
            }

            // 2. Create new appeal record
            var appeal = new Appeal
            {
                FactoryRegistrationNumber = dto.FactoryRegistrationNumber,
                AppealRegistrationNumber = appealRegistrationNumber,
                AppealApplicationNumber = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10).ToUpper(),
                DateOfAccident = dto.DateOfAccident,
                DateOfInspection = dto.DateOfInspection,
                NoticeNumber = dto.NoticeNumber,
                NoticeDate = dto.NoticeDate,
                OrderNumber = dto.OrderNumber,
                OrderDate = dto.OrderDate,
                FactsAndGrounds = dto.FactsAndGrounds,
                ReliefSought = dto.ReliefSought,
                ChallanNumber = dto.ChallanNumber,
                EnclosureDetails1 = dto.EnclosureDetails1,
                EnclosureDetails2 = dto.EnclosureDetails2,
                SignatureOfOccupier = dto.SignatureOfOccupier,
                Signature = dto.Signature,
                Place = dto.Place,
                Status = "Pending",
                Date = dto.Date,
                Version = newVersion, // set calculated version
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Appeals.Add(appeal);
            await _context.SaveChangesAsync();

            return MapToDetailDto(appeal);
        }


        // GET ALL
        public async Task<IEnumerable<AppealListDto>> GetAllAsync()
        {
            return await _context.Appeals
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedAt)
                .Select(a => new AppealListDto
                {
                    Id = a.Id,
                    FactoryRegistrationNumber = a.FactoryRegistrationNumber,
                    DateOfAccident = a.DateOfAccident,
                    NoticeNumber = a.NoticeNumber,
                    OrderNumber = a.OrderNumber,
                    IsActive = a.IsActive,
                    Status = a.Status,
                    ApplicationType = "Appeal",
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();
        }

        // GET BY ID
        public async Task<AppealDetailDto?> GetByIdAsync(string id)
        {
            var appeal = await _context.Appeals
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

            if (appeal == null) return null;

            return MapToDetailDto(appeal);
        }

        // UPDATE
        public async Task<bool> UpdateAsync(string id, AppealUpdateDto dto)
        {
            var existing = await _context.Appeals.FindAsync(id);
            if (existing == null) return false;

            existing.FactoryRegistrationNumber = dto.FactoryRegistrationNumber;
            existing.DateOfAccident = dto.DateOfAccident;
            existing.DateOfInspection = dto.DateOfInspection;
            existing.NoticeNumber = dto.NoticeNumber;
            existing.NoticeDate = dto.NoticeDate;
            existing.OrderNumber = dto.OrderNumber;
            existing.OrderDate = dto.OrderDate;
            existing.FactsAndGrounds = dto.FactsAndGrounds;
            existing.ReliefSought = dto.ReliefSought;
            existing.ChallanNumber = dto.ChallanNumber;
            existing.EnclosureDetails1 = dto.EnclosureDetails1;
            existing.EnclosureDetails2 = dto.EnclosureDetails2;
            existing.SignatureOfOccupier = dto.SignatureOfOccupier;
            existing.Signature = dto.Signature;
            existing.Place = dto.Place;
            existing.Date = dto.Date;
            // existing.Version = dto.Version;
            existing.IsActive = dto.IsActive;

            existing.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // Helper method to map to DetailDto
        private AppealDetailDto MapToDetailDto(Appeal a) => new AppealDetailDto
        {
            Id = a.Id,
            AppealApplicationNumber = a.AppealApplicationNumber,
            AppealRegistrationNumber = a.AppealRegistrationNumber,
            FactoryRegistrationNumber = a.FactoryRegistrationNumber,
            DateOfAccident = a.DateOfAccident,
            DateOfInspection = a.DateOfInspection,
            NoticeNumber = a.NoticeNumber,
            NoticeDate = a.NoticeDate,
            OrderNumber = a.OrderNumber,
            OrderDate = a.OrderDate,
            FactsAndGrounds = a.FactsAndGrounds,
            ReliefSought = a.ReliefSought,
            ChallanNumber = a.ChallanNumber,
            EnclosureDetails1 = a.EnclosureDetails1,
            EnclosureDetails2 = a.EnclosureDetails2,
            SignatureOfOccupier = a.SignatureOfOccupier,
            Signature = a.Signature,
            Place = a.Place,
            Date = a.Date,
            Version = a.Version,
            IsActive = a.IsActive,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt,
            Status = a.Status,
            ApplicationType = "Appeal",
        };

        public string GenerateRegistrationNumber()
        {
            var year = DateTime.Now.Year;
            var sequence = DateTime.Now.Ticks.ToString().Substring(8, 6);
            return $"FA{year}{sequence}";
        }
    }
}
