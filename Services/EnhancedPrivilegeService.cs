using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Load role with existing privileges
                var role = await _context.Roles
                    .Include(r => r.Privileges)
                    .FirstOrDefaultAsync(r => r.Id == dto.RoleId);

                if (role == null)
                    return false;

                // 2. Clear existing privileges
                if (role.Privileges.Any())
                    _context.RolePrivileges.RemoveRange(role.Privileges);

                // 3. Validate incoming permissions
                var validModulePermissions = dto.ModulePermissions
                    .Where(mp => mp.Permissions != null && mp.Permissions.Any())
                    .ToList();

                if (!validModulePermissions.Any())
                {
                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();
                    return true;
                }

                // 4. Fetch matching privilege IDs using raw SQL
                //    Bypasses EF Core 8 CTE generation — safe for SQL Server 2014
                var privilegeIds = new List<Guid>();
                var conn = _context.Database.GetDbConnection();

                if (conn.State != System.Data.ConnectionState.Open)
                    await conn.OpenAsync();

                foreach (var mp in validModulePermissions)
                {
                    var permissionList = mp.Permissions
                        .Where(p => !string.IsNullOrWhiteSpace(p))
                        .Distinct()
                        .ToList();

                    if (!permissionList.Any())
                        continue;

                    // Build: @p0, @p1, @p2 ...
                    var paramNames = permissionList
                        .Select((_, i) => $"@p{i}")
                        .ToList();

                    var sql = $@"
                SELECT Id 
                FROM Privileges 
                WHERE ModuleId = @moduleId 
                AND Action IN ({string.Join(", ", paramNames)})";

                    using var cmd = conn.CreateCommand();
                    cmd.Transaction = tx.GetDbTransaction();
                    cmd.CommandText = sql;

                    // Add ModuleId param
                    var moduleParam = new Microsoft.Data.SqlClient.SqlParameter(
                        "@moduleId", System.Data.SqlDbType.UniqueIdentifier)
                    {
                        Value = mp.ModuleId
                    };
                    cmd.Parameters.Add(moduleParam);

                    // Add Action params
                    for (int i = 0; i < permissionList.Count; i++)
                    {
                        var actionParam = new Microsoft.Data.SqlClient.SqlParameter(
                            $"@p{i}", System.Data.SqlDbType.NVarChar, 256)
                        {
                            Value = permissionList[i]
                        };
                        cmd.Parameters.Add(actionParam);
                    }

                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                        privilegeIds.Add(reader.GetGuid(0));
                }

                // 5. Insert new RolePrivileges
                if (privilegeIds.Any())
                {
                    var newPrivileges = privilegeIds
                        .Distinct()
                        .Select(pid => new RolePrivilege
                        {
                            RoleId = role.Id,
                            PrivilegeId = pid
                        })
                        .ToList();

                    await _context.RolePrivileges.AddRangeAsync(newPrivileges);
                }

                // 6. Save and commit
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
