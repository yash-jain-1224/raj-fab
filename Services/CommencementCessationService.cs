using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
    public class CommencementCessationService : ICommencementCessationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CommencementCessationService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<CommencementCessationDto>> GetAllAsync()
        {
            var entities = await _context.CommencementCessationApplication.ToListAsync();
            return entities.Select(MapToDto).ToList();
        }

        public async Task<CommencementCessationDto?> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            var entity = await _context.CommencementCessationApplication
                .FirstOrDefaultAsync(x => x.ApplicationId == id);

            return entity == null ? null : MapToDto(entity);
        }

        public async Task<bool> UpdateStatusAndRemark(string registrationId, string status)
        {
            try
            {
                var reg = _context.CommencementCessationApplication.FirstOrDefault(x => x.ApplicationId == registrationId);
                if (reg == null)
                    return false;
                reg.Status = status;
                reg.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<CommencementCessationDto?> CreateAsync(CommencementCessationRequestDto request)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var userIdString = _httpContextAccessor.HttpContext?
                .User?
                .FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrWhiteSpace(userIdString))
                    throw new UnauthorizedAccessException("User is not authenticated");
                var userId = Guid.TryParse(userIdString, out var parsedGuid) ? parsedGuid : Guid.Empty;

                var entity = new CommencementCessationApplication
                {
                    ApplicationId = Guid.NewGuid().ToString().ToUpper(),
                    Type = request.Type,
                    FactoryRegistrationNumber = request.FactoryRegistrationNumber,
                    CessationIntimationDate = request.CessationIntimationDate,
                    CessationIntimationEffectiveDate = request.CessationIntimationEffectiveDate,
                    ApproxDurationOfWork = request.ApproxDurationOfWork,
                    OccupierSignature = request.OccupierSignature,
                    Status = "Pending",
                    Version = 1.0m,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                _context.CommencementCessationApplication.Add(entity);
                var result = await _context.SaveChangesAsync();

                if (result <= 0)
                {
                    throw new Exception("Failed to create Commencement/Cessation application.");
                }


                // Get ModuleId from Modules table (assuming ApplicationTypeId is available in DTO or context)
                var module = await _context.Set<FormModule>().FirstOrDefaultAsync(m => m.Name == "Factory Commencement And Cessation");
                if (module == null)
                    throw new Exception("Module not found for ApplicationTypeId");
                var EstablishmentDetailId = await _context.Set<EstablishmentRegistration>()
                                    .Where(m => m.RegistrationNumber == request.FactoryRegistrationNumber)
                                    .Select(m => m.EstablishmentDetailId)
                                    .FirstOrDefaultAsync();

                var appReg = new ApplicationRegistration
                {
                    Id = Guid.NewGuid().ToString(),
                    ModuleId = module.Id,
                    UserId = userId,
                    ApplicationId = entity.ApplicationId,
                    ApplicationRegistrationNumber = "",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };
                _context.Set<ApplicationRegistration>().Add(appReg);
                await _context.SaveChangesAsync();


                var areaId = await _context.Set<EstablishmentDetail>()
                    .Where(m => m.Id == EstablishmentDetailId)
                    .Select(m => m.AreaId)
                    .FirstOrDefaultAsync();

                var factoryCategoryId = Guid.Parse("EB857143-2FBB-4C6E-88F8-888C3D6DB671");

                var officeApplicationArea = await _context.Set<OfficeApplicationArea>()
                    .FirstOrDefaultAsync(oaa => oaa.CityId == Guid.Parse(areaId));
                if (officeApplicationArea != null)
                {
                    var officeId = officeApplicationArea?.OfficeId;
                    var workflow = await _context.Set<ApplicationWorkFlow>()
                        .FirstOrDefaultAsync(wf => wf.ModuleId == module.Id && wf.FactoryCategoryId == factoryCategoryId && wf.OfficeId == officeId);
                    var workflowLevel = await _context.Set<ApplicationWorkFlowLevel>()
                        .Where(wfl => wfl.ApplicationWorkFlowId == (workflow != null ? workflow.Id : Guid.Empty))
                        .OrderBy(wfl => wfl.LevelNumber)
                        .FirstOrDefaultAsync();

                    if (workflow != null)
                    {
                        var applicationApprovalRequest = new ApplicationApprovalRequest
                        {
                            ModuleId = module.Id,
                            ApplicationRegistrationId = appReg.Id,
                            ApplicationWorkFlowLevelId = workflowLevel.Id,
                            Status = "Pending",
                            CreatedDate = DateTime.Now,
                            UpdatedDate = DateTime.Now
                        };
                        _context.Set<ApplicationApprovalRequest>().Add(applicationApprovalRequest);
                        await _context.SaveChangesAsync();
                    }
                }
                await tx.CommitAsync();

                return MapToDto(entity);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<CommencementCessationDto> UpdateAsync(Guid id, CommencementCessationRequestDto request)
        {
            var entity = await _context.CommencementCessationApplication.FindAsync(id);
            if (entity == null)
            {
                throw new Exception("Application not found.");
            }

            // Update fields
            entity.Type = request.Type;
            entity.FactoryRegistrationNumber = request.FactoryRegistrationNumber;
            entity.CessationIntimationDate = request.CessationIntimationDate;
            entity.CessationIntimationEffectiveDate = request.CessationIntimationEffectiveDate;
            entity.ApproxDurationOfWork = request.ApproxDurationOfWork;
            entity.OccupierSignature = request.OccupierSignature;
            entity.UpdatedDate = DateTime.Now;

            // Optional: Update version if needed
            entity.Version += 0.1m;

            _context.CommencementCessationApplication.Update(entity);
            var result = await _context.SaveChangesAsync();

            if (result <= 0)
            {
                throw new Exception("Failed to update application.");
            }

            return MapToDto(entity);
        }

        private static CommencementCessationDto MapToDto(CommencementCessationApplication entity)
        {
            return new CommencementCessationDto
            {
                Id = entity.Id,
                Type = entity.Type,
                FactoryRegistrationNumber = entity.FactoryRegistrationNumber,
                CessationIntimationDate = entity.CessationIntimationDate,
                CessationIntimationEffectiveDate = entity.CessationIntimationEffectiveDate,
                ApproxDurationOfWork = entity.ApproxDurationOfWork,
                OccupierSignature = entity.OccupierSignature,
                Status = entity.Status,
                Version = entity.Version,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate
            };
        }
        public string GenerateRegistrationNumber()
        {
            var year = DateTime.Now.Year;
            var sequence = DateTime.Now.Ticks.ToString().Substring(8, 6);
            return $"FC{year}{sequence}";
        }
    }
}
