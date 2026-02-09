using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services
{
    public class EnhancedPrivilegeService : IEnhancedPrivilegeService
    {
        private readonly ApplicationDbContext _context;

        public EnhancedPrivilegeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ModulePermissionDto>> GetModulePermissionsAsync(Guid moduleId)
        {
            return await _context.Privileges
                .Where(p => p.ModuleId == moduleId)
                .Include(p => p.Module)
                .Select(p => new ModulePermissionDto
                {
                    Id = p.Id,
                    ModuleId = p.ModuleId,
                    ModuleName = p.Module.Name,
                    PermissionName = p.Action,
                    PermissionCode = p.Action,
                    Description = null
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ModulePermissionDto>> GetAllModulePermissionsAsync()
        {
            return await _context.Privileges
                .Include(p => p.Module)
                .Select(p => new ModulePermissionDto
                {
                    Id = p.Id,
                    ModuleId = p.ModuleId,
                    ModuleName = p.Module.Name,
                    PermissionName = p.Action,
                    PermissionCode = p.Action,
                    Description = null
                })
                .ToListAsync();
        }

        public async Task<RolePrivilegeDataDto?> GetRolePrivilegesAsync(Guid roleId)
        {
            var role = await _context.Roles
                .Include(r => r.Post)
                .Include(r => r.Privileges)
                    .ThenInclude(rp => rp.Privilege)
                        .ThenInclude(p => p.Module)
                .FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null) return null;

            return new RolePrivilegeDataDto
            {
                RoleId = role.Id,
                PostName = role.Post.Name,
                ModulePermissions = role.Privileges
                    .Where(rp => rp.Privilege.Module != null)
                    .GroupBy(rp => new
                    {
                        rp.Privilege.ModuleId,
                        rp.Privilege.Module.Name,
                        rp.Privilege.Module.ActId,
                        rp.Privilege.Module.RuleId
                    })
                    .Select(g => new RoleModulePermissionDto
                    {
                        ModuleId = g.Key.ModuleId,
                        ModuleName = g.Key.Name,
                        ActId = g.Key.ActId,
                        RuleId = g.Key.RuleId,
                        Permissions = g
                            .Select(x => x.Privilege.Action)
                            .Distinct()
                            .ToList()
                    })
                    .OrderBy(x => x.ModuleName)
                    .ToList()
            };
        }

        public async Task<bool> AssignRolePrivilegesAsync(AssignRolePrivilegesDto dto)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var role = await _context.Roles
                    .Include(r => r.Privileges)
                    .FirstOrDefaultAsync(r => r.Id == dto.RoleId);

                if (role == null)
                    return false;

                if (role.Privileges.Any())
                    _context.RolePrivileges.RemoveRange(role.Privileges);

                var privilegeIds = new List<Guid>();

                foreach (var mp in dto.ModulePermissions)
                {
                    if (mp.Permissions == null || mp.Permissions.Count == 0)
                        continue;

                    // Force materialization (IMPORTANT for SQL 2014)
                    var permissionsList = mp.Permissions.ToList(); // materialize
                    var ids = _context.Privileges
                        .AsEnumerable() // forces evaluation in memory
                        .Where(p => p.ModuleId == mp.ModuleId && permissionsList.Contains(p.Action))
                        .Select(p => p.Id)
                        .ToList();

                    privilegeIds.AddRange(ids);
                }

                if (privilegeIds.Any())
                {
                    var newPrivileges = privilegeIds
                        .Distinct()
                        .Select(pid => new RolePrivilege
                        {
                            RoleId = role.Id,
                            PrivilegeId = pid
                        });

                    await _context.RolePrivileges.AddRangeAsync(newPrivileges);
                }
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> RemoveRoleModulePrivilegesAsync(Guid roleId, Guid moduleId)
        {
            var privileges = await _context.RolePrivileges
                .Where(rp =>
                    rp.RoleId == roleId &&
                    rp.Privilege.ModuleId == moduleId)
                .ToListAsync();

            if (!privileges.Any())
                return false;

            _context.RolePrivileges.RemoveRange(privileges);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CheckRolePermissionAsync(
            Guid roleId,
            Guid moduleId,
            string permission)
        {
            return await _context.RolePrivileges
                .AnyAsync(rp =>
                    rp.RoleId == roleId &&
                    rp.Privilege.ModuleId == moduleId &&
                    rp.Privilege.Action == permission);
        }

        public async Task<PermissionCheckResultDto> CheckUserPermissionAsync(
            Guid userId,
            Guid moduleId,
            string permission)
        {
            var hasPermission = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => ur.Role.Privileges)
                .AnyAsync(rp =>
                    rp.Privilege.ModuleId == moduleId &&
                    rp.Privilege.Action == permission);

            return new PermissionCheckResultDto
            {
                HasPermission = hasPermission,
                Reason = hasPermission ? "Permission granted" : "Permission denied"
            };
        }

        public async Task<bool> InitializeDefaultPermissionsAsync()
        {
            var modules = await _context.Modules.ToListAsync();

            var actions = new[]
            {
                "VIEW",
                "EDIT",
                "FORWARD",
                "FORWARD_TO_APPLIER",
                "APPROVE",
                "REJECT"
            };

            foreach (var module in modules)
            {
                foreach (var action in actions)
                {
                    var exists = await _context.Privileges
                        .AnyAsync(p =>
                            p.ModuleId == module.Id &&
                            p.Action == action);

                    if (!exists)
                    {
                        _context.Privileges.Add(new Privilege
                        {
                            ModuleId = module.Id,
                            Action = action
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
