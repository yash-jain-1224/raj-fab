using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
    public class RoleService : IRoleService
    {
        private readonly ApplicationDbContext _context;
        public RoleService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<RoleResponseDto>> GetAllAsync()
        {
            return await _context.Roles
                .Include(r => r.Office)
                .Include(r => r.Post)
                .Select(r => new RoleResponseDto
                {
                    Id = r.Id,
                    PostId = r.PostId,
                    PostName = r.Post != null ? r.Post.Name : string.Empty,
                    OfficeId = r.OfficeId,
                    OfficeName = r.Office != null ? r.Office.Name : string.Empty
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<RoleResponseDto>> GetByOfficeAsync(Guid officeId)
        {
            return await _context.Roles
                .Where(r => r.OfficeId == officeId)
                .Include(r => r.Post)
                .Include(r => r.Office).ThenInclude(o => o.City)
                .Select(r => new RoleResponseDto
                {
                    Id = r.Id,
                    PostId = r.PostId,
                    PostName = r.Post != null ? r.Post.Name : string.Empty,
                    OfficeId = r.OfficeId,
                    OfficeName = r.Office != null ? r.Office.Name : string.Empty,
                    // OfficeCityId = r.Office != null ? r.Office.CityId : Guid.Empty,
                    OfficeCityName = r.Office != null && r.Office.City != null
                ? r.Office.City.Name
                : string.Empty
                })
                .ToListAsync();
        }

        public async Task<List<RoleWithPrivilegesDto>> GetAllWithPrivilegesAsync(Guid? officeId = null)
        {
            var query = _context.Roles
                .AsNoTracking()
                .Include(r => r.Post)
                .Include(r => r.Office)
                    .ThenInclude(o => o.City)
                .Include(r => r.Privileges)
                    .ThenInclude(rp => rp.Privilege)
                        .ThenInclude(p => p.Module)
                .AsQueryable();

            if (officeId.HasValue)
            {
                query = query.Where(r => r.OfficeId == officeId.Value);
            }

            var roles = await query.ToListAsync();

            return roles.Select(r => new RoleWithPrivilegesDto
            {
                Id = r.Id,

                PostId = r.PostId,
                PostName = r.Post?.Name ?? string.Empty,

                OfficeId = r.OfficeId,
                OfficeName = r.Office?.Name ?? string.Empty,
                OfficeCityName = r.Office?.City?.Name ?? string.Empty,

                IsActive = r.IsActive,

                PrivilegeCount = r.Privileges
                    .Select(p => p.PrivilegeId)
                    .Distinct()
                    .Count(),

                ModuleNames = r.Privileges
                    .Where(p => p.Privilege?.Module != null)
                    .Select(p => p.Privilege!.Module!.Name)
                    .Distinct()
                    .ToList()

            }).ToList();
        }


        public async Task<Role> CreateAsync(CreateRoleDto dto)
        {
            // Check duplicate per office
            var exists = await _context.Roles.AnyAsync(r =>
                r.PostId == dto.PostId && r.OfficeId == dto.OfficeId
            );

            if (exists)
            {
                throw new InvalidOperationException(
                    "A role with the same post already exists in this office."
                );
            }

            var role = new Role
            {
                PostId = dto.PostId,
                OfficeId = dto.OfficeId
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<Role?> UpdateAsync(Guid id, CreateRoleDto dto)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return null;

            // Check duplicate per office excluding current
            var exists = await _context.Roles.AnyAsync(r =>
                r.Id != id &&
                r.PostId == dto.PostId &&
                r.OfficeId == dto.OfficeId
            );

            if (exists)
            {
                throw new InvalidOperationException(
                    "A role with the same post already exists in this office."
                );
            }

            role.PostId = dto.PostId;
            role.OfficeId = dto.OfficeId;

            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return false;

            // Check if any users are assigned to this role
            var usersWithRole = await _context.UserRoles.AnyAsync(ur => ur.RoleId == id);

            if (usersWithRole)
            {
                throw new InvalidOperationException(
                    "Cannot delete role - users are currently assigned to this role. " +
                    "Please reassign or remove these users first."
                );
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
