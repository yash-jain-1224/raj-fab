using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;
using System.Data;
using System.Text.Json;
using static RajFabAPI.Constants.AppConstants;
using ImageDataFactory = iText.IO.Image.ImageDataFactory;
using PdfCell = iText.Layout.Element.Cell;
using PdfImage = iText.Layout.Element.Image;
using PdfTable = iText.Layout.Element.Table;
using iText.Layout.Properties;

namespace RajFabAPI.Services
{
    public partial class FactoryMapApprovalService : IFactoryMapApprovalService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;


        public FactoryMapApprovalService(ApplicationDbContext context, IConfiguration config, IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _config = config;
            _environment = environment;
        }

        public async Task<bool> UpdateStatusAndRemark(string registrationId, string status)
        {
            try
            {
                var reg = _context.FactoryMapApprovals.FirstOrDefault(x => x.Id == registrationId);
                if (reg == null)
                    return false;
                reg.Status = status;
                reg.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public async Task<ApiResponseDto<List<FactoryMapApprovalDto>>> GetAllApplicationsAsync(Guid userId)
        //{
        //    try
        //    {
        //        var userAppIds = await _context.ApplicationRegistrations
        //            .Where(ar => ar.UserId == userId)
        //            .Select(ar => ar.ApplicationId)
        //            .ToListAsync();

        //        //var applications = await _context.FactoryMapApprovals
        //        //    .Where(f => userAppIds.Contains(f.Id))
        //        //    .Include(f => f.RawMaterials)
        //        //    .Include(f => f.IntermediateProducts)
        //        //    .Include(f => f.FinishGoods)
        //        //    .Include(f => f.Chemicals)
        //        //    .OrderByDescending(f => f.CreatedAt)
        //        //    .ToListAsync();
        //        var applications = await (
        //            from f in _context.FactoryMapApprovals
        //            join ar in _context.ApplicationRegistrations
        //                on f.Id equals ar.ApplicationId
        //            where ar.UserId == userId
        //            select f
        //        )
        //        .Include(f => f.RawMaterials)
        //        .Include(f => f.IntermediateProducts)
        //        .Include(f => f.FinishGoods)
        //        .Include(f => f.Chemicals)
        //        .OrderByDescending(f => f.CreatedAt)
        //        .ToListAsync();

        //        var applicationDtos = applications
        //            .Select(a => MapToDto(a))
        //            .ToList();
        //        return new ApiResponseDto<List<FactoryMapApprovalDto>>
        //        {
        //            Success = true,
        //            Message = "Applications retrieved successfully",
        //            Data = applicationDtos
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ApiResponseDto<List<FactoryMapApprovalDto>>
        //        {
        //            Success = false,
        //            Message = $"Error retrieving applications: {ex.Message}"
        //        };
        //    }
        //}
        public async Task<ApiResponseDto<List<FactoryMapApprovalDto>>> GetAllApplicationsAsync(Guid userId)
        {
            try
            {
                // Directly query approvals where user has registration
                var applicationDtos = await (
                 from f in _context.FactoryMapApprovals
                 join ar in _context.ApplicationRegistrations
                     on f.Id equals ar.ApplicationId
                 where ar.UserId == userId
                 orderby f.CreatedAt descending
                 select new FactoryMapApprovalDto
                 {
                     Id = f.Id,
                     AcknowledgementNumber = f.AcknowledgementNumber,
                     Date = f.Date,
                     PlantParticulars = f.PlantParticulars,
                     ProductName = f.ProductName,
                     ManufacturingProcess = f.ManufacturingProcess,
                     MaxWorkerMale = f.MaxWorkerMale,
                     MaxWorkerFemale = f.MaxWorkerFemale,
                     AreaFactoryPremise = f.AreaFactoryPremise,
                     NoOfFactoriesIfCommonPremise = f.NoOfFactoriesIfCommonPremise,
                     PremiseOwnerName = f.PremiseOwnerName,
                     PremiseOwnerContactNo = f.PremiseOwnerContactNo,
                     PremiseOwnerAddressPlotNo = f.PremiseOwnerAddressPlotNo,
                     PremiseOwnerAddressStreet = f.PremiseOwnerAddressStreet,
                     PremiseOwnerAddressCity = f.PremiseOwnerAddressCity,
                     PremiseOwnerAddressDistrict = f.PremiseOwnerAddressDistrict,
                     PremiseOwnerAddressState = f.PremiseOwnerAddressState,
                     PremiseOwnerAddressPinCode = f.PremiseOwnerAddressPinCode,
                     Place = f.Place,
                     Status = f.Status,
                     IsNew = f.IsNew,
                     Version = f.Version,
                     ApplicationPDFUrl = f.ApplicationPDFUrl,
                     FactoryDetails = f.FactoryDetails,
                     OccupierDetails = f.OccupierDetails,
                     CreatedAt = f.CreatedAt,
                 })
                 .AsNoTracking()
                 .ToListAsync();

                return new ApiResponseDto<List<FactoryMapApprovalDto>>
                {
                    Success = true,
                    Message = "Applications retrieved successfully",
                    Data = applicationDtos
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<List<FactoryMapApprovalDto>>
                {
                    Success = false,
                    Message = $"Error retrieving applications: {ex.Message}"
                };
            }
        }


        public async Task<ApiResponseDto<FactoryMapApprovalDto>> GetApplicationByIdAsync(string id)
        {
            try
            {
                var application = await _context.FactoryMapApprovals
                    .Include(f => f.RawMaterials)
                    .Include(f => f.IntermediateProducts)
                    .Include(f => f.FinishGoods)
                    .Include(f => f.Chemicals)
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (application == null)
                {
                    return new ApiResponseDto<FactoryMapApprovalDto>
                    {
                        Success = false,
                        Message = "Application not found"
                    };
                }

                var dto = MapToDto(application);

                dto.ApplicationHistory = await _context.Set<ApplicationHistory>()
                    .AsNoTracking()
                    .Where(x => x.ApplicationId == application.Id)
                    .OrderByDescending(x => x.ActionDate)
                    .ToListAsync();

                // dto.ApplicationHistory = await _context.ApplicationHistories
                //     .Where(h => h.ApplicationId == id)
                //     .OrderBy(h => h.ActionDate)
                //     .ToListAsync();

                var activeCertificate = await _context.Certificates
                    .AsNoTracking()
                    .Where(c => c.ApplicationId == application.Id)
                    .OrderByDescending(c => c.CertificateVersion)
                    .FirstOrDefaultAsync();
                dto.CertificatePDFUrl = activeCertificate?.CertificateUrl;

                return new ApiResponseDto<FactoryMapApprovalDto>
                {
                    Success = true,
                    Message = "Application retrieved successfully",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<FactoryMapApprovalDto>
                {
                    Success = false,
                    Message = $"Error retrieving application: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<FactoryMapApprovalDto>> GetApplicationByAcknowledgementNumberAsync(string acknowledgementNumber)
        {
            try
            {
                var application = await _context.FactoryMapApprovals
                    .Include(f => f.RawMaterials)
                    .Include(f => f.IntermediateProducts)
                    .Include(f => f.Chemicals)
                    .FirstOrDefaultAsync(f => f.AcknowledgementNumber == acknowledgementNumber);

                if (application == null)
                {
                    return new ApiResponseDto<FactoryMapApprovalDto>
                    {
                        Success = false,
                        Message = "Application not found"
                    };
                }

                return new ApiResponseDto<FactoryMapApprovalDto>
                {
                    Success = true,
                    Message = "Application retrieved successfully",
                    Data = MapToDto(application)
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<FactoryMapApprovalDto>
                {
                    Success = false,
                    Message = $"Error retrieving application: {ex.Message}"
                };
            }
        }

        public async Task<string> CreateApplicationAsync(CreateFactoryMapApprovalRequest request, Guid userId, bool? isNew, string? factoryMapApprovalId)
        {
            try
            {
                decimal newVersion;
                string acknowledgementNumber;

                if (isNew == false && !string.IsNullOrWhiteSpace(factoryMapApprovalId))
                {
                    var lastApproved = await _context.Set<FactoryMapApproval>().Where(r =>
                            r.Id == factoryMapApprovalId &&
                            r.Status == ApplicationStatus.Approved).OrderByDescending(r => r.Version).FirstOrDefaultAsync(m => m.Id == factoryMapApprovalId);

                    if (lastApproved == null)
                        throw new ArgumentException("Existing approved registration not found.");

                    // Calculate next version
                    newVersion = Math.Round(lastApproved.Version + 0.1m, 1);
                    acknowledgementNumber = lastApproved.AcknowledgementNumber;
                }
                else
                {
                    acknowledgementNumber = GenerateAcknowledgementNumber();
                    newVersion = 1.0m;
                }

                // Ensure Id is set before using as FK
                var application = new FactoryMapApproval
                {
                    Id = Guid.NewGuid().ToString(),
                    AcknowledgementNumber = acknowledgementNumber,
                    PlantParticulars = request.PlantParticulars,
                    ProductName = request.ProductName,
                    ManufacturingProcess = request.ManufacturingProcess,
                    MaxWorkerMale = request.MaxWorkerMale,
                    MaxWorkerFemale = request.MaxWorkerFemale,
                    AreaFactoryPremise = request.AreaFactoryPremise,
                    NoOfFactoriesIfCommonPremise = request.NoOfFactoriesIfCommonPremise,
                    PremiseOwnerName = request.PremiseOwnerName,
                    PremiseOwnerContactNo = request.PremiseOwnerContactNo,
                    PremiseOwnerAddressPlotNo = request.PremiseOwnerAddressPlotNo,
                    PremiseOwnerAddressStreet = request.PremiseOwnerAddressStreet,
                    PremiseOwnerAddressCity = request.PremiseOwnerAddressCity,
                    PremiseOwnerAddressDistrict = request.PremiseOwnerAddressDistrict,
                    PremiseOwnerAddressState = request.PremiseOwnerAddressState,
                    PremiseOwnerAddressPinCode = request.PremiseOwnerAddressPinCode,
                    Place = request.Place,
                    Date = request.Date,
                    OccupierDetails = request.OccupierDetails,
                    FactoryDetails = request.FactoryDetails,
                    IsNew = isNew ?? true,
                    Version = newVersion
                };

                // Add raw materials if provided
                if (request.RawMaterials != null && request.RawMaterials.Any())
                {
                    foreach (var rawMaterial in request.RawMaterials)
                    {
                        application.RawMaterials.Add(new FactoryMapRawMaterial
                        {
                            FactoryMapApprovalId = application.Id,
                            MaterialName = rawMaterial.MaterialName,
                            MaxStorageQuantity = rawMaterial.MaxStorageQuantity
                        });
                    }
                }

                // Add intermediate products if provided
                if (request.IntermediateProducts != null && request.IntermediateProducts.Any())
                {
                    foreach (var product in request.IntermediateProducts)
                    {
                        application.IntermediateProducts.Add(new FactoryMapIntermediateProduct
                        {
                            FactoryMapApprovalId = application.Id,
                            ProductName = product.ProductName,
                            MaxStorageQuantity = product.MaxStorageQuantity,
                        });
                    }
                }

                // Add finish goods if provided
                if (request.FinishGoods != null && request.FinishGoods.Any())
                {
                    foreach (var product in request.FinishGoods)
                    {
                        decimal res = 0;
                        Decimal.TryParse(product.MaxStorageCapacity, out res);
                        application.FinishGoods.Add(new FactoryMapFinishGood
                        {
                            FactoryMapApprovalId = application.Id,
                            ProductName = product.ProductName,
                            QuantityPerDay = product.QuantityPerDay,
                            Unit = product.Unit ?? "",
                            MaxStorageCapacity = res == 0 ? null : res,
                            StorageMethod = product.StorageMethod,
                            Remarks = product.Remarks
                        });
                    }
                }

                // Add chemicals if provided
                if (request.Chemicals != null && request.Chemicals.Any())
                {
                    foreach (var chemical in request.Chemicals)
                    {
                        application.Chemicals.Add(new FactoryMapApprovalChemical
                        {
                            FactoryMapApprovalId = application.Id,
                            ChemicalName = chemical.ChemicalName,
                            TradeName = chemical.TradeName,
                            MaxStorageQuantity = chemical.MaxStorageQuantity
                        });
                    }
                }

                _context.FactoryMapApprovals.Add(application);
                await _context.SaveChangesAsync();

                // Create initial history entry
                var history = new Models.ApplicationHistory
                {
                    ApplicationId = application.Id,
                    ApplicationType = "FactoryMapApproval",
                    Action = "Submitted",
                    PreviousStatus = null,
                    NewStatus = "Pending",
                    Comments = "Application submitted for review",
                    ActionBy = "Applicant",
                    ActionDate = DateTime.Now
                };
                _context.ApplicationHistories.Add(history);
                await _context.SaveChangesAsync();

                var moduleName = isNew != false
                    ? ApplicationTypeNames.MapApproval
                    : ApplicationTypeNames.MapApprovalAmendment;

                var module = await _context.Set<FormModule>().FirstOrDefaultAsync(m => m.Name == moduleName);

                var appReg = new ApplicationRegistration
                {
                    Id = Guid.NewGuid().ToString(),
                    ModuleId = module.Id,
                    UserId = userId,
                    ApplicationId = application.Id,
                    ApplicationRegistrationNumber = acknowledgementNumber,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };
                _context.Set<ApplicationRegistration>().Add(appReg);
                await _context.SaveChangesAsync();
                return application.Id;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating application: {ex.Message}");
                //return new ApiResponseDto<FactoryMapApprovalDto>
                //{
                //    Success = false,
                //    Message = $"Error creating application: {ex.Message}"
                //};
            }
        }

        public async Task<ApiResponseDto<FactoryMapApprovalDto>> UpdateApplicationStatusAsync(string id, UpdateFactoryMapApprovalStatusRequest request, string reviewedBy)
        {
            try
            {
                var application = await _context.FactoryMapApprovals
                    .Include(f => f.RawMaterials)
                    .Include(f => f.IntermediateProducts)
                    .Include(f => f.FinishGoods)
                    .Include(f => f.Chemicals)
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (application == null)
                {
                    return new ApiResponseDto<FactoryMapApprovalDto>
                    {
                        Success = false,
                        Message = "Application not found"
                    };
                }

                var previousStatus = application.Status;
                application.Status = request.Status;
                application.UpdatedAt = DateTime.Now;

                var action = request.Status == "Approved" ? "Approved"
                    : request.Status == "Returned to applicant" ? "Returned to Applicant"
                    : "Forwarded";

                var history = new Models.ApplicationHistory
                {
                    ApplicationId = id,
                    ApplicationType = "FactoryMapApproval",
                    Action = action,
                    PreviousStatus = previousStatus,
                    NewStatus = request.Status,
                    Comments = request.Comments,
                    ActionBy = reviewedBy,
                    ActionDate = DateTime.Now
                };
                _context.ApplicationHistories.Add(history);

                await _context.SaveChangesAsync();

                return new ApiResponseDto<FactoryMapApprovalDto>
                {
                    Success = true,
                    Message = "Application status updated successfully",
                    Data = MapToDto(application)
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<FactoryMapApprovalDto>
                {
                    Success = false,
                    Message = $"Error updating application status: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteApplicationAsync(string id)
        {
            try
            {
                var application = await _context.FactoryMapApprovals
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (application == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Application not found"
                    };
                }

                _context.FactoryMapApprovals.Remove(application);
                await _context.SaveChangesAsync();

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Application deleted successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = $"Error deleting application: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<FactoryMapDocumentDto>> UploadDocumentAsync(string applicationId, IFormFile file, string documentType)
        {
            try
            {
                var application = await _context.FactoryMapApprovals.FindAsync(applicationId);
                if (application == null)
                {
                    return new ApiResponseDto<FactoryMapDocumentDto>
                    {
                        Success = false,
                        Message = "Application not found"
                    };
                }

                // Handle WebRootPath properly - use fallback if null
                string baseUploadPath;
                if (!string.IsNullOrEmpty(_environment.WebRootPath))
                {
                    baseUploadPath = _environment.WebRootPath;
                }
                else
                {
                    // Fallback to Content Root if WebRootPath is not available
                    baseUploadPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
                }

                var uploadsPath = Path.Combine(baseUploadPath, "uploads", "factory-maps");

                // Ensure directory exists with proper error handling
                try
                {
                    Directory.CreateDirectory(uploadsPath);
                }
                catch (Exception dirEx)
                {
                    return new ApiResponseDto<FactoryMapDocumentDto>
                    {
                        Success = false,
                        Message = $"Failed to create upload directory: {dirEx.Message}"
                    };
                }

                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);
                var relativePath = $"/uploads/factory-maps/{fileName}";

                // Save file to disk with better error handling
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
                catch (Exception fileEx)
                {
                    return new ApiResponseDto<FactoryMapDocumentDto>
                    {
                        Success = false,
                        Message = $"Failed to save file: {fileEx.Message}"
                    };
                }

                // Save document record
                var document = new FactoryMapDocument
                {
                    FactoryMapApprovalId = applicationId,
                    DocumentType = documentType,
                    FileName = file.FileName,
                    FilePath = relativePath,
                    FileSize = $"{file.Length / 1024} KB",
                    FileExtension = fileExtension
                };

                try
                {
                    _context.FactoryMapDocuments.Add(document);
                    await _context.SaveChangesAsync();
                }
                catch (Exception dbEx)
                {
                    // Clean up the file if database save fails
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    return new ApiResponseDto<FactoryMapDocumentDto>
                    {
                        Success = false,
                        Message = $"Failed to save document record: {dbEx.Message}"
                    };
                }

                return new ApiResponseDto<FactoryMapDocumentDto>
                {
                    Success = true,
                    Message = "Document uploaded successfully",
                    Data = MapDocumentToDto(document)
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<FactoryMapDocumentDto>
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
                var document = await _context.FactoryMapDocuments.FindAsync(documentId);
                if (document == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Document not found"
                    };
                }

                // Delete file from disk
                var filePath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                _context.FactoryMapDocuments.Remove(document);
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
                    Message = $"Error deleting document: {ex.Message}"
                };
            }
        }

        public string GenerateAcknowledgementNumber()
        {
            var year = DateTime.Now.Year;
            var sequence = DateTime.Now.Ticks.ToString().Substring(8, 6);
            return $"FMA{year}{sequence}";
        }

        private async Task<Dictionary<string, string>> LoadDistricts(IEnumerable<string?> districtIds)
        {
            try
            {
                var ids = districtIds.Where(d => !string.IsNullOrEmpty(d)).Distinct().ToList();
                if (!ids.Any()) return new Dictionary<string, string>();
                List<District> districtList = new List<District>();
                var guidIds = ids.Select(Guid.Parse).ToList();
                foreach (var id in guidIds)
                {
                    districtList.Add(
                        await _context.Districts
                        .FirstOrDefaultAsync(d => d.Id == id) ?? new District());
                }

                return districtList
                    .ToDictionary(d => d.Id.ToString(), d => d.Name ?? string.Empty);
            }
            catch (Exception ex)
            {
                // Optionally log the exception here
                return new Dictionary<string, string>();
            }
        }

        private async Task<Dictionary<string, string>> LoadAreas(IEnumerable<string?> areaIds)
        {
            var ids = areaIds.Where(a => !string.IsNullOrEmpty(a)).Distinct().ToList();
            if (!ids.Any()) return new Dictionary<string, string>();
            List<Area> areaList = new List<Area>();

            var guidIds = ids.Select(Guid.Parse).ToList();
            foreach (var id in guidIds)
            {
                areaList.Add(
                    await _context.Areas
                    .FirstOrDefaultAsync(d => d.Id == id) ?? new Area());
            }

            return areaList
                .GroupBy(a => a.Id.ToString())
                .Where(g => g.Key != null)
                .ToDictionary(g => g.Key!, g => g.First().Name ?? string.Empty);
        }

        private async Task<Dictionary<string, string>> LoadPoliceStations(IEnumerable<string?> stationIds)
        {
            var ids = stationIds.Where(p => !string.IsNullOrEmpty(p)).Distinct().ToList();
            if (!ids.Any()) return new Dictionary<string, string>();

            var stationList = await _context.PoliceStations
                .Where(p => p.Id != null && ids.Contains(p.Id.ToString()))
                .ToListAsync();

            return stationList
                .GroupBy(p => p.Id.ToString())
                .Where(g => g.Key != null)
                .ToDictionary(g => g.Key!, g => g.First().Name ?? string.Empty);
        }

        private async Task<Dictionary<string, string>> LoadRailwayStations(IEnumerable<string?> stationIds)
        {
            var ids = stationIds.Where(r => !string.IsNullOrEmpty(r)).Distinct().ToList();
            if (!ids.Any()) return new Dictionary<string, string>();

            var stationList = await _context.RailwayStations
                .Where(r => r.Id != null && ids.Contains(r.Id.ToString()))
                .ToListAsync();

            return stationList
                .GroupBy(r => r.Id.ToString())
                .Where(g => g.Key != null)
                .ToDictionary(g => g.Key!, g => g.First().Name ?? string.Empty);
        }

        private static FactoryMapApprovalDto MapToDto(
            FactoryMapApproval application)
        {
            var res = new FactoryMapApprovalDto
            {
                Id = application.Id,
                AcknowledgementNumber = application.AcknowledgementNumber,
                FactoryDetails = application.FactoryDetails != null ? application?.FactoryDetails : null,
                OccupierDetails = application.OccupierDetails != null ? application.OccupierDetails : null,
                PlantParticulars = application.PlantParticulars,
                ProductName = application.ProductName,
                ManufacturingProcess = application.ManufacturingProcess,
                MaxWorkerMale = application.MaxWorkerMale,
                MaxWorkerFemale = application.MaxWorkerFemale,
                AreaFactoryPremise = application.AreaFactoryPremise,
                NoOfFactoriesIfCommonPremise = application.NoOfFactoriesIfCommonPremise,
                PremiseOwnerName = application.PremiseOwnerName,
                PremiseOwnerAddressPlotNo = application.PremiseOwnerAddressPlotNo,
                PremiseOwnerAddressStreet = application.PremiseOwnerAddressStreet,
                PremiseOwnerAddressCity = application.PremiseOwnerAddressCity,
                PremiseOwnerAddressDistrict = application.PremiseOwnerAddressDistrict,
                PremiseOwnerAddressState = application.PremiseOwnerAddressState,
                PremiseOwnerAddressPinCode = application.PremiseOwnerAddressPinCode,
                PremiseOwnerContactNo = application.PremiseOwnerContactNo,
                Place = application.Place,
                Date = application.Date,
                Status = application.Status,
                Version = application.Version,
                ApplicationPDFUrl = application.ApplicationPDFUrl,
                CreatedAt = application.CreatedAt,
                UpdatedAt = application.UpdatedAt,
                RawMaterials = application.RawMaterials.Select(r => new FactoryMapRawMaterialDto
                {
                    Id = r.Id,
                    MaterialName = r.MaterialName,
                    MaxStorageQuantity = r.MaxStorageQuantity,
                }).ToList(),
                IntermediateProducts = application.IntermediateProducts.Select(p => new FactoryMapIntermediateProductDto
                {
                    Id = p.Id,
                    ProductName = p.ProductName,
                    MaxStorageQuantity = p.MaxStorageQuantity
                }).ToList(),
                FinishGoods = application.FinishGoods.Select(f => new FactoryMapFinishGoodDto
                {
                    Id = f.Id,
                    ProductName = f.ProductName,
                    QuantityPerDay = f.QuantityPerDay,
                    Unit = f.Unit,
                    MaxStorageCapacity = f.MaxStorageCapacity,
                    StorageMethod = f.StorageMethod,
                    Remarks = f.Remarks
                }).ToList(),
                Chemicals = application.Chemicals.Select(h => new ChemicalDto
                {
                    Id = h.Id,
                    ChemicalName = h.ChemicalName,
                    TradeName = h.TradeName,
                    MaxStorageQuantity = h.MaxStorageQuantity
                }).ToList(),
            };
            return res;
        }

        private static FactoryMapDocumentDto MapDocumentToDto(FactoryMapDocument document)
        {
            return new FactoryMapDocumentDto
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

        public string GenerateRegistrationNumber()
        {
            var year = DateTime.Now.Year;
            var sequence = DateTime.Now.Ticks.ToString().Substring(8, 6);
            return $"FMA{year}{sequence}";
        }

        public async Task<string> GenerateCertificateAsync(MapApprovalCertificateRequestDto dto, Guid userId, string applicationId)
        {
            var application = await _context.FactoryMapApprovals
                .Include(f => f.RawMaterials)
                .Include(f => f.IntermediateProducts)
                .Include(f => f.FinishGoods)
                .Include(f => f.Chemicals)
                .FirstOrDefaultAsync(f => f.Id == applicationId);

            if (application == null)
                throw new KeyNotFoundException("Application not found.");

            var certificateUrl = await GenerateMapApprovalCertificatePdf(application, dto);

            var module = await _context.Set<FormModule>().FirstOrDefaultAsync(m => m.Name == ApplicationTypeNames.MapApproval);
            if (module == null)
                throw new InvalidOperationException("MapApproval module not found.");

            var certificate = new Certificate
            {
                RegistrationNumber = application.AcknowledgementNumber,
                CertificateVersion = 1.0m,
                StartDate = DateTime.TryParse(dto.StartDate, out var start) ? start : DateTime.Now,
                EndDate = DateTime.TryParse(dto.EndDate, out var end) ? end : DateTime.Now.AddYears(1),
                CertificateUrl = certificateUrl,
                IssuedByUserId = userId,
                IssuedAt = DateTime.TryParse(dto.IssuedAt, out var issuedAt) ? issuedAt : DateTime.Now,
                Place = dto.Place,
                Signature = dto.Signature,
                Status = "PendingESign",
                ModuleId = module.Id,
                Remarks = dto.Remarks,
                ApplicationId = applicationId,
                IsESignCompleted = false
            };
            _context.Set<Certificate>().Add(certificate);

            var history = new Models.ApplicationHistory
            {
                ApplicationId = applicationId,
                ApplicationType = "FactoryMapApproval",
                Action = "Certificate Generated",
                PreviousStatus = application.Status,
                NewStatus = application.Status,
                Comments = "Certificate generated and sent for e-sign",
                ActionBy = userId.ToString(),
                ActionDate = DateTime.Now
            };
            _context.ApplicationHistories.Add(history);

            await _context.SaveChangesAsync();

            return certificate.Id.ToString();
        }

        private async Task<string> GenerateMapApprovalCertificatePdf(FactoryMapApproval application, MapApprovalCertificateRequestDto dto)
        {
            var fileName = $"map_approval_certificate_{application.AcknowledgementNumber}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

            var webRootPath = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
                throw new InvalidOperationException("wwwroot is not configured.");

            var uploadPath = Path.Combine(webRootPath, "certificates");
            Directory.CreateDirectory(uploadPath);
            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/certificates/{fileName}";

            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            var boldFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

            var occupier = string.IsNullOrWhiteSpace(application.OccupierDetails)
                ? null
                : JsonSerializer.Deserialize<OccupierDetailsModel>(application.OccupierDetails);

            var factory = string.IsNullOrWhiteSpace(application.FactoryDetails)
                ? null
                : JsonSerializer.Deserialize<FactoryDetailsModel>(application.FactoryDetails);

            // Header
            var headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 4 })).UseAllAvailableWidth();
            headerTable.AddCell(new Cell().Add(new PdfImage(ImageDataFactory.Create("wwwroot/Emblem_of_India.png")).ScaleToFit(40, 40)).SetBorder(Border.NO_BORDER));
            headerTable.AddCell(new Cell()
                .Add(new Paragraph("FACTORY MAP APPROVAL CERTIFICATE").SetFont(boldFont).SetFontSize(16))
                .Add(new Paragraph("Under the Factories Act, 1948").SetFont(regularFont).SetFontSize(12))
                .Add(new Paragraph("Government of Rajasthan").SetFontColor(ColorConstants.BLUE).SetFontSize(12))
                .SetBorder(Border.NO_BORDER));
            document.Add(headerTable);
            document.Add(new Paragraph().SetMarginBottom(10));

            document.Add(new Paragraph($"Certificate No: {application.AcknowledgementNumber}").SetFont(boldFont));
            document.Add(new Paragraph($"Date: {DateTime.Now:dd/MM/yyyy}").SetMarginBottom(10));

            var detailTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 2 })).UseAllAvailableWidth().SetMarginBottom(10);

            if (factory != null)
            {
                detailTable.AddCell(new Cell().Add(new Paragraph("Factory Name").SetFont(boldFont)));
                detailTable.AddCell(new Cell().Add(new Paragraph(factory.name ?? "-")));

                detailTable.AddCell(new Cell().Add(new Paragraph("Factory Address").SetFont(boldFont)));
                detailTable.AddCell(new Cell().Add(new Paragraph($"{factory.addressLine1}, {factory.addressLine2}, {factory.area} - {factory.pincode}")));
            }

            if (occupier != null)
            {
                detailTable.AddCell(new Cell().Add(new Paragraph("Occupier Name").SetFont(boldFont)));
                detailTable.AddCell(new Cell().Add(new Paragraph(occupier.name ?? "-")));
            }

            detailTable.AddCell(new Cell().Add(new Paragraph("Acknowledgement No.").SetFont(boldFont)));
            detailTable.AddCell(new Cell().Add(new Paragraph(application.AcknowledgementNumber)));

            detailTable.AddCell(new Cell().Add(new Paragraph("Product Name").SetFont(boldFont)));
            detailTable.AddCell(new Cell().Add(new Paragraph(application.ProductName ?? "-")));

            detailTable.AddCell(new Cell().Add(new Paragraph("Max Workers (Male / Female)").SetFont(boldFont)));
            detailTable.AddCell(new Cell().Add(new Paragraph($"{application.MaxWorkerMale} / {application.MaxWorkerFemale}")));

            detailTable.AddCell(new Cell().Add(new Paragraph("Valid From").SetFont(boldFont)));
            detailTable.AddCell(new Cell().Add(new Paragraph(dto.StartDate ?? "-")));

            detailTable.AddCell(new Cell().Add(new Paragraph("Valid To").SetFont(boldFont)));
            detailTable.AddCell(new Cell().Add(new Paragraph(dto.EndDate ?? "-")));

            document.Add(detailTable);

            document.Add(new Paragraph("This certificate is issued certifying that the factory map plan has been approved under the Factories Act, 1948.")
                .SetFont(regularFont).SetMarginTop(10));

            if (!string.IsNullOrWhiteSpace(dto.Remarks))
                document.Add(new Paragraph($"Remarks: {dto.Remarks}").SetFont(regularFont));

            document.Add(new Paragraph().SetMarginTop(20));
            document.Add(new Paragraph($"Place: {dto.Place ?? "-"}").SetFont(regularFont));
            document.Add(new Paragraph($"Date: {DateTime.Now:dd/MM/yyyy}").SetFont(regularFont));
            document.Add(new Paragraph("This is a system generated document.").SetFontSize(8).SetFontColor(ColorConstants.GRAY));

            document.Close();

            return fileUrl;
        }

        public async Task<string> GenerateFactoryMapApprovalPdf(FactoryMapApprovalDto dto)
        {
            try
            {
                if (dto == null) throw new ArgumentNullException(nameof(dto));

                var fileName = $"factory_map_{dto.AcknowledgementNumber}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

                var webRootPath = _environment.WebRootPath;
                if (string.IsNullOrWhiteSpace(webRootPath))
                    throw new InvalidOperationException("wwwroot is not configured.");

                var uploadPath = Path.Combine(webRootPath, "factory-map-forms");
                Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);

                var httpContext = _httpContextAccessor.HttpContext
                    ?? throw new InvalidOperationException("HTTP context unavailable");

                var request = httpContext.Request;
                var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
                var fileUrl = $"{baseUrl}/factory-map-forms/{fileName}";

                using var writer = new PdfWriter(filePath);
                using var pdf = new PdfDocument(writer);
                using var document = new Document(pdf);

                var boldFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
                var regularFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

                var occupier = string.IsNullOrWhiteSpace(dto.OccupierDetails)
                    ? null
                    : JsonSerializer.Deserialize<OccupierDetailsModel>(dto.OccupierDetails);

                var factory = string.IsNullOrWhiteSpace(dto.FactoryDetails)
                    ? null
                    : JsonSerializer.Deserialize<FactoryDetailsModel>(dto.FactoryDetails);

                if (dto == null) throw new ArgumentNullException(nameof(dto));

                // ================= HEADER =================
                var headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 4 })).UseAllAvailableWidth();
                headerTable.AddCell(new Cell().Add(new PdfImage(ImageDataFactory.Create("wwwroot/Emblem_of_India.png")).ScaleToFit(40, 40)).SetBorder(Border.NO_BORDER));
                headerTable.AddCell(new Cell()
                    .Add(new Paragraph("Form - 6").SetFont(boldFont).SetFontSize(18))
                    .Add(new Paragraph("(See clause (d) of sub rule (1) of rule 5)").SetFont(regularFont).SetFontSize(12))
                    .Add(new Paragraph("FACTORY MAP APPROVAL FORM").SetFontColor(ColorConstants.BLUE).SetFontSize(12))
                    .SetBorder(Border.NO_BORDER));
                document.Add(headerTable);
                document.Add(new Paragraph().SetMarginBottom(10));

                // ================= ACKNOWLEDGEMENT =================
                document.Add(new Paragraph($"Acknowledgement No: {dto.AcknowledgementNumber}").SetFont(regularFont));
                document.Add(new Paragraph($"Date: {(dto.Date?.ToString("dd/MM/yyyy") ?? "-")}").SetMarginBottom(10));

                // ================= FACTORY + OCCUPIER DETAILS =================
                var sectionDiv = new Div().SetKeepTogether(true);
                var factoryTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 2 })).UseAllAvailableWidth().SetMarginBottom(5);

                if (factory != null)
                {
                    factoryTable.AddCell(new Cell().Add(new Paragraph("Factory Name").SetFont(boldFont)));
                    factoryTable.AddCell(new Cell().Add(new Paragraph(factory.name ?? "-")));

                    factoryTable.AddCell(new Cell().Add(new Paragraph("Situation").SetFont(boldFont)));
                    factoryTable.AddCell(new Cell().Add(new Paragraph(factory.situation ?? "-")));

                    factoryTable.AddCell(new Cell().Add(new Paragraph("Address").SetFont(boldFont)));
                    factoryTable.AddCell(new Cell().Add(new Paragraph($"{factory.addressLine1}, {factory.addressLine2}, {factory.area} - {factory.pincode}")));

                    factoryTable.AddCell(new Cell().Add(new Paragraph("Email").SetFont(boldFont)));
                    factoryTable.AddCell(new Cell().Add(new Paragraph(factory.email ?? "-")));

                    factoryTable.AddCell(new Cell().Add(new Paragraph("Mobile").SetFont(boldFont)));
                    factoryTable.AddCell(new Cell().Add(new Paragraph(factory.mobile ?? "-")));

                    factoryTable.AddCell(new Cell().Add(new Paragraph("Telephone").SetFont(boldFont)));
                    factoryTable.AddCell(new Cell().Add(new Paragraph(factory.telephone ?? "-")));

                    factoryTable.AddCell(new Cell().Add(new Paragraph("Website").SetFont(boldFont)));
                    factoryTable.AddCell(new Cell().Add(new Paragraph(factory.website ?? "-")));
                }

                if (occupier != null)
                {
                    factoryTable.AddCell(new Cell().Add(new Paragraph("Occupier Name").SetFont(boldFont)));
                    factoryTable.AddCell(new Cell().Add(new Paragraph(occupier.name ?? "-")));

                    factoryTable.AddCell(new Cell().Add(new Paragraph("Designation").SetFont(boldFont)));
                    factoryTable.AddCell(new Cell().Add(new Paragraph(occupier.designation ?? "-")));

                    factoryTable.AddCell(new Cell().Add(new Paragraph("Relation").SetFont(boldFont)));
                    factoryTable.AddCell(new Cell().Add(new Paragraph($"{occupier.relationType} {occupier.relativeName}")));

                    factoryTable.AddCell(new Cell().Add(new Paragraph("Address").SetFont(boldFont)));
                    factoryTable.AddCell(new Cell().Add(new Paragraph($"{occupier.addressLine1}, {occupier.addressLine2}, {occupier.area}, {occupier.tehsil}, {occupier.district} - {occupier.pincode}")));

                    factoryTable.AddCell(new Cell().Add(new Paragraph("Email").SetFont(boldFont)));
                    factoryTable.AddCell(new Cell().Add(new Paragraph(occupier.email ?? "-")));

                    factoryTable.AddCell(new Cell().Add(new Paragraph("Mobile").SetFont(boldFont)));
                    factoryTable.AddCell(new Cell().Add(new Paragraph(occupier.mobile ?? "-")));

                    factoryTable.AddCell(new Cell().Add(new Paragraph("Telephone").SetFont(boldFont)));
                    factoryTable.AddCell(new Cell().Add(new Paragraph(occupier.telephone ?? "-")));
                }

                factoryTable.AddCell(new Cell().Add(new Paragraph("Plant Particulars").SetFont(boldFont)));
                factoryTable.AddCell(new Cell().Add(new Paragraph(dto.PlantParticulars ?? "-")));

                factoryTable.AddCell(new Cell().Add(new Paragraph("Product Name").SetFont(boldFont)));
                factoryTable.AddCell(new Cell().Add(new Paragraph(dto.ProductName ?? "-")));

                factoryTable.AddCell(new Cell().Add(new Paragraph("Manufacturing Process").SetFont(boldFont)));
                factoryTable.AddCell(new Cell().Add(new Paragraph(dto.ManufacturingProcess ?? "-")));

                factoryTable.AddCell(new Cell().Add(new Paragraph("Max Workers (Male)").SetFont(boldFont)));
                factoryTable.AddCell(new Cell().Add(new Paragraph(dto.MaxWorkerMale.ToString())));

                factoryTable.AddCell(new Cell().Add(new Paragraph("Max Workers (Female)").SetFont(boldFont)));
                factoryTable.AddCell(new Cell().Add(new Paragraph(dto.MaxWorkerFemale.ToString())));

                factoryTable.AddCell(new Cell().Add(new Paragraph("Factory Area (Sq. Mtr)").SetFont(boldFont)));
                factoryTable.AddCell(new Cell().Add(new Paragraph(dto.AreaFactoryPremise.ToString())));

                sectionDiv.Add(factoryTable);
                document.Add(sectionDiv);

                // ================= PREMISE OWNER =================
                if (dto.NoOfFactoriesIfCommonPremise.HasValue)
                {
                    var premiseTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 2 })).UseAllAvailableWidth().SetKeepTogether(true).SetMarginBottom(5);
                    premiseTable.AddCell(new Cell().Add(new Paragraph("No. of Factories (Common Premise)").SetFont(boldFont)));
                    premiseTable.AddCell(new Cell().Add(new Paragraph(dto.NoOfFactoriesIfCommonPremise.ToString())));

                    premiseTable.AddCell(new Cell().Add(new Paragraph("Premise Owner Name").SetFont(boldFont)));
                    premiseTable.AddCell(new Cell().Add(new Paragraph(dto.PremiseOwnerName ?? "-")));

                    premiseTable.AddCell(new Cell().Add(new Paragraph("Owner Contact").SetFont(boldFont)));
                    premiseTable.AddCell(new Cell().Add(new Paragraph(dto.PremiseOwnerContactNo ?? "-")));

                    premiseTable.AddCell(new Cell().Add(new Paragraph("Owner Address").SetFont(boldFont)));
                    premiseTable.AddCell(new Cell().Add(new Paragraph(
                        $"{dto.PremiseOwnerAddressPlotNo}, {dto.PremiseOwnerAddressStreet}, {dto.PremiseOwnerAddressCity}, {dto.PremiseOwnerAddressDistrict}, {dto.PremiseOwnerAddressState} - {dto.PremiseOwnerAddressPinCode}"
                    )));
                    document.Add(premiseTable);
                }

                // ================= DYNAMIC TABLES =================
                void AddItemTable<T>(string title, List<T> items, Func<T, string> nameSelector, Func<T, string> qtySelector)
                {
                    if (!items.Any()) return;

                    document.Add(new Paragraph(title).SetFont(boldFont).SetMarginBottom(3));
                    var table = new Table(UnitValue.CreatePercentArray(new float[] { 3, 1 })).UseAllAvailableWidth().SetKeepTogether(true);

                    table.AddHeaderCell(new Cell().Add(new Paragraph(title).SetFont(boldFont)));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Quantity/Capacity").SetFont(boldFont)));

                    foreach (var item in items)
                    {
                        table.AddCell(new Cell().Add(new Paragraph(nameSelector(item))));
                        table.AddCell(new Cell().Add(new Paragraph(qtySelector(item))));
                    }

                    document.Add(table.SetMarginBottom(5));
                }

                AddItemTable("Raw Materials", dto.RawMaterials, x => x.MaterialName, x => (x.MaxStorageQuantity ?? "-").ToString());
                AddItemTable("Intermediate Products", dto.IntermediateProducts, x => x.ProductName, x => (x.MaxStorageQuantity ?? "-").ToString());
                AddItemTable("Finished Goods", dto.FinishGoods, x => x.ProductName, x => (x.MaxStorageCapacity ?? 0).ToString());
                AddItemTable("Chemicals Used", dto.Chemicals, x => x.ChemicalName, x => (x.MaxStorageQuantity ?? "-").ToString());

                // ================= DECLARATION =================
                document.Add(new Paragraph("Declaration").SetFont(boldFont).SetMarginTop(10));
                document.Add(new Paragraph($"Place: {dto.Place ?? "-"}"));
                document.Add(new Paragraph($"Status: {dto.Status}"));
                document.Add(new Paragraph("This is a system generated Factory Map Approval document.")
                    .SetFontSize(8)
                    .SetFontColor(ColorConstants.GRAY));

                document.Close();

                var approval = await _context.FactoryMapApprovals.FirstOrDefaultAsync(x => x.Id == dto.Id);
                if (approval != null)
                {
                    approval.ApplicationPDFUrl = fileUrl;
                    await _context.SaveChangesAsync();
                }

                return filePath;
            }
            catch
            {
                throw;
            }
        }

        public async Task<ApiResponseDto<FactoryMapApprovalDto>> UpdateApplicationAsync(string applicationId, CreateFactoryMapApprovalRequest request)
        {
            try
            {
                var appReg = await _context.ApplicationRegistrations
                    .OrderByDescending(x => x.CreatedDate)
                    .FirstOrDefaultAsync(x => x.ApplicationId == applicationId);
                var application = await _context.FactoryMapApprovals
                    .Include(f => f.RawMaterials)
                    .Include(f => f.IntermediateProducts)
                    .Include(f => f.FinishGoods)
                    .Include(f => f.Chemicals)
                    .FirstOrDefaultAsync(f => f.Id == applicationId);

                if (application == null)
                    return new ApiResponseDto<FactoryMapApprovalDto> { Success = false, Message = "Application not found." };

                // Update scalar fields
                application.PlantParticulars = request.PlantParticulars;
                application.ProductName = request.ProductName;
                application.ManufacturingProcess = request.ManufacturingProcess;
                application.MaxWorkerMale = request.MaxWorkerMale;
                application.MaxWorkerFemale = request.MaxWorkerFemale;
                application.AreaFactoryPremise = request.AreaFactoryPremise;
                application.NoOfFactoriesIfCommonPremise = request.NoOfFactoriesIfCommonPremise;
                application.PremiseOwnerName = request.PremiseOwnerName;
                application.PremiseOwnerContactNo = request.PremiseOwnerContactNo;
                application.PremiseOwnerAddressPlotNo = request.PremiseOwnerAddressPlotNo;
                application.PremiseOwnerAddressStreet = request.PremiseOwnerAddressStreet;
                application.PremiseOwnerAddressCity = request.PremiseOwnerAddressCity;
                application.PremiseOwnerAddressDistrict = request.PremiseOwnerAddressDistrict;
                application.PremiseOwnerAddressState = request.PremiseOwnerAddressState;
                application.PremiseOwnerAddressPinCode = request.PremiseOwnerAddressPinCode;
                application.Place = request.Place;
                application.Date = request.Date;
                application.OccupierDetails = request.OccupierDetails;
                application.FactoryDetails = request.FactoryDetails;
                application.UpdatedAt = DateTime.Now;
                application.Status = "Pending";

                // Replace child collections
                _context.Set<FactoryMapRawMaterial>().RemoveRange(application.RawMaterials);
                _context.Set<FactoryMapIntermediateProduct>().RemoveRange(application.IntermediateProducts);
                _context.Set<FactoryMapFinishGood>().RemoveRange(application.FinishGoods);
                _context.Set<FactoryMapApprovalChemical>().RemoveRange(application.Chemicals);

                application.RawMaterials = request.RawMaterials?.Select(r => new FactoryMapRawMaterial
                {
                    FactoryMapApprovalId = application.Id,
                    MaterialName = r.MaterialName,
                    MaxStorageQuantity = r.MaxStorageQuantity
                }).ToList() ?? new List<FactoryMapRawMaterial>();

                application.IntermediateProducts = request.IntermediateProducts?.Select(p => new FactoryMapIntermediateProduct
                {
                    FactoryMapApprovalId = application.Id,
                    ProductName = p.ProductName,
                    MaxStorageQuantity = p.MaxStorageQuantity
                }).ToList() ?? new List<FactoryMapIntermediateProduct>();

                application.FinishGoods = request.FinishGoods?.Select(p =>
                {
                    decimal.TryParse(p.MaxStorageCapacity, out var maxStorage);
                    return new FactoryMapFinishGood
                    {
                        FactoryMapApprovalId = application.Id,
                        ProductName = p.ProductName,
                        QuantityPerDay = p.QuantityPerDay,
                        Unit = p.Unit ?? "",
                        MaxStorageCapacity = maxStorage == 0 ? null : maxStorage,
                        StorageMethod = p.StorageMethod,
                        Remarks = p.Remarks
                    };
                }).ToList() ?? new List<FactoryMapFinishGood>();

                application.Chemicals = request.Chemicals?.Select(c => new FactoryMapApprovalChemical
                {
                    FactoryMapApprovalId = application.Id,
                    ChemicalName = c.ChemicalName,
                    TradeName = c.TradeName,
                    MaxStorageQuantity = c.MaxStorageQuantity
                }).ToList() ?? new List<FactoryMapApprovalChemical>();

                var history = new ApplicationHistory
                {
                    ApplicationId = applicationId,
                    ApplicationType = "Map Approval",
                    Action = "Application data updated",
                    Comments = "Application data updated by citizen",
                    ActionByName = "Applicant",
                    ActionBy = appReg.UserId.ToString(),
                    ActionDate = DateTime.Now
                };

                _context.ApplicationHistories.Add(history);

                await _context.SaveChangesAsync();

                string applicationTypeName = application.IsNew
                    ? ApplicationTypeNames.MapApproval
                    : ApplicationTypeNames.MapApprovalAmendment;
                // Add ApplicationApprovalRequest at Level 1 (resubmission pattern)
                var module = await _context.Set<FormModule>().FirstOrDefaultAsync(m => m.Name == applicationTypeName);


                if (module != null && appReg != null)
                {
                    int totalWorkers = application.MaxWorkerMale + application.MaxWorkerFemale;
                    var workerRange = await _context.Set<WorkerRange>()
                        .FirstOrDefaultAsync(wr => totalWorkers >= wr.MinWorkers && totalWorkers <= wr.MaxWorkers);

                    var factoryType = await _context.FactoryTypes.FirstOrDefaultAsync(x => x.Name == "Not Applicable");
                    Guid? workerRangeId = workerRange?.Id;
                    Guid? factoryTypeId = factoryType?.Id;
                    var factoryCategory = await _context.Set<FactoryCategory>()
                        .FirstOrDefaultAsync(fc => fc.WorkerRangeId == workerRangeId && fc.FactoryTypeId == factoryTypeId);
                    Guid? factoryCategoryId = factoryCategory?.Id;

                    FactoryDetailsModel factoryDetails = null;
                    if (!string.IsNullOrWhiteSpace(application.FactoryDetails))
                        factoryDetails = JsonSerializer.Deserialize<FactoryDetailsModel>(application.FactoryDetails);

                    if (factoryDetails?.subDivisionId != null && Guid.TryParse(factoryDetails.subDivisionId, out var subDivisionGuid))
                    {
                        var officeApplicationArea = await _context.Set<OfficeApplicationArea>()
                            .FirstOrDefaultAsync(oaa => oaa.CityId == subDivisionGuid);

                        if (officeApplicationArea != null)
                        {
                            var officeId = officeApplicationArea.OfficeId;
                            var workflow = await _context.Set<ApplicationWorkFlow>()
                                .FirstOrDefaultAsync(wf => wf.ModuleId == module.Id && wf.FactoryCategoryId == factoryCategoryId && wf.OfficeId == officeId);
                            var workflowLevel = await _context.Set<ApplicationWorkFlowLevel>()
                                .Where(wfl => wfl.ApplicationWorkFlowId == (workflow != null ? workflow.Id : Guid.Empty))
                                .OrderBy(wfl => wfl.LevelNumber)
                                .FirstOrDefaultAsync();

                            if (workflow != null && workflowLevel != null)
                            {
                                var approvalRequest = new ApplicationApprovalRequest
                                {
                                    ModuleId = module.Id,
                                    ApplicationRegistrationId = appReg.Id,
                                    ApplicationWorkFlowLevelId = workflowLevel.Id,
                                    Status = "Pending",
                                    CreatedDate = DateTime.Now,
                                    UpdatedDate = DateTime.Now
                                };
                                _context.Set<ApplicationApprovalRequest>().Add(approvalRequest);
                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                }

                var dto = MapToDto(application);
                return new ApiResponseDto<FactoryMapApprovalDto> { Success = true, Message = "Application updated successfully.", Data = dto };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<FactoryMapApprovalDto> { Success = false, Message = $"Error updating application: {ex.Message}" };
            }
        }
    }
}