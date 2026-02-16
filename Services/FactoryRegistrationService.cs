using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
    public partial class FactoryRegistrationService : IFactoryRegistrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IFeeCalculationService _feeCalculationService;

        public FactoryRegistrationService(
            ApplicationDbContext context, 
            IWebHostEnvironment environment,
            IFeeCalculationService feeCalculationService)
        {
            _context = context;
            _environment = environment;
            _feeCalculationService = feeCalculationService;
        }

        public async Task<ApiResponseDto<List<FactoryRegistrationDto>>> GetAllRegistrationsAsync()
        {
            try
            {
                var registrations = await _context.FactoryRegistrations
                    .Include(r => r.Documents)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                // Load district and area names
                var districtIds = registrations.Select(r => r.District).Distinct().Where(d => !string.IsNullOrEmpty(d)).ToList();
                var areaIds = registrations.SelectMany(r => new[] { r.Area, r.FactoryManagerArea, r.OccupierArea, r.LandOwnerArea })
                    .Distinct().Where(a => !string.IsNullOrEmpty(a)).ToList();
                
                var districts = await _context.Districts.Where(d => districtIds.Contains(d.Id.ToString())).ToDictionaryAsync(d => d.Id.ToString(), d => d.Name);
                var areas = await _context.Areas.Where(a => areaIds.Contains(a.Id.ToString())).ToDictionaryAsync(a => a.Id.ToString(), a => a.Name);

                var registrationDtos = registrations.Select(r => MapToDto(r, districts, areas)).ToList();

                return new ApiResponseDto<List<FactoryRegistrationDto>>
                {
                    Success = true,
                    Message = "Factory registrations retrieved successfully",
                    Data = registrationDtos
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<List<FactoryRegistrationDto>>
                {
                    Success = false,
                    Message = $"Error retrieving factory registrations: {ex.Message}",
                    Data = new List<FactoryRegistrationDto>()
                };
            }
        }

        public async Task<ApiResponseDto<FactoryRegistrationDto>> GetRegistrationByIdAsync(string id)
        {
            try
            {
                var registration = await _context.FactoryRegistrations
                    .Include(r => r.Documents)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (registration == null)
                {
                    return new ApiResponseDto<FactoryRegistrationDto>
                    {
                        Success = false,
                        Message = "Factory registration not found",
                        Data = null
                    };
                }

                // Load district and area names
                var districts = await LoadDistricts(new[] { registration.District, registration.FactoryManagerDistrict, registration.OccupierDistrict, registration.LandOwnerDistrict });
                var areas = await LoadAreas(new[] { registration.Area, registration.FactoryManagerArea, registration.OccupierArea, registration.LandOwnerArea });

                return new ApiResponseDto<FactoryRegistrationDto>
                {
                    Success = true,
                    Message = "Factory registration retrieved successfully",
                    Data = MapToDto(registration, districts, areas)
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<FactoryRegistrationDto>
                {
                    Success = false,
                    Message = $"Error retrieving factory registration: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponseDto<FactoryRegistrationDto>> GetRegistrationByRegistrationNumberAsync(string registrationNumber)
        {
            try
            {
                var registration = await _context.FactoryRegistrations
                    .Include(r => r.Documents)
                    .FirstOrDefaultAsync(r => r.RegistrationNumber == registrationNumber);

                if (registration == null)
                {
                    return new ApiResponseDto<FactoryRegistrationDto>
                    {
                        Success = false,
                        Message = "Factory registration not found",
                        Data = null
                    };
                }

                // Load district and area names
                var districts = await LoadDistricts(new[] { registration.District, registration.FactoryManagerDistrict, registration.OccupierDistrict, registration.LandOwnerDistrict });
                var areas = await LoadAreas(new[] { registration.Area, registration.FactoryManagerArea, registration.OccupierArea, registration.LandOwnerArea });

                return new ApiResponseDto<FactoryRegistrationDto>
                {
                    Success = true,
                    Message = "Factory registration retrieved successfully",
                    Data = MapToDto(registration, districts, areas)
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<FactoryRegistrationDto>
                {
                    Success = false,
                    Message = $"Error retrieving factory registration: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponseDto<FactoryRegistrationDto>> CreateRegistrationAsync(CreateFactoryRegistrationRequest request)
        {
            try
            {
                // Calculate total workers
                int totalWorkers = request.MaxWorkersMaleProposed + 
                                   request.MaxWorkersFemaleProposed + 
                                   request.MaxWorkersTransgenderProposed;
                
                // Calculate fee
                var feeResult = await _feeCalculationService.CalculateFactoryRegistrationFee(
                    totalWorkers,
                    request.TotalRatedHorsePower,
                    request.PowerUnit
                );
                
                var registration = new FactoryRegistration
                {
                    RegistrationNumber = GenerateRegistrationNumber(),
                    MapApprovalAcknowledgementNumber = request.MapApprovalAcknowledgementNumber,
                    
                    // Period of License
                    LicenseFromDate = request.LicenseFromDate,
                    LicenseToDate = request.LicenseToDate,
                    LicenseYears = request.LicenseYears,
                    
                    // General Information
                    FactoryName = request.FactoryName,
                    FactoryRegistrationNumber = request.FactoryRegistrationNumber,
                    
                    // Factory Address and Contact Information
                    PlotNumber = request.PlotNumber,
                    StreetLocality = request.StreetLocality,
                    District = request.District,
                    CityTown = request.CityTown,
                    Area = request.Area,
                    Pincode = request.Pincode,
                    Mobile = request.Mobile,
                    Email = request.Email,
                    
                    // Nature of manufacturing process
                    ManufacturingProcess = request.ManufacturingProcess,
                    ProductionStartDate = request.ProductionStartDate,
                    ManufacturingProcessLast12Months = request.ManufacturingProcessLast12Months,
                    ManufacturingProcessNext12Months = request.ManufacturingProcessNext12Months,
                    
                    // Workers Employed
                    MaxWorkersMaleProposed = request.MaxWorkersMaleProposed,
                    MaxWorkersFemaleProposed = request.MaxWorkersFemaleProposed,
                    MaxWorkersTransgenderProposed = request.MaxWorkersTransgenderProposed,
                    MaxWorkersMaleEmployed = request.MaxWorkersMaleEmployed,
                    MaxWorkersFemaleEmployed = request.MaxWorkersFemaleEmployed,
                    MaxWorkersTransgenderEmployed = request.MaxWorkersTransgenderEmployed,
                    WorkersMaleOrdinary = request.WorkersMaleOrdinary,
                    WorkersFemaleOrdinary = request.WorkersFemaleOrdinary,
                    WorkersTransgenderOrdinary = request.WorkersTransgenderOrdinary,
                    
                    // Power Installed
                    TotalRatedHorsePower = request.TotalRatedHorsePower,
                    PowerUnit = request.PowerUnit,
                    KNumber = request.KNumber,
                    
                    // Particulars of Factory Manager
                    FactoryManagerName = request.FactoryManagerName,
                    FactoryManagerFatherName = request.FactoryManagerFatherName,
                    FactoryManagerPlotNumber = request.FactoryManagerPlotNumber,
                    FactoryManagerStreetLocality = request.FactoryManagerStreetLocality,
                    FactoryManagerDistrict = request.FactoryManagerDistrict,
                    FactoryManagerArea = request.FactoryManagerArea,
                    FactoryManagerCityTown = request.FactoryManagerCityTown,
                    FactoryManagerPincode = request.FactoryManagerPincode,
                    FactoryManagerMobile = request.FactoryManagerMobile,
                    FactoryManagerEmail = request.FactoryManagerEmail,
                    FactoryManagerPanCard = request.FactoryManagerPanCard,
                    
                    // Particulars of Occupier
                    OccupierType = request.OccupierType,
                    OccupierName = request.OccupierName,
                    OccupierFatherName = request.OccupierFatherName,
                    OccupierPlotNumber = request.OccupierPlotNumber,
                    OccupierStreetLocality = request.OccupierStreetLocality,
                    OccupierCityTown = request.OccupierCityTown,
                    OccupierDistrict = request.OccupierDistrict,
                    OccupierArea = request.OccupierArea,
                    OccupierPincode = request.OccupierPincode,
                    OccupierMobile = request.OccupierMobile,
                    OccupierEmail = request.OccupierEmail,
                    OccupierPanCard = request.OccupierPanCard,
                    
                    // Land and Building
                    LandOwnerName = request.LandOwnerName,
                    LandOwnerPlotNumber = request.LandOwnerPlotNumber,
                    LandOwnerStreetLocality = request.LandOwnerStreetLocality,
                    LandOwnerDistrict = request.LandOwnerDistrict,
                    LandOwnerArea = request.LandOwnerArea,
                    LandOwnerCityTown = request.LandOwnerCityTown,
                    LandOwnerPincode = request.LandOwnerPincode,
                    LandOwnerMobile = request.LandOwnerMobile,
                    LandOwnerEmail = request.LandOwnerEmail,
                    
                    // Building Plan Approval
                    BuildingPlanReferenceNumber = request.BuildingPlanReferenceNumber,
                    BuildingPlanApprovalDate = request.BuildingPlanApprovalDate,
                    
                    // Disposal of wastes and effluents
                    WasteDisposalReferenceNumber = request.WasteDisposalReferenceNumber,
                    WasteDisposalApprovalDate = request.WasteDisposalApprovalDate,
                    WasteDisposalAuthority = request.WasteDisposalAuthority,
                    
                    // Payment
                    WantToMakePaymentNow = request.WantToMakePaymentNow,
                    
                    // Declaration
                    DeclarationAccepted = request.DeclarationAccepted,
                    
                    Status = "Pending",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.FactoryRegistrations.Add(registration);
                await _context.SaveChangesAsync();
                
                // Create initial history entry
                var history = new Models.ApplicationHistory
                {
                    ApplicationId = registration.Id,
                    ApplicationType = "FactoryRegistration",
                    Action = "Submitted",
                    PreviousStatus = null,
                    NewStatus = "Pending",
                    Comments = "Application submitted for review",
                    ActionBy = "Applicant",
                    ActionByName = registration.OccupierName ?? "Applicant",
                    ActionDate = DateTime.Now
                };
                _context.ApplicationHistories.Add(history);
                await _context.SaveChangesAsync();
                
                // Save fee calculation
                var fee = new FactoryRegistrationFee
                {
                    Id = Guid.NewGuid(),
                    FactoryRegistrationId = registration.Id,
                    TotalWorkers = feeResult.TotalWorkers,
                    TotalPowerHP = feeResult.TotalPowerHP,
                    TotalPowerKW = feeResult.TotalPowerKW,
                    FactoryFee = feeResult.FactoryFee,
                    ElectricityFee = feeResult.ElectricityFee,
                    TotalFee = feeResult.TotalFee,
                    FeeBreakdown = System.Text.Json.JsonSerializer.Serialize(feeResult.FeeBreakdown),
                    CalculatedAt = DateTime.Now
                };
                _context.FactoryRegistrationFees.Add(fee);
                await _context.SaveChangesAsync();

                var districts = await LoadDistricts(new[] { registration.District, registration.FactoryManagerDistrict, registration.OccupierDistrict, registration.LandOwnerDistrict });
                var areas = await LoadAreas(new[] { registration.Area, registration.FactoryManagerArea, registration.OccupierArea, registration.LandOwnerArea });

                var dto = MapToDto(registration, districts, areas);
                dto.CalculatedFee = feeResult;

                return new ApiResponseDto<FactoryRegistrationDto>
                {
                    Success = true,
                    Message = "Factory registration created successfully",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<FactoryRegistrationDto>
                {
                    Success = false,
                    Message = $"Error creating factory registration: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponseDto<FactoryRegistrationDto>> UpdateRegistrationStatusAsync(string id, UpdateFactoryRegistrationStatusRequest request, string reviewedBy)
        {
            try
            {
                var registration = await _context.FactoryRegistrations
                    .Include(r => r.Documents)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (registration == null)
                {
                    return new ApiResponseDto<FactoryRegistrationDto>
                    {
                        Success = false,
                        Message = "Factory registration not found",
                        Data = null
                    };
                }

                registration.Status = request.Status;
                registration.Comments = request.Comments;
                registration.ReviewedBy = reviewedBy;
                registration.ReviewedAt = DateTime.Now;
                registration.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                var districts = await LoadDistricts(new[] { registration.District, registration.FactoryManagerDistrict, registration.OccupierDistrict, registration.LandOwnerDistrict });
                var areas = await LoadAreas(new[] { registration.Area, registration.FactoryManagerArea, registration.OccupierArea, registration.LandOwnerArea });

                return new ApiResponseDto<FactoryRegistrationDto>
                {
                    Success = true,
                    Message = "Factory registration status updated successfully",
                    Data = MapToDto(registration, districts, areas)
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<FactoryRegistrationDto>
                {
                    Success = false,
                    Message = $"Error updating factory registration status: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteRegistrationAsync(string id)
        {
            try
            {
                var registration = await _context.FactoryRegistrations
                    .Include(r => r.Documents)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (registration == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Factory registration not found",
                        Data = false
                    };
                }

                // Delete associated files
                var webRoot = string.IsNullOrWhiteSpace(_environment.WebRootPath)
                    ? Path.Combine(_environment.ContentRootPath, "wwwroot")
                    : _environment.WebRootPath;
                if (!Directory.Exists(webRoot))
                {
                    Directory.CreateDirectory(webRoot);
                }
                foreach (var document in registration.Documents)
                {
                    var filePath = Path.Combine(webRoot, document.FilePath.TrimStart('/'));
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }

                _context.FactoryRegistrations.Remove(registration);
                await _context.SaveChangesAsync();

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Factory registration deleted successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = $"Error deleting factory registration: {ex.Message}",
                    Data = false
                };
            }
        }

        public async Task<ApiResponseDto<FactoryRegistrationDocumentDto>> UploadDocumentAsync(string registrationId, IFormFile file, string documentType)
        {
            try
            {
                var registration = await _context.FactoryRegistrations.FindAsync(registrationId);
                if (registration == null)
                {
                    return new ApiResponseDto<FactoryRegistrationDocumentDto>
                    {
                        Success = false,
                        Message = "Factory registration not found",
                        Data = null
                    };
                }

                // Resolve a valid web root (fallback to ContentRoot/wwwroot)
                var webRoot = string.IsNullOrWhiteSpace(_environment.WebRootPath)
                    ? Path.Combine(_environment.ContentRootPath, "wwwroot")
                    : _environment.WebRootPath;
                if (!Directory.Exists(webRoot))
                {
                    Directory.CreateDirectory(webRoot);
                }
                // Create upload directory if it doesn't exist
                var uploadPath = Path.Combine(webRoot, "uploads", "factory-registrations", registrationId);
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var document = new FactoryRegistrationDocument
                {
                    FactoryRegistrationId = registrationId,
                    DocumentType = documentType,
                    FileName = file.FileName,
                    FilePath = $"/uploads/factory-registrations/{registrationId}/{fileName}",
                    FileSize = file.Length,
                    FileExtension = Path.GetExtension(file.FileName),
                    UploadedAt = DateTime.Now
                };

                _context.FactoryRegistrationDocuments.Add(document);
                await _context.SaveChangesAsync();

                return new ApiResponseDto<FactoryRegistrationDocumentDto>
                {
                    Success = true,
                    Message = "Document uploaded successfully",
                    Data = MapDocumentToDto(document)
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<FactoryRegistrationDocumentDto>
                {
                    Success = false,
                    Message = $"Error uploading document: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteDocumentAsync(string documentId)
        {
            try
            {
                var document = await _context.FactoryRegistrationDocuments.FindAsync(documentId);
                if (document == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Document not found",
                        Data = false
                    };
                }

                // Delete file from disk
                var webRoot = string.IsNullOrWhiteSpace(_environment.WebRootPath)
                    ? Path.Combine(_environment.ContentRootPath, "wwwroot")
                    : _environment.WebRootPath;
                if (!Directory.Exists(webRoot))
                {
                    Directory.CreateDirectory(webRoot);
                }
                var filePath = Path.Combine(webRoot, document.FilePath.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                _context.FactoryRegistrationDocuments.Remove(document);
                await _context.SaveChangesAsync();

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Document deleted successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = $"Error deleting document: {ex.Message}",
                    Data = false
                };
            }
        }

        public string GenerateRegistrationNumber()
        {
            return $"FR{DateTime.Now:yyyyMMdd}{DateTime.Now:HHmmss}";
        }

        private async Task<Dictionary<string?, string>> LoadDistricts(IEnumerable<string> districtIds)
        {
            var ids = districtIds.Where(d => !string.IsNullOrEmpty(d)).Distinct().ToList();
            if (!ids.Any()) return new Dictionary<string?, string>();
            return await _context.Districts.Where(d => ids.Contains(d.Id.ToString())).ToDictionaryAsync<District, string?, string>(d => d.Id.ToString(), d => d.Name);
        }

        private async Task<Dictionary<string?, string>> LoadAreas(IEnumerable<string> areaIds)
        {
            var ids = areaIds.Where(a => !string.IsNullOrEmpty(a)).Distinct().ToList();
            if (!ids.Any()) return new Dictionary<string?, string>();
            return await _context.Areas.Where(a => ids.Contains(a.Id.ToString())).ToDictionaryAsync<Area, string?, string>(a => a.Id.ToString(), a => a.Name);
        }

        private FactoryRegistrationDto MapToDto(FactoryRegistration registration, Dictionary<string?, string> districts, Dictionary<string?, string> areas)
        {
            return new FactoryRegistrationDto
            {
                Id = registration.Id,
                RegistrationNumber = registration.RegistrationNumber,
                MapApprovalAcknowledgementNumber = registration.MapApprovalAcknowledgementNumber,
                
                // Period of License
                LicenseFromDate = registration.LicenseFromDate,
                LicenseToDate = registration.LicenseToDate,
                LicenseYears = registration.LicenseYears,
                
                // General Information
                FactoryName = registration.FactoryName,
                FactoryRegistrationNumber = registration.FactoryRegistrationNumber,
                
                // Factory Address and Contact Information
                PlotNumber = registration.PlotNumber,
                StreetLocality = registration.StreetLocality,
                District = registration.District,
                DistrictName = districts.GetValueOrDefault(registration.District),
                CityTown = registration.CityTown,
                Area = registration.Area,
                AreaName = areas.GetValueOrDefault(registration.Area),
                Pincode = registration.Pincode,
                Mobile = registration.Mobile,
                Email = registration.Email,
                
                // Nature of manufacturing process
                ManufacturingProcess = registration.ManufacturingProcess,
                ProductionStartDate = registration.ProductionStartDate,
                ManufacturingProcessLast12Months = registration.ManufacturingProcessLast12Months,
                ManufacturingProcessNext12Months = registration.ManufacturingProcessNext12Months,
                
                // Workers Employed
                MaxWorkersMaleProposed = registration.MaxWorkersMaleProposed,
                MaxWorkersFemaleProposed = registration.MaxWorkersFemaleProposed,
                MaxWorkersTransgenderProposed = registration.MaxWorkersTransgenderProposed,
                MaxWorkersMaleEmployed = registration.MaxWorkersMaleEmployed,
                MaxWorkersFemaleEmployed = registration.MaxWorkersFemaleEmployed,
                MaxWorkersTransgenderEmployed = registration.MaxWorkersTransgenderEmployed,
                WorkersMaleOrdinary = registration.WorkersMaleOrdinary,
                WorkersFemaleOrdinary = registration.WorkersFemaleOrdinary,
                WorkersTransgenderOrdinary = registration.WorkersTransgenderOrdinary,
                
                // Power Installed
                TotalRatedHorsePower = registration.TotalRatedHorsePower,
                PowerUnit = registration.PowerUnit,
                KNumber = registration.KNumber,
                
                // Particulars of Factory Manager
                FactoryManagerName = registration.FactoryManagerName,
                FactoryManagerFatherName = registration.FactoryManagerFatherName,
                FactoryManagerPlotNumber = registration.FactoryManagerPlotNumber,
                FactoryManagerStreetLocality = registration.FactoryManagerStreetLocality,
                FactoryManagerDistrict = registration.FactoryManagerDistrict,
                FactoryManagerDistrictName = districts.GetValueOrDefault(registration.FactoryManagerDistrict),
                FactoryManagerArea = registration.FactoryManagerArea,
                FactoryManagerAreaName = areas.GetValueOrDefault(registration.FactoryManagerArea),
                FactoryManagerCityTown = registration.FactoryManagerCityTown,
                FactoryManagerPincode = registration.FactoryManagerPincode,
                FactoryManagerMobile = registration.FactoryManagerMobile,
                FactoryManagerEmail = registration.FactoryManagerEmail,
                FactoryManagerPanCard = registration.FactoryManagerPanCard,
                
                // Particulars of Occupier
                OccupierType = registration.OccupierType,
                OccupierName = registration.OccupierName,
                OccupierFatherName = registration.OccupierFatherName,
                OccupierPlotNumber = registration.OccupierPlotNumber,
                OccupierStreetLocality = registration.OccupierStreetLocality,
                OccupierCityTown = registration.OccupierCityTown,
                OccupierDistrict = registration.OccupierDistrict,
                OccupierDistrictName = districts.GetValueOrDefault(registration.OccupierDistrict),
                OccupierArea = registration.OccupierArea,
                OccupierAreaName = areas.GetValueOrDefault(registration.OccupierArea),
                OccupierPincode = registration.OccupierPincode,
                OccupierMobile = registration.OccupierMobile,
                OccupierEmail = registration.OccupierEmail,
                OccupierPanCard = registration.OccupierPanCard,
                
                // Land and Building
                LandOwnerName = registration.LandOwnerName,
                LandOwnerPlotNumber = registration.LandOwnerPlotNumber,
                LandOwnerStreetLocality = registration.LandOwnerStreetLocality,
                LandOwnerDistrict = registration.LandOwnerDistrict,
                LandOwnerDistrictName = districts.GetValueOrDefault(registration.LandOwnerDistrict),
                LandOwnerArea = registration.LandOwnerArea,
                LandOwnerAreaName = areas.GetValueOrDefault(registration.LandOwnerArea),
                LandOwnerCityTown = registration.LandOwnerCityTown,
                LandOwnerPincode = registration.LandOwnerPincode,
                LandOwnerMobile = registration.LandOwnerMobile,
                LandOwnerEmail = registration.LandOwnerEmail,
                
                // Building Plan Approval
                BuildingPlanReferenceNumber = registration.BuildingPlanReferenceNumber,
                BuildingPlanApprovalDate = registration.BuildingPlanApprovalDate,
                
                // Disposal of wastes and effluents
                WasteDisposalReferenceNumber = registration.WasteDisposalReferenceNumber,
                WasteDisposalApprovalDate = registration.WasteDisposalApprovalDate,
                WasteDisposalAuthority = registration.WasteDisposalAuthority,
                
                // Payment
                WantToMakePaymentNow = registration.WantToMakePaymentNow,
                
                // Declaration
                DeclarationAccepted = registration.DeclarationAccepted,
                
                // Status and tracking
                Status = registration.Status,
                Comments = registration.Comments,
                ReviewedBy = registration.ReviewedBy,
                ReviewedAt = registration.ReviewedAt,
                
                CreatedAt = registration.CreatedAt,
                UpdatedAt = registration.UpdatedAt,
                
                Documents = registration.Documents?.Select(MapDocumentToDto).ToList() ?? new List<FactoryRegistrationDocumentDto>()
            };
        }

        private FactoryRegistrationDocumentDto MapDocumentToDto(FactoryRegistrationDocument document)
        {
            return new FactoryRegistrationDocumentDto
            {
                Id = document.Id,
                DocumentType = document.DocumentType,
                FileName = document.FileName,
                FilePath = document.FilePath,
                FileSize = document.FileSize,
                FileExtension = document.FileExtension,
                UploadedAt = document.UploadedAt
            };
        }
    }
}