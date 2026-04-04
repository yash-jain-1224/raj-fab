using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Event;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;
using System.Data;
using System.Text.Json;
using static RajFabAPI.Constants.AppConstants;
using ImageDataFactory = iText.IO.Image.ImageDataFactory;
using PdfCell = iText.Layout.Element.Cell;
using PdfDoc = iText.Layout.Document;
using PdfImage = iText.Layout.Element.Image;
using PdfTable = iText.Layout.Element.Table;
using Text = iText.Layout.Element.Text;

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
                     //  Date = f.Date,
                     PlantParticulars = f.PlantParticulars,
                     FactoryTypeId = f.ProductName,
                     ManufacturingProcess = f.ManufacturingProcess,
                     MaxWorkerMale = f.MaxWorkerMale,
                     MaxWorkerFemale = f.MaxWorkerFemale,
                     MaxWorkerTransgender = f.MaxWorkerTransgender,
                     AreaFactoryPremise = f.AreaFactoryPremise,
                     NoOfFactoriesIfCommonPremise = f.NoOfFactoriesIfCommonPremise,
                     PremiseOwnerDetails = f.PremiseOwnerDetails,
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
                    .Include(f => f.File)
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

                var dto = await MapToDto(application);

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
                    .Include(f => f.File)
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
                    Data = await MapToDto(application)
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
                    ProductName = request.FactoryTypeId,
                    ManufacturingProcess = request.ManufacturingProcess,
                    MaxWorkerMale = request.MaxWorkerMale,
                    MaxWorkerFemale = request.MaxWorkerFemale,
                    MaxWorkerTransgender = request.MaxWorkerTransgender,
                    AreaFactoryPremise = request.AreaFactoryPremise,
                    NoOfFactoriesIfCommonPremise = request.NoOfFactoriesIfCommonPremise,
                    PremiseOwnerDetails = request.PremiseOwnerDetails,
                    OccupierDetails = request.OccupierDetails,
                    FactoryDetails = request.FactoryDetails,
                    NoOfShifts = request.NoOfShifts,
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
                            MaxStorageQuantity = rawMaterial.MaxStorageQuantity,
                            Unit = rawMaterial.Unit
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
                            Unit = product.Unit
                        });
                    }
                }

                // Add finish goods if provided
                if (request.FinishGoods != null && request.FinishGoods.Any())
                {
                    foreach (var product in request.FinishGoods)
                    {
                        decimal res = 0;
                        Decimal.TryParse(product.MaxStorageQuantity, out res);
                        application.FinishGoods.Add(new FactoryMapFinishGood
                        {
                            FactoryMapApprovalId = application.Id,
                            ProductName = product.ProductName,
                            Unit = product.Unit ?? "",
                            MaxStorageCapacity = res == 0 ? null : res
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
                            MaxStorageQuantity = chemical.MaxStorageQuantity,
                            Unit = chemical.Unit
                        });
                    }
                }

                if (request.File != null)
                {
                    application.File = new FactoryMapApprovalFile
                    {
                        Id = Guid.NewGuid().ToString(),
                        FactoryMapApprovalId = application.Id, // important

                        LandOwnershipDocumentUrl = request.File.LandOwnershipDocumentUrl,
                        ApprovedLandPlanUrl = request.File.ApprovedLandPlanUrl,
                        ManufacturingProcessDescriptionUrl = request.File.ManufacturingProcessDescriptionUrl,
                        ProcessFlowChartUrl = request.File.ProcessFlowChartUrl,
                        RawMaterialsListUrl = request.File.RawMaterialsListUrl,
                        HazardousProcessesListUrl = request.File.HazardousProcessesListUrl,
                        EmergencyPlanUrl = request.File.EmergencyPlanUrl,
                        SafetyHealthPolicyUrl = request.File.SafetyHealthPolicyUrl,
                        FactoryPlanDrawingUrl = request.File.FactoryPlanDrawingUrl,
                        SafetyPolicyApplicableUrl = request.File.SafetyPolicyApplicableUrl,
                        OccupierPhotoIdProofUrl = request.File.OccupierPhotoIdProofUrl,
                        OccupierAddressProofUrl = request.File.OccupierAddressProofUrl,

                        CreatedAt = DateTime.Now
                    };
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
                    Data = await MapToDto(application)
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

        private async Task<FactoryMapApprovalDto> MapToDto(
            FactoryMapApproval application)
        {
            string? factoryTypeName = null;
            if (!string.IsNullOrWhiteSpace(application.ProductName) &&
                Guid.TryParse(application.ProductName, out var ftGuid))
            {
                var ft = await _context.FactoryTypes.FindAsync(ftGuid);
                factoryTypeName = ft?.Name;
            }

            var res = new FactoryMapApprovalDto
            {
                Id = application.Id,
                AcknowledgementNumber = application.AcknowledgementNumber,
                FactoryDetails = application.FactoryDetails != null ? application?.FactoryDetails : null,
                OccupierDetails = application.OccupierDetails != null ? application.OccupierDetails : null,
                PlantParticulars = application.PlantParticulars,
                FactoryTypeId = application.ProductName,
                FactoryTypeName = factoryTypeName,
                ManufacturingProcess = application.ManufacturingProcess,
                MaxWorkerMale = application.MaxWorkerMale,
                MaxWorkerFemale = application.MaxWorkerFemale,
                MaxWorkerTransgender = application.MaxWorkerTransgender,
                AreaFactoryPremise = application.AreaFactoryPremise,
                NoOfFactoriesIfCommonPremise = application.NoOfFactoriesIfCommonPremise,
                PremiseOwnerDetails = application.PremiseOwnerDetails,
                Status = application.Status,
                Version = application.Version,
                ApplicationPDFUrl = application.ApplicationPDFUrl,
                ObjectionLetterUrl = application.ObjectionLetterUrl,
                CreatedAt = application.CreatedAt,
                UpdatedAt = application.UpdatedAt,
                RawMaterials = application.RawMaterials.Select(r => new FactoryMapRawMaterialDto
                {
                    Id = r.Id,
                    MaterialName = r.MaterialName,
                    MaxStorageQuantity = r.MaxStorageQuantity,
                    Unit = r.Unit
                }).ToList(),
                IntermediateProducts = application.IntermediateProducts.Select(p => new FactoryMapIntermediateProductDto
                {
                    Id = p.Id,
                    ProductName = p.ProductName,
                    MaxStorageQuantity = p.MaxStorageQuantity,
                    Unit = p.Unit
                }).ToList(),
                FinishGoods = application.FinishGoods.Select(f => new FactoryMapFinishGoodDto
                {
                    Id = f.Id,
                    ProductName = f.ProductName,
                    Unit = f.Unit,
                    MaxStorageQuantity = f.MaxStorageCapacity?.ToString()
                }).ToList(),
                Chemicals = application.Chemicals.Select(h => new ChemicalDto
                {
                    Id = h.Id,
                    ChemicalName = h.ChemicalName,
                    TradeName = h.TradeName,
                    MaxStorageQuantity = h.MaxStorageQuantity,
                    Unit = h.Unit
                }).ToList(),

                File = application.File == null ? null : new FactoryMapApprovalFileDto
                {
                    LandOwnershipDocumentUrl = application.File.LandOwnershipDocumentUrl,
                    ApprovedLandPlanUrl = application.File.ApprovedLandPlanUrl,
                    ManufacturingProcessDescriptionUrl = application.File.ManufacturingProcessDescriptionUrl,
                    ProcessFlowChartUrl = application.File.ProcessFlowChartUrl,
                    RawMaterialsListUrl = application.File.RawMaterialsListUrl,
                    HazardousProcessesListUrl = application.File.HazardousProcessesListUrl,
                    EmergencyPlanUrl = application.File.EmergencyPlanUrl,
                    SafetyHealthPolicyUrl = application.File.SafetyHealthPolicyUrl,
                    FactoryPlanDrawingUrl = application.File.FactoryPlanDrawingUrl,
                    SafetyPolicyApplicableUrl = application.File.SafetyPolicyApplicableUrl,
                    OccupierPhotoIdProofUrl = application.File.OccupierPhotoIdProofUrl,
                    OccupierAddressProofUrl = application.File.OccupierAddressProofUrl
                },
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

            // Get user details (Approval Authority - the signer)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            var officePost = await (
                from ur in _context.UserRoles
                join r in _context.Roles on ur.RoleId equals r.Id
                join p in _context.Posts on r.PostId equals p.Id
                join o in _context.Offices on r.OfficeId equals o.Id
                join c in _context.Cities on o.CityId equals c.Id
                where ur.UserId == userId
                select new
                {
                    OfficeName = o.Name,
                    PostName = p.Name,
                    CityName = c.Name,
                    PostId = p.Id
                }
            ).FirstOrDefaultAsync();

            if (officePost == null)
            {
                throw new Exception("No office post found for this user");
            }

            var certificateUrl = await GenerateMapApprovalCertificatePdf(application, dto, officePost.PostName + ", " + officePost.CityName, user.FullName);

            var module = await _context.Set<FormModule>().FirstOrDefaultAsync(m => m.Name == ApplicationTypeNames.MapApproval);
            if (module == null)
                throw new InvalidOperationException("MapApproval module not found.");

            var certificate = new Certificate
            {
                RegistrationNumber = application.AcknowledgementNumber,
                CertificateVersion = 1.0m,
                StartDate = DateTime.TryParse(dto.StartDate, out var start) ? start : DateTime.Now,
                // EndDate = DateTime.TryParse(dto.EndDate, out var end) ? end : DateTime.Now.AddYears(1),
                CertificateUrl = certificateUrl,
                IssuedByUserId = userId,
                IssuedAt = DateTime.TryParse(dto.IssuedAt, out var issuedAt) ? issuedAt : DateTime.Now,
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

        private async Task<string> GenerateMapApprovalCertificatePdf(FactoryMapApproval application, MapApprovalCertificateRequestDto dto, string postName, string userName)
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

            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            var occupier = string.IsNullOrWhiteSpace(application.OccupierDetails)
                ? null
                : JsonSerializer.Deserialize<OccupierDetailsModel>(application.OccupierDetails);

            var factory = string.IsNullOrWhiteSpace(application.FactoryDetails)
                ? null
                : JsonSerializer.Deserialize<FactoryDetailsModel>(application.FactoryDetails);

            int totalWorkers = application.MaxWorkerMale + application.MaxWorkerFemale + application.MaxWorkerTransgender;
            string? factoryTypeName = null;
            if (!string.IsNullOrWhiteSpace(application.ProductName) &&
                Guid.TryParse(application.ProductName, out var ftGuid))
            {
                var ft = await _context.FactoryTypes.FindAsync(ftGuid); // ✅ await is OUTSIDE iText scope
                factoryTypeName = ft?.Name;
            }

            DateOnly footerDate = DateOnly.FromDateTime(DateTime.Today);

            using (var writer = new PdfWriter(filePath))
            using (var pdf = new PdfDocument(writer))
            using (var document = new Document(pdf))
            {
                pdf.AddEventHandler(PdfDocumentEvent.END_PAGE,
                    new PageBorderAndFooterEventHandler(
                        boldFont, regularFont,
                        footerDate, "wwwroot/chief_signature.jpg", postName, userName));

                document.SetMargins(40, 40, 130, 40);

                // ─── HEADER ──────────────────────────────────────────────────────────
                var headerTable = new Table(new float[] { 90f, 320f, 90f })
                    .UseAllAvailableWidth()
                    .SetBorder(Border.NO_BORDER)
                    .SetMarginBottom(6f);

                headerTable.AddCell(new Cell().SetBorder(Border.NO_BORDER));

                var centerCell = new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetTextAlignment(TextAlignment.CENTER);
                centerCell.Add(new Paragraph("Government of Rajasthan")
                    .SetFont(boldFont).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(1f));
                centerCell.Add(new Paragraph("Factories and Boilers Inspection Department")
                    .SetFont(boldFont).SetFontSize(11).SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(1f));
                centerCell.Add(new Paragraph("6-C, Jhalana Institutional Area, Jaipur, 302004")
                    .SetFont(regularFont).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(4f));
                headerTable.AddCell(centerCell);

                headerTable.AddCell(new Cell().SetBorder(Border.NO_BORDER));
                document.Add(headerTable);

                // ─── Application Id + Date ────────────────────────────────────────────
                var topRow = new Table(new float[] { 1f, 1f })
                    .UseAllAvailableWidth()
                    .SetBorder(Border.NO_BORDER)
                    .SetMarginBottom(2f);
                topRow.AddCell(new Cell()
                    .Add(new Paragraph($"Plan Application No.:-  P-{application.AcknowledgementNumber}")
                        .SetFont(boldFont).SetFontSize(10))
                    .SetBorder(Border.NO_BORDER));
                topRow.AddCell(new Cell()
                    .Add(new Paragraph($"Dated:-  {(application.Date.HasValue ? application.Date.Value.ToString("dd/MM/yyyy") : DateTime.Now.ToString("dd/MM/yyyy"))}")
                        .SetFont(boldFont).SetFontSize(10).SetTextAlignment(TextAlignment.RIGHT))
                    .SetBorder(Border.NO_BORDER));
                document.Add(topRow);

                document.Add(new Paragraph($"Plan No.:-  P-{application.AcknowledgementNumber}")
                    .SetFont(boldFont).SetFontSize(10).SetMarginBottom(6f));

                // ─── Factory name + address ───────────────────────────────────────────
                if (factory != null)
                {
                    document.Add(new Paragraph(factory.name ?? "-")
                        .SetFont(boldFont).SetFontSize(11).SetMarginBottom(1f));

                    var line1 = string.Join(", ", new[] { factory.addressLine1, factory.addressLine2 }
                        .Where(s => !string.IsNullOrWhiteSpace(s)));
                    var line2 = string.Join(", ", new[] { factory.area, factory.tehsilName, factory.subDivisionName }
                        .Where(s => !string.IsNullOrWhiteSpace(s)));
                    var line3 = string.Join(", ", new[] { factory.districtName, factory.pincode }
                        .Where(s => !string.IsNullOrWhiteSpace(s)));

                    var fullAddress = string.Join("\n", new[] { line1, line2, line3 }
                        .Where(s => !string.IsNullOrWhiteSpace(s)));

                    document.Add(new Paragraph(fullAddress)
                        .SetFont(regularFont).SetFontSize(11).SetMarginBottom(8f));
                }

                // ─── Sub heading ──────────────────────────────────────────────────────
                document.Add(new Paragraph("Sub:-  Approval of Factory Building drawings")
                    .SetFont(boldFont).SetFontSize(11).SetMarginBottom(2f));
                document.Add(new Paragraph("The details of your factory as per application, drawings and documents are shown below:-")
                    .SetFont(regularFont).SetFontSize(11).SetMarginBottom(6f));

                // ─── Details table ────────────────────────────────────────────────────
                var blackBorder = new SolidBorder(new DeviceRgb(0, 0, 0), 0.75f);

                Cell BlackCell(string text, PdfFont font, float size = 10f)
                    => new Cell()
                        .Add(new Paragraph(text ?? "-").SetFont(font).SetFontSize(size))
                        .SetBorderTop(blackBorder).SetBorderBottom(blackBorder)
                        .SetBorderLeft(blackBorder).SetBorderRight(blackBorder)
                        .SetPadding(5f);

                var detailsTable = new Table(new float[] { 150f, 350f })
                    .UseAllAvailableWidth()
                    .SetMarginBottom(10f);

                detailsTable.AddCell(BlackCell("Manufacturing Process", boldFont));
                detailsTable.AddCell(BlackCell(application.ManufacturingProcess ?? "-", regularFont));
                detailsTable.AddCell(BlackCell("Type", boldFont));
                detailsTable.AddCell(BlackCell("-", regularFont));
                detailsTable.AddCell(BlackCell("Category", boldFont));
                detailsTable.AddCell(BlackCell(factoryTypeName ?? "-", regularFont));
                detailsTable.AddCell(BlackCell("Workers", boldFont));
                detailsTable.AddCell(BlackCell(totalWorkers.ToString(), regularFont));

                document.Add(detailsTable);

                document.Add(new Paragraph(
                        "Drawings of your factory are approved under Section 119 of The Occupational Safety Health and Working Conditions Code, 2020 with the following conditions:-")
                    .SetFont(regularFont).SetFontSize(11).SetMarginBottom(4f));

                var conditions = new[]
                {
                    "Certificate of Stability obtained from a competent person shall be submitted as per OSH Code 2020 and the rules, regulations made there under.",
                    "Disposal of the trade waste effluents shall be as per OSH Code 2020 and the rules, regulations made there under.",
                    "Disposal of the waste from latrines & urinals shall be as per OSH Code 2020 and the rules, regulations made there under.",
                    "Drinking water facilities should be provided as per OSH Code 2020 and the rules, regulations made there under.",
                    "Fire fighting arrangement should be provided as per OSH Code 2020 and the rules, regulations made there under.",
                };

                for (int i = 0; i < conditions.Length; i++)
                    document.Add(new Paragraph($"{i + 1}. {conditions[i]}")
                        .SetFont(regularFont).SetFontSize(11).SetMarginBottom(0f));

                document.Add(new Paragraph()
                    .Add(new Text("6. Drawings are approved for   ").SetFont(regularFont).SetFontSize(11))
                    .Add(new Text($"{application.MaxWorkerMale}").SetFont(boldFont).SetFontSize(11))
                    .Add(new Text("   male,   ").SetFont(regularFont).SetFontSize(11))
                    .Add(new Text($"{application.MaxWorkerFemale}").SetFont(boldFont).SetFontSize(11))
                    .Add(new Text("   female,   ").SetFont(regularFont).SetFontSize(11))
                    .Add(new Text($"{application.MaxWorkerTransgender}").SetFont(boldFont).SetFontSize(11))
                    .Add(new Text("   transgender (Total -   ").SetFont(regularFont).SetFontSize(11))
                    .Add(new Text($"{totalWorkers}").SetFont(boldFont).SetFontSize(11))
                    .Add(new Text(") workers only.").SetFont(regularFont).SetFontSize(11))
                    .SetMarginBottom(20f));

                // ─── Footer disclaimer (fixed position) ───────────────────────────────
                var pageWidth = pdf.GetDefaultPageSize().GetWidth();
                document.Add(new Paragraph(
                        "This is a computer generated certificate and bears scanned signature. No physical signature is required on this approval. You " +
                        "can verify this approval by visiting www.rajfab.rajasthan.gov.in and entering Application No./ID after clicking the link for " +
                        "verification on the page.")
                    .SetFont(regularFont).SetFontSize(6.5f)
                    .SetFontColor(ColorConstants.GRAY)
                    .SetTextAlignment(TextAlignment.JUSTIFIED)
                    .SetMultipliedLeading(1.1f)
                    .SetFixedPosition(35, 8, pageWidth - 70));

            }

            return fileUrl;
        }
        private static byte[] GenerateMapApprovalQrPng(string url)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrData);
            return qrCode.GetGraphic(5);
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
                document.SetMargins(40, 40, 130, 40);

                var boldFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
                var regularFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
                var italicFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_OBLIQUE);

                var footerDate = DateTime.Now.ToString("dd/MM/yyyy");
                pdf.AddEventHandler(PdfDocumentEvent.END_PAGE,
                    new MapApprovalPageBorderAndFooterEventHandler(boldFont, regularFont, footerDate));

                var occupier = string.IsNullOrWhiteSpace(dto.OccupierDetails)
                    ? null
                    : JsonSerializer.Deserialize<OccupierDetailsModel>(dto.OccupierDetails);

                var factory = string.IsNullOrWhiteSpace(dto.FactoryDetails)
                    ? null
                    : JsonSerializer.Deserialize<FactoryDetailsModel>(dto.FactoryDetails);

                var premiseOwner = string.IsNullOrWhiteSpace(dto.PremiseOwnerDetails)
                    ? null
                    : JsonSerializer.Deserialize<OccupierDetailsModel>(dto.PremiseOwnerDetails);

                // ── helpers ──────────────────────────────────────────────────────────
                // 2-column borderless row: label | value
                void AddRow(Table t, string label, string value)
                {
                    t.AddCell(new Cell().Add(new Paragraph(label)
                            .SetFont(boldFont).SetFontSize(9))
                        .SetBorder(Border.NO_BORDER).SetPaddingLeft(4));
                    t.AddCell(new Cell().Add(new Paragraph(value ?? "-")
                            .SetFont(regularFont).SetFontSize(9))
                        .SetBorder(Border.NO_BORDER));
                }
                // 3-column sub-item: indent | (x) label | value
                void AddSubRow(Table t, string code, string label, string value)
                {
                    t.AddCell(new Cell().Add(new Paragraph("")).SetBorder(Border.NO_BORDER).SetWidth(20));
                    t.AddCell(new Cell().Add(new Paragraph($"{code} {label}")
                            .SetFont(regularFont).SetFontSize(9))
                        .SetBorder(Border.NO_BORDER));
                    t.AddCell(new Cell().Add(new Paragraph(value ?? "-")
                            .SetFont(regularFont).SetFontSize(9))
                        .SetBorder(Border.NO_BORDER));
                }
                Table TwoCol() => new Table(new float[] { 200f, 320f }).UseAllAvailableWidth().SetBorder(Border.NO_BORDER);
                Table ThreeCol() => new Table(new float[] { 20f, 180f, 320f }).UseAllAvailableWidth().SetBorder(Border.NO_BORDER);

                // ── HEADER ───────────────────────────────────────────────────────────
                document.Add(new Paragraph("Form-6")
                    .SetFont(boldFont).SetFontSize(13)
                .SetTextAlignment(TextAlignment.CENTER));
                document.Add(new Paragraph("(See rule 8(2) & (4) & rule 106)")
                    .SetFont(regularFont).SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER));
                document.Add(new Paragraph("Submission and approval of plans")
                    .SetFont(boldFont).SetFontSize(10).SetTextAlignment(TextAlignment.CENTER));
                document.Add(new Paragraph(
                        "Application for permission for the site on which the factory is to be situated and for the \n construction or extension thereof")
                    .SetFont(boldFont).SetFontSize(10)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(8));

                // Acknowledgement No + Date (right-aligned pair)
                var ackTable = new Table(new float[] { 1f, 1f }).UseAllAvailableWidth().SetBorder(Border.NO_BORDER).SetMarginBottom(8);
                ackTable.AddCell(new Cell().Add(new Paragraph($"Plan Application No: P-{dto.AcknowledgementNumber}")
                        .SetFont(boldFont).SetFontSize(9)).SetBorder(Border.NO_BORDER));
                ackTable.AddCell(new Cell().Add(new Paragraph($"Date: {dto.CreatedAt.ToString("dd/MM/yyyy")}")
                        .SetFont(boldFont).SetFontSize(9).SetTextAlignment(TextAlignment.RIGHT)).SetBorder(Border.NO_BORDER));
                document.Add(ackTable);

                // ── SECTION 1: Details of Occupier ───────────────────────────────────
                document.Add(new Paragraph("1.   Details of Occupier")
                    .SetFont(boldFont).SetFontSize(10).SetMarginBottom(3));

                string relationLabel = string.IsNullOrWhiteSpace(occupier?.relationType)
                     ? "Relative's Name:"
                     : $"{(occupier.relationType == "father" ? "Father" : "Husband")}'s Name:";

                string relationValue = string.IsNullOrWhiteSpace(occupier?.relativeName)
                    ? "-"
                    : occupier.relativeName;

                var occTable = ThreeCol();
                AddSubRow(occTable, "(a)", "Name:", occupier?.name ?? "-");
                AddSubRow(occTable, "(b)", relationLabel, relationValue);
                AddSubRow(occTable, "(c)", "Address :",
                    string.Join(", ", new[] { occupier?.addressLine1, occupier?.addressLine2, occupier?.area, occupier?.tehsil, occupier?.district, occupier?.pincode }
                        .Where(s => !string.IsNullOrWhiteSpace(s))));
                // AddSubRow(occTable, "(d)", "Address (residential):", "-");
                AddSubRow(occTable, "(d)", "Mobile Number:", occupier?.mobile ?? "-");
                AddSubRow(occTable, "(e)", "Email:", occupier?.email ?? "-");
                document.Add(occTable.SetMarginBottom(6));

                // ── SECTION 2: Details of factory ────────────────────────────────────
                document.Add(new Paragraph("2.   Details of Factory")
                    .SetFont(boldFont).SetFontSize(10).SetMarginBottom(3));

                var facTable = ThreeCol();
                AddSubRow(facTable, "(a)", "Name of Factory:", factory?.name ?? "-");
                AddSubRow(facTable, "(b)", "Situation of Factory:", factory?.situation ?? "-");
                AddSubRow(facTable, "(c)", "Address with Pincode:",
                    string.Join(", ", new[] { factory?.addressLine1, factory?.addressLine2, factory?.area, factory?.pincode }
                        .Where(s => !string.IsNullOrWhiteSpace(s))));
                AddSubRow(facTable, "(d)", "District:", factory?.districtName ?? "-");
                AddSubRow(facTable, "(e)", "Contact Number:", factory?.mobile ?? factory?.telephone ?? "-");
                AddSubRow(facTable, "(f)", "Email:", factory?.email ?? "-");
                AddSubRow(facTable, "(g)", "Website:", factory?.website ?? "-");
                document.Add(facTable.SetMarginBottom(6));

                var premTT = TwoCol();
                AddRow(premTT, "3.   Particulars of plant to be installed:",
                    dto.PlantParticulars ?? "-");
                AddRow(premTT, "4.   Name of Manufacturing process:",
                    dto.ManufacturingProcess ?? "-");
                document.Add(premTT.SetMarginBottom(4));

                int male = dto?.MaxWorkerMale ?? 0;
                int female = dto?.MaxWorkerFemale ?? 0;
                int transgender = dto?.MaxWorkerTransgender ?? 0;
                int total = male + female + transgender;

                var wrkT = new Table(new float[] { 240f, 60f, 60f, 80f, 60f })
                    .UseAllAvailableWidth()
                    .SetBorder(Border.NO_BORDER);

                // ── Header Row ──
                wrkT.AddCell(new Cell()
                    .Add(new Paragraph("5.   Maximum number of Workers\n(Proposed to employ)")
                        .SetFont(boldFont).SetFontSize(10))
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetBorder(Border.NO_BORDER));

                wrkT.AddCell(new Cell()
                    .Add(new Paragraph("Male")
                        .SetFont(boldFont).SetFontSize(9)
                        .SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER)
                    .SetVerticalAlignment(VerticalAlignment.BOTTOM));

                wrkT.AddCell(new Cell()
                    .Add(new Paragraph("Female")
                        .SetFont(boldFont).SetFontSize(9)
                        .SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER)
                    .SetVerticalAlignment(VerticalAlignment.BOTTOM));

                wrkT.AddCell(new Cell()
                    .Add(new Paragraph("Transgender")
                        .SetFont(boldFont).SetFontSize(9)
                        .SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER)
                    .SetVerticalAlignment(VerticalAlignment.BOTTOM));

                wrkT.AddCell(new Cell()
                    .Add(new Paragraph("Total")
                        .SetFont(boldFont).SetFontSize(9)
                        .SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER)
                    .SetVerticalAlignment(VerticalAlignment.BOTTOM));

                // ── Value Row ──
                wrkT.AddCell(new Cell()
                    .SetBorder(Border.NO_BORDER)); // empty label cell

                wrkT.AddCell(new Cell()
                    .Add(new Paragraph($"{male}")
                        .SetFont(regularFont).SetFontSize(9)
                        .SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER));

                wrkT.AddCell(new Cell()
                    .Add(new Paragraph($"{female}")
                        .SetFont(regularFont).SetFontSize(9)
                        .SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER));

                wrkT.AddCell(new Cell()
                    .Add(new Paragraph($"{transgender}")
                        .SetFont(regularFont).SetFontSize(9)
                        .SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER));

                wrkT.AddCell(new Cell()
                    .Add(new Paragraph($"{total}")
                        .SetFont(boldFont).SetFontSize(9)
                        .SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER));

                document.Add(wrkT.SetMarginBottom(6));

                var premTTT = TwoCol();
                AddRow(premTTT, "6.   Number of shifts:", dto?.NoOfShifts.ToString() ?? "-");
                document.Add(premTTT.SetMarginBottom(4));

                // ── SECTION 7 ───────────────────────────────────────────────────────
                document.Add(new Paragraph("7.   Details of Products and Materials")
                    .SetFont(boldFont).SetFontSize(9));

                // Helper: build a 4-column S.No. | Name | Max Storage Qty | Unit table
                Table BuildMaterialsTable<T>(IEnumerable<T>? items,
                    Func<T, string?> name, Func<T, string?> qty, Func<T, string?> unit)
                {
                    var t = new Table(new float[] { 40f, 200f, 130f, 90f })
                        .UseAllAvailableWidth().SetMarginBottom(6);
                    foreach (var h in new[] { "S. No.", "Name", "Max Storage Qty", "Unit" })
                        t.AddHeaderCell(new Cell()
                            .Add(new Paragraph(h).SetFont(boldFont).SetFontSize(8))
                            .SetBackgroundColor(new DeviceRgb(240, 240, 240))
                            .SetTextAlignment(TextAlignment.CENTER).SetPadding(4));

                    if (items != null && items.Any())
                    {
                        int sno = 1;
                        foreach (var item in items)
                        {
                            t.AddCell(new Cell().Add(new Paragraph(sno++.ToString()).SetFont(regularFont).SetFontSize(9))
                                .SetTextAlignment(TextAlignment.CENTER).SetPadding(4));
                            t.AddCell(new Cell().Add(new Paragraph(name(item) ?? "-").SetFont(regularFont).SetFontSize(9)).SetPadding(4));
                            t.AddCell(new Cell().Add(new Paragraph(qty(item) ?? "-").SetFont(regularFont).SetFontSize(9))
                                .SetTextAlignment(TextAlignment.CENTER).SetPadding(4));
                            t.AddCell(new Cell().Add(new Paragraph(unit(item) ?? "-").SetFont(regularFont).SetFontSize(9))
                                .SetTextAlignment(TextAlignment.CENTER).SetPadding(4));
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                            t.AddCell(new Cell().Add(new Paragraph("-").SetFont(regularFont).SetFontSize(9))
                                .SetMinHeight(18).SetPadding(4));
                    }
                    return t;
                }

                // ── (a) Raw Materials ──
                document.Add(new Paragraph("     (a) Raw Materials").SetFont(boldFont).SetFontSize(9).SetMarginBottom(2));
                document.Add(BuildMaterialsTable(dto.RawMaterials,
                    r => r.MaterialName, r => r.MaxStorageQuantity, r => r.Unit));

                // ── (b) Intermediate Products ──
                document.Add(new Paragraph("     (b) Intermediate Products / By-products").SetFont(boldFont).SetFontSize(9).SetMarginBottom(2));
                document.Add(BuildMaterialsTable(dto.IntermediateProducts,
                    p => p.ProductName, p => p.MaxStorageQuantity, p => p.Unit));

                // ── (c) Finished Products ──
                document.Add(new Paragraph("     (c) Finished Products").SetFont(boldFont).SetFontSize(9).SetMarginBottom(2));
                document.Add(BuildMaterialsTable(dto.FinishGoods,
                    f => f.ProductName, f => f.MaxStorageQuantity, f => f.Unit));

                document.Add(new Paragraph("\n").SetFontSize(4));

                // ── SECTION 8: Chemicals table ───────────────────────────────────────
                document.Add(new Paragraph("8.   Name of Chemicals for use in the manufacturing process, if any")
                    .SetFont(boldFont).SetFontSize(10).SetMarginBottom(4));

                var chemT = new Table(new float[] { 40f, 120f, 140f, 130f, 90f }).UseAllAvailableWidth().SetMarginBottom(8);
                foreach (var h in new[] { "S. No.", "Trade Name", "Chemical Name", "Max Storage Qty", "Unit" })
                    chemT.AddHeaderCell(new Cell()
                        .Add(new Paragraph(h).SetFont(boldFont).SetFontSize(8))
                        .SetBackgroundColor(new DeviceRgb(240, 240, 240))
                        .SetTextAlignment(TextAlignment.CENTER).SetPadding(4));

                if (dto.Chemicals?.Count > 0)
                {
                    int sno = 1;
                    foreach (var chem in dto.Chemicals)
                    {
                        chemT.AddCell(new Cell().Add(new Paragraph(sno++.ToString()).SetFont(regularFont).SetFontSize(9))
                            .SetTextAlignment(TextAlignment.CENTER).SetPadding(4));
                        chemT.AddCell(new Cell().Add(new Paragraph(chem.TradeName ?? "-").SetFont(regularFont).SetFontSize(9)).SetPadding(4));
                        chemT.AddCell(new Cell().Add(new Paragraph(chem.ChemicalName ?? "-").SetFont(regularFont).SetFontSize(9)).SetPadding(4));
                        chemT.AddCell(new Cell().Add(new Paragraph(chem.MaxStorageQuantity ?? "-").SetFont(regularFont).SetFontSize(9))
                            .SetTextAlignment(TextAlignment.CENTER).SetPadding(4));
                        chemT.AddCell(new Cell().Add(new Paragraph(chem.Unit ?? "-").SetFont(regularFont).SetFontSize(9))
                            .SetTextAlignment(TextAlignment.CENTER).SetPadding(4));
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                        chemT.AddCell(new Cell().Add(new Paragraph("-").SetFontSize(9)).SetMinHeight(18).SetPadding(4));
                }
                document.Add(chemT);

                // ── SECTIONS 9-10: Premises ───────────────────────────────────────────
                var premT = TwoCol();
                AddRow(premT, "9.   Area of the factory premises:",
                    dto.AreaFactoryPremise > 0 ? $"{dto.AreaFactoryPremise.ToString()} (In Sq.M)" : "-");
                AddRow(premT, "10.   No. of factories if common premises:",
                    dto.NoOfFactoriesIfCommonPremise.HasValue ? dto.NoOfFactoriesIfCommonPremise.Value.ToString() : "-");
                document.Add(premT.SetMarginBottom(4));

                // ── SECTION 11: Premise Owner ─────────────────────────────────────────
                document.Add(new Paragraph("11.   Details of Premise Owner")
                    .SetFont(boldFont).SetFontSize(10).SetMarginBottom(3));

                var occTable1 = ThreeCol();
                AddSubRow(occTable1, "(a)", "Name:", premiseOwner?.name ?? "-");
                AddSubRow(occTable1, "(c)", "Address :",
                    string.Join(", ", new[] { premiseOwner?.addressLine1, premiseOwner?.addressLine2, premiseOwner?.area, premiseOwner?.tehsil, premiseOwner?.district, premiseOwner?.pincode }
                        .Where(s => !string.IsNullOrWhiteSpace(s))));
                // AddSubRow(occTable, "(d)", "Address (residential):", "-");
                AddSubRow(occTable1, "(d)", "Mobile Number:", premiseOwner?.mobile ?? "-");
                AddSubRow(occTable1, "(e)", "Email:", premiseOwner?.email ?? "-");
                document.Add(occTable1.SetMarginBottom(6));

                // ── SECTION 12: NOTE ─────────────────────────────────────────────────
                document.Add(new Paragraph("12.  NOTE").SetFont(boldFont).SetFontSize(10).SetMarginBottom(3));
                document.Add(new Paragraph("     a.  In case of any change in the above information, Department shall be informed in writing within 30 days.")
                    .SetFont(regularFont).SetFontSize(9).SetMarginBottom(2));
                document.Add(new Paragraph("     b.  Seal bearing \"Authorised Signatory\" shall not be used on any document.")
                    .SetFont(regularFont).SetFontSize(9).SetMarginBottom(12));

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
        private Table BuildSectionTable(string[] headers, float[] widths, string[]? values, PdfFont boldFont, PdfFont regularFont)
        {
            var table = new Table(widths)
                .UseAllAvailableWidth()
                .SetMarginLeft(24);

            // Header
            foreach (var h in headers)
                table.AddCell(BuildHeaderCell(h, boldFont));

            // Number row
            for (int i = 1; i <= headers.Length; i++)
                table.AddCell(BuildCenterCell(i.ToString(), regularFont));

            // Data row — "-----" placeholder when section is not applicable
            var dataValues = values ?? Enumerable.Repeat("-----", headers.Length).ToArray();
            foreach (var val in dataValues)
                table.AddCell(BuildDataCell(string.IsNullOrWhiteSpace(val) ? "-" : val, regularFont).SetTextAlignment(TextAlignment.CENTER));

            return table;
        }
        /// <summary>Builds a centered cell (used for column-index row).</summary>
        private static PdfCell BuildCenterCell(string text, PdfFont font)
        {
            return new PdfCell()
                .Add(new Paragraph(text).SetFont(font).SetFontSize(8))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(3);
        }
        /// <summary>Builds a bold, centered, bordered header cell with small font.</summary>
        private static PdfCell BuildHeaderCell(string text, PdfFont boldFont)
        {
            return new PdfCell()
                .Add(new Paragraph(text).SetFont(boldFont).SetFontSize(8))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBackgroundColor(new DeviceRgb(240, 240, 240))
                .SetPadding(4);
        }
        private static PdfCell BuildDataCell(string text, PdfFont font)
        {
            return new PdfCell()
                .Add(new Paragraph(text ?? "-").SetFont(font).SetFontSize(9))
                .SetPadding(4);
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
                application.ProductName = request.FactoryTypeId;
                application.ManufacturingProcess = request.ManufacturingProcess;
                application.MaxWorkerMale = request.MaxWorkerMale;
                application.MaxWorkerFemale = request.MaxWorkerFemale;
                application.MaxWorkerTransgender = request.MaxWorkerTransgender;
                application.AreaFactoryPremise = request.AreaFactoryPremise;
                application.NoOfFactoriesIfCommonPremise = request.NoOfFactoriesIfCommonPremise;
                application.PremiseOwnerDetails = request.PremiseOwnerDetails;
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
                    MaxStorageQuantity = r.MaxStorageQuantity,
                    Unit = r.Unit
                }).ToList() ?? new List<FactoryMapRawMaterial>();

                application.IntermediateProducts = request.IntermediateProducts?.Select(p => new FactoryMapIntermediateProduct
                {
                    FactoryMapApprovalId = application.Id,
                    ProductName = p.ProductName,
                    MaxStorageQuantity = p.MaxStorageQuantity,
                    Unit = p.Unit
                }).ToList() ?? new List<FactoryMapIntermediateProduct>();

                application.FinishGoods = request.FinishGoods?.Select(p =>
                {
                    decimal.TryParse(p.MaxStorageQuantity, out var maxStorage);
                    return new FactoryMapFinishGood
                    {
                        FactoryMapApprovalId = application.Id,
                        ProductName = p.ProductName,
                        Unit = p.Unit ?? "",
                        MaxStorageCapacity = maxStorage == 0 ? null : maxStorage
                    };
                }).ToList() ?? new List<FactoryMapFinishGood>();

                application.Chemicals = request.Chemicals?.Select(c => new FactoryMapApprovalChemical
                {
                    FactoryMapApprovalId = application.Id,
                    ChemicalName = c.ChemicalName,
                    TradeName = c.TradeName,
                    MaxStorageQuantity = c.MaxStorageQuantity,
                    Unit = c.Unit
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
                    int totalWorkers = application.MaxWorkerMale + application.MaxWorkerFemale + application.MaxWorkerTransgender;
                    var workerRange = await _context.Set<WorkerRange>()
                        .FirstOrDefaultAsync(wr => totalWorkers >= wr.MinWorkers && totalWorkers <= wr.MaxWorkers);

                    var factoryType = await _context.FactoryTypes.FirstOrDefaultAsync(x => x.Name == "Not Applicable");
                    Guid? workerRangeId = workerRange?.Id;
                    Guid? factoryTypeId = Guid.TryParse(request.FactoryTypeId, out var factoryTypeGuid)
                     ? factoryTypeGuid
                     : (Guid?)null;
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

                var dto = await MapToDto(application);
                return new ApiResponseDto<FactoryMapApprovalDto> { Success = true, Message = "Application updated successfully.", Data = dto };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<FactoryMapApprovalDto> { Success = false, Message = $"Error updating application: {ex.Message}" };
            }
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // Generate Objection Letter — Map Approval
        // ─────────────────────────────────────────────────────────────────────────────
        public async Task<string> GenerateObjectionLetter(MapApprovalObjectionLetterDto dto, string applicationId)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var fileName = $"objection_map_{applicationId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var webRootPath = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
                throw new InvalidOperationException("wwwroot is not configured.");

            var uploadPath = Path.Combine(webRootPath, "objection-letters");
            _ = Directory.CreateDirectory(uploadPath);
            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/objection-letters/{fileName}";

            // ── Fonts ─────────────────────────────────────────────────────────────────
            var boldFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

            // ── PDF setup ─────────────────────────────────────────────────────────────
            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new PageBorderEventHandler());
            using var document = new PdfDoc(pdf);
            document.SetMargins(50, 50, 65, 50);

            var factory = string.IsNullOrWhiteSpace(dto.FactoryDetails)
                   ? null
                   : JsonSerializer.Deserialize<FactoryDetailsModel>(dto.FactoryDetails);

            var rawLoad = factory?.sanctionedLoad ?? 0;
            var loadUnit = (factory?.sanctionedLoadUnit ?? "HP").ToUpper();

            // Normalize any unit to KW first, then convert to target
            decimal ToKW(decimal val, string unit) => unit switch
            {
                "HP" => val * 0.746m,
                "KW" => val,
                "KVA" => val,          // assuming power factor = 1
                "MW" => val * 1000m,
                "MVA" => val * 1000m,
                _ => val
            };
            decimal ConvertToHP(decimal val, string unit) => ToKW(val, unit) / 0.746m;

            var Type = "-";
            var power = ConvertToHP(rawLoad, loadUnit); // in H.P. This is a placeholder. The actual power value should come from the application data (e.g., dto.PowerInHP).

            if (dto.MaxWorkers < 20)
            {
                Type = "Section 85";
            }
            else if (dto.MaxWorkers > 40 && power == 0)
            {
                Type = "2 (1)(w)(ii)";
            }
            else if (dto.MaxWorkers >= 20 && power > 0)
            {
                Type = "2 (1)(w)(i)";
            }

            // ═════════════════════════════════════════════════════════════════════════
            // HEADER
            // ═════════════════════════════════════════════════════════════════════════
            _ = document.Add(new Paragraph("Government of Rajasthan")
                .SetFont(boldFont).SetFontSize(14)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(1f));

            _ = document.Add(new Paragraph("Factories and Boilers Inspection Department")
                .SetFont(boldFont).SetFontSize(13)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(1f));

            _ = document.Add(new Paragraph("6-C, Jhalana Institutional Area, Jaipur, 302004")
                .SetFont(regularFont).SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10f));

            // ═════════════════════════════════════════════════════════════════════════
            // Application Id  +  Dated
            // ═════════════════════════════════════════════════════════════════════════
            var topRow = new PdfTable(new float[] { 1f, 1f })
                .UseAllAvailableWidth().SetBorder(Border.NO_BORDER).SetMarginBottom(12f);

            _ = topRow.AddCell(new PdfCell()
                .Add(new Paragraph($"Plan Application No.:-  P-{dto.ApplicationId ?? "-"}")
                    .SetFont(boldFont).SetFontSize(12))
                .SetBorder(Border.NO_BORDER));

            _ = topRow.AddCell(new PdfCell()
                .Add(new Paragraph($"Dated:-  {dto.Date:dd/MM/yyyy}")
                    .SetFont(boldFont).SetFontSize(12)
                    .SetTextAlignment(TextAlignment.RIGHT))
                .SetBorder(Border.NO_BORDER));

            _ = document.Add(topRow);

            // ═════════════════════════════════════════════════════════════════════════
            // Factory name + address
            // ═════════════════════════════════════════════════════════════════════════
            _ = document.Add(new Paragraph(factory.name ?? "-")
                .SetFont(regularFont).SetFontSize(12)
                .SetMarginBottom(1f));
            _ = document.Add(new Paragraph($"{factory.addressLine1}, {factory.addressLine2},\n{factory.area}, {factory?.tehsilName},\n{factory?.subDivisionName}, {factory.districtName} - {factory.pincode}")
                .SetFont(regularFont).SetFontSize(12));

            // ═════════════════════════════════════════════════════════════════════════
            // Sub:-
            // ═════════════════════════════════════════════════════════════════════════
            var subPara = new Paragraph();
            subPara.Add(new Text("Sub:- ").SetFont(boldFont).SetFontSize(12));
            subPara.Add(new Text("Regarding approval of your Maps").SetFont(regularFont).SetFontSize(12));
            _ = document.Add(subPara.SetMarginBottom(4f));

            // ═════════════════════════════════════════════════════════════════════════
            // Intro line
            // ═════════════════════════════════════════════════════════════════════════
            _ = document.Add(new Paragraph(
                    "The details of your factory as per application, drawings and documents are shown below:-")
                .SetFont(regularFont).SetFontSize(12)
                .SetMarginBottom(4f));

            // ═════════════════════════════════════════════════════════════════════════
            // Factory details table (red border)
            // ═════════════════════════════════════════════════════════════════════════
            var detailsTable = new PdfTable(new float[] { 150f, 1f })
                .UseAllAvailableWidth().SetMarginBottom(12f);

            PdfCell RedCell(string text, PdfFont font, float fontSize = 12f)
            {
                var border = new iText.Layout.Borders.SolidBorder(new DeviceRgb(220, 0, 0), 1.5f);
                return new PdfCell()
                    .Add(new Paragraph(text ?? "-").SetFont(font).SetFontSize(fontSize))
                    .SetBorderTop(border).SetBorderBottom(border)
                    .SetBorderLeft(border).SetBorderRight(border)
                    .SetPadding(5f);
            }

            _ = detailsTable.AddCell(RedCell("Manufacturing Process", boldFont));
            _ = detailsTable.AddCell(RedCell(dto.ManufacturingProcess ?? "-", regularFont));

            _ = detailsTable.AddCell(RedCell("Type", boldFont));
            _ = detailsTable.AddCell(RedCell(Type, regularFont));

            _ = detailsTable.AddCell(RedCell("Category", boldFont));
            _ = detailsTable.AddCell(RedCell(dto.ProductName ?? "-", regularFont));

            _ = detailsTable.AddCell(RedCell("Workers", boldFont));
            _ = detailsTable.AddCell(RedCell(dto.MaxWorkers?.ToString() ?? "-", regularFont));

            _ = document.Add(detailsTable);

            // ═════════════════════════════════════════════════════════════════════════
            // Objections heading
            // ═════════════════════════════════════════════════════════════════════════
            _ = document.Add(new Paragraph("Following objections are need to be removed related to your factory - ")
                .SetFont(regularFont).SetFontSize(12)
                .SetMarginBottom(12f));

            // ═════════════════════════════════════════════════════════════════════════
            // Numbered objections list
            // ═════════════════════════════════════════════════════════════════════════
            if (dto.Objections != null && dto.Objections.Any())
            {
                for (int i = 0; i < dto.Objections.Count; i++)
                {
                    _ = document.Add(new Paragraph($"{i + 1}.{dto.Objections[i]}")
                        .SetFont(regularFont).SetFontSize(12)
                        .SetMarginBottom(6f));
                }
            }

            _ = document.Add(new Paragraph("").SetMarginBottom(10f));

            // ═════════════════════════════════════════════════════════════════════════
            // Closing line
            // ═════════════════════════════════════════════════════════════════════════
            _ = document.Add(new Paragraph(
                    "Please comply with the above observations and submit relevant details/documents")
                .SetFont(regularFont).SetFontSize(12)
                .SetMarginBottom(30f));

            // ═════════════════════════════════════════════════════════════════════════
            // Signature block (right-aligned)
            // ═════════════════════════════════════════════════════════════════════════
            var imageData = ImageDataFactory.Create("wwwroot/chief_signature.jpg");

            var sigOuterTable = new PdfTable(new float[] { 1f, 1f })
                .UseAllAvailableWidth().SetBorder(Border.NO_BORDER).SetMarginBottom(8f);

            sigOuterTable.AddCell(new PdfCell().SetBorder(Border.NO_BORDER));

            var sigCell = new PdfCell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT);

            var innerDiv = new Div()
                .SetWidth(250)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetHorizontalAlignment(HorizontalAlignment.RIGHT);

            innerDiv.Add(new PdfImage(imageData).ScaleToFit(150, 50).SetHorizontalAlignment(HorizontalAlignment.CENTER));
            innerDiv.Add(new Paragraph($"( {dto.SignatoryName} )").SetFont(regularFont).SetFontSize(12).SetMarginTop(2f));
            innerDiv.Add(new Paragraph(dto.SignatoryDesignation).SetFont(regularFont).SetFontSize(12).SetMarginTop(2f));
            innerDiv.Add(new Paragraph(dto.SignatoryLocation).SetFont(regularFont).SetFontSize(12).SetMarginTop(0f));

            sigCell.Add(innerDiv);
            sigOuterTable.AddCell(sigCell);
            document.Add(sigOuterTable);

            // ═════════════════════════════════════════════════════════════════════════
            // Footer disclaimer
            // ═════════════════════════════════════════════════════════════════════════
            var pageWidth = pdf.GetDefaultPageSize().GetWidth();
            _ = document.Add(new Paragraph(
                    "This is a computer generated certificate and bears scanned signature. No physical signature is required on this document. You " +
                    "can verify this document by visiting rajnivesh.rajasthan.gov.in or rajfab.rajasthan.gov.in and entering Application No./ID after clicking the link for " +
                    "verification on the page.")
                .SetFont(regularFont).SetFontSize(7)
                .SetFontColor(ColorConstants.GRAY)
                .SetTextAlignment(TextAlignment.JUSTIFIED)
                .SetFixedPosition(35, 33, pageWidth - 70));

            document.Close();

            // ── Save URL to FactoryMapApprovals ───────────────────────────────────────
            var mapApp = await _context.FactoryMapApprovals
                .FirstOrDefaultAsync(x => x.Id == applicationId);
            if (mapApp != null)
            {
                mapApp.ObjectionLetterUrl = fileUrl;
                await _context.SaveChangesAsync();
            }

            return fileUrl;
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // Helper — convert base64 string or data-URI to bytes
        // ─────────────────────────────────────────────────────────────────────────────
        private static Task<byte[]?> DownloadImageAsync(string? source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return Task.FromResult<byte[]?>(null);
            try
            {
                if (source.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                {
                    var commaIndex = source.IndexOf(',');
                    if (commaIndex >= 0)
                        return Task.FromResult<byte[]?>(Convert.FromBase64String(source[(commaIndex + 1)..]));
                }
                return Task.FromResult<byte[]?>(Convert.FromBase64String(source));
            }
            catch
            {
                return Task.FromResult<byte[]?>(null);
            }
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // Certificate: vertical side-text on every page (right edge, rotated 90° CCW)
        // ─────────────────────────────────────────────────────────────────────────────
        private sealed class MapApprovalCertSideTextEventHandler : AbstractPdfDocumentEventHandler
        {
            private readonly PdfFont _font;
            private readonly string _text;

            public MapApprovalCertSideTextEventHandler(PdfFont font, string text)
            {
                _font = font;
                _text = text;
            }

            protected override void OnAcceptedEvent(AbstractPdfDocumentEvent @event)
            {
                if (@event is not PdfDocumentEvent docEvent) return;
                var page = docEvent.GetPage();
                var rect = page.GetPageSize();
                var canvas = new PdfCanvas(page);
                canvas.SaveState();
                canvas.BeginText();
                canvas.SetFontAndSize(_font, 9);
                float tx = rect.GetWidth() - 18f;
                float ty = (rect.GetHeight() / 2f) - 80f;
                canvas.SetTextMatrix(0, 1, -1, 0, tx, ty);
                canvas.ShowText(_text);
                canvas.EndText();
                canvas.RestoreState();
                canvas.Release();
            }
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // Draws a border rectangle on every page
        // ─────────────────────────────────────────────────────────────────────────────
        private sealed class PageBorderEventHandler : AbstractPdfDocumentEventHandler
        {
            protected override void OnAcceptedEvent(AbstractPdfDocumentEvent @event)
            {
                if (@event is not PdfDocumentEvent docEvent) return;
                var page = docEvent.GetPage();
                var rect = page.GetPageSize();
                var canvas = new PdfCanvas(page);
                canvas
                    .SetStrokeColor(ColorConstants.BLACK)
                    .SetLineWidth(1.5f)
                    .Rectangle(25, 25, rect.GetWidth() - 50, rect.GetHeight() - 50)
                    .Stroke();
                canvas.Release();
            }
        }

        private sealed class PageBorderAndFooterEventHandler : AbstractPdfDocumentEventHandler
        {
            private readonly PdfFont _boldFont;
            private readonly PdfFont _regularFont;
            private readonly DateOnly _date;
            private readonly string _postName;
            private readonly string _userName;
            private readonly string? _signatureUrl;

            public PageBorderAndFooterEventHandler(PdfFont boldFont, PdfFont regularFont, DateOnly date, string? signatureUrl = null, string postName = "", string userName = "")
            {
                _boldFont = boldFont;
                _regularFont = regularFont;
                _date = date;
                _postName = postName;
                _userName = userName;
                _signatureUrl = signatureUrl;
            }

            protected override void OnAcceptedEvent(AbstractPdfDocumentEvent @event)
            {
                if (@event is not PdfDocumentEvent docEvent) return;
                var pdfDoc = docEvent.GetDocument();
                var page = docEvent.GetPage();
                var rect = page.GetPageSize();

                // ONE PdfCanvas per page — reused for all drawing operations.
                // Creating multiple PdfCanvas(page) instances for the same page appends
                // independent content streams; when all are released at document close
                // iText7 tries to finalise the pages tree multiple times → error.
                var pdfCanvas = new PdfCanvas(page);

                float pageWidth = rect.GetWidth();
                float pageHeight = rect.GetHeight();

                // ───── Page Border
                pdfCanvas
                    .SetStrokeColor(ColorConstants.BLACK)
                    .SetLineWidth(1.5f)
                    .Rectangle(25, 25, pageWidth - 50, pageHeight - 50)
                    .Stroke();

                // ───── Separator Line (above footer zone)
                float lineY = 70f;
                pdfCanvas
                    .SetStrokeColor(new DeviceRgb(180, 180, 180))
                    .SetLineWidth(0.5f)
                    .MoveTo(30, lineY)
                    .LineTo(pageWidth - 30, lineY)
                    .Stroke();

                float zoneHeight = 65f;
                float zoneY = lineY + 4f;
                float signColWidth = 180f;
                float stampBoxWidth = 220f;
                float belowY = lineY - 4f - zoneHeight;
                float signBoxX = pageWidth - 30f - signColWidth;
                int pageNumber = pdfDoc.GetPageNumber(page);

                // ── All Canvas wrappers share the same PdfCanvas ──────────────────────

                // Left: ONLY LAYOUT APPROVED stamp box
                using (var stampCanvas = new Canvas(pdfCanvas,
                    new iText.Kernel.Geom.Rectangle(30f, zoneY, stampBoxWidth, zoneHeight)))
                {
                    var stampTable = new Table(new float[] { 1f }).UseAllAvailableWidth()
                        .SetBorder(new iText.Layout.Borders.SolidBorder(ColorConstants.BLACK, 1f));
                    stampTable.AddCell(new Cell()
                        .Add(new Paragraph("ONLY LAYOUT APPROVED")
                            .SetFont(_boldFont).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER))
                        .Add(new Paragraph("NOTE : STRUCTURAL DESIGN/STABILITY NOT VERIFIED")
                            .SetFont(_regularFont).SetFontSize(7).SetTextAlignment(TextAlignment.CENTER))
                        .SetBorder(Border.NO_BORDER).SetPadding(6f));
                    stampCanvas.Add(stampTable);
                }

                // Right: Signature image
                if (!string.IsNullOrWhiteSpace(_signatureUrl))
                {
                    float sigX = pageWidth - 30f - signColWidth;
                    using (var sigCanvas = new Canvas(pdfCanvas,
                        new iText.Kernel.Geom.Rectangle(sigX, zoneY, signColWidth, zoneHeight)))
                    {
                        sigCanvas.Add(new PdfImage(ImageDataFactory.Create(_signatureUrl))
                            .ScaleToFit(signColWidth, zoneHeight - 10f)
                            .SetHorizontalAlignment(HorizontalAlignment.CENTER));
                    }
                }

                // Below separator — Left: Dated
                using (var leftCanvas = new Canvas(pdfCanvas,
                    new iText.Kernel.Geom.Rectangle(30f, belowY, stampBoxWidth / 2f, zoneHeight)))
                {
                    leftCanvas.Add(new Paragraph($"Dated: {_date}")
                        .SetFont(_regularFont).SetFontSize(7.5f).SetMargin(0f).SetPaddingTop(6f));
                }

                // Below separator — Center: Page N
                using (var centerCanvas = new Canvas(pdfCanvas,
                    new iText.Kernel.Geom.Rectangle(0f, belowY, pageWidth, zoneHeight)))
                {
                    centerCanvas.Add(new Paragraph($"Page {pageNumber}")
                        .SetFont(_regularFont).SetFontSize(7.5f)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMargin(0f).SetPaddingTop(6f));
                }

                // Below separator — Right: signature label
                using (var signLabelCanvas = new Canvas(pdfCanvas,
                    new iText.Kernel.Geom.Rectangle(signBoxX, belowY, signColWidth, zoneHeight)))
                {
                    if (!string.IsNullOrWhiteSpace(_userName))
                    {
                        // Name (top)
                        signLabelCanvas.Add(new Paragraph($"({_userName ?? "-"})")
                            .SetFont(_boldFont)
                            .SetFontSize(7f)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMargin(0f)
                            .SetPaddingTop(2f));
                    }
                    if (!string.IsNullOrWhiteSpace(_postName))
                    {
                        // Post name (middle)
                        signLabelCanvas.Add(new Paragraph(_postName ?? "-")
                            .SetFont(_regularFont)
                            .SetFontSize(6.5f)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMargin(0f)
                            .SetPaddingTop(1f));
                    }
                    // Signature label (bottom)
                    signLabelCanvas.Add(new Paragraph("Signature / E-sign / Digital sign")
                        .SetFont(_regularFont)
                        .SetFontSize(6.5f)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMargin(0f)
                        .SetPaddingTop(4f));
                }

                // Release the single PdfCanvas only after all drawing is complete
                pdfCanvas.Release();
            }
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // Certificate: vertical side-text on every page
        // ─────────────────────────────────────────────────────────────────────────────
        private sealed class CertSideTextEventHandler : AbstractPdfDocumentEventHandler
        {
            private readonly PdfFont _font;
            private readonly string _text;

            public CertSideTextEventHandler(PdfFont font, string text)
            {
                _font = font;
                _text = text;
            }

            protected override void OnAcceptedEvent(AbstractPdfDocumentEvent @event)
            {
                if (@event is not PdfDocumentEvent docEvent) return;
                var page = docEvent.GetPage();
                var rect = page.GetPageSize();
                var canvas = new PdfCanvas(page);

                // Vertical text: rotated 90° CCW, positioned on the right edge
                canvas.SaveState();
                canvas.BeginText();
                canvas.SetFontAndSize(_font, 15);
                // 90° CCW rotation matrix: [cos90, sin90, -sin90, cos90, tx, ty] = [0, 1, -1, 0, tx, ty]
                float tx = rect.GetWidth() - 10f;
                float ty = (rect.GetHeight() / 2f) - 80f; // vertically centered
                canvas.SetTextMatrix(0, 1, -1, 0, tx, ty);
                canvas.ShowText(_text);
                canvas.EndText();
                canvas.RestoreState();
                canvas.Release();
            }
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // Per-page border + footer handler for Map Approval PDF
        // ─────────────────────────────────────────────────────────────────────────────
        private sealed class MapApprovalPageBorderAndFooterEventHandler : AbstractPdfDocumentEventHandler
        {
            private readonly PdfFont _boldFont;
            private readonly PdfFont _regularFont;
            private readonly string _date;

            public MapApprovalPageBorderAndFooterEventHandler(PdfFont boldFont, PdfFont regularFont, string date)
            {
                _boldFont = boldFont;
                _regularFont = regularFont;
                _date = date;
            }

            protected override void OnAcceptedEvent(AbstractPdfDocumentEvent @event)
            {
                if (@event is not PdfDocumentEvent docEvent) return;
                var pdfDoc = docEvent.GetDocument();
                var page = docEvent.GetPage();
                var rect = page.GetPageSize();
                var canvas = new PdfCanvas(page);

                // Border
                canvas
                    .SetStrokeColor(ColorConstants.BLACK)
                    .SetLineWidth(1.5f)
                    .Rectangle(25, 25, rect.GetWidth() - 50, rect.GetHeight() - 50)
                    .Stroke();

                // Separator line above footer
                float lineY = 65f;
                canvas
                    .SetStrokeColor(new DeviceRgb(180, 180, 180))
                    .SetLineWidth(0.5f)
                    .MoveTo(30, lineY)
                    .LineTo(rect.GetWidth() - 30, lineY)
                    .Stroke();

                float footerY = 38f;
                float pageWidth = rect.GetWidth();
                int pageNumber = pdfDoc.GetPageNumber(page);

                // Left: Dated
                using (var leftCanvas = new Canvas(canvas,
                    new iText.Kernel.Geom.Rectangle(30, footerY, 250, 22)))
                {
                    leftCanvas.Add(new Paragraph($"Dated: {_date}")
                        .SetFont(_regularFont).SetFontSize(9).SetMargin(0));
                }

                // Center: Page number
                using (var centerCanvas = new Canvas(canvas,
                    new iText.Kernel.Geom.Rectangle(0, footerY, pageWidth, 22)))
                {
                    centerCanvas.Add(new Paragraph($"Page {pageNumber}")
                        .SetFont(_regularFont).SetFontSize(9)
                        .SetTextAlignment(TextAlignment.CENTER).SetMargin(0));
                }

                // Right: Signature line
                using (var rightCanvas = new Canvas(canvas,
                    new iText.Kernel.Geom.Rectangle(pageWidth - 280, footerY, 250, 22)))
                {
                    rightCanvas.Add(new Paragraph("e-sign / Signature of occupier")
                        .SetFont(_regularFont).SetFontSize(9)
                        .SetTextAlignment(TextAlignment.RIGHT).SetMargin(0));
                }
                canvas.Release();
            }
        }
    }
}