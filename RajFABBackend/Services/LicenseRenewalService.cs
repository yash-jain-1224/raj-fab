using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
    public partial class LicenseRenewalService : ILicenseRenewalService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<LicenseRenewalService> _logger;

        public LicenseRenewalService(
            ApplicationDbContext context, 
            IWebHostEnvironment environment,
            ILogger<LicenseRenewalService> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public async Task<ApiResponseDto<List<LicenseRenewalDto>>> GetAllRenewalsAsync()
        {
            try
            {
                var renewals = await _context.LicenseRenewals
                    .Include(r => r.Documents)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                // Load district and area names
                var districtIds = renewals.Select(r => r.District).Distinct().Where(d => !string.IsNullOrEmpty(d)).ToList();
                var areaIds = renewals.Select(r => r.Area).Distinct().Where(a => !string.IsNullOrEmpty(a)).ToList();
                
                // Safely build dictionaries with null checks
                var districtList = await _context.Districts.ToListAsync();
                var districts = districtList
                    .Where(d => d.Id != null && districtIds.Contains(d.Id.ToString()))
                    .GroupBy(d => d.Id.ToString())
                    .Where(g => g.Key != null)
                    .ToDictionary(g => g.Key!, g => g.First().Name ?? string.Empty);
                
                var areaList = await _context.Areas.ToListAsync();
                var areas = areaList
                    .Where(a => a.Id != null && areaIds.Contains(a.Id.ToString()))
                    .GroupBy(a => a.Id.ToString())
                    .Where(g => g.Key != null)
                    .ToDictionary(g => g.Key!, g => g.First().Name ?? string.Empty);

                var renewalDtos = renewals.Select(r => MapToDto(r, districts, areas)).ToList();

                return new ApiResponseDto<List<LicenseRenewalDto>>
                {
                    Success = true,
                    Data = renewalDtos,
                    Message = "Renewals retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving renewals");
                return new ApiResponseDto<List<LicenseRenewalDto>>
                {
                    Success = false,
                    Message = $"Error retrieving renewals: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<LicenseRenewalDto>> GetRenewalByIdAsync(string id)
        {
            try
            {
                var renewal = await _context.LicenseRenewals
                    .Include(r => r.Documents)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (renewal == null)
                {
                    return new ApiResponseDto<LicenseRenewalDto>
                    {
                        Success = false,
                        Message = "Renewal not found"
                    };
                }

                var districts = await LoadDistricts(new[] { renewal.District });
                var areas = await LoadAreas(new[] { renewal.Area });

                return new ApiResponseDto<LicenseRenewalDto>
                {
                    Success = true,
                    Data = MapToDto(renewal, districts, areas),
                    Message = "Renewal retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving renewal {Id}", id);
                return new ApiResponseDto<LicenseRenewalDto>
                {
                    Success = false,
                    Message = $"Error retrieving renewal: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<LicenseRenewalDto>> GetRenewalByNumberAsync(string renewalNumber)
        {
            try
            {
                var renewal = await _context.LicenseRenewals
                    .Include(r => r.Documents)
                    .FirstOrDefaultAsync(r => r.RenewalNumber == renewalNumber);

                if (renewal == null)
                {
                    return new ApiResponseDto<LicenseRenewalDto>
                    {
                        Success = false,
                        Message = "Renewal not found"
                    };
                }

                var districts = await LoadDistricts(new[] { renewal.District });
                var areas = await LoadAreas(new[] { renewal.Area });

                return new ApiResponseDto<LicenseRenewalDto>
                {
                    Success = true,
                    Data = MapToDto(renewal, districts, areas),
                    Message = "Renewal retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving renewal {Number}", renewalNumber);
                return new ApiResponseDto<LicenseRenewalDto>
                {
                    Success = false,
                    Message = $"Error retrieving renewal: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<List<LicenseRenewalDto>>> GetRenewalsByRegistrationIdAsync(string registrationId)
        {
            try
            {
                var renewals = await _context.LicenseRenewals
                    .Include(r => r.Documents)
                    .Where(r => r.OriginalRegistrationId == registrationId)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                var districtIds = renewals.Select(r => r.District).Distinct().Where(d => !string.IsNullOrEmpty(d)).ToList();
                var areaIds = renewals.Select(r => r.Area).Distinct().Where(a => !string.IsNullOrEmpty(a)).ToList();
                
                // Safely build dictionaries with null checks
                var districtList = await _context.Districts.ToListAsync();
                var districts = districtList
                    .Where(d => d.Id != null && districtIds.Contains(d.Id.ToString()))
                    .GroupBy(d => d.Id.ToString())
                    .Where(g => g.Key != null)
                    .ToDictionary(g => g.Key!, g => g.First().Name ?? string.Empty);
                
                var areaList = await _context.Areas.ToListAsync();
                var areas = areaList
                    .Where(a => a.Id != null && areaIds.Contains(a.Id.ToString()))
                    .GroupBy(a => a.Id.ToString())
                    .Where(g => g.Key != null)
                    .ToDictionary(g => g.Key!, g => g.First().Name ?? string.Empty);

                var renewalDtos = renewals.Select(r => MapToDto(r, districts, areas)).ToList();

                return new ApiResponseDto<List<LicenseRenewalDto>>
                {
                    Success = true,
                    Data = renewalDtos,
                    Message = "Renewals retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving renewals for registration {Id}", registrationId);
                return new ApiResponseDto<List<LicenseRenewalDto>>
                {
                    Success = false,
                    Message = $"Error retrieving renewals: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<LicenseRenewalDto>> CreateRenewalAsync(CreateLicenseRenewalRequest request)
        {
            try
            {
                // Verify original registration exists
                var originalRegistration = await _context.FactoryRegistrations
                    .FirstOrDefaultAsync(r => r.Id == request.OriginalRegistrationId);

                if (originalRegistration == null)
                {
                    return new ApiResponseDto<LicenseRenewalDto>
                    {
                        Success = false,
                        Message = "Original registration not found"
                    };
                }

                var renewal = new LicenseRenewal
                {
                    Id = Guid.NewGuid().ToString(),
                    RenewalNumber = GenerateRenewalNumber(),
                    OriginalRegistrationId = request.OriginalRegistrationId,
                    OriginalRegistrationNumber = originalRegistration.RegistrationNumber,
                    RenewalYears = request.RenewalYears,
                    LicenseRenewalFrom = request.LicenseRenewalFrom,
                    LicenseRenewalTo = request.LicenseRenewalTo,
                    FactoryName = request.FactoryName,
                    FactoryRegistrationNumber = request.FactoryRegistrationNumber,
                    PlotNumber = request.PlotNumber,
                    StreetLocality = request.StreetLocality,
                    CityTown = request.CityTown,
                    District = request.District,
                    Area = request.Area,
                    Pincode = request.Pincode,
                    Mobile = request.Mobile,
                    Email = request.Email,
                    ManufacturingProcess = request.ManufacturingProcess,
                    ProductionStartDate = request.ProductionStartDate,
                    ManufacturingProcessLast12Months = request.ManufacturingProcessLast12Months,
                    ManufacturingProcessNext12Months = request.ManufacturingProcessNext12Months,
                    ProductDetailsLast12Months = request.ProductDetailsLast12Months,
                    MaxWorkersMaleProposed = request.MaxWorkersMaleProposed,
                    MaxWorkersFemaleProposed = request.MaxWorkersFemaleProposed,
                    MaxWorkersTransgenderProposed = request.MaxWorkersTransgenderProposed,
                    MaxWorkersMaleEmployed = request.MaxWorkersMaleEmployed,
                    MaxWorkersFemaleEmployed = request.MaxWorkersFemaleEmployed,
                    MaxWorkersTransgenderEmployed = request.MaxWorkersTransgenderEmployed,
                    WorkersMaleOrdinary = request.WorkersMaleOrdinary,
                    WorkersFemaleOrdinary = request.WorkersFemaleOrdinary,
                    WorkersTransgenderOrdinary = request.WorkersTransgenderOrdinary,
                    TotalRatedHorsePower = request.TotalRatedHorsePower,
                    MaximumPowerToBeUsed = request.MaximumPowerToBeUsed,
                    FactoryManagerName = request.FactoryManagerName,
                    FactoryManagerFatherName = request.FactoryManagerFatherName,
                    FactoryManagerAddress = request.FactoryManagerAddress,
                    OccupierType = request.OccupierType,
                    OccupierName = request.OccupierName,
                    OccupierFatherName = request.OccupierFatherName,
                    OccupierAddress = request.OccupierAddress,
                    LandOwnerName = request.LandOwnerName,
                    LandOwnerAddress = request.LandOwnerAddress,
                    BuildingPlanReferenceNumber = request.BuildingPlanReferenceNumber,
                    BuildingPlanApprovalDate = request.BuildingPlanApprovalDate,
                    WasteDisposalReferenceNumber = request.WasteDisposalReferenceNumber,
                    WasteDisposalApprovalDate = request.WasteDisposalApprovalDate,
                    WasteDisposalAuthority = request.WasteDisposalAuthority,
                    Place = request.Place,
                    DeclarationDate = request.DeclarationDate,
                    Declaration1Accepted = request.Declaration1Accepted,
                    Declaration2Accepted = request.Declaration2Accepted,
                    Declaration3Accepted = request.Declaration3Accepted,
                    Status = "Submitted",
                    PaymentStatus = "Pending",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // Calculate payment amount based on workers and power
                var paymentResult = await CalculatePaymentAmountAsync(renewal.Id);
                renewal.PaymentAmount = paymentResult.Data;

                _context.LicenseRenewals.Add(renewal);
                await _context.SaveChangesAsync();

                _logger.LogInformation("License renewal created: {Number}", renewal.RenewalNumber);

                var districts = await LoadDistricts(new[] { renewal.District });
                var areas = await LoadAreas(new[] { renewal.Area });

                return new ApiResponseDto<LicenseRenewalDto>
                {
                    Success = true,
                    Data = MapToDto(renewal, districts, areas),
                    Message = $"License renewal created successfully with number: {renewal.RenewalNumber}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating renewal");
                return new ApiResponseDto<LicenseRenewalDto>
                {
                    Success = false,
                    Message = $"Error creating renewal: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<LicenseRenewalDto>> UpdateRenewalStatusAsync(
            string id, 
            UpdateLicenseRenewalStatusRequest request, 
            string reviewedBy)
        {
            try
            {
                var renewal = await _context.LicenseRenewals.FindAsync(id);
                if (renewal == null)
                {
                    return new ApiResponseDto<LicenseRenewalDto>
                    {
                        Success = false,
                        Message = "Renewal not found"
                    };
                }

                renewal.Status = request.Status;
                renewal.Comments = request.Comments;
                renewal.ReviewedBy = reviewedBy;
                renewal.ReviewedAt = DateTime.Now;
                renewal.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Renewal status updated: {Number} to {Status}", renewal.RenewalNumber, request.Status);

                var districts = await LoadDistricts(new[] { renewal.District });
                var areas = await LoadAreas(new[] { renewal.Area });

                return new ApiResponseDto<LicenseRenewalDto>
                {
                    Success = true,
                    Data = MapToDto(renewal, districts, areas),
                    Message = "Renewal status updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating renewal status {Id}", id);
                return new ApiResponseDto<LicenseRenewalDto>
                {
                    Success = false,
                    Message = $"Error updating renewal: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteRenewalAsync(string id)
        {
            try
            {
                var renewal = await _context.LicenseRenewals
                    .Include(r => r.Documents)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (renewal == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Renewal not found"
                    };
                }

                // Delete associated documents from file system
                foreach (var doc in renewal.Documents)
                {
                    var filePath = Path.Combine(_environment.WebRootPath, doc.FilePath.TrimStart('/'));
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }

                _context.LicenseRenewals.Remove(renewal);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Renewal deleted: {Number}", renewal.RenewalNumber);

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Renewal deleted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting renewal {Id}", id);
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = $"Error deleting renewal: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<LicenseRenewalDocumentDto>> UploadDocumentAsync(
            string renewalId, 
            IFormFile file, 
            string documentType)
        {
            try
            {
                var renewal = await _context.LicenseRenewals.FindAsync(renewalId);
                if (renewal == null)
                {
                    return new ApiResponseDto<LicenseRenewalDocumentDto>
                    {
                        Success = false,
                        Message = "Renewal not found"
                    };
                }

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "renewals", renewalId);
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var document = new LicenseRenewalDocument
                {
                    Id = Guid.NewGuid().ToString(),
                    RenewalId = renewalId,
                    DocumentType = documentType,
                    FileName = file.FileName,
                    FilePath = $"/uploads/renewals/{renewalId}/{uniqueFileName}",
                    FileSize = file.Length,
                    FileExtension = Path.GetExtension(file.FileName),
                    UploadedAt = DateTime.Now
                };

                _context.LicenseRenewalDocuments.Add(document);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Document uploaded for renewal: {RenewalId}", renewalId);

                return new ApiResponseDto<LicenseRenewalDocumentDto>
                {
                    Success = true,
                    Data = new LicenseRenewalDocumentDto
                    {
                        Id = document.Id,
                        DocumentType = document.DocumentType,
                        FileName = document.FileName,
                        FilePath = document.FilePath,
                        FileSize = document.FileSize,
                        FileExtension = document.FileExtension,
                        UploadedAt = document.UploadedAt
                    },
                    Message = "Document uploaded successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document for renewal {Id}", renewalId);
                return new ApiResponseDto<LicenseRenewalDocumentDto>
                {
                    Success = false,
                    Message = $"Error uploading document: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteDocumentAsync(string documentId)
        {
            try
            {
                var document = await _context.LicenseRenewalDocuments.FindAsync(documentId);
                if (document == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Document not found"
                    };
                }

                var filePath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                _context.LicenseRenewalDocuments.Remove(document);
                await _context.SaveChangesAsync();

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Document deleted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document {Id}", documentId);
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = $"Error deleting document: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<PaymentResponseDto>> InitiatePaymentAsync(InitiatePaymentRequest request)
        {
            try
            {
                var renewal = await _context.LicenseRenewals.FindAsync(request.RenewalId);
                if (renewal == null)
                {
                    return new ApiResponseDto<PaymentResponseDto>
                    {
                        Success = false,
                        Message = "Renewal not found"
                    };
                }

                // In a real application, integrate with payment gateway here
                // For now, generate a mock transaction ID
                var transactionId = $"TXN_{DateTime.Now.Ticks}_{Guid.NewGuid().ToString().Substring(0, 8)}";

                var paymentResponse = new PaymentResponseDto
                {
                    Success = true,
                    Message = "Payment initiated successfully",
                    TransactionId = transactionId,
                    Amount = request.Amount,
                    PaymentUrl = $"/payment/complete?txn={transactionId}&renewalId={request.RenewalId}"
                };

                _logger.LogInformation("Payment initiated for renewal: {Number}", renewal.RenewalNumber);

                return new ApiResponseDto<PaymentResponseDto>
                {
                    Success = true,
                    Data = paymentResponse,
                    Message = "Payment initiated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating payment");
                return new ApiResponseDto<PaymentResponseDto>
                {
                    Success = false,
                    Message = $"Error initiating payment: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<LicenseRenewalDto>> CompletePaymentAsync(CompletePaymentRequest request)
        {
            try
            {
                var renewal = await _context.LicenseRenewals.FindAsync(request.RenewalId);
                if (renewal == null)
                {
                    return new ApiResponseDto<LicenseRenewalDto>
                    {
                        Success = false,
                        Message = "Renewal not found"
                    };
                }

                renewal.PaymentTransactionId = request.TransactionId;
                renewal.PaymentStatus = request.PaymentStatus;
                renewal.PaymentDate = DateTime.Now;
                renewal.UpdatedAt = DateTime.Now;

                if (request.PaymentStatus == "Success")
                {
                    renewal.Status = "Under Review";
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Payment completed for renewal: {Number}", renewal.RenewalNumber);

                var districts = await LoadDistricts(new[] { renewal.District });
                var areas = await LoadAreas(new[] { renewal.Area });

                return new ApiResponseDto<LicenseRenewalDto>
                {
                    Success = true,
                    Data = MapToDto(renewal, districts, areas),
                    Message = "Payment completed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing payment");
                return new ApiResponseDto<LicenseRenewalDto>
                {
                    Success = false,
                    Message = $"Error completing payment: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<decimal>> CalculatePaymentAmountAsync(string renewalId)
        {
            try
            {
                var renewal = await _context.LicenseRenewals.FindAsync(renewalId);
                if (renewal == null)
                {
                    return new ApiResponseDto<decimal>
                    {
                        Success = false,
                        Message = "Renewal not found"
                    };
                }

                // Payment calculation logic based on workers and power
                // Base fee: 1000
                // Per worker: 50
                // Per HP: 10
                // Per year: multiply by renewal years

                decimal baseFee = 1000;
                int totalWorkers = renewal.MaxWorkersMaleProposed + renewal.MaxWorkersFemaleProposed + renewal.MaxWorkersTransgenderProposed;
                decimal workerFee = totalWorkers * 50;
                decimal powerFee = renewal.TotalRatedHorsePower * 10;
                
                decimal totalPerYear = baseFee + workerFee + powerFee;
                decimal totalAmount = totalPerYear * renewal.RenewalYears;

                return new ApiResponseDto<decimal>
                {
                    Success = true,
                    Data = totalAmount,
                    Message = "Payment amount calculated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating payment amount");
                return new ApiResponseDto<decimal>
                {
                    Success = false,
                    Message = $"Error calculating payment: {ex.Message}"
                };
            }
        }

        public string GenerateRenewalNumber()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"REN{timestamp}{random}";
        }

        private async Task<Dictionary<string?, string>> LoadDistricts(IEnumerable<string> districtIds)
        {
            var ids = districtIds.Where(d => !string.IsNullOrEmpty(d)).Distinct().ToList();
            if (!ids.Any()) return new Dictionary<string?, string>();
            var districtList = await _context.Districts.Where(d => ids.Contains(d.Id.ToString())).ToListAsync();
            return districtList
                .GroupBy(d => d.Id.ToString())
                .Where(g => g.Key != null)
                .ToDictionary(g => g.Key!, g => g.First().Name ?? string.Empty);
        }

        private async Task<Dictionary<string?, string>> LoadAreas(IEnumerable<string> areaIds)
        {
            var ids = areaIds.Where(a => !string.IsNullOrEmpty(a)).Distinct().ToList();
            if (!ids.Any()) return new Dictionary<string?, string>();
            var areaList = await _context.Areas.Where(a => ids.Contains(a.Id.ToString())).ToListAsync();
            return areaList
                .GroupBy(a => a.Id.ToString())
                .Where(g => g.Key != null)
                .ToDictionary(g => g.Key!, g => g.First().Name ?? string.Empty);
        }

        private LicenseRenewalDto MapToDto(LicenseRenewal renewal, Dictionary<string?, string> districts, Dictionary<string?, string> areas)
        {
            return new LicenseRenewalDto
            {
                Id = renewal.Id,
                RenewalNumber = renewal.RenewalNumber,
                OriginalRegistrationId = renewal.OriginalRegistrationId,
                OriginalRegistrationNumber = renewal.OriginalRegistrationNumber,
                RenewalYears = renewal.RenewalYears,
                LicenseRenewalFrom = renewal.LicenseRenewalFrom,
                LicenseRenewalTo = renewal.LicenseRenewalTo,
                FactoryName = renewal.FactoryName,
                FactoryRegistrationNumber = renewal.FactoryRegistrationNumber,
                PlotNumber = renewal.PlotNumber,
                StreetLocality = renewal.StreetLocality,
                CityTown = renewal.CityTown,
                District = renewal.District,
                DistrictName = districts.GetValueOrDefault(renewal.District),
                Area = renewal.Area,
                AreaName = areas.GetValueOrDefault(renewal.Area),
                Pincode = renewal.Pincode,
                Mobile = renewal.Mobile,
                Email = renewal.Email,
                ManufacturingProcess = renewal.ManufacturingProcess,
                ProductionStartDate = renewal.ProductionStartDate,
                ManufacturingProcessLast12Months = renewal.ManufacturingProcessLast12Months,
                ManufacturingProcessNext12Months = renewal.ManufacturingProcessNext12Months,
                ProductDetailsLast12Months = renewal.ProductDetailsLast12Months,
                MaxWorkersMaleProposed = renewal.MaxWorkersMaleProposed,
                MaxWorkersFemaleProposed = renewal.MaxWorkersFemaleProposed,
                MaxWorkersTransgenderProposed = renewal.MaxWorkersTransgenderProposed,
                MaxWorkersMaleEmployed = renewal.MaxWorkersMaleEmployed,
                MaxWorkersFemaleEmployed = renewal.MaxWorkersFemaleEmployed,
                MaxWorkersTransgenderEmployed = renewal.MaxWorkersTransgenderEmployed,
                WorkersMaleOrdinary = renewal.WorkersMaleOrdinary,
                WorkersFemaleOrdinary = renewal.WorkersFemaleOrdinary,
                WorkersTransgenderOrdinary = renewal.WorkersTransgenderOrdinary,
                TotalRatedHorsePower = renewal.TotalRatedHorsePower,
                MaximumPowerToBeUsed = renewal.MaximumPowerToBeUsed,
                FactoryManagerName = renewal.FactoryManagerName,
                FactoryManagerFatherName = renewal.FactoryManagerFatherName,
                FactoryManagerAddress = renewal.FactoryManagerAddress,
                OccupierType = renewal.OccupierType,
                OccupierName = renewal.OccupierName,
                OccupierFatherName = renewal.OccupierFatherName,
                OccupierAddress = renewal.OccupierAddress,
                LandOwnerName = renewal.LandOwnerName,
                LandOwnerAddress = renewal.LandOwnerAddress,
                BuildingPlanReferenceNumber = renewal.BuildingPlanReferenceNumber,
                BuildingPlanApprovalDate = renewal.BuildingPlanApprovalDate,
                WasteDisposalReferenceNumber = renewal.WasteDisposalReferenceNumber,
                WasteDisposalApprovalDate = renewal.WasteDisposalApprovalDate,
                WasteDisposalAuthority = renewal.WasteDisposalAuthority,
                Place = renewal.Place,
                DeclarationDate = renewal.DeclarationDate,
                Declaration1Accepted = renewal.Declaration1Accepted,
                Declaration2Accepted = renewal.Declaration2Accepted,
                Declaration3Accepted = renewal.Declaration3Accepted,
                PaymentAmount = renewal.PaymentAmount,
                PaymentStatus = renewal.PaymentStatus,
                PaymentTransactionId = renewal.PaymentTransactionId,
                PaymentDate = renewal.PaymentDate,
                Status = renewal.Status,
                Comments = renewal.Comments,
                ReviewedBy = renewal.ReviewedBy,
                ReviewedAt = renewal.ReviewedAt,
                AmendmentCount = renewal.AmendmentCount,
                CreatedAt = renewal.CreatedAt,
                UpdatedAt = renewal.UpdatedAt,
                Documents = renewal.Documents?.Select(d => new LicenseRenewalDocumentDto
                {
                    Id = d.Id,
                    DocumentType = d.DocumentType,
                    FileName = d.FileName,
                    FilePath = d.FilePath,
                    FileSize = d.FileSize,
                    FileExtension = d.FileExtension,
                    UploadedAt = d.UploadedAt
                }).ToList() ?? new List<LicenseRenewalDocumentDto>()
            };
        }
    }
}
