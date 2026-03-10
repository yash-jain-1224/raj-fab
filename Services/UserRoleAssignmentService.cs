using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
    public class UserRoleAssignmentService : IUserRoleAssignmentService
    {
        private readonly ApplicationDbContext _context;

        public UserRoleAssignmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET ALL
        public async Task<IEnumerable<UserRoleResponseDto>> GetAllAsync()
        {
            return await _context.UserRoles
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                    .ThenInclude(r => r.Post)
                .Include(ur => ur.Role)
                    .ThenInclude(r => r.Office)
                        .ThenInclude(o => o.City)
                .Select(ur => new UserRoleResponseDto
                {
                    Id = ur.Id,
                    UserId = ur.UserId,
                    Username = ur.User.Username,

                    RoleId = ur.RoleId,
                    RoleName = ur.Role.Post.Name,

                    OfficeId = ur.Role.Office.Id,
                    OfficeName = ur.Role.Office.Name,
                    OfficeCityName = ur.Role.Office.City.Name,

                    JoiningDate = ur.JoiningDate,
                    JoiningDetail = ur.JoiningDetail,
                    JoiningType = ur.JoiningType,
                    IsInspector = ur.IsInspector
                })
                .ToListAsync();
        }


        // GET BY USER ID
        public async Task<IEnumerable<UserRoleResponseDto>> GetByUserIdAsync(Guid userId)
        {
            return await _context.UserRoles
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                    .ThenInclude(r => r.Post)
                .Include(ur => ur.Role)
                    .ThenInclude(r => r.Office)
                        .ThenInclude(o => o.City)
                .Where(ur => ur.UserId == userId)
                .Select(ur => new UserRoleResponseDto
                {
                    Id = ur.Id,
                    UserId = ur.UserId,
                    Username = ur.User.Username,

                    RoleId = ur.RoleId,
                    RoleName = ur.Role.Post.Name,

                    OfficeId = ur.Role.Office.Id,
                    OfficeName = ur.Role.Office.Name,
                    OfficeCityName = ur.Role.Office.City.Name,

                    JoiningDate = ur.JoiningDate,
                    JoiningDetail = ur.JoiningDetail,
                    JoiningType = ur.JoiningType,
                    IsInspector = ur.IsInspector
                })
                .ToListAsync();
        }


        // CREATE
        public async Task<UserRoleResponseDto> CreateAsync(CreateUserRoleRequest dto)
        {
            // Only one user can be assigned one role
            var roleAlreadyAssigned = await _context.UserRoles
                .AnyAsync(x => x.RoleId == dto.RoleId);

            if (roleAlreadyAssigned)
                throw new InvalidOperationException("This role is already assigned to another user.");

            var assignment = new UserRole
            {
                UserId = dto.UserId,
                RoleId = dto.RoleId,
                JoiningDate = dto.JoiningDate,
                JoiningDetail = dto.JoiningDetail,
                JoiningType = dto.JoiningType,
                IsInspector = dto.IsInspector,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.UserRoles.Add(assignment);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(dto.UserId);
            var role = await _context.Roles.Include(r => r.Post)
                                           .FirstOrDefaultAsync(r => r.Id == dto.RoleId);

            return new UserRoleResponseDto
            {
                Id = assignment.Id,
                UserId = assignment.UserId,
                Username = user?.Username ?? "",
                RoleId = assignment.RoleId,
                RoleName = role?.Post?.Name ?? "",
                JoiningDate = assignment.JoiningDate,
                JoiningDetail = assignment.JoiningDetail,
                JoiningType = assignment.JoiningType,
                IsInspector = assignment.IsInspector
            };
        }

        // UPDATE
        public async Task<UserRoleResponseDto?> UpdateAsync(Guid id, CreateUserRoleRequest dto)
        {
            var assignment = await _context.UserRoles
                .FirstOrDefaultAsync(x => x.Id == id);

            if (assignment == null)
                throw new Exception("Assignment not found");

            // New role check (only one user allowed)
            var roleInUse = await _context.UserRoles
                .AnyAsync(x => x.RoleId == dto.RoleId && x.Id != id);

            if (roleInUse)
                throw new InvalidOperationException("This role is already assigned to another user.");

            assignment.UserId = dto.UserId;
            assignment.RoleId = dto.RoleId;
            assignment.JoiningDate = dto.JoiningDate;
            assignment.JoiningDetail = dto.JoiningDetail;
            assignment.JoiningType = dto.JoiningType;
            assignment.IsInspector = dto.IsInspector;
            assignment.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(dto.UserId);
            var role = await _context.Roles.Include(r => r.Post)
                                           .FirstOrDefaultAsync(r => r.Id == dto.RoleId);

            return new UserRoleResponseDto
            {
                Id = assignment.Id,
                UserId = assignment.UserId,
                Username = user?.Username ?? "",
                RoleId = assignment.RoleId,
                RoleName = role?.Post?.Name ?? "",
                JoiningDate = assignment.JoiningDate,
                JoiningDetail = assignment.JoiningDetail,
                JoiningType = assignment.JoiningType,
                IsInspector = assignment.IsInspector
            };
        }

        // DELETE
        public async Task<bool> DeleteAsync(Guid id)
        {
            var assignment = await _context.UserRoles.FirstOrDefaultAsync(x => x.Id == id);

            if (assignment == null)
                return false;

            _context.UserRoles.Remove(assignment);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
