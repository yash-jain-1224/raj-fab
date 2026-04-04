using System.Data;
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;
using static RajFabAPI.Constants.AppConstants;

namespace RajFabAPI.Services
{
    public class InspectorApplicationAssignmentService : IInspectorApplicationAssignmentService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<InspectorApplicationAssignmentService> _logger;

        public InspectorApplicationAssignmentService(ApplicationDbContext db, ILogger<InspectorApplicationAssignmentService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ── Raw SQL helpers ────────────────────────────────────────────────────

        private async Task<T> WithConnectionAsync<T>(Func<IDbConnection, Task<T>> func)
        {
            var conn = _db.Database.GetDbConnection();
            bool opened = false;
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
                opened = true;
            }
            try
            {
                return await func(conn);
            }
            finally
            {
                if (opened) await conn.CloseAsync();
            }
        }

        private static IDbDataParameter Param(IDbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            return p;
        }

        /// <summary>Fetch user Id → FullName map using raw SQL (avoids EF Core CTEs).</summary>
        private async Task<Dictionary<Guid, string>> GetUserNamesAsync(IEnumerable<Guid> userIds)
        {
            var ids = userIds.Distinct().ToList();
            if (!ids.Any()) return new Dictionary<Guid, string>();

            return await WithConnectionAsync(async conn =>
            {
                var result = new Dictionary<Guid, string>();
                using var cmd = conn.CreateCommand();
                var paramNames = ids.Select((_, i) => $"@uid{i}").ToList();
                cmd.CommandText = $"SELECT Id, FullName FROM Users WHERE Id IN ({string.Join(",", paramNames)})";
                for (int i = 0; i < ids.Count; i++)
                    cmd.Parameters.Add(Param(cmd, $"@uid{i}", ids[i]));

                using var reader = await ((System.Data.Common.DbCommand)cmd).ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var id = reader.GetGuid(0);
                    var name = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    result[id] = name;
                }
                return result;
            });
        }

        /// <summary>Fetch boiler registration status keyed by ApplicationRegistrationId.</summary>
        private async Task<Dictionary<string, string>> GetApplicationStatusesAsync(IEnumerable<string> appRegIds)
        {
            var ids = appRegIds.Distinct().ToList();
            if (!ids.Any()) return new Dictionary<string, string>();

            // ApplicationRegistrations.Id → ApplicationId → BoilerRegistrations.Status
            var appRegs = await _db.ApplicationRegistrations
                .Where(r => ids.Contains(r.Id))
                .Select(r => new { r.Id, r.ApplicationId })
                .ToListAsync();

            var appIds = appRegs.Select(r => r.ApplicationId).Where(a => a != null).ToList();

            var boilerStatuses = await _db.BoilerRegistrations
                .Where(b => appIds.Contains(b.ApplicationId))
                .Select(b => new { b.ApplicationId, b.Status })
                .ToListAsync();

            var statusByAppId = boilerStatuses.ToDictionary(b => b.ApplicationId!, b => b.Status ?? "");

            return appRegs.ToDictionary(
                r => r.Id,
                r => r.ApplicationId != null && statusByAppId.TryGetValue(r.ApplicationId, out var s) ? s : ""
            );
        }

        /// <summary>Get set of assignmentIds that have a matching inspection row.</summary>
        private async Task<HashSet<Guid>> GetInspectionSetAsync(IEnumerable<Guid> assignmentIds)
        {
            var ids = assignmentIds.ToList();
            if (!ids.Any()) return new HashSet<Guid>();

            return await WithConnectionAsync(async conn =>
            {
                var result = new HashSet<Guid>();
                try
                {
                    using var cmd = conn.CreateCommand();
                    var paramNames = ids.Select((_, i) => $"@iid{i}").ToList();
                    cmd.CommandText = $"SELECT InspectorApplicationAssignmentId FROM InspectorApplicationInspections WHERE InspectorApplicationAssignmentId IN ({string.Join(",", paramNames)})";
                    for (int i = 0; i < ids.Count; i++)
                        cmd.Parameters.Add(Param(cmd, $"@iid{i}", ids[i]));

                    using var reader = await ((System.Data.Common.DbCommand)cmd).ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                        result.Add(reader.GetGuid(0));
                }
                catch (Exception ex) when (ex.Message.Contains("Invalid object name"))
                {
                    _logger.LogWarning("InspectorApplicationInspections table not found — run migrations.");
                }
                return result;
            });
        }

        /// <summary>Read InspectorApplicationAssignment rows from a DataReader.</summary>
        private static InspectorApplicationAssignment ReadAssignment(System.Data.Common.DbDataReader r) =>
            new InspectorApplicationAssignment
            {
                Id = r.GetGuid(r.GetOrdinal("Id")),
                ApplicationRegistrationId = r.IsDBNull(r.GetOrdinal("ApplicationRegistrationId")) ? "" : r.GetString(r.GetOrdinal("ApplicationRegistrationId")),
                ApplicationType = r.IsDBNull(r.GetOrdinal("ApplicationType")) ? "" : r.GetString(r.GetOrdinal("ApplicationType")),
                ApplicationTitle = r.IsDBNull(r.GetOrdinal("ApplicationTitle")) ? "" : r.GetString(r.GetOrdinal("ApplicationTitle")),
                ApplicationRegistrationNumber = r.IsDBNull(r.GetOrdinal("ApplicationRegistrationNumber")) ? "" : r.GetString(r.GetOrdinal("ApplicationRegistrationNumber")),
                AssignedToUserId = r.GetGuid(r.GetOrdinal("AssignedToUserId")),
                AssignedByUserId = r.GetGuid(r.GetOrdinal("AssignedByUserId")),
                Status = r.IsDBNull(r.GetOrdinal("Status")) ? "" : r.GetString(r.GetOrdinal("Status")),
                Remarks = r.IsDBNull(r.GetOrdinal("Remarks")) ? null : r.GetString(r.GetOrdinal("Remarks")),
                AssignedDate = r.GetDateTime(r.GetOrdinal("AssignedDate")),
                UpdatedDate = r.GetDateTime(r.GetOrdinal("UpdatedDate")),
            };

        private static InspectorApplicationAssignmentDto MapAssignment(
            InspectorApplicationAssignment x,
            Dictionary<Guid, string> userNames,
            HashSet<Guid> inspectionSet) =>
            new InspectorApplicationAssignmentDto
            {
                Id = x.Id,
                ApplicationRegistrationId = x.ApplicationRegistrationId,
                ApplicationType = x.ApplicationType,
                ApplicationTitle = x.ApplicationTitle,
                ApplicationRegistrationNumber = x.ApplicationRegistrationNumber,
                AssignedToUserId = x.AssignedToUserId,
                AssignedToName = userNames.GetValueOrDefault(x.AssignedToUserId, ""),
                AssignedByUserId = x.AssignedByUserId,
                AssignedByName = userNames.GetValueOrDefault(x.AssignedByUserId, ""),
                Status = x.Status,
                Remarks = x.Remarks,
                AssignedDate = x.AssignedDate,
                UpdatedDate = x.UpdatedDate,
                HasInspection = inspectionSet.Contains(x.Id)
            };

        // ── Service methods ────────────────────────────────────────────────────

        public async Task<IEnumerable<InspectorApplicationAssignmentDto>> GetAllAsync(string? officeId, string? applicationType)
        {
            var assignments = await WithConnectionAsync(async conn =>
            {
                var list = new List<InspectorApplicationAssignment>();
                using var cmd = conn.CreateCommand();
                var where = "1=1";
                if (!string.IsNullOrEmpty(applicationType))
                {
                    where += " AND ApplicationType LIKE @appType";
                    cmd.Parameters.Add(Param(cmd, "@appType", $"%{applicationType}%"));
                }
                cmd.CommandText = $"SELECT * FROM InspectorApplicationAssignments WHERE {where}";
                using var reader = await ((System.Data.Common.DbCommand)cmd).ExecuteReaderAsync();
                while (await reader.ReadAsync()) list.Add(ReadAssignment(reader));
                return list;
            });

            var userNames = await GetUserNamesAsync(assignments.SelectMany(a => new[] { a.AssignedToUserId, a.AssignedByUserId }));
            var inspectionSet = await GetInspectionSetAsync(assignments.Select(a => a.Id));
            return assignments.Select(x => MapAssignment(x, userNames, inspectionSet)).ToList();
        }

        public async Task<IEnumerable<InspectorApplicationAssignmentDto>> GetByInspectorIdAsync(Guid inspectorUserId)
        {
            var assignments = await WithConnectionAsync(async conn =>
            {
                var list = new List<InspectorApplicationAssignment>();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM InspectorApplicationAssignments WHERE AssignedToUserId = @userId";
                cmd.Parameters.Add(Param(cmd, "@userId", inspectorUserId));
                using var reader = await ((System.Data.Common.DbCommand)cmd).ExecuteReaderAsync();
                while (await reader.ReadAsync()) list.Add(ReadAssignment(reader));
                return list;
            });

            var userNames = await GetUserNamesAsync(assignments.SelectMany(a => new[] { a.AssignedToUserId, a.AssignedByUserId }));
            var inspectionSet = await GetInspectionSetAsync(assignments.Select(a => a.Id));
            var appStatuses = await GetApplicationStatusesAsync(assignments.Select(a => a.ApplicationRegistrationId));

            return assignments.Select(x =>
            {
                var dto = MapAssignment(x, userNames, inspectionSet);
                dto.ApplicationStatus = appStatuses.GetValueOrDefault(x.ApplicationRegistrationId, "");
                return dto;
            }).ToList();
        }

        public async Task<InspectorApplicationAssignmentDto> AssignAsync(CreateInspectorApplicationAssignmentDto dto)
        {
            // Upsert: update existing assignment if already assigned (auto-assign on submit may have created one)
            var existing = await _db.InspectorApplicationAssignments
                .FirstOrDefaultAsync(x => x.ApplicationRegistrationId == dto.ApplicationRegistrationId);

            InspectorApplicationAssignment assignment;
            if (existing != null)
            {
                existing.AssignedToUserId = dto.AssignedToUserId;
                existing.AssignedByUserId = dto.AssignedByUserId;
                existing.UpdatedDate = DateTime.Now;
                assignment = existing;
            }
            else
            {
                assignment = new InspectorApplicationAssignment
                {
                    ApplicationRegistrationId = dto.ApplicationRegistrationId,
                    ApplicationType = dto.ApplicationType,
                    ApplicationTitle = dto.ApplicationTitle,
                    ApplicationRegistrationNumber = dto.ApplicationRegistrationNumber,
                    AssignedToUserId = dto.AssignedToUserId,
                    AssignedByUserId = dto.AssignedByUserId,
                    Status = "Pending",
                    AssignedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };
                _db.InspectorApplicationAssignments.Add(assignment);
            }

            await _db.SaveChangesAsync();

            var userNames = await GetUserNamesAsync(new[] { dto.AssignedToUserId, dto.AssignedByUserId });

            return new InspectorApplicationAssignmentDto
            {
                Id = assignment.Id,
                ApplicationRegistrationId = assignment.ApplicationRegistrationId,
                ApplicationType = assignment.ApplicationType,
                ApplicationTitle = assignment.ApplicationTitle,
                ApplicationRegistrationNumber = assignment.ApplicationRegistrationNumber,
                AssignedToUserId = assignment.AssignedToUserId,
                AssignedToName = userNames.GetValueOrDefault(assignment.AssignedToUserId, ""),
                AssignedByUserId = assignment.AssignedByUserId,
                AssignedByName = userNames.GetValueOrDefault(assignment.AssignedByUserId, ""),
                Status = assignment.Status,
                AssignedDate = assignment.AssignedDate,
                UpdatedDate = assignment.UpdatedDate,
                HasInspection = false
            };
        }

        public async Task<InspectorApplicationAssignmentDto?> UpdateInspectorAsync(UpdateInspectorAssignmentDto dto, Guid updatedByUserId)
        {
            var assignment = await _db.InspectorApplicationAssignments
                .FirstOrDefaultAsync(x => x.ApplicationRegistrationId == dto.ApplicationRegistrationId);
            if (assignment == null) return null;

            assignment.AssignedToUserId = dto.NewInspectorUserId;
            assignment.AssignedByUserId = updatedByUserId;
            assignment.UpdatedDate = DateTime.Now;
            await _db.SaveChangesAsync();

            var userNames = await GetUserNamesAsync(new[] { assignment.AssignedToUserId, assignment.AssignedByUserId });
            var inspectionSet = await GetInspectionSetAsync(new[] { assignment.Id });

            return new InspectorApplicationAssignmentDto
            {
                Id = assignment.Id,
                ApplicationRegistrationId = assignment.ApplicationRegistrationId,
                ApplicationType = assignment.ApplicationType,
                ApplicationTitle = assignment.ApplicationTitle,
                ApplicationRegistrationNumber = assignment.ApplicationRegistrationNumber,
                AssignedToUserId = assignment.AssignedToUserId,
                AssignedToName = userNames.GetValueOrDefault(assignment.AssignedToUserId, ""),
                AssignedByUserId = assignment.AssignedByUserId,
                AssignedByName = userNames.GetValueOrDefault(assignment.AssignedByUserId, ""),
                Status = assignment.Status,
                Remarks = assignment.Remarks,
                AssignedDate = assignment.AssignedDate,
                UpdatedDate = assignment.UpdatedDate,
                HasInspection = inspectionSet.Contains(assignment.Id)
            };
        }

        public async Task<InspectorApplicationAssignmentDto?> TakeActionAsync(Guid id, InspectorApplicationActionDto dto)
        {
            var assignment = await _db.InspectorApplicationAssignments.FirstOrDefaultAsync(x => x.Id == id);
            if (assignment == null) return null;

            assignment.Status = dto.Action;
            assignment.Remarks = dto.Remarks;
            assignment.UpdatedDate = DateTime.Now;
            await _db.SaveChangesAsync();

            var userNames = await GetUserNamesAsync(new[] { assignment.AssignedToUserId, assignment.AssignedByUserId });
            var inspectionSet = await GetInspectionSetAsync(new[] { id });

            return new InspectorApplicationAssignmentDto
            {
                Id = assignment.Id,
                ApplicationRegistrationId = assignment.ApplicationRegistrationId,
                ApplicationType = assignment.ApplicationType,
                ApplicationTitle = assignment.ApplicationTitle,
                ApplicationRegistrationNumber = assignment.ApplicationRegistrationNumber,
                AssignedToUserId = assignment.AssignedToUserId,
                AssignedToName = userNames.GetValueOrDefault(assignment.AssignedToUserId, ""),
                AssignedByUserId = assignment.AssignedByUserId,
                AssignedByName = userNames.GetValueOrDefault(assignment.AssignedByUserId, ""),
                Status = assignment.Status,
                Remarks = assignment.Remarks,
                AssignedDate = assignment.AssignedDate,
                UpdatedDate = assignment.UpdatedDate,
                HasInspection = inspectionSet.Contains(id)
            };
        }

        public async Task<IEnumerable<BoilerApplicationListItemDto>> GetAllBoilerApplicationsAsync(string? officeId, string? applicationType)
        {
            try
            {
                // Step 1: Get boiler module IDs
                var boilerModules = await _db.Modules
                    .Where(m => m.Category == "Boiler" || m.Category == "boiler")
                    .Select(m => new { m.Id, m.Name })
                    .ToListAsync();

                if (!boilerModules.Any())
                {
                    boilerModules = await _db.Modules
                        .Where(m => m.IsActive)
                        .Select(m => new { m.Id, m.Name })
                        .ToListAsync();
                    boilerModules = boilerModules
                        .Where(m => m.Name.ToLower().Contains("boiler") ||
                                    m.Name.ToLower().Contains("steam") ||
                                    m.Name.ToLower().Contains("welder") ||
                                    m.Name.ToLower().Contains("economiser"))
                        .ToList();
                }

                if (!boilerModules.Any())
                    return Enumerable.Empty<BoilerApplicationListItemDto>();

                var boilerModuleIds = boilerModules.Select(m => m.Id).ToList();
                var moduleNameMap = boilerModules.ToDictionary(m => m.Id, m => m.Name);

                var joinedData = await WithConnectionAsync(async conn =>
                {
                    var list = new List<(int Id, string AppRegId, string Status, DateTime CreatedDate, Guid ModuleId, string RegNumber, Guid WorkflowLevelId)>();
                    using var cmd = conn.CreateCommand();

                    var modParams = boilerModuleIds.Select((_, i) => $"@mid{i}").ToList();
                    for (int i = 0; i < boilerModuleIds.Count; i++)
                        cmd.Parameters.Add(Param(cmd, $"@mid{i}", boilerModuleIds[i]));

                    cmd.CommandText = $@"
                        SELECT 
                            aar.Id,
                            ar.Id,
                            aar.Status,
                            aar.CreatedDate,
                            ar.ModuleId,
                            ar.ApplicationRegistrationNumber,
                            aar.ApplicationWorkFlowLevelId
                        FROM ApplicationRegistrations ar
                        LEFT JOIN ApplicationApprovalRequests aar
                            ON aar.ApplicationRegistrationId = ar.Id
                        WHERE ar.ModuleId IN ({string.Join(",", modParams)})";

                    using var reader = await ((System.Data.Common.DbCommand)cmd).ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        list.Add((
                            reader.IsDBNull(0) ? 0 : reader.GetInt32(0),                 // aar.Id
                            reader.IsDBNull(1) ? "" : reader.GetString(1),                // ar.Id
                            reader.IsDBNull(2) ? "" : reader.GetString(2),                // aar.Status
                            reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3), // aar.CreatedDate
                            reader.IsDBNull(4) ? Guid.Empty : reader.GetGuid(4),          // ar.ModuleId
                            reader.IsDBNull(5) ? "" : reader.GetString(5),                // ar.ApplicationRegistrationNumber
                            reader.IsDBNull(6) ? Guid.Empty : reader.GetGuid(6)           // aar.ApplicationWorkFlowLevelId
                        ));
                    }

                    return list;
                });

                // Step 3: Enrich with module name in memory
                var rawData = joinedData.Select(x => new
                {
                    x.Id,
                    ApplicationRegistrationId = x.AppRegId,
                    x.Status,
                    x.CreatedDate,
                    ApplicationType = moduleNameMap.TryGetValue(x.ModuleId, out var mName) ? mName : "",
                    ApplicationRegistrationNumber = x.RegNumber,
                    ApplicationWorkFlowLevelId = x.WorkflowLevelId
                }).ToList();

                if (!string.IsNullOrEmpty(applicationType))
                    rawData = rawData.Where(x => x.ApplicationType.ToLower().Contains(applicationType.ToLower())).ToList();

                // Step 4: Office filter
                if (!string.IsNullOrEmpty(officeId) && Guid.TryParse(officeId, out var officeGuid))
                {
                    var roleIds = await _db.Roles.Where(r => r.OfficeId == officeGuid).Select(r => r.Id).ToListAsync();
                    var wflIds = await _db.ApplicationWorkFlowLevels.Where(w => roleIds.Contains(w.RoleId)).Select(w => w.Id).ToListAsync();
                    rawData = rawData.Where(x => wflIds.Contains(x.ApplicationWorkFlowLevelId)).ToList();
                }

                // Step 5: Group by registration, take latest
                var grouped = rawData
                    .GroupBy(x => x.ApplicationRegistrationId)
                    .Select(g => g.OrderByDescending(x => x.CreatedDate).First())
                    .OrderByDescending(x => x.CreatedDate)
                    .ToList();

                if (!grouped.Any())
                    return Enumerable.Empty<BoilerApplicationListItemDto>();

                // Step 6: Fetch assignments with raw SQL
                var regIds = grouped.Select(x => x.ApplicationRegistrationId).ToList();
                var assignments = await WithConnectionAsync(async conn =>
                {
                    var list = new List<InspectorApplicationAssignment>();
                    using var cmd = conn.CreateCommand();
                    var paramNames = regIds.Select((_, i) => $"@rid{i}").ToList();
                    for (int i = 0; i < regIds.Count; i++)
                        cmd.Parameters.Add(Param(cmd, $"@rid{i}", regIds[i]));
                    cmd.CommandText = $"SELECT * FROM InspectorApplicationAssignments WHERE ApplicationRegistrationId IN ({string.Join(",", paramNames)})";
                    using var reader = await ((System.Data.Common.DbCommand)cmd).ExecuteReaderAsync();
                    while (await reader.ReadAsync()) list.Add(ReadAssignment(reader));
                    return list;
                });

                var userNames = await GetUserNamesAsync(assignments.Select(a => a.AssignedToUserId));
                var inspectionSet = await GetInspectionSetAsync(assignments.Select(a => a.Id));

                return grouped.Select(x =>
                {
                    var a = assignments.FirstOrDefault(a => a.ApplicationRegistrationId == x.ApplicationRegistrationId);
                    return new BoilerApplicationListItemDto
                    {
                        ApprovalRequestId = x.Id,
                        ApplicationType = x.ApplicationType,
                        ApplicationTitle = x.ApplicationRegistrationNumber,
                        ApplicationRegistrationNumber = x.ApplicationRegistrationNumber,
                        Status = x.Status,
                        CreatedDate = x.CreatedDate,
                        IsAssigned = a != null,
                        AssignmentId = a?.Id,
                        AssignedToName = a != null ? userNames.GetValueOrDefault(a.AssignedToUserId, "") : null,
                        AssignedToUserId = a?.AssignedToUserId,
                        AssignmentStatus = a?.Status,
                        HasInspection = a != null && inspectionSet.Contains(a.Id)
                    };
                }).ToList();
            }
            catch
            {
                throw;
            }
        }

        public async Task<InspectorApplicationInspectionDto> SubmitInspectionAsync(Guid assignmentId, CreateInspectorApplicationInspectionDto dto)
        {
            var assignment = await _db.InspectorApplicationAssignments.FindAsync(assignmentId)
                ?? throw new InvalidOperationException("Assignment not found.");

            // Only allow inspection when the boiler registration is Approved
            var appReg = await _db.ApplicationRegistrations
                .FirstOrDefaultAsync(r => r.Id == assignment.ApplicationRegistrationId);
            if (appReg != null)
            {
                var boilerReg = await _db.BoilerRegistrations
                    .FirstOrDefaultAsync(b => b.ApplicationId == appReg.ApplicationId);
                if (boilerReg != null && boilerReg.Status != ApplicationStatus.Approved)
                    throw new InvalidOperationException("Inspection can only be submitted after the boiler registration is approved.");
            }

            var existing = await _db.InspectorApplicationInspections
                .AnyAsync(i => i.InspectorApplicationAssignmentId == assignmentId);
            if (existing)
                throw new InvalidOperationException("Inspection already submitted for this assignment.");

            var inspection = new InspectorApplicationInspection
            {
                InspectorApplicationAssignmentId = assignmentId,
                InspectionDate = dto.InspectionDate,
                BoilerCondition = dto.BoilerCondition,
                MaxAllowableWorkingPressure = dto.MaxAllowableWorkingPressure,
                Observations = dto.Observations,
                DefectsFound = dto.DefectsFound,
                DefectDetails = dto.DefectDetails,
                InspectionReportNumber = dto.InspectionReportNumber,
                HydraulicTestPressure = dto.HydraulicTestPressure,
                HydraulicTestDuration = dto.HydraulicTestDuration,
                JointsCondition = dto.JointsCondition,
                RivetsCondition = dto.RivetsCondition,
                PlatingCondition = dto.PlatingCondition,
                StaysCondition = dto.StaysCondition,
                CrownCondition = dto.CrownCondition,
                FireboxCondition = dto.FireboxCondition,
                FusiblePlugCondition = dto.FusiblePlugCondition,
                FireTubesCondition = dto.FireTubesCondition,
                FlueFurnaceCondition = dto.FlueFurnaceCondition,
                SmokeBoxCondition = dto.SmokeBoxCondition,
                SteamDrumCondition = dto.SteamDrumCondition,
                SafetyValveCondition = dto.SafetyValveCondition,
                PressureGaugeCondition = dto.PressureGaugeCondition,
                FeedCheckCondition = dto.FeedCheckCondition,
                StopValveCondition = dto.StopValveCondition,
                BlowDownCondition = dto.BlowDownCondition,
                EconomiserCondition = dto.EconomiserCondition,
                SuperheaterCondition = dto.SuperheaterCondition,
                AirPressureGaugeCondition = dto.AirPressureGaugeCondition,
                AllowedWorkingPressure = dto.AllowedWorkingPressure,
                ProvisionalOrderNumber = dto.ProvisionalOrderNumber,
                ProvisionalOrderDate = dto.ProvisionalOrderDate,
                BoilerAttendantName = dto.BoilerAttendantName,
                BoilerAttendantCertNo = dto.BoilerAttendantCertNo,
                FeeAmount = dto.FeeAmount,
                ChallanNumber = dto.ChallanNumber,
                CreatedDate = DateTime.Now
            };

            _db.InspectorApplicationInspections.Add(inspection);
            assignment.Status = "Inspected";
            assignment.UpdatedDate = DateTime.Now;
            await _db.SaveChangesAsync();

            // Create ApplicationApprovalRequest for "Boiler Inspection" workflow starting at level 1
            await CreateInspectionApprovalRequestAsync(assignment.ApplicationRegistrationId);

            return MapInspection(inspection);
        }

        private async Task CreateInspectionApprovalRequestAsync(string applicationRegistrationId)
        {
            try
            {
                var appReg = await _db.ApplicationRegistrations
                    .FirstOrDefaultAsync(r => r.Id == applicationRegistrationId);
                if (appReg == null) return;

                var inspectionModule = await _db.Modules
                    .FirstOrDefaultAsync(m => m.Name == ApplicationTypeNames.BoilerInspection);
                if (inspectionModule == null)
                {
                    _logger.LogWarning("Boiler Inspection module not found in DB — run migration.");
                    return;
                }

                // Find the original boiler registration approval request to get the workflow
                var originalApprovalReq = await _db.Set<ApplicationApprovalRequest>()
                    .Where(r => r.ApplicationRegistrationId == applicationRegistrationId)
                    .OrderBy(r => r.CreatedDate)
                    .FirstOrDefaultAsync();
                if (originalApprovalReq == null) return;

                var currentLevel = await _db.Set<ApplicationWorkFlowLevel>()
                    .FirstOrDefaultAsync(l => l.Id == originalApprovalReq.ApplicationWorkFlowLevelId);
                if (currentLevel == null) return;

                // Get level 1 (lowest LevelNumber) of the same workflow
                var level1 = await _db.Set<ApplicationWorkFlowLevel>()
                    .Where(l => l.ApplicationWorkFlowId == currentLevel.ApplicationWorkFlowId)
                    .OrderBy(l => l.LevelNumber)
                    .FirstOrDefaultAsync();
                if (level1 == null) return;

                _db.Set<ApplicationApprovalRequest>().Add(new ApplicationApprovalRequest
                {
                    ModuleId = inspectionModule.Id,
                    ApplicationRegistrationId = applicationRegistrationId,
                    ApplicationWorkFlowLevelId = level1.Id,
                    Status = ApplicationStatus.Pending,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                });

                // Update boiler registration status to "Under Inspection"
                var boilerReg = await _db.BoilerRegistrations
                    .FirstOrDefaultAsync(b => b.ApplicationId == appReg.ApplicationId);
                if (boilerReg != null)
                {
                    boilerReg.Status = "Under Inspection";
                    boilerReg.UpdatedAt = DateTime.Now;
                }

                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create inspection approval request for applicationRegistrationId {Id}", applicationRegistrationId);
            }
        }

        public async Task<InspectorApplicationInspectionDto?> GetInspectionAsync(Guid assignmentId)
        {
            var inspection = await _db.InspectorApplicationInspections
                .FirstOrDefaultAsync(i => i.InspectorApplicationAssignmentId == assignmentId);
            return inspection == null ? null : MapInspection(inspection);
        }

        private static InspectorApplicationInspectionDto MapInspection(InspectorApplicationInspection i) =>
            new InspectorApplicationInspectionDto
            {
                Id = i.Id,
                InspectorApplicationAssignmentId = i.InspectorApplicationAssignmentId,
                InspectionDate = i.InspectionDate,
                BoilerCondition = i.BoilerCondition,
                MaxAllowableWorkingPressure = i.MaxAllowableWorkingPressure,
                Observations = i.Observations,
                DefectsFound = i.DefectsFound,
                DefectDetails = i.DefectDetails,
                InspectionReportNumber = i.InspectionReportNumber,
                HydraulicTestPressure = i.HydraulicTestPressure,
                HydraulicTestDuration = i.HydraulicTestDuration,
                JointsCondition = i.JointsCondition,
                RivetsCondition = i.RivetsCondition,
                PlatingCondition = i.PlatingCondition,
                StaysCondition = i.StaysCondition,
                CrownCondition = i.CrownCondition,
                FireboxCondition = i.FireboxCondition,
                FusiblePlugCondition = i.FusiblePlugCondition,
                FireTubesCondition = i.FireTubesCondition,
                FlueFurnaceCondition = i.FlueFurnaceCondition,
                SmokeBoxCondition = i.SmokeBoxCondition,
                SteamDrumCondition = i.SteamDrumCondition,
                SafetyValveCondition = i.SafetyValveCondition,
                PressureGaugeCondition = i.PressureGaugeCondition,
                FeedCheckCondition = i.FeedCheckCondition,
                StopValveCondition = i.StopValveCondition,
                BlowDownCondition = i.BlowDownCondition,
                EconomiserCondition = i.EconomiserCondition,
                SuperheaterCondition = i.SuperheaterCondition,
                AirPressureGaugeCondition = i.AirPressureGaugeCondition,
                AllowedWorkingPressure = i.AllowedWorkingPressure,
                ProvisionalOrderNumber = i.ProvisionalOrderNumber,
                ProvisionalOrderDate = i.ProvisionalOrderDate,
                BoilerAttendantName = i.BoilerAttendantName,
                BoilerAttendantCertNo = i.BoilerAttendantCertNo,
                FeeAmount = i.FeeAmount,
                ChallanNumber = i.ChallanNumber,
                CreatedDate = i.CreatedDate
            };
    }
}
