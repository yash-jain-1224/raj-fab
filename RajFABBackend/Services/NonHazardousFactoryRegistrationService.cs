using RajFabAPI.DTOs;
using RajFabAPI.Models.FactoryModels;
using RajFabAPI.Services.Interface;
using RajFabAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace RajFabAPI.Services
{
    public class NonHazardousFactoryRegistrationService : INonHazardousFactoryRegistrationService
    {
        private readonly ApplicationDbContext _context;

        public NonHazardousFactoryRegistrationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<NonHazardousFactoryRegistrationDto>> GetAllAsync()
        {
            return await _context.Set<NonHazardousFactoryRegistration>()
                .AsNoTracking()
                .Select(f => new NonHazardousFactoryRegistrationDto
                {
                    Id = f.Id,
                    RegistrationNo = f.RegistrationNo,
                    FactoryName = f.FactoryName,
                    ApplicantName = f.ApplicantName,
                    RelationType = f.RelationType,
                    RelationName = f.RelationName,
                    ApplicantAddress = f.ApplicantAddress,
                    AreaId = f.AreaId,
                    DistrictId = f.DistrictId,
                    DivisionId = f.DivisionId,
                    Address = f.FactoryAddress,
                    Pincode = f.FactoryPincode,
                    DeclarationAccepted = f.DeclarationAccepted,
                    RequiredInfoAccepted = f.RequiredInfoAccepted,
                    VerifyAccepted = f.VerifyAccepted,
                    WorkersLimitAccepted = f.WorkersLimitAccepted,
                    ApplicationDate = f.ApplicationDate,
                    ApplicationPlace = f.ApplicationPlace,
                    ApplicantSignature = f.ApplicantSignature,
                    VerifyDate = f.VerifyDate,
                    VerifyPlace = f.VerifyPlace,
                    VerifierSignature = f.VerifierSignature,
                    Status = f.Status,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt
                }).ToListAsync();
        }

        public async Task<NonHazardousFactoryRegistrationDto?> GetByIdAsync(Guid id)
        {
            var f = await _context.Set<NonHazardousFactoryRegistration>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (f == null) return null;
            return new NonHazardousFactoryRegistrationDto
            {
                Id = f.Id,
                RegistrationNo = f.RegistrationNo,
                FactoryName = f.FactoryName,
                ApplicantName = f.ApplicantName,
                RelationType = f.RelationType,
                RelationName = f.RelationName,
                ApplicantAddress = f.ApplicantAddress,
                AreaId = f.AreaId,
                DistrictId = f.DistrictId,
                DivisionId = f.DivisionId,
                Address = f.FactoryAddress,
                Pincode = f.FactoryPincode,
                DeclarationAccepted = f.DeclarationAccepted,
                RequiredInfoAccepted = f.RequiredInfoAccepted,
                VerifyAccepted = f.VerifyAccepted,
                WorkersLimitAccepted = f.WorkersLimitAccepted,
                ApplicationDate = f.ApplicationDate,
                ApplicationPlace = f.ApplicationPlace,
                ApplicantSignature = f.ApplicantSignature,
                VerifyDate = f.VerifyDate,
                VerifyPlace = f.VerifyPlace,
                VerifierSignature = f.VerifierSignature,
                Status = f.Status,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            };
        }

        public async Task<NonHazardousFactoryRegistrationDto> CreateAsync(CreateNonHazardousFactoryRegistrationRequest request)
        {
            var entity = new NonHazardousFactoryRegistration
            {
                RegistrationNo = request.RegistrationNo,
                FactoryName = request.FactoryName,
                ApplicantName = request.ApplicantName,
                RelationType = request.RelationType,
                RelationName = request.RelationName,
                ApplicantAddress = request.ApplicantAddress,
                AreaId = request.AreaId,
                DistrictId = request.DistrictId,
                DivisionId = request.DivisionId,
                FactoryAddress = request.Address,
                FactoryPincode = request.Pincode,
                DeclarationAccepted = request.DeclarationAccepted,
                RequiredInfoAccepted = request.RequiredInfoAccepted,
                VerifyAccepted = request.VerifyAccepted,
                WorkersLimitAccepted = request.WorkersLimitAccepted,
                ApplicationDate = request.ApplicationDate,
                ApplicationPlace = request.ApplicationPlace,
                ApplicantSignature = request.ApplicantSignature,
                VerifyDate = request.VerifyDate,
                VerifyPlace = request.VerifyPlace,
                VerifierSignature = request.VerifierSignature,
                Status = "Pending",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.Add(entity);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(entity.Id) ?? throw new Exception("Failed to create registration");
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