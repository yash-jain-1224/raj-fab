using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;
using System.Text.Json;

namespace RajFabAPI.Services
{
    public class BoilerService : IBoilerService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public BoilerService(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<BoilerApplication> RegisterBoilerAsync(BoilerRegistrationDto dto)
        {
            var applicationNumber = GenerateApplicationNumber("REG");
            
            var application = new BoilerApplication
            {
                Id = Guid.NewGuid(),
                ApplicationNumber = applicationNumber,
                ApplicationType = "registration",
                ApplicantName = dto.ApplicantInfo.OwnerName,
                OrganizationName = dto.ApplicantInfo.OrganizationName,
                ContactPerson = dto.ApplicantInfo.ContactPerson,
                Mobile = dto.ApplicantInfo.Mobile,
                Email = dto.ApplicantInfo.Email,
                Address = dto.ApplicantInfo.Address,
                ApplicationData = JsonSerializer.Serialize(dto),
                Status = "pending",
                SubmissionDate = DateTime.Now
            };

            _context.BoilerApplications.Add(application);
            await _context.SaveChangesAsync();

            return application;
        }

        public async Task<BoilerApplication> RenewCertificateAsync(BoilerRenewalDto dto)
        {
            var applicationNumber = GenerateApplicationNumber("REN");
            
            var application = new BoilerApplication
            {
                Id = Guid.NewGuid(),
                ApplicationNumber = applicationNumber,
                ApplicationType = "renewal",
                ApplicantName = "Renewal Application", // Will be populated from boiler record
                OrganizationName = "Organization", // Will be populated from boiler record  
                ContactPerson = "Contact", // Will be populated from boiler record
                Mobile = "0000000000", // Will be populated from boiler record
                Email = "email@example.com", // Will be populated from boiler record
                Address = "Address", // Will be populated from boiler record
                ApplicationData = JsonSerializer.Serialize(dto),
                Status = "pending",
                SubmissionDate = DateTime.Now
            };

            _context.BoilerApplications.Add(application);
            await _context.SaveChangesAsync();

            return application;
        }

        public async Task<BoilerApplication> ModifyBoilerAsync(BoilerModificationDto dto)
        {
            var applicationNumber = GenerateApplicationNumber("MOD");
            
            var application = new BoilerApplication
            {
                Id = Guid.NewGuid(),
                ApplicationNumber = applicationNumber,
                ApplicationType = "modification",
                ApplicantName = "Modification Application", // Will be populated from boiler record
                OrganizationName = "Organization", // Will be populated from boiler record
                ContactPerson = "Contact", // Will be populated from boiler record
                Mobile = "0000000000", // Will be populated from boiler record
                Email = "email@example.com", // Will be populated from boiler record
                Address = "Address", // Will be populated from boiler record
                ApplicationData = JsonSerializer.Serialize(dto),
                Status = "pending",
                SubmissionDate = DateTime.Now
            };

            _context.BoilerApplications.Add(application);
            await _context.SaveChangesAsync();

            return application;
        }

        public async Task<BoilerApplication> TransferBoilerAsync(BoilerTransferDto dto)
        {
            var applicationNumber = GenerateApplicationNumber("TRF");
            
            var application = new BoilerApplication
            {
                Id = Guid.NewGuid(),
                ApplicationNumber = applicationNumber,
                ApplicationType = "transfer",
                ApplicantName = dto.NewOwner.Name,
                OrganizationName = dto.NewOwner.OrganizationName,
                ContactPerson = dto.NewOwner.ContactPerson,
                Mobile = dto.NewOwner.Mobile,
                Email = dto.NewOwner.Email,
                Address = dto.NewOwner.Address,
                ApplicationData = JsonSerializer.Serialize(dto),
                Status = "pending",
                SubmissionDate = DateTime.Now
            };

            _context.BoilerApplications.Add(application);
            await _context.SaveChangesAsync();

            return application;
        }

        public async Task<PagedResult<RegisteredBoiler>> GetAllBoilersAsync(int page, int pageSize)
        {
            var query = _context.RegisteredBoilers
                .Include(b => b.Specifications)
                .Include(b => b.Location)
                .Include(b => b.SafetyFeatures)
                .Include(b => b.CurrentCertificate)
                .OrderByDescending(b => b.RegistrationDate);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<RegisteredBoiler>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<RegisteredBoiler?> GetBoilerByRegistrationNumberAsync(string registrationNumber)
        {
            return await _context.RegisteredBoilers
                .Include(b => b.Specifications)
                .Include(b => b.Location)
                .Include(b => b.SafetyFeatures)
                .Include(b => b.CurrentCertificate)
                .Include(b => b.InspectionHistory)
                .FirstOrDefaultAsync(b => b.RegistrationNumber == registrationNumber);
        }

        public async Task<PagedResult<BoilerApplication>> GetApplicationsAsync(string? status, int page, int pageSize)
        {
            var query = _context.BoilerApplications.AsQueryable();
            
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.Status == status);
            }

            query = query.OrderByDescending(a => a.SubmissionDate);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<BoilerApplication>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<BoilerApplication?> GetApplicationByNumberAsync(string applicationNumber)
        {
            return await _context.BoilerApplications
                .Include(a => a.Boiler)
                .FirstOrDefaultAsync(a => a.ApplicationNumber == applicationNumber);
        }

        public async Task<BoilerApplication> UpdateApplicationStatusAsync(string applicationNumber, string status, string? comments, string? processedBy)
        {
            var application = await _context.BoilerApplications
                .FirstOrDefaultAsync(a => a.ApplicationNumber == applicationNumber);
            
            if (application == null)
                throw new ArgumentException("Application not found");

            application.Status = status;
            application.Comments = comments;
            application.ProcessedBy = processedBy;
            application.ProcessingDate = DateTime.Now;
            
            if (status == "approved" || status == "rejected")
            {
                application.CompletionDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return application;
        }

        public async Task<string> UploadDocumentAsync(string applicationNumber, IFormFile file, string documentType)
        {
            var application = await _context.BoilerApplications
                .FirstOrDefaultAsync(a => a.ApplicationNumber == applicationNumber);
            
            if (application == null)
                throw new ArgumentException("Application not found");

            // Create upload directory
            var uploadDir = Path.Combine(_environment.WebRootPath, "uploads", "boiler", applicationNumber);
            Directory.CreateDirectory(uploadDir);

            // Generate unique filename
            var fileName = $"{documentType}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadDir, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Update document paths in application
            var documentPaths = JsonSerializer.Deserialize<List<string>>(application.DocumentPaths) ?? new List<string>();
            documentPaths.Add($"uploads/boiler/{applicationNumber}/{fileName}");
            application.DocumentPaths = JsonSerializer.Serialize(documentPaths);

            await _context.SaveChangesAsync();

            return $"uploads/boiler/{applicationNumber}/{fileName}";
        }

        public async Task<List<BoilerInspectionHistory>> GetInspectionHistoryAsync(Guid boilerId)
        {
            return await _context.BoilerInspectionHistories
                .Where(h => h.BoilerId == boilerId)
                .OrderByDescending(h => h.InspectionDate)
                .ToListAsync();
        }

        public async Task<BoilerInspectionHistory> AddInspectionRecordAsync(Guid boilerId, BoilerInspectionDto dto)
        {
            var inspection = new BoilerInspectionHistory
            {
                Id = Guid.NewGuid(),
                InspectionId = GenerateInspectionId(),
                BoilerId = boilerId,
                InspectionDate = dto.InspectionDate,
                InspectionType = dto.InspectionType,
                InspectorName = dto.InspectorName,
                Findings = dto.Findings,
                Recommendations = dto.Recommendations,
                NextInspectionDue = dto.NextInspectionDue,
                CertificateIssued = dto.CertificateIssued
            };

            _context.BoilerInspectionHistories.Add(inspection);
            await _context.SaveChangesAsync();

            return inspection;
        }

        private string GenerateApplicationNumber(string prefix)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"{prefix}{timestamp}{random}";
        }

        private string GenerateInspectionId()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(100, 999);
            return $"INS{timestamp}{random}";
        }
    }
}