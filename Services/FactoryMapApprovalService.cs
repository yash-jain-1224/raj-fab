using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;
using static RajFabAPI.Constants.AppConstants;

namespace RajFabAPI.Services
{
    public partial class FactoryMapApprovalService : IFactoryMapApprovalService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public FactoryMapApprovalService(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
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

        public async Task<ApiResponseDto<List<FactoryMapApprovalDto>>> GetAllApplicationsAsync(Guid userId)
        {
            try
            {
                var userAppIds = await _context.ApplicationRegistrations
                    .Where(ar => ar.UserId == userId)
                    .Select(ar => ar.ApplicationId)
                    .ToListAsync();

                var applications = await _context.FactoryMapApprovals
                    .Where(f => userAppIds.Contains(f.Id))
                    .Include(f => f.RawMaterials)
                    .Include(f => f.IntermediateProducts)
                    .Include(f => f.FinishGoods)
                    .Include(f => f.Chemicals)
                    .Include(f => f.MapApprovalFactoryDetails)
                    .Include(f => f.MapApprovalOccupierDetails)
                    .OrderByDescending(f => f.CreatedAt)
                    .ToListAsync();

                var districtIds = applications
                    .SelectMany(a => new[]
                    {
                        a.MapApprovalFactoryDetails?.DistrictId,
                        a.MapApprovalOccupierDetails?.OfficeAddressDistrict,
                        a.MapApprovalOccupierDetails?.ResidentialAddressDistrict,
                        a.PremiseOwnerAddressDistrict
                    })
                    .Where(d => !string.IsNullOrEmpty(d))
                    .Distinct()
                    .ToList();

                var areaIds = applications
                    .Select(a => a.MapApprovalFactoryDetails?.AreaId)
                    .Where(a => !string.IsNullOrEmpty(a))
                    .Distinct()
                    .ToList();

                var districts = await _context.Districts
                    .Where(d => districtIds.Contains(d.Id.ToString()))
                    .ToDictionaryAsync(d => d.Id.ToString(), d => d.Name ?? string.Empty);

                var areas = await _context.Areas
                    .Where(a => areaIds.Contains(a.Id.ToString()))
                    .ToDictionaryAsync(a => a.Id.ToString(), a => a.Name ?? string.Empty);

                var applicationDtos = applications
                    .Select(a => MapToDto(a, districts, areas))
                    .ToList();
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
                    .Include(f => f.MapApprovalFactoryDetails)
                    .Include(f => f.MapApprovalOccupierDetails)
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (application == null)
                {
                    return new ApiResponseDto<FactoryMapApprovalDto>
                    {
                        Success = false,
                        Message = "Application not found"
                    };
                }

                var districts = await LoadDistricts(new[] { application.MapApprovalFactoryDetails.DistrictId, application.MapApprovalOccupierDetails.OfficeAddressDistrict, application.MapApprovalOccupierDetails.ResidentialAddressDistrict });
                var areas = await LoadAreas(new[] { application.MapApprovalFactoryDetails.AreaId});

                return new ApiResponseDto<FactoryMapApprovalDto>
                {
                    Success = true,
                    Message = "Application retrieved successfully",
                    Data = MapToDto(application, districts, areas)
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
                    .Include(f => f.MapApprovalFactoryDetails)
                    .Include(f => f.MapApprovalOccupierDetails)
                    .FirstOrDefaultAsync(f => f.AcknowledgementNumber == acknowledgementNumber);

                if (application == null)
                {
                    return new ApiResponseDto<FactoryMapApprovalDto>
                    {
                        Success = false,
                        Message = "Application not found"
                    };
                }

                var districts = await LoadDistricts(new[] { application.MapApprovalFactoryDetails.DistrictId});
                var areas = await LoadAreas(new[] { application.MapApprovalFactoryDetails.AreaId });

                return new ApiResponseDto<FactoryMapApprovalDto>
                {
                    Success = true,
                    Message = "Application retrieved successfully",
                    Data = MapToDto(application, districts, areas)
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
                    IsNew = isNew ?? true,
                    Version = newVersion
                };
                
                var factoryDetails = new MapApprovalFactoryDetail
                {
                    FactoryMapApprovalId = application.Id, // Set FK for navigation
                    FactoryName = request.MapApprovalFactoryDetail.FactoryName,
                    FactorySituation = request.MapApprovalFactoryDetail.FactorySituation,
                    FactoryPlotNo = request.MapApprovalFactoryDetail.FactoryPlotNo,
                    DivisionId = request.MapApprovalFactoryDetail.DivisionId,
                    DistrictId = request.MapApprovalFactoryDetail.DistrictId,
                    AreaId = request.MapApprovalFactoryDetail.AreaId,
                    FactoryPincode = request.MapApprovalFactoryDetail.FactoryPincode,
                    ContactNo = request.MapApprovalFactoryDetail.ContactNo,
                    Email = request.MapApprovalFactoryDetail.Email,
                    Website = request.MapApprovalFactoryDetail.Website
                };

                application.MapApprovalFactoryDetails = factoryDetails;
                application.MapApprovalOccupierDetails = new MapApprovalOccupierDetail
                {
                    FactoryMapApprovalId = application.Id,
                    Name = request.OccupierDetail.Name,
                    RelationTypeId = request.OccupierDetail.RelationTypeId,
                    RelativeName = request.OccupierDetail.RelativeName,
                    OfficeAddressPlotno = request.OccupierDetail.OfficeAddressPlotno,
                    OfficeAddressStreet = request.OccupierDetail.OfficeAddressStreet,
                    OfficeAddressCity = request.OccupierDetail.OfficeAddressCity,
                    OfficeAddressDistrict = request.OccupierDetail.OfficeAddressDistrict,
                    OfficeAddressState = request.OccupierDetail.OfficeAddressState,
                    OfficeAddressPinCode = request.OccupierDetail.OfficeAddressPinCode,
                    ResidentialAddressPlotno = request.OccupierDetail.ResidentialAddressPlotno,
                    ResidentialAddressStreet = request.OccupierDetail.ResidentialAddressStreet,
                    ResidentialAddressCity = request.OccupierDetail.ResidentialAddressCity,
                    ResidentialAddressDistrict = request.OccupierDetail.ResidentialAddressDistrict,
                    ResidentialAddressState = request.OccupierDetail.ResidentialAddressState,
                    ResidentialAddressPinCode = request.OccupierDetail.ResidentialAddressPinCode,
                    OccupierMobile = request.OccupierDetail.OccupierMobile,
                    OccupierEmail = request.OccupierDetail.OccupierEmail
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
                            QuantityPerDay = 0,
                            //Unit = product.Unit,
                            Unit = "",
                            MaxStorageCapacity = res,
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

                // Calculate total workers
                //int totalWorkers = application.MaxWorkerMale + application.MaxWorkerFemale;

                // Get WorkerRange and FactoryCategoryId
                //var workerRange = await _context.Set<WorkerRange>()
                //	.FirstOrDefaultAsync(wr => totalWorkers >= wr.MinWorkers && totalWorkers <= wr.MaxWorkers);

                //var factoryType = _context.FactoryTypes.FirstOrDefault(x => x.Name == "Not Applicable");
                //var factoryTypeIdGuid = factoryType?.Id;
                //Guid? workerRangeId = workerRange?.Id;
                //var factoryCategory = await _context.Set<FactoryCategory>()
                //	.FirstOrDefaultAsync(fc => fc.WorkerRangeId == workerRangeId && fc.FactoryTypeId == factoryTypeIdGuid);
                //Guid? factoryCategoryId = factoryCategory?.Id;

                //var officeApplicationArea = await _context.Set<OfficeApplicationArea>()
                //	.FirstOrDefaultAsync(oaa => oaa.CityId == Guid.Parse(application.MapApprovalFactoryDetails.AreaId));
                //if (officeApplicationArea != null)  
                //{
                //	var officeId = officeApplicationArea?.OfficeId;
                //	var workflow = await _context.Set<ApplicationWorkFlow>()
                //		.FirstOrDefaultAsync(wf => wf.ModuleId == module.Id && wf.FactoryCategoryId == factoryCategoryId && wf.OfficeId == officeId);
                //	var workflowLevel = await _context.Set<ApplicationWorkFlowLevel>()
                //		.Where(wfl => wfl.ApplicationWorkFlowId == (workflow != null ? workflow.Id : Guid.Empty))
                //		.OrderBy(wfl => wfl.LevelNumber)
                //		.FirstOrDefaultAsync();

                //	if (workflow != null)
                //	{
                //		var applicationApprovalRequest = new ApplicationApprovalRequest
                //		{
                //			ModuleId = module.Id,
                //			ApplicationRegistrationId = appReg.Id,
                //			ApplicationWorkFlowLevelId = workflowLevel.Id,
                //			Status = "Pending",
                //			CreatedDate = DateTime.Now,
                //			UpdatedDate = DateTime.Now
                //		};
                //		_context.Set<ApplicationApprovalRequest>().Add(applicationApprovalRequest);
                //		await _context.SaveChangesAsync();
                //	}
                //}


                // Reload with related data
                //application = await _context.FactoryMapApprovals
                //                .Include(f => f.MapApprovalFactoryDetails)
                //                .Include(f => f.MapApprovalOccupierDetails)
                //                .Include(f => f.RawMaterials)
                //                .Include(f => f.IntermediateProducts)
                //                .Include(f => f.FinishGoods)
                //                .Include(f => f.Chemicals)
                //                .FirstAsync();

                //            var districts = await LoadDistricts(new[] { application.MapApprovalFactoryDetails.DistrictId });
                //            var areas = await LoadAreas(new[] { application.MapApprovalFactoryDetails.AreaId });
                //return new ApiResponseDto<FactoryMapApprovalDto>
                //{
                //    Success = true,
                //    Message = "Application created successfully. Acknowledgement Number: " + application.AcknowledgementNumber,
                //    Data = new FactoryMapApprovalDto { Id = application.Id, AcknowledgementNumber = application.AcknowledgementNumber }
                //};
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

                application.Status = request.Status;
                application.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                var districts = await LoadDistricts(new[] { application.MapApprovalFactoryDetails.DistrictId, application.MapApprovalOccupierDetails.OfficeAddressDistrict, application.MapApprovalOccupierDetails.ResidentialAddressDistrict });
                var areas = await LoadAreas(new[] { application.MapApprovalFactoryDetails.AreaId });

                return new ApiResponseDto<FactoryMapApprovalDto>
                {
                    Success = true,
                    Message = "Application status updated successfully",
                    Data = MapToDto(application, districts, areas)
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
                foreach(var id in guidIds)
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
            FactoryMapApproval application,
            Dictionary<string, string> districts,
            Dictionary<string, string> areas)
        {
            var res = new FactoryMapApprovalDto
            {
                Id = application.Id,
                AcknowledgementNumber = application.AcknowledgementNumber,
                MapApprovalFactoryDetail = application.MapApprovalFactoryDetails != null ? new MapApprovalFactoryDetailsDto
                {
                    FactoryName = application.MapApprovalFactoryDetails.FactoryName,
                    FactorySituation = application.MapApprovalFactoryDetails.FactorySituation,
                    FactoryPlotNo = application.MapApprovalFactoryDetails.FactoryPlotNo,
                    DivisionId = application.MapApprovalFactoryDetails.DivisionId,
                    DistrictId = application.MapApprovalFactoryDetails.DistrictId,
                    DistrictName = districts.GetValueOrDefault(application.MapApprovalFactoryDetails.DistrictId),
                    AreaId = application.MapApprovalFactoryDetails.AreaId,
                    AreaName = areas.GetValueOrDefault(application.MapApprovalFactoryDetails.AreaId),
                    FactoryPincode = application.MapApprovalFactoryDetails.FactoryPincode,
                    ContactNo = application.MapApprovalFactoryDetails.ContactNo,
                    Email = application.MapApprovalFactoryDetails.Email,
                    Website = application.MapApprovalFactoryDetails.Website
                } : null,
                OccupierDetail = application.MapApprovalOccupierDetails != null ? new OccupierDetailsDto
                {
                    Name = application.MapApprovalOccupierDetails.Name,
                    RelationTypeId = application.MapApprovalOccupierDetails.RelationTypeId,
                    RelativeName = application.MapApprovalOccupierDetails.RelativeName,
                    OfficeAddressPlotno = application.MapApprovalOccupierDetails.OfficeAddressPlotno,
                    OfficeAddressStreet = application.MapApprovalOccupierDetails.OfficeAddressStreet,
                    OfficeAddressCity = application.MapApprovalOccupierDetails.OfficeAddressCity,
                    OfficeAddressDistrict = application.MapApprovalOccupierDetails.OfficeAddressDistrict,
                    OfficeAddressDistrictName = districts.GetValueOrDefault(application.MapApprovalOccupierDetails.OfficeAddressDistrict),
                    OfficeAddressState = application.MapApprovalOccupierDetails.OfficeAddressState,
                    OfficeAddressPinCode = application.MapApprovalOccupierDetails.OfficeAddressPinCode,
                    ResidentialAddressPlotno = application.MapApprovalOccupierDetails.ResidentialAddressPlotno,
                    ResidentialAddressStreet = application.MapApprovalOccupierDetails.ResidentialAddressStreet,
                    ResidentialAddressCity = application.MapApprovalOccupierDetails.ResidentialAddressCity,
                    ResidentialAddressDistrict = application.MapApprovalOccupierDetails.ResidentialAddressDistrict,
                    ResidentialAddressDistrictName = districts.GetValueOrDefault(application.MapApprovalOccupierDetails.ResidentialAddressDistrict),
                    ResidentialAddressState = application.MapApprovalOccupierDetails.ResidentialAddressState,
                    ResidentialAddressPinCode = application.MapApprovalOccupierDetails.ResidentialAddressPinCode,
                    OccupierMobile = application.MapApprovalOccupierDetails.OccupierMobile,
                    OccupierEmail = application.MapApprovalOccupierDetails.OccupierEmail
                } : null,
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
                PremiseOwnerAddressDistrict = districts.GetValueOrDefault(application.PremiseOwnerAddressDistrict),
                PremiseOwnerAddressState = application.PremiseOwnerAddressState,
                PremiseOwnerAddressPinCode = application.PremiseOwnerAddressPinCode,
                PremiseOwnerContactNo = application.PremiseOwnerContactNo,
                Place = application.Place,
                Date = application.Date,
                Status = application.Status,
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
    }
}