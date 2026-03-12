using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
    public class BoilerWorkflowService : IBoilerWorkflowService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBoilerRegistartionService _boilerRegService;

        public BoilerWorkflowService(
            ApplicationDbContext context,
            IBoilerRegistartionService boilerRegService)
        {
            _context = context;
            _boilerRegService = boilerRegService;
        }

        // ────────────────────────────────────────────────────────────────────
        // MANAGEMENT PAGE
        // ────────────────────────────────────────────────────────────────────

        public async Task<BoilerWorkflowManagementResponseDto> GetBoilerWorkflowByOfficeAsync(Guid officeId)
        {
            // Part 1: read from existing ApplicationWorkFlow (filter by application_type LIKE '%Boiler%')
            var part1Workflow = await _context.ApplicationWorkFlows
                .Include(w => w.Module)
                .Include(w => w.Levels)
                .Where(w => w.OfficeId == officeId && w.IsActive
                         && w.Module.Name.Contains("Boiler"))
                .FirstOrDefaultAsync();

            BoilerWorkflowPart1Dto? part1 = null;
            if (part1Workflow != null)
            {
                var levels = await _context.ApplicationWorkFlowLevels
                    .Where(l => l.ApplicationWorkFlowId == part1Workflow.Id && l.IsActive)
                    .OrderBy(l => l.LevelNumber)
                    .ToListAsync();

                part1 = new BoilerWorkflowPart1Dto
                {
                    WorkflowId = part1Workflow.Id,
                    ApplicationType = part1Workflow.Module?.Name,
                    LevelCount = part1Workflow.LevelCount,
                    Levels = levels.Select(l => new InspectionScrutinyLevelDto
                    {
                        Id = l.Id,
                        LevelNumber = l.LevelNumber,
                        OfficePostId = l.RoleId,
                    }).ToList()
                };
            }

            // Part 2: find Inspector assigned to this office
            BoilerWorkflowPart2Dto? part2 = null;
            var inspectorRole = await _context.Roles
                .Include(r => r.Post)
                .Where(r => r.OfficeId == officeId
                         && r.Post.Name.ToLower().Contains("inspector"))
                .FirstOrDefaultAsync();

            if (inspectorRole != null)
            {
                var inspectorUser = await _context.UserRoles
                    .Include(ur => ur.User)
                    .Where(ur => ur.RoleId == inspectorRole.Id)
                    .Select(ur => ur.User)
                    .FirstOrDefaultAsync();

                part2 = new BoilerWorkflowPart2Dto
                {
                    InspectorName = inspectorUser?.FullName,
                    InspectorPost = inspectorRole.Post.Name,
                    InspectorUserId = inspectorUser?.Id
                };
            }

            // Part 3: InspectionScrutinyWorkflow for this office
            var part3Workflow = await _context.InspectionScrutinyWorkflows
                .Include(w => w.Levels)
                .Include(w => w.Office)
                .Where(w => w.OfficeId == officeId && w.IsActive)
                .FirstOrDefaultAsync();

            InspectionScrutinyWorkflowResponseDto? part3 = null;
            if (part3Workflow != null)
            {
                part3 = await MapToWorkflowResponseDto(part3Workflow);
            }

            return new BoilerWorkflowManagementResponseDto
            {
                Part1 = part1,
                Part2 = part2,
                Part3 = part3
            };
        }

        public async Task<InspectionScrutinyWorkflowResponseDto> SaveInspectionScrutinyWorkflowAsync(
            SaveInspectionScrutinyWorkflowDto dto)
        {
            if (dto.LevelCount != 2 && dto.LevelCount != 3)
                throw new InvalidOperationException("LevelCount must be 2 or 3.");

            if (dto.LevelCount == 3 && dto.Level2OfficePostId == null)
                throw new InvalidOperationException("Level2OfficePostId is required for a 3-level workflow.");

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // Deactivate any existing config for this office
                var existing = await _context.InspectionScrutinyWorkflows
                    .Include(w => w.Levels)
                    .Where(w => w.OfficeId == dto.OfficeId && w.IsActive)
                    .FirstOrDefaultAsync();

                if (existing != null)
                {
                    existing.IsActive = false;
                    existing.UpdatedAt = DateTime.Now;
                }

                // Resolve Level 1 post: same as Part 1 (Application Scrutiny) Level 1
                var part1Level1 = await _context.ApplicationWorkFlows
                    .Where(w => w.OfficeId == dto.OfficeId && w.IsActive
                             && w.Module.Name.Contains("Boiler"))
                    .SelectMany(w => w.Levels)
                    .Where(l => l.LevelNumber == 1 && l.IsActive)
                    .Select(l => l.RoleId)
                    .FirstOrDefaultAsync();

                // Resolve Chief Inspector role (by Post name)
                var chiefRole = await _context.Roles
                    .Include(r => r.Post)
                    .Where(r => r.Post.Name.Contains("Chief Inspector of Factories"))
                    .Select(r => r.Id)
                    .FirstOrDefaultAsync();

                var workflow = new InspectionScrutinyWorkflow
                {
                    OfficeId = dto.OfficeId,
                    LevelCount = dto.LevelCount,
                    IsBidirectional = true,
                    IsActive = true
                };

                // Level 1 — auto from Part 1 L1
                workflow.Levels.Add(new InspectionScrutinyLevel
                {
                    LevelNumber = 1,
                    OfficePostId = part1Level1,
                    IsPrefilled = true,
                    PrefillSource = "APPLICATION_SCRUTINY_L1"
                });

                if (dto.LevelCount == 2)
                {
                    // Level 2 — Chief (fixed)
                    workflow.Levels.Add(new InspectionScrutinyLevel
                    {
                        LevelNumber = 2,
                        OfficePostId = chiefRole,
                        IsPrefilled = true,
                        PrefillSource = "CHIEF_FIXED"
                    });
                }
                else
                {
                    // Level 2 — admin-selected
                    workflow.Levels.Add(new InspectionScrutinyLevel
                    {
                        LevelNumber = 2,
                        OfficePostId = dto.Level2OfficePostId!.Value,
                        IsPrefilled = false
                    });

                    // Level 3 — Chief (fixed)
                    workflow.Levels.Add(new InspectionScrutinyLevel
                    {
                        LevelNumber = 3,
                        OfficePostId = chiefRole,
                        IsPrefilled = true,
                        PrefillSource = "CHIEF_FIXED"
                    });
                }

                _context.InspectionScrutinyWorkflows.Add(workflow);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return await MapToWorkflowResponseDto(workflow);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // CHIEF REMARKS MASTER
        // ────────────────────────────────────────────────────────────────────

        public async Task<IEnumerable<ChiefRemarkDto>> GetChiefRemarksAsync()
        {
            return await _context.ChiefInspectionScrutinyRemarks
                .Where(r => r.IsActive)
                .OrderBy(r => r.DisplayOrder)
                .Select(r => new ChiefRemarkDto
                {
                    Id = r.Id,
                    RemarkText = r.RemarkText,
                    IsActive = r.IsActive,
                    DisplayOrder = r.DisplayOrder
                })
                .ToListAsync();
        }

        public async Task<ChiefRemarkDto> CreateChiefRemarkAsync(SaveChiefRemarkDto dto)
        {
            var remark = new ChiefInspectionScrutinyRemark
            {
                RemarkText = dto.RemarkText,
                DisplayOrder = dto.DisplayOrder,
                IsActive = true
            };
            _context.ChiefInspectionScrutinyRemarks.Add(remark);
            await _context.SaveChangesAsync();

            return new ChiefRemarkDto
            {
                Id = remark.Id,
                RemarkText = remark.RemarkText,
                IsActive = remark.IsActive,
                DisplayOrder = remark.DisplayOrder
            };
        }

        public async Task<ChiefRemarkDto?> UpdateChiefRemarkAsync(Guid id, SaveChiefRemarkDto dto)
        {
            var remark = await _context.ChiefInspectionScrutinyRemarks.FindAsync(id);
            if (remark == null) return null;

            remark.RemarkText = dto.RemarkText;
            remark.DisplayOrder = dto.DisplayOrder;
            remark.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return new ChiefRemarkDto
            {
                Id = remark.Id,
                RemarkText = remark.RemarkText,
                IsActive = remark.IsActive,
                DisplayOrder = remark.DisplayOrder
            };
        }

        public async Task<bool> DeleteChiefRemarkAsync(Guid id)
        {
            var remark = await _context.ChiefInspectionScrutinyRemarks.FindAsync(id);
            if (remark == null) return false;

            remark.IsActive = false;
            remark.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        // ────────────────────────────────────────────────────────────────────
        // APPLICATION STATE
        // ────────────────────────────────────────────────────────────────────

        public async Task<BoilerApplicationStateDto?> GetApplicationStateAsync(string applicationId)
        {
            var state = await _context.BoilerApplicationStates
                .Include(s => s.AssignedInspector)
                .FirstOrDefaultAsync(s => s.ApplicationId == applicationId);

            return state == null ? null : MapStateToDto(state);
        }

        // ────────────────────────────────────────────────────────────────────
        // PART 1: FORWARD TO INSPECTOR
        // ────────────────────────────────────────────────────────────────────

        public async Task<BoilerApplicationStateDto> ForwardToInspectorAsync(
            ForwardToInspectorDto dto, Guid actorUserId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var state = await GetOrCreateStateAsync(dto.ApplicationId);

                state.InspectorActionsEnabled = true;
                state.AuthorityForwardedAt = DateTime.Now;
                state.CurrentStatus = BoilerWorkflowStatuses.PendingAtInspector;
                state.CurrentPart = 2;
                state.UpdatedAt = DateTime.Now;

                await LogActionAsync(dto.ApplicationId, 1, actorUserId, null, null, null,
                    "FORWARD_TO_INSPECTOR", null, null, null);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return MapStateToDto(state);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // PART 2: INSPECTOR ACTIONS
        // ────────────────────────────────────────────────────────────────────

        public async Task<BoilerApplicationStateDto> BackToCitizenAsync(
            BackToCitizenDto dto, Guid actorUserId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var state = await GetOrCreateStateAsync(dto.ApplicationId);

                state.CurrentStatus = dto.ActorRole == "CHIEF"
                    ? BoilerWorkflowStatuses.PendingAtCitizenChiefRemarks
                    : BoilerWorkflowStatuses.PendingAtCitizenInspectorRemarks;
                state.UpdatedAt = DateTime.Now;

                await LogActionAsync(dto.ApplicationId, state.CurrentPart, actorUserId, null,
                    state.CurrentLevel, null, "BACK_TO_CITIZEN", dto.Remarks, null, null);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return MapStateToDto(state);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<BoilerApplicationStateDto> SendToApplicationScrutinyAsync(
            SendToAppScrutinyDto dto, Guid actorUserId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var state = await GetOrCreateStateAsync(dto.ApplicationId);

                state.CurrentStatus = BoilerWorkflowStatuses.PendingAtLevel(1);
                state.CurrentPart = 1;
                state.CurrentLevel = 1;
                state.UpdatedAt = DateTime.Now;

                await LogActionAsync(dto.ApplicationId, 2, actorUserId, null,
                    null, 1, "SEND_TO_APP_SCRUTINY", dto.Remarks, null, null);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return MapStateToDto(state);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<InspectionScheduleResponseDto> SaveInspectionScheduleAsync(
            SaveInspectionScheduleDto dto, Guid actorUserId)
        {
            // Parse time
            if (!TimeSpan.TryParse(dto.InspectionTime, out var timeSpan))
                throw new InvalidOperationException("Invalid InspectionTime format. Use HH:mm.");

            // Must be a future date
            var scheduledDateTime = dto.InspectionDate.Date + timeSpan;
            if (scheduledDateTime <= DateTime.Now)
                throw new InvalidOperationException("Inspection date/time must be in the future.");

            // Check if already scheduled — if locked, disallow reschedule
            var existing = await _context.InspectionSchedules
                .Where(s => s.ApplicationId == dto.ApplicationId)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            if (existing != null && existing.IsLocked)
                throw new InvalidOperationException(
                    "Inspection schedule is locked. Scheduled time has passed.");

            InspectionSchedule schedule;
            if (existing != null)
            {
                // Reschedule
                existing.InspectionDate = dto.InspectionDate;
                existing.InspectionTime = timeSpan;
                existing.PlaceAddress = dto.PlaceAddress;
                existing.InspectionType = dto.InspectionType;
                existing.EstimatedDuration = dto.EstimatedDuration;
                existing.InspectorNotes = dto.InspectorNotes;
                existing.UpdatedAt = DateTime.Now;
                schedule = existing;
            }
            else
            {
                schedule = new InspectionSchedule
                {
                    ApplicationId = dto.ApplicationId,
                    InspectorId = dto.InspectorId,
                    InspectionDate = dto.InspectionDate,
                    InspectionTime = timeSpan,
                    PlaceAddress = dto.PlaceAddress,
                    InspectionType = dto.InspectionType,
                    EstimatedDuration = dto.EstimatedDuration,
                    InspectorNotes = dto.InspectorNotes
                };
                _context.InspectionSchedules.Add(schedule);

                // Update status to "Inspection Scheduled"
                var state = await GetOrCreateStateAsync(dto.ApplicationId);
                state.CurrentStatus = BoilerWorkflowStatuses.InspectionScheduled;
                state.UpdatedAt = DateTime.Now;
            }

            await LogActionAsync(dto.ApplicationId, 2, actorUserId, null, null, null,
                "INSPECTION_SCHEDULED", null, null, null);

            await _context.SaveChangesAsync();
            return MapScheduleToDto(schedule);
        }

        public async Task<InspectionScheduleResponseDto?> GetInspectionScheduleAsync(string applicationId)
        {
            var schedule = await _context.InspectionSchedules
                .Where(s => s.ApplicationId == applicationId)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            if (schedule == null) return null;

            // Auto-lock if datetime passed
            var scheduledDt = schedule.InspectionDate.Date + schedule.InspectionTime;
            if (!schedule.IsLocked && scheduledDt <= DateTime.Now)
            {
                schedule.IsLocked = true;
                schedule.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return MapScheduleToDto(schedule);
        }

        public async Task<InspectionFormSubmissionResponseDto> SaveInspectionFormAsync(
            SaveInspectionFormDto dto, Guid actorUserId)
        {
            var existing = await _context.InspectionFormSubmissions
                .FirstOrDefaultAsync(f => f.ApplicationId == dto.ApplicationId);

            InspectionFormSubmission submission;
            if (existing != null)
            {
                existing.FormData = dto.FormData;
                existing.Photos = dto.Photos;
                existing.Documents = dto.Documents;
                existing.UpdatedAt = DateTime.Now;
                submission = existing;
            }
            else
            {
                submission = new InspectionFormSubmission
                {
                    ApplicationId = dto.ApplicationId,
                    InspectorId = dto.InspectorId,
                    FormData = dto.FormData,
                    Photos = dto.Photos,
                    Documents = dto.Documents
                };
                _context.InspectionFormSubmissions.Add(submission);
            }

            await _context.SaveChangesAsync();
            return MapFormToDto(submission);
        }

        public async Task<InspectionFormSubmissionResponseDto?> GetInspectionFormAsync(string applicationId)
        {
            var form = await _context.InspectionFormSubmissions
                .FirstOrDefaultAsync(f => f.ApplicationId == applicationId);

            return form == null ? null : MapFormToDto(form);
        }

        public async Task<BoilerApplicationStateDto> ForwardToLdcAsync(
            ForwardToLdcDto dto, Guid actorUserId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // Verify eSign completed
                var form = await _context.InspectionFormSubmissions
                    .FirstOrDefaultAsync(f => f.ApplicationId == dto.ApplicationId);

                if (form == null || string.IsNullOrEmpty(form.ESignData))
                    throw new InvalidOperationException(
                        "Inspection Form must be e-Signed before forwarding to LDC.");

                var state = await GetOrCreateStateAsync(dto.ApplicationId);
                state.CurrentStatus = BoilerWorkflowStatuses.PendingAtLdc;
                state.CurrentPart = 3;
                state.CurrentLevel = 1;
                state.UpdatedAt = DateTime.Now;

                await LogActionAsync(dto.ApplicationId, 2, actorUserId, null, null, 1,
                    "FORWARD_TO_LDC", null, null, null);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return MapStateToDto(state);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // PART 3: INSPECTION SCRUTINY
        // ────────────────────────────────────────────────────────────────────

        public async Task<BoilerApplicationStateDto> Part3ForwardAsync(
            Part3ForwardDto dto, Guid actorUserId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var state = await GetOrCreateStateAsync(dto.ApplicationId);

                // Find the workflow for this application's office
                var workflow = await GetWorkflowForApplicationAsync(dto.ApplicationId);
                if (workflow == null)
                    throw new InvalidOperationException("No Inspection Scrutiny workflow configured for this office.");

                int nextLevel = state.CurrentLevel + 1;
                if (nextLevel > workflow.LevelCount)
                    throw new InvalidOperationException("Already at the highest level.");

                state.CurrentLevel = nextLevel;
                state.CurrentStatus = nextLevel == workflow.LevelCount
                    ? BoilerWorkflowStatuses.PendingAtChief
                    : BoilerWorkflowStatuses.PendingAtInspectionScrutinyLevel(nextLevel);
                state.UpdatedAt = DateTime.Now;

                await LogActionAsync(dto.ApplicationId, 3, actorUserId, null,
                    state.CurrentLevel - 1, nextLevel, "PART3_FORWARD", dto.Remarks, null, null);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return MapStateToDto(state);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<BoilerApplicationStateDto> ForwardToOthersAsync(
            ForwardToOthersDto dto, Guid actorUserId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var state = await GetOrCreateStateAsync(dto.ApplicationId);

                await LogActionAsync(dto.ApplicationId, 3, actorUserId, null,
                    state.CurrentLevel, null, "FORWARD_TO_OTHERS", dto.Remarks, null, null);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return MapStateToDto(state);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<BoilerApplicationStateDto> ForwardToChiefAsync(
            Part3ForwardDto dto, Guid actorUserId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var state = await GetOrCreateStateAsync(dto.ApplicationId);

                var workflow = await GetWorkflowForApplicationAsync(dto.ApplicationId);
                state.CurrentLevel = workflow?.LevelCount ?? state.CurrentLevel;
                state.CurrentStatus = BoilerWorkflowStatuses.PendingAtChief;
                state.UpdatedAt = DateTime.Now;

                await LogActionAsync(dto.ApplicationId, 3, actorUserId, null,
                    null, state.CurrentLevel, "FORWARD_TO_CHIEF", dto.Remarks, null, null);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return MapStateToDto(state);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<BoilerApplicationStateDto> ChiefForwardToLdcAsync(
            ChiefForwardToLdcDto dto, Guid actorUserId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var state = await GetOrCreateStateAsync(dto.ApplicationId);

                // Enforce cycle rule: "Generate Registration Number" hidden until chief_cycle_count >= 2
                if (dto.ActionValue == "GENERATE_REG_NUMBER" && state.ChiefCycleCount < 2)
                    throw new InvalidOperationException(
                        "Generate Registration Number is available only after 2 completed Chief forward-receive cycles.");

                state.ChiefCycleCount += 1;
                state.LastChiefActionValue = dto.ActionValue;
                state.CurrentLevel = 1;
                state.CurrentStatus = BoilerWorkflowStatuses.PendingAtLdc;
                state.UpdatedAt = DateTime.Now;

                await LogActionAsync(dto.ApplicationId, 3, actorUserId, null,
                    null, 1, "CHIEF_FORWARD_TO_LDC", dto.Remarks, state.ChiefCycleCount, dto.ActionValue);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return MapStateToDto(state);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<BoilerApplicationStateDto> GenerateRegistrationNumberAsync(
            GenerateRegistrationNumberDto dto, Guid actorUserId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var state = await GetOrCreateStateAsync(dto.ApplicationId);

                if (state.LastChiefActionValue != "GENERATE_REG_NUMBER")
                    throw new InvalidOperationException(
                        "Chief has not instructed to generate a Registration Number for this application.");

                // Generate registration number via existing Boiler Register Service
                var regNo = await _boilerRegService.GenerateBoilerRegistrationNoAsync();

                state.RegistrationNumber = regNo;
                state.CurrentStatus = BoilerWorkflowStatuses.RegistrationNumberGenerated;
                state.LastChiefActionValue = null;
                state.UpdatedAt = DateTime.Now;

                await LogActionAsync(dto.ApplicationId, 3, actorUserId, null,
                    1, null, "GENERATE_REG_NUMBER", regNo, null, null);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return MapStateToDto(state);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<BoilerApplicationStateDto> IntimateToInspectorAsync(
            IntimateToInspectorDto dto, Guid actorUserId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var state = await GetOrCreateStateAsync(dto.ApplicationId);

                if (string.IsNullOrEmpty(state.RegistrationNumber))
                    throw new InvalidOperationException(
                        "Registration number must be generated before intimating Inspector.");

                state.CurrentStatus = BoilerWorkflowStatuses.PendingCertificateGeneration;
                state.UpdatedAt = DateTime.Now;

                await LogActionAsync(dto.ApplicationId, 3, actorUserId, state.AssignedInspectorId,
                    null, null, "INTIMATE_TO_INSPECTOR", null, null, null);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return MapStateToDto(state);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // PART 4: CERTIFICATE
        // ────────────────────────────────────────────────────────────────────

        public async Task<BoilerApplicationStateDto> GenerateCertificateAsync(
            GenerateInspectionCertificateDto dto, Guid actorUserId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var state = await GetOrCreateStateAsync(dto.ApplicationId);

                if (state.CurrentStatus != BoilerWorkflowStatuses.PendingCertificateGeneration)
                    throw new InvalidOperationException("Application is not in 'Pending Certificate Generation' status.");

                if (state.AssignedInspectorId != actorUserId)
                    throw new InvalidOperationException("Only the assigned Inspector can generate the certificate.");

                state.CurrentStatus = BoilerWorkflowStatuses.CertificateGenerated;
                state.UpdatedAt = DateTime.Now;

                await LogActionAsync(dto.ApplicationId, 4, actorUserId, null,
                    null, null, "GENERATE_CERTIFICATE", null, null, null);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return MapStateToDto(state);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // WORKFLOW LOGS
        // ────────────────────────────────────────────────────────────────────

        public async Task<IEnumerable<BoilerWorkflowLogDto>> GetWorkflowLogsAsync(string applicationId)
        {
            var logs = await _context.BoilerWorkflowLogs
                .Where(l => l.ApplicationId == applicationId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            var userIds = logs
                .SelectMany(l => new[] { l.FromUserId, l.ToUserId })
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var userNames = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName);

            return logs.Select(l => new BoilerWorkflowLogDto
            {
                Id = l.Id,
                ApplicationId = l.ApplicationId,
                Part = l.Part,
                FromUserName = l.FromUserId.HasValue && userNames.TryGetValue(l.FromUserId.Value, out var fn) ? fn : null,
                ToUserName = l.ToUserId.HasValue && userNames.TryGetValue(l.ToUserId.Value, out var tn) ? tn : null,
                FromLevel = l.FromLevel,
                ToLevel = l.ToLevel,
                ActionType = l.ActionType,
                Remarks = l.Remarks,
                CycleNumber = l.CycleNumber,
                ChiefActionValue = l.ChiefActionValue,
                CreatedAt = l.CreatedAt
            });
        }

        // ────────────────────────────────────────────────────────────────────
        // HELPERS
        // ────────────────────────────────────────────────────────────────────

        private async Task<BoilerApplicationState> GetOrCreateStateAsync(string applicationId)
        {
            var state = await _context.BoilerApplicationStates
                .Include(s => s.AssignedInspector)
                .FirstOrDefaultAsync(s => s.ApplicationId == applicationId);

            if (state == null)
            {
                state = new BoilerApplicationState
                {
                    ApplicationId = applicationId,
                    CurrentStatus = "Submitted",
                    CurrentPart = 1,
                    CurrentLevel = 1
                };
                _context.BoilerApplicationStates.Add(state);
            }

            return state;
        }

        private async Task LogActionAsync(
            string applicationId, int part,
            Guid? fromUserId, Guid? toUserId,
            int? fromLevel, int? toLevel,
            string actionType, string? remarks,
            int? cycleNumber, string? chiefActionValue)
        {
            _context.BoilerWorkflowLogs.Add(new BoilerWorkflowLog
            {
                ApplicationId = applicationId,
                Part = part,
                FromUserId = fromUserId,
                ToUserId = toUserId,
                FromLevel = fromLevel,
                ToLevel = toLevel,
                ActionType = actionType,
                Remarks = remarks,
                CycleNumber = cycleNumber,
                ChiefActionValue = chiefActionValue
            });
            await Task.CompletedTask;
        }

        private async Task<InspectionScrutinyWorkflow?> GetWorkflowForApplicationAsync(string applicationId)
        {
            var state = await _context.BoilerApplicationStates
                .FirstOrDefaultAsync(s => s.ApplicationId == applicationId);

            // Find inspector's office to get the right workflow
            if (state?.AssignedInspectorId == null) return null;

            var inspectorOffice = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == state.AssignedInspectorId)
                .Select(ur => ur.Role.OfficeId)
                .FirstOrDefaultAsync();

            return await _context.InspectionScrutinyWorkflows
                .Include(w => w.Levels)
                .Where(w => w.OfficeId == inspectorOffice && w.IsActive)
                .FirstOrDefaultAsync();
        }

        private async Task<InspectionScrutinyWorkflowResponseDto> MapToWorkflowResponseDto(
            InspectionScrutinyWorkflow workflow)
        {
            var levels = workflow.Levels.OrderBy(l => l.LevelNumber).ToList();

            var postIds = levels.Select(l => l.OfficePostId).Distinct().ToList();
            var postNames = await _context.Roles
                .Include(r => r.Post)
                .Where(r => postIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id, r => r.Post.Name ?? string.Empty);

            return new InspectionScrutinyWorkflowResponseDto
            {
                Id = workflow.Id,
                OfficeId = workflow.OfficeId,
                OfficeName = workflow.Office?.Name,
                LevelCount = workflow.LevelCount,
                IsBidirectional = workflow.IsBidirectional,
                IsActive = workflow.IsActive,
                Levels = levels.Select(l => new InspectionScrutinyLevelDto
                {
                    Id = l.Id,
                    LevelNumber = l.LevelNumber,
                    OfficePostId = l.OfficePostId,
                    OfficePostName = postNames.TryGetValue(l.OfficePostId, out var name) ? name : null,
                    IsPrefilled = l.IsPrefilled,
                    PrefillSource = l.PrefillSource
                }).ToList()
            };
        }

        private static BoilerApplicationStateDto MapStateToDto(BoilerApplicationState s) => new()
        {
            Id = s.Id,
            ApplicationId = s.ApplicationId,
            CurrentStatus = s.CurrentStatus,
            CurrentPart = s.CurrentPart,
            CurrentLevel = s.CurrentLevel,
            AssignedInspectorId = s.AssignedInspectorId,
            AssignedInspectorName = s.AssignedInspector?.FullName,
            InspectorActionsEnabled = s.InspectorActionsEnabled,
            ChiefCycleCount = s.ChiefCycleCount,
            LastChiefActionValue = s.LastChiefActionValue,
            RegistrationNumber = s.RegistrationNumber,
            CertificatePath = s.CertificatePath
        };

        private static InspectionScheduleResponseDto MapScheduleToDto(InspectionSchedule s)
        {
            var scheduledDt = s.InspectionDate.Date + s.InspectionTime;
            return new InspectionScheduleResponseDto
            {
                Id = s.Id,
                ApplicationId = s.ApplicationId,
                InspectorId = s.InspectorId,
                InspectionDate = s.InspectionDate,
                InspectionTime = s.InspectionTime.ToString(@"hh\:mm"),
                PlaceAddress = s.PlaceAddress,
                InspectionType = s.InspectionType,
                EstimatedDuration = s.EstimatedDuration,
                InspectorNotes = s.InspectorNotes,
                IsLocked = s.IsLocked,
                CanStartInspection = DateTime.Now >= scheduledDt
            };
        }

        private static InspectionFormSubmissionResponseDto MapFormToDto(InspectionFormSubmission f) => new()
        {
            Id = f.Id,
            ApplicationId = f.ApplicationId,
            FormData = f.FormData,
            Photos = f.Photos,
            Documents = f.Documents,
            GeneratedPdfPath = f.GeneratedPdfPath,
            IsESignCompleted = !string.IsNullOrEmpty(f.ESignData),
            SubmittedAt = f.SubmittedAt
        };
    }

    // ── Status Constants ─────────────────────────────────────────────────────

    public static class BoilerWorkflowStatuses
    {
        public const string Draft = "Draft";
        public const string Submitted = "Submitted";
        public const string PendingAtAuthority = "Pending at Authority";
        public const string PendingAtInspector = "Pending at Inspector";
        public const string PendingAtCitizenInspectorRemarks = "Pending at Citizen (Inspector Remarks)";
        public const string PendingAtCitizenChiefRemarks = "Pending at Citizen (Chief Remarks)";
        public const string InspectionScheduled = "Inspection Scheduled";
        public const string InspectionInProgress = "Inspection In Progress";
        public const string InspectionCompleted = "Inspection Completed";
        public const string PendingAtLdc = "Pending at LDC";
        public const string PendingAtChief = "Pending at Chief";
        public const string RegistrationNumberGenerated = "Registration Number Generated";
        public const string PendingCertificateGeneration = "Pending Certificate Generation";
        public const string CertificateGenerated = "Certificate Generated";
        public const string Completed = "Completed";
        public const string Rejected = "Rejected";

        public static string PendingAtLevel(int n) => $"Pending at Level {n}";
        public static string PendingAtInspectionScrutinyLevel(int n) => $"Pending at Level {n} (Inspection Scrutiny)";
    }
}
