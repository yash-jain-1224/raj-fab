using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services
{
    public class UserHierarchyService
    {
        private readonly ApplicationDbContext _context;

        public UserHierarchyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserHierarchyDto>> GetAllAsync()
        {
            return await _context.UserHierarchies
                .Select(uh => new UserHierarchyDto
                {
                    Id = uh.Id,
                    UserId = uh.UserId,
                    ReportsToId = uh.ReportsToId,
                    EmergencyReportToId = uh.EmergencyReportToId,
                    CreatedAt = uh.CreatedAt,
                    UpdatedAt = uh.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<UserHierarchyDto?> GetByIdAsync(Guid id)
        {
            var hierarchy = await _context.UserHierarchies
                .FirstOrDefaultAsync(uh => uh.Id == id);

            if (hierarchy == null) return null;

            return new UserHierarchyDto
            {
                Id = hierarchy.Id,
                UserId = hierarchy.UserId,
                ReportsToId = hierarchy.ReportsToId,
                EmergencyReportToId = hierarchy.EmergencyReportToId,
                CreatedAt = hierarchy.CreatedAt,
                UpdatedAt = hierarchy.UpdatedAt
            };
        }

        public async Task<UserHierarchyDto> CreateAsync(CreateUserHierarchyDto dto)
        {
            // Validate that user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId);
            if (!userExists)
                throw new ArgumentException("User not found");

            // Validate that reportsTo user exists if provided
            if (dto.ReportsToId.HasValue)
            {
                var reportsToExists = await _context.Users.AnyAsync(u => u.Id == dto.ReportsToId);
                if (!reportsToExists)
                    throw new ArgumentException("Reports to user not found");
            }

            // Validate that emergencyReportTo user exists if provided
            if (dto.EmergencyReportToId.HasValue)
            {
                var emergencyExists = await _context.Users.AnyAsync(u => u.Id == dto.EmergencyReportToId);
                if (!emergencyExists)
                    throw new ArgumentException("Emergency report to user not found");
            }

            // Check if hierarchy already exists for this user
            var existingHierarchy = await _context.UserHierarchies
                .FirstOrDefaultAsync(uh => uh.UserId == dto.UserId);

            if (existingHierarchy != null)
                throw new InvalidOperationException("User hierarchy already exists for this user");

            var hierarchy = new UserHierarchy
            {
                UserId = dto.UserId,
                ReportsToId = dto.ReportsToId,
                EmergencyReportToId = dto.EmergencyReportToId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.UserHierarchies.Add(hierarchy);
            await _context.SaveChangesAsync();

            return new UserHierarchyDto
            {
                Id = hierarchy.Id,
                UserId = hierarchy.UserId,
                ReportsToId = hierarchy.ReportsToId,
                EmergencyReportToId = hierarchy.EmergencyReportToId,
                CreatedAt = hierarchy.CreatedAt,
                UpdatedAt = hierarchy.UpdatedAt
            };
        }

        public async Task<UserHierarchyDto> UpdateAsync(Guid id, CreateUserHierarchyDto dto)
        {
            var hierarchy = await _context.UserHierarchies.FindAsync(id);
            if (hierarchy == null)
                throw new ArgumentException("User hierarchy not found");

            // Validate that user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId);
            if (!userExists)
                throw new ArgumentException("User not found");

            // Validate that reportsTo user exists if provided
            if (dto.ReportsToId.HasValue)
            {
                var reportsToExists = await _context.Users.AnyAsync(u => u.Id == dto.ReportsToId);
                if (!reportsToExists)
                    throw new ArgumentException("Reports to user not found");
            }

            // Validate that emergencyReportTo user exists if provided
            if (dto.EmergencyReportToId.HasValue)
            {
                var emergencyExists = await _context.Users.AnyAsync(u => u.Id == dto.EmergencyReportToId);
                if (!emergencyExists)
                    throw new ArgumentException("Emergency report to user not found");
            }

            hierarchy.UserId = dto.UserId;
            hierarchy.ReportsToId = dto.ReportsToId;
            hierarchy.EmergencyReportToId = dto.EmergencyReportToId;
            hierarchy.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return new UserHierarchyDto
            {
                Id = hierarchy.Id,
                UserId = hierarchy.UserId,
                ReportsToId = hierarchy.ReportsToId,
                EmergencyReportToId = hierarchy.EmergencyReportToId,
                CreatedAt = hierarchy.CreatedAt,
                UpdatedAt = hierarchy.UpdatedAt
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var hierarchy = await _context.UserHierarchies.FindAsync(id);
            if (hierarchy == null)
                return false;

            _context.UserHierarchies.Remove(hierarchy);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}