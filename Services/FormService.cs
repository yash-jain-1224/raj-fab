using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;
using System.Text.Json;

namespace RajFabAPI.Services
{
    public class FormService : IFormService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDynamicTableService _dynamicTableService;

        public FormService(ApplicationDbContext context, IDynamicTableService dynamicTableService)
        {
            _context = context;
            _dynamicTableService = dynamicTableService;
        }

        public async Task<IEnumerable<FormResponseDto>> GetAllFormsAsync()
        {
            var forms = await _context.Forms
                .Include(f => f.Module)
                .Include(f => f.Sections)
                .Include(f => f.WorkflowConfig)
                .OrderBy(f => f.Title)
                .ToListAsync();

            return forms.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<FormResponseDto>> GetFormsByModuleAsync(Guid moduleId)
        {
            var forms = await _context.Forms
                .Include(f => f.Module)
                .Include(f => f.Sections)
                .Include(f => f.WorkflowConfig)
                .Where(f => f.ModuleId == moduleId)
                .OrderBy(f => f.Title)
                .ToListAsync();

            return forms.Select(MapToResponseDto);
        }

        public async Task<FormResponseDto?> GetFormByIdAsync(Guid id)
        {
            var form = await _context.Forms
                .Include(f => f.Module)
                .Include(f => f.Sections)
                .Include(f => f.WorkflowConfig)
                .FirstOrDefaultAsync(f => f.Id == id);

            return form == null ? null : MapToResponseDto(form);
        }

        public async Task<FormResponseDto> CreateFormAsync(CreateFormDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var form = new DynamicForm
                {
                    ModuleId = dto.ModuleId,
                    Title = dto.Title,
                    Description = dto.Description,
                    FieldsJson = JsonSerializer.Serialize(dto.Fields, new JsonSerializerOptions 
                    { 
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                    })
                };

                _context.Forms.Add(form);
                await _context.SaveChangesAsync();

                // Add sections if provided
                if (dto.Sections?.Any() == true)
                {
                    var sections = dto.Sections.Select(s => new FormSection
                    {
                        FormId = form.Id,
                        Name = s.Name,
                        Description = s.Description ?? string.Empty,
                        Order = s.Order,
                        Collapsible = s.Collapsible
                    }).ToList();

                    _context.FormSections.AddRange(sections);
                    await _context.SaveChangesAsync();
                }

                // Add workflow configuration if provided
                if (dto.Workflow != null)
                {
                    var workflow = new WorkflowConfig
                    {
                        FormId = form.Id,
                        OnSubmitApiEndpoint = dto.Workflow.OnSubmit?.ApiEndpoint,
                        OnSubmitMethod = dto.Workflow.OnSubmit?.Method ?? "POST",
                        OnSubmitNotificationEmail = dto.Workflow.OnSubmit?.NotificationEmail,
                        OnSubmitRedirectUrl = dto.Workflow.OnSubmit?.RedirectUrl,
                        OnSubmitCustomActions = dto.Workflow.OnSubmit?.CustomActions?.Any() == true 
                            ? JsonSerializer.Serialize(dto.Workflow.OnSubmit.CustomActions) : null,
                        OnApprovalApiEndpoint = dto.Workflow.OnApproval?.ApiEndpoint,
                        OnApprovalNotificationEmail = dto.Workflow.OnApproval?.NotificationEmail,
                        OnApprovalCustomActions = dto.Workflow.OnApproval?.CustomActions?.Any() == true 
                            ? JsonSerializer.Serialize(dto.Workflow.OnApproval.CustomActions) : null
                    };

                    _context.WorkflowConfigs.Add(workflow);
                    await _context.SaveChangesAsync();
                }

                // Create dynamic table for the module if it doesn't exist
                var module = await _context.Modules.FindAsync(dto.ModuleId);
                if (module != null)
                {
                    await _dynamicTableService.CreateTableForModuleAsync(module, dto.Fields);
                }

                await transaction.CommitAsync();

                // Load the module and related data for response
                await _context.Entry(form).Reference(f => f.Module).LoadAsync();
                await _context.Entry(form).Collection(f => f.Sections).LoadAsync();
                await _context.Entry(form).Reference(f => f.WorkflowConfig).LoadAsync();

                return MapToResponseDto(form);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<FormResponseDto?> UpdateFormAsync(Guid id, UpdateFormDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var form = await _context.Forms
                    .Include(f => f.Module)
                    .Include(f => f.Sections)
                    .Include(f => f.WorkflowConfig)
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (form == null)
                    return null;

                if (!string.IsNullOrEmpty(dto.Title))
                    form.Title = dto.Title;

                if (dto.Description != null)
                    form.Description = dto.Description;

                if (dto.Fields != null)
                {
                    form.FieldsJson = JsonSerializer.Serialize(dto.Fields, new JsonSerializerOptions 
                    { 
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                    });

                    // Update dynamic table
                    if (form.Module != null)
                    {
                        await _dynamicTableService.UpdateTableForModuleAsync(form.Module, dto.Fields);
                    }
                }

                if (dto.IsActive.HasValue)
                    form.IsActive = dto.IsActive.Value;

                // Update sections
                if (dto.Sections != null)
                {
                    // Remove existing sections
                    _context.FormSections.RemoveRange(form.Sections);
                    
                    // Add new sections
                    var sections = dto.Sections.Select(s => new FormSection
                    {
                        FormId = form.Id,
                        Name = s.Name,
                        Description = s.Description ?? string.Empty,
                        Order = s.Order,
                        Collapsible = s.Collapsible
                    }).ToList();

                    _context.FormSections.AddRange(sections);
                }

                // Update workflow configuration
                if (dto.Workflow != null)
                {
                    if (form.WorkflowConfig == null)
                    {
                        form.WorkflowConfig = new WorkflowConfig { FormId = form.Id };
                        _context.WorkflowConfigs.Add(form.WorkflowConfig);
                    }

                    form.WorkflowConfig.OnSubmitApiEndpoint = dto.Workflow.OnSubmit?.ApiEndpoint;
                    form.WorkflowConfig.OnSubmitMethod = dto.Workflow.OnSubmit?.Method ?? "POST";
                    form.WorkflowConfig.OnSubmitNotificationEmail = dto.Workflow.OnSubmit?.NotificationEmail;
                    form.WorkflowConfig.OnSubmitRedirectUrl = dto.Workflow.OnSubmit?.RedirectUrl;
                    form.WorkflowConfig.OnSubmitCustomActions = dto.Workflow.OnSubmit?.CustomActions?.Any() == true 
                        ? JsonSerializer.Serialize(dto.Workflow.OnSubmit.CustomActions) : null;
                    form.WorkflowConfig.OnApprovalApiEndpoint = dto.Workflow.OnApproval?.ApiEndpoint;
                    form.WorkflowConfig.OnApprovalNotificationEmail = dto.Workflow.OnApproval?.NotificationEmail;
                    form.WorkflowConfig.OnApprovalCustomActions = dto.Workflow.OnApproval?.CustomActions?.Any() == true 
                        ? JsonSerializer.Serialize(dto.Workflow.OnApproval.CustomActions) : null;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return MapToResponseDto(form);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteFormAsync(Guid id)
        {
            var form = await _context.Forms.FindAsync(id);
            if (form == null)
                return false;

            _context.Forms.Remove(form);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> FormExistsAsync(Guid id)
        {
            return await _context.Forms.AnyAsync(f => f.Id == id);
        }

        private static FormResponseDto MapToResponseDto(DynamicForm form)
        {
            var fields = new List<FormField>();
            
            if (!string.IsNullOrEmpty(form.FieldsJson))
            {
                try
                {
                    fields = JsonSerializer.Deserialize<List<FormField>>(form.FieldsJson, 
                        new JsonSerializerOptions 
                        { 
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                        }) ?? new List<FormField>();
                }
                catch (JsonException)
                {
                    // Handle deserialization error gracefully
                    fields = new List<FormField>();
                }
            }

            var sections = form.Sections?.Select(s => new FormSectionDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Order = s.Order,
                Collapsible = s.Collapsible
            }).ToList() ?? new List<FormSectionDto>();

            WorkflowConfigDto? workflow = null;
            if (form.WorkflowConfig != null)
            {
                var onSubmitActions = !string.IsNullOrEmpty(form.WorkflowConfig.OnSubmitCustomActions)
                    ? JsonSerializer.Deserialize<List<string>>(form.WorkflowConfig.OnSubmitCustomActions) ?? new List<string>()
                    : new List<string>();

                var onApprovalActions = !string.IsNullOrEmpty(form.WorkflowConfig.OnApprovalCustomActions)
                    ? JsonSerializer.Deserialize<List<string>>(form.WorkflowConfig.OnApprovalCustomActions) ?? new List<string>()
                    : new List<string>();

                workflow = new WorkflowConfigDto
                {
                    OnSubmit = new OnSubmitConfigDto
                    {
                        ApiEndpoint = form.WorkflowConfig.OnSubmitApiEndpoint,
                        Method = form.WorkflowConfig.OnSubmitMethod,
                        NotificationEmail = form.WorkflowConfig.OnSubmitNotificationEmail,
                        RedirectUrl = form.WorkflowConfig.OnSubmitRedirectUrl,
                        CustomActions = onSubmitActions
                    },
                    OnApproval = new OnApprovalConfigDto
                    {
                        ApiEndpoint = form.WorkflowConfig.OnApprovalApiEndpoint,
                        NotificationEmail = form.WorkflowConfig.OnApprovalNotificationEmail,
                        CustomActions = onApprovalActions
                    }
                };
            }

            return new FormResponseDto
            {
                Id = form.Id,
                ModuleId = form.ModuleId,
                ModuleName = form.Module?.Name ?? string.Empty,
                Title = form.Title,
                Description = form.Description,
                Fields = fields,
                Sections = sections,
                Workflow = workflow,
                IsActive = form.IsActive,
                CreatedAt = form.CreatedAt,
                UpdatedAt = form.UpdatedAt
            };
        }
    }
}