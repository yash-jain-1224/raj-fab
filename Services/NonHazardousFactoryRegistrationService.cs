using RajFabAPI.DTOs;
using RajFabAPI.Models.FactoryModels;
using RajFabAPI.Services.Interface;
using RajFabAPI.Data;
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Models;
using static RajFabAPI.Constants.AppConstants;

namespace RajFabAPI.Services
{
    public class NonHazardousFactoryRegistrationService : INonHazardousFactoryRegistrationService
    {
        private readonly ApplicationDbContext _context;

        public NonHazardousFactoryRegistrationService(ApplicationDbContext context)
        {
            _context = context;
        }


        private async Task<string> GenerateRegistrationNumberAsync()
        {
            var year = DateTime.Now.Year;
            string prefix = $"NHF-{year}-";

            var last = await _context.NonHazardousFactoryRegistrations
                .Where(x => x.RegistrationNo.StartsWith(prefix))
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.RegistrationNo)
                .FirstOrDefaultAsync();

            int next = 1;

            if (!string.IsNullOrEmpty(last))
            {
                var numberPart = last.Replace(prefix, "");

                if (int.TryParse(numberPart, out int lastNumber))
                {
                    next = lastNumber + 1;
                }
            }

            return $"{prefix}{next:D6}";
        }

        private async Task<string> GenerateApplicationNumberAsync()
        {
            var year = DateTime.Now.Year;
            string prefix = $"NHF-";

            var lastApp = await _context.NonHazardousFactoryRegistrations
                .Where(x => x.RegistrationNo.StartsWith(prefix)
                         && x.RegistrationNo.Contains($"/CIFB/{year}"))
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.RegistrationNo)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastApp))
            {
                int dashIndex = lastApp.IndexOf('-');
                int slashIndex = lastApp.IndexOf("/CIFB");

                if (dashIndex != -1 && slashIndex != -1)
                {
                    var numberPart = lastApp.Substring(dashIndex + 1, slashIndex - (dashIndex + 1));

                    if (int.TryParse(numberPart, out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }
            }

            return $"{prefix}{nextNumber:D6}/CIFB/{year}";
        }


        public async Task<NonHazardousFactoryRegistrationDto?> GetByIdAsync(Guid id)
        {
            var result = await (
                from f in _context.NonHazardousFactoryRegistrations.AsNoTracking()
                join app in _context.ApplicationRegistrations.AsNoTracking()
                    on f.RegistrationNo equals app.ApplicationRegistrationNumber into appJoin
                from appReg in appJoin.DefaultIfEmpty()

                where f.Id == id

                select new NonHazardousFactoryRegistrationDto
                {
                    Id = f.Id,

                    ApplicationId = appReg != null ? appReg.ApplicationId : null, 
                    RegistrationNo = f.RegistrationNo,

                    FactoryName = f.FactoryName,
                    ApplicantName = f.ApplicantName,

                    RelationType = f.RelationType,
                    RelationName = f.RelationName,

                    ApplicantAddressLine1 = f.ApplicantAddressLine1,
                    ApplicantAddressLine2 = f.ApplicantAddressLine2,                  
                    TehsilName = f.TehsilName,
                    DistrictName = f.DistrictName,
                    Area = f.Area,
                    Pincode = f.Pincode,
                    SubdivisionName = f.SubdivisionName,
                    DeclarationAccepted = f.DeclarationAccepted,
                    RequiredInfoAccepted = f.RequiredInfoAccepted,
                    VerifyAccepted = f.VerifyAccepted,
                    WorkersLimitAccepted = f.WorkersLimitAccepted,

                    Status = f.Status,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt
                }
            ).FirstOrDefaultAsync();

            return result;
        }

        public async Task<List<NonHazardousFactoryRegistrationDto>> GetAllAsync()
        {
            var result = await (
                from f in _context.NonHazardousFactoryRegistrations.AsNoTracking()
                join app in _context.ApplicationRegistrations.AsNoTracking()
                    on f.RegistrationNo equals app.ApplicationRegistrationNumber into appJoin
                from appReg in appJoin.DefaultIfEmpty()

                select new NonHazardousFactoryRegistrationDto
                {
                    Id = f.Id,

                    ApplicationId = appReg != null ? appReg.ApplicationId : null, 
                    RegistrationNo = f.RegistrationNo,

                    FactoryName = f.FactoryName,
                    ApplicantName = f.ApplicantName,

                    RelationType = f.RelationType,
                    RelationName = f.RelationName,

                    ApplicantAddressLine1 = f.ApplicantAddressLine1,
                    ApplicantAddressLine2 = f.ApplicantAddressLine2,

                    TehsilName = f.TehsilName,
                    DistrictName = f.DistrictName,
                    Area = f.Area,
                    Pincode = f.Pincode,
                    SubdivisionName = f.SubdivisionName,
                    DeclarationAccepted = f.DeclarationAccepted,
                    RequiredInfoAccepted = f.RequiredInfoAccepted,
                    VerifyAccepted = f.VerifyAccepted,
                    WorkersLimitAccepted = f.WorkersLimitAccepted,

                    Status = f.Status,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt
                })
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return result;
        }

        public async Task<NonHazardousApplicationResponseDto> GetByApplicationIdAsync(string applicationId)
        {
            // ? Main query (single join like ManagerChange)
            var appDto = await (
                from app in _context.ApplicationRegistrations.AsNoTracking()
                where app.ApplicationId == applicationId

                join f in _context.NonHazardousFactoryRegistrations.AsNoTracking()
                    on app.ApplicationRegistrationNumber equals f.RegistrationNo into fJoin
                from factory in fJoin.DefaultIfEmpty()

                select new NonHazardousFactoryRegistrationDto
                {
                    Id = factory.Id,

                    ApplicationId = app.ApplicationId,
                    RegistrationNo = factory.RegistrationNo,

                    FactoryName = factory.FactoryName,
                    ApplicantName = factory.ApplicantName,

                    RelationType = factory.RelationType,
                    RelationName = factory.RelationName,

                    ApplicantAddressLine1 = factory.ApplicantAddressLine1,
                    ApplicantAddressLine2 = factory.ApplicantAddressLine2,

                   
                    TehsilName = factory.TehsilName,
                    DistrictName = factory.DistrictName,
                    Area = factory.Area,
                    Pincode = factory.Pincode,
                    SubdivisionName = factory.SubdivisionName,
                    DeclarationAccepted = factory.DeclarationAccepted,
                    RequiredInfoAccepted = factory.RequiredInfoAccepted,
                    VerifyAccepted = factory.VerifyAccepted,
                    WorkersLimitAccepted = factory.WorkersLimitAccepted,

                    Status = factory.Status,
                    CreatedAt = factory.CreatedAt,
                    UpdatedAt = factory.UpdatedAt
                }
            ).FirstOrDefaultAsync();

            if (appDto == null)
                throw new Exception("Application not found");

           

            // ? History
            var history = await _context.Set<ApplicationHistory>()
                .AsNoTracking()
                .Where(x => x.ApplicationId == applicationId)
                .OrderByDescending(x => x.ActionDate)
                .ToListAsync();

            return new NonHazardousApplicationResponseDto
            {
                ApplicationDetails = appDto,
                ApplicationHistory = history,
               
            };
        }


        public async Task<NonHazardousFactoryRegistrationDto> CreateAsync( CreateNonHazardousFactoryRegistrationRequest request, Guid userId)
        {

            var registrationNo = await GenerateRegistrationNumberAsync();
            var applicationId = await GenerateApplicationNumberAsync();
            var entity = new NonHazardousFactoryRegistration
            {
                RegistrationNo = registrationNo,
                FactoryName = request.FactoryName,
                ApplicantName = request.ApplicantName,
                ApplicationNumber = applicationId,

                RelationType = request.RelationType,
                RelationName = request.RelationName,

                ApplicantAddressLine1 = request.ApplicantAddressLine1,
                ApplicantAddressLine2 = request.ApplicantAddressLine2,

                SubdivisionName = request.SubdivisionName,
                TehsilName = request.TehsilName,
                DistrictName = request.DistrictName,
                Area = request.Area,
                Pincode = request.Pincode,
                Version = 1.0m,
                DeclarationAccepted = request.DeclarationAccepted,
                RequiredInfoAccepted = request.RequiredInfoAccepted,
                VerifyAccepted = request.VerifyAccepted,
                WorkersLimitAccepted = request.WorkersLimitAccepted,

                Status = "Pending",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.NonHazardousFactoryRegistrations.Add(entity);
            await _context.SaveChangesAsync();

            // ================= ? ADD APPLICATION REGISTRATION =================
            var module = await _context.Set<FormModule>()
                .FirstOrDefaultAsync(m => m.Name == ApplicationTypeNames.FactoryNonHazardous);

            if (module == null)
                throw new Exception("Module not found");

            var appReg = new ApplicationRegistration
            {
                Id = Guid.NewGuid().ToString(),
                ModuleId = module.Id,
                UserId = userId,

                ApplicationId = applicationId,
                ApplicationRegistrationNumber = registrationNo,

                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            _context.ApplicationRegistrations.Add(appReg);

            // ================= ? ADD HISTORY =================
            var history = new ApplicationHistory
            {
                ApplicationId = applicationId,
                ApplicationType = module.Name,
                Action = "Application Submitted",
                PreviousStatus = null,
                NewStatus = "Pending",
                Comments = "Application Submitted successfully",
                ActionBy = "Applicant",
                ActionDate = DateTime.Now
            };

            _context.ApplicationHistories.Add(history);

            await _context.SaveChangesAsync();

            // ================= RETURN DTO =================
            return new NonHazardousFactoryRegistrationDto
            {
                Id = entity.Id,
                RegistrationNo = entity.RegistrationNo,
                ApplicationId = entity.ApplicationNumber,            

                FactoryName = entity.FactoryName,
                ApplicantName = entity.ApplicantName,

                RelationType = entity.RelationType,
                RelationName = entity.RelationName,

                ApplicantAddressLine1 = entity.ApplicantAddressLine1,
                ApplicantAddressLine2 = entity.ApplicantAddressLine2,

                SubdivisionName = entity.SubdivisionName, 
                TehsilName = entity.TehsilName,
                DistrictName = entity.DistrictName,
                Area = entity.Area,
                Pincode = entity.Pincode,

                DeclarationAccepted = entity.DeclarationAccepted,
                RequiredInfoAccepted = entity.RequiredInfoAccepted,
                VerifyAccepted = entity.VerifyAccepted,
                WorkersLimitAccepted = entity.WorkersLimitAccepted,

                Status = entity.Status,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }


        public async Task<NonHazardousFactoryRegistrationDto> UpdateAsync(   string applicationId,   CreateNonHazardousFactoryRegistrationRequest request,    Guid userId)
        {
            
            var appReg = await _context.ApplicationRegistrations
                .FirstOrDefaultAsync(x => x.ApplicationId == applicationId);

            if (appReg == null)
                throw new Exception("Application not found");

          
            var entity = await _context.NonHazardousFactoryRegistrations
                .FirstOrDefaultAsync(x => x.RegistrationNo == appReg.ApplicationRegistrationNumber);

            if (entity == null)
                throw new Exception("Record not found");

            
            entity.FactoryName = request.FactoryName;
            entity.ApplicantName = request.ApplicantName;

            entity.RelationType = request.RelationType;
            entity.RelationName = request.RelationName;

            entity.ApplicantAddressLine1 = request.ApplicantAddressLine1;
            entity.ApplicantAddressLine2 = request.ApplicantAddressLine2;

            entity.SubdivisionName = request.SubdivisionName;
            entity.TehsilName = request.TehsilName;
            entity.DistrictName = request.DistrictName;
            entity.Area = request.Area;
            entity.Pincode = request.Pincode;

            entity.DeclarationAccepted = request.DeclarationAccepted;
            entity.RequiredInfoAccepted = request.RequiredInfoAccepted;
            entity.VerifyAccepted = request.VerifyAccepted;
            entity.WorkersLimitAccepted = request.WorkersLimitAccepted;

            entity.UpdatedAt = DateTime.Now;

            
            _context.ApplicationHistories.Add(new ApplicationHistory
            {
                ApplicationId = applicationId,
                ApplicationType = ApplicationTypeNames.FactoryNonHazardous,
                Action = "Application Updated",
                PreviousStatus = entity.Status,
                NewStatus = entity.Status, // status not changed
                Comments = "Application updated by applicant",
                ActionBy = "Applicant",
                ActionDate = DateTime.Now
            });

            await _context.SaveChangesAsync();

           
            return new NonHazardousFactoryRegistrationDto
            {
                Id = entity.Id,
                ApplicationId = applicationId,
                RegistrationNo = entity.RegistrationNo,

                FactoryName = entity.FactoryName,
                ApplicantName = entity.ApplicantName,

                RelationType = entity.RelationType,
                RelationName = entity.RelationName,

                ApplicantAddressLine1 = entity.ApplicantAddressLine1,
                ApplicantAddressLine2 = entity.ApplicantAddressLine2,

                SubdivisionName = entity.SubdivisionName,
                TehsilName = entity.TehsilName,
                DistrictName = entity.DistrictName,
                Area = entity.Area,
                Pincode = entity.Pincode,

                DeclarationAccepted = entity.DeclarationAccepted,
                RequiredInfoAccepted = entity.RequiredInfoAccepted,
                VerifyAccepted = entity.VerifyAccepted,
                WorkersLimitAccepted = entity.WorkersLimitAccepted,

                Status = entity.Status,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.Set<NonHazardousFactoryRegistration>().FindAsync(id);
            if (entity == null) return false;
            _context.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}