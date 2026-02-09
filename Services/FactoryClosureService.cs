using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
    public class FactoryClosureService : IFactoryClosureService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FactoryClosureService> _logger;

        public FactoryClosureService(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            ILogger<FactoryClosureService> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public async Task<ApiResponseDto<List<FactoryClosureDto>>> GetAllClosuresAsync()
        {
            try
            {
                var closures = await _context.FactoryClosures
                    .Include(c => c.Documents)
                    .Include(c => c.FactoryRegistration)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                var closureDtos = closures.Select(MapToDto).ToList();
                
                return new ApiResponseDto<List<FactoryClosureDto>>
                {
                    Success = true,
                    Message = "Closures retrieved successfully",
                    Data = closureDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving factory closures");
                return new ApiResponseDto<List<FactoryClosureDto>>
                {
                    Success = false,
                    Message = $"Error retrieving closures: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<FactoryClosureDto>> GetClosureByIdAsync(string id)
        {
            try
            {
                var closure = await _context.FactoryClosures
                    .Include(c => c.Documents)
                    .Include(c => c.FactoryRegistration)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (closure == null)
                {
                    return new ApiResponseDto<FactoryClosureDto>
                    {
                        Success = false,
                        Message = "Closure not found"
                    };
                }

                return new ApiResponseDto<FactoryClosureDto>
                {
                    Success = true,
                    Message = "Closure retrieved successfully",
                    Data = MapToDto(closure)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving closure {ClosureId}", id);
                return new ApiResponseDto<FactoryClosureDto>
                {
                    Success = false,
                    Message = $"Error retrieving closure: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<List<FactoryClosureDto>>> GetClosuresByFactoryRegistrationIdAsync(string factoryRegistrationId)
        {
            try
            {
                var closures = await _context.FactoryClosures
                    .Include(c => c.Documents)
                    .Include(c => c.FactoryRegistration)
                    .Where(c => c.FactoryRegistrationId == factoryRegistrationId)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                var closureDtos = closures.Select(MapToDto).ToList();
                
                return new ApiResponseDto<List<FactoryClosureDto>>
                {
                    Success = true,
                    Message = "Closures retrieved successfully",
                    Data = closureDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving closures for factory {FactoryId}", factoryRegistrationId);
                return new ApiResponseDto<List<FactoryClosureDto>>
                {
                    Success = false,
                    Message = $"Error retrieving closures: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<FactoryClosureDto>> CreateClosureAsync(CreateFactoryClosureRequest request)
        {
            try
            {
                // Get factory registration details
                var factoryRegistration = await _context.FactoryRegistrations
                    .FirstOrDefaultAsync(f => f.Id == request.FactoryRegistrationId);

                if (factoryRegistration == null)
                {
                    return new ApiResponseDto<FactoryClosureDto>
                    {
                        Success = false,
                        Message = "Factory registration not found"
                    };
                }

                // Generate closure number
                var closureNumber = await GenerateClosureNumberAsync();

                var closure = new FactoryClosure
                {
                    Id = Guid.NewGuid().ToString(),
                    ClosureNumber = closureNumber,
                    FactoryRegistrationId = request.FactoryRegistrationId,
                    FactoryName = factoryRegistration.FactoryName,
                    RegistrationNumber = factoryRegistration.RegistrationNumber,
                    OccupierName = factoryRegistration.OccupierName,
                    FactoryAddress = $"{factoryRegistration.PlotNumber}, {factoryRegistration.StreetLocality}, {factoryRegistration.CityTown}",
                    FeesDue = request.FeesDue,
                    LastRenewalDate = request.LastRenewalDate,
                    ClosureDate = request.ClosureDate,
                    ReasonForClosure = request.ReasonForClosure,
                    InspectingOfficerName = request.InspectingOfficerName,
                    InspectionRemarks = request.InspectionRemarks,
                    InspectionDate = request.InspectionDate,
                    Status = "Pending",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.FactoryClosures.Add(closure);
                await _context.SaveChangesAsync();

                // Create history entry
                await CreateHistoryEntryAsync(closure.Id, "FactoryClosure", "Submitted", null, "Pending", 
                    "Factory closure application submitted", "System", "System User");

                return new ApiResponseDto<FactoryClosureDto>
                {
                    Success = true,
                    Message = "Factory closure created successfully",
                    Data = MapToDto(closure)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating factory closure");
                return new ApiResponseDto<FactoryClosureDto>
                {
                    Success = false,
                    Message = $"Error creating closure: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<FactoryClosureDto>> UpdateStatusAsync(
            string id, 
            UpdateFactoryClosureStatusRequest request, 
            string reviewedBy)
        {
            try
            {
                var closure = await _context.FactoryClosures
                    .Include(c => c.Documents)
                    .Include(c => c.FactoryRegistration)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (closure == null)
                {
                    return new ApiResponseDto<FactoryClosureDto>
                    {
                        Success = false,
                        Message = "Closure not found"
                    };
                }

                var previousStatus = closure.Status;
                closure.Status = request.Status;
                closure.Comments = request.Comments;
                closure.ReviewedBy = reviewedBy;
                closure.ReviewedAt = DateTime.Now;
                closure.UpdatedAt = DateTime.Now;

                if (request.Status == "Closed")
                {
                    closure.ClosedAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                // Create history entry
                var action = request.Status switch
                {
                    "Approved" => "Approved",
                    "Rejected" => "Rejected",
                    "Closed" => "Closed",
                    _ => "Status Updated"
                };

                await CreateHistoryEntryAsync(closure.Id, "FactoryClosure", action, previousStatus, 
                    request.Status, request.Comments ?? "", reviewedBy, reviewedBy);

                return new ApiResponseDto<FactoryClosureDto>
                {
                    Success = true,
                    Message = "Closure status updated successfully",
                    Data = MapToDto(closure)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating closure status {ClosureId}", id);
                return new ApiResponseDto<FactoryClosureDto>
                {
                    Success = false,
                    Message = $"Error updating status: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<FactoryClosureDocumentDto>> UploadDocumentAsync(
            string closureId, 
            IFormFile file, 
            string documentType)
        {
            try
            {
                var closure = await _context.FactoryClosures.FindAsync(closureId);
                if (closure == null)
                {
                    return new ApiResponseDto<FactoryClosureDocumentDto>
                    {
                        Success = false,
                        Message = "Closure not found"
                    };
                }

                // Create uploads directory
                var uploadsPath = Path.Combine(_environment.ContentRootPath, "Uploads", "FactoryClosures", closureId);
                Directory.CreateDirectory(uploadsPath);

                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = $"{documentType}_{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, uniqueFileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Create document record
                var document = new FactoryClosureDocument
                {
                    Id = Guid.NewGuid().ToString(),
                    FactoryClosureId = closureId,
                    DocumentType = documentType,
                    FileName = file.FileName,
                    FilePath = filePath,
                    FileSize = file.Length,
                    FileExtension = fileExtension,
                    UploadedAt = DateTime.Now
                };

                _context.FactoryClosureDocuments.Add(document);
                await _context.SaveChangesAsync();

                return new ApiResponseDto<FactoryClosureDocumentDto>
                {
                    Success = true,
                    Message = "Document uploaded successfully",
                    Data = new FactoryClosureDocumentDto
                    {
                        Id = document.Id,
                        DocumentType = document.DocumentType,
                        FileName = document.FileName,
                        FilePath = document.FilePath,
                        FileSize = document.FileSize,
                        FileExtension = document.FileExtension,
                        UploadedAt = document.UploadedAt
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document for closure {ClosureId}", closureId);
                return new ApiResponseDto<FactoryClosureDocumentDto>
                {
                    Success = false,
                    Message = $"Error uploading document: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteClosureAsync(string id)
        {
            try
            {
                var closure = await _context.FactoryClosures.FindAsync(id);
                if (closure == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Closure not found"
                    };
                }

                _context.FactoryClosures.Remove(closure);
                await _context.SaveChangesAsync();

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Closure deleted successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting closure {ClosureId}", id);
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = $"Error deleting closure: {ex.Message}"
                };
            }
        }

        private async Task<string> GenerateClosureNumberAsync()
        {
            var year = DateTime.Now.Year;
            var prefix = $"CLO-{year}-";
            
            var lastClosure = await _context.FactoryClosures
                .Where(c => c.ClosureNumber.StartsWith(prefix))
                .OrderByDescending(c => c.ClosureNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastClosure != null)
            {
                var lastNumberStr = lastClosure.ClosureNumber.Substring(prefix.Length);
                if (int.TryParse(lastNumberStr, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D4}";
        }

        private async Task CreateHistoryEntryAsync(
            string applicationId,
            string applicationType,
            string action,
            string? previousStatus,
            string newStatus,
            string comments,
            string actionBy,
            string actionByName)
        {
            try
            {
                var history = new ApplicationHistory
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = applicationId,
                    ApplicationType = applicationType,
                    Action = action,
                    PreviousStatus = previousStatus,
                    NewStatus = newStatus,
                    Comments = comments,
                    ActionBy = actionBy,
                    ActionByName = actionByName,
                    ActionDate = DateTime.Now
                };

                _context.ApplicationHistories.Add(history);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating history entry");
            }
        }

        private FactoryClosureDto MapToDto(FactoryClosure closure)
        {
            return new FactoryClosureDto
            {
                Id = closure.Id,
                ClosureNumber = closure.ClosureNumber,
                FactoryRegistrationId = closure.FactoryRegistrationId,
                FactoryName = closure.FactoryName,
                RegistrationNumber = closure.RegistrationNumber,
                OccupierName = closure.OccupierName,
                FactoryAddress = closure.FactoryAddress,
                FeesDue = closure.FeesDue,
                LastRenewalDate = closure.LastRenewalDate,
                ClosureDate = closure.ClosureDate,
                ReasonForClosure = closure.ReasonForClosure,
                InspectingOfficerName = closure.InspectingOfficerName,
                InspectionRemarks = closure.InspectionRemarks,
                InspectionDate = closure.InspectionDate,
                Status = closure.Status,
                CurrentStage = closure.CurrentStage,
                AssignedTo = closure.AssignedTo,
                AssignedToName = closure.AssignedToName,
                Comments = closure.Comments,
                ReviewedBy = closure.ReviewedBy,
                ReviewedAt = closure.ReviewedAt,
                CreatedAt = closure.CreatedAt,
                UpdatedAt = closure.UpdatedAt,
                ClosedAt = closure.ClosedAt,
                Documents = closure.Documents?.Select(d => new FactoryClosureDocumentDto
                {
                    Id = d.Id,
                    DocumentType = d.DocumentType,
                    FileName = d.FileName,
                    FilePath = d.FilePath,
                    FileSize = d.FileSize,
                    FileExtension = d.FileExtension,
                    UploadedAt = d.UploadedAt
                }).ToList() ?? new List<FactoryClosureDocumentDto>()
            };
        }
    }
}
