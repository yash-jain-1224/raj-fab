using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
    public class FactoryTypeService : IFactoryTypeService
    {
        private readonly ApplicationDbContext _context;

        public FactoryTypeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponseDto<List<FactoryTypeOldDto>>> GetAllFactoryTypesAsync()
        {
            try
            {
                var factoryTypes = await _context.FactoryTypes_Old
                    .Where(ft => ft.IsActive)
                    .Include(ft => ft.RequiredDocuments)
                        .ThenInclude(ftd => ftd.DocumentType)
                    .Include(ft => ft.AllowedProcessTypes)
                        .ThenInclude(pt => pt.RequiredDocuments)
                    .OrderBy(ft => ft.Name)
                    .ToListAsync();

                var factoryTypeDtos = factoryTypes.Select(MapToDto).ToList();

                return new ApiResponseDto<List<FactoryTypeOldDto>>
                {
                    Success = true,
                    Data = factoryTypeDtos,
                    Message = "Factory types retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<List<FactoryTypeOldDto>>
                {
                    Success = false,
                    Message = $"Error retrieving factory types: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<FactoryTypeOldDto>> GetFactoryTypeByIdAsync(string id)
        {
            try
            {
                var factoryType = await _context.FactoryTypes_Old
                    .Include(ft => ft.RequiredDocuments)
                        .ThenInclude(ftd => ftd.DocumentType)
                    .Include(ft => ft.AllowedProcessTypes)
                        .ThenInclude(pt => pt.RequiredDocuments)
                    .FirstOrDefaultAsync(ft => ft.Id == id && ft.IsActive);

                if (factoryType == null)
                {
                    return new ApiResponseDto<FactoryTypeOldDto>
                    {
                        Success = false,
                        Message = "Factory type not found"
                    };
                }

                return new ApiResponseDto<FactoryTypeOldDto>
                {
                    Success = true,
                    Data = MapToDto(factoryType),
                    Message = "Factory type retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<FactoryTypeOldDto>
                {
                    Success = false,
                    Message = $"Error retrieving factory type: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<FactoryTypeOldDto>> CreateFactoryTypeAsync(CreateFactoryTypeRequest request)
        {
            try
            {
                var factoryType = new FactoryTypeOld
                {
                    Name = request.Name,
                    Description = request.Description,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // Add required documents
                foreach (var docTypeId in request.DocumentTypeIds)
                {
                    factoryType.RequiredDocuments.Add(new FactoryTypeDocument
                    {
                        FactoryTypeId = factoryType.Id,
                        DocumentTypeId = docTypeId,
                        IsRequired = true,
                        Order = factoryType.RequiredDocuments.Count + 1
                    });
                }

                // Add manufacturing process types
                foreach (var processTypeRequest in request.ProcessTypes)
                {
                    var processType = new ManufacturingProcessType
                    {
                        FactoryTypeId = factoryType.Id,
                        Name = processTypeRequest.Name,
                        Description = processTypeRequest.Description,
                        HasHazardousChemicals = processTypeRequest.HasHazardousChemicals,
                        HasDangerousOperations = processTypeRequest.HasDangerousOperations,
                        WorkerLimit = processTypeRequest.WorkerLimit
                    };

                    foreach (var docRequest in processTypeRequest.RequiredDocuments)
                    {
                        processType.RequiredDocuments.Add(new ProcessDocument
                        {
                            ManufacturingProcessTypeId = processType.Id,
                            DocumentTypeId = docRequest.DocumentTypeId,
                            IsRequired = docRequest.IsRequired,
                            ConditionalField = docRequest.ConditionalField,
                            ConditionalValue = docRequest.ConditionalValue
                        });
                    }

                    factoryType.AllowedProcessTypes.Add(processType);
                }

                _context.FactoryTypes_Old.Add(factoryType);
                await _context.SaveChangesAsync();

                // Reload with includes
                var createdFactoryType = await _context.FactoryTypes_Old
                    .Include(ft => ft.RequiredDocuments)
                        .ThenInclude(ftd => ftd.DocumentType)
                    .Include(ft => ft.AllowedProcessTypes)
                        .ThenInclude(pt => pt.RequiredDocuments)
                    .FirstAsync(ft => ft.Id == factoryType.Id);

                return new ApiResponseDto<FactoryTypeOldDto>
                {
                    Success = true,
                    Data = MapToDto(createdFactoryType),
                    Message = "Factory type created successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<FactoryTypeOldDto>
                {
                    Success = false,
                    Message = $"Error creating factory type: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<FactoryTypeOldDto>> UpdateFactoryTypeAsync(string id, CreateFactoryTypeRequest request)
        {
            try
            {
                var factoryType = await _context.FactoryTypes_Old
                    .Include(ft => ft.RequiredDocuments)
                    .Include(ft => ft.AllowedProcessTypes)
                        .ThenInclude(pt => pt.RequiredDocuments)
                    .FirstOrDefaultAsync(ft => ft.Id == id);

                if (factoryType == null)
                {
                    return new ApiResponseDto<FactoryTypeOldDto>
                    {
                        Success = false,
                        Message = "Factory type not found"
                    };
                }

                factoryType.Name = request.Name;
                factoryType.Description = request.Description;
                factoryType.UpdatedAt = DateTime.Now;

                // Clear existing documents and process types
                _context.FactoryTypeDocuments.RemoveRange(factoryType.RequiredDocuments);
                _context.ManufacturingProcessTypes.RemoveRange(factoryType.AllowedProcessTypes);

                factoryType.RequiredDocuments.Clear();
                factoryType.AllowedProcessTypes.Clear();

                // Add updated documents
                foreach (var docTypeId in request.DocumentTypeIds)
                {
                    factoryType.RequiredDocuments.Add(new FactoryTypeDocument
                    {
                        FactoryTypeId = factoryType.Id,
                        DocumentTypeId = docTypeId,
                        IsRequired = true,
                        Order = factoryType.RequiredDocuments.Count + 1
                    });
                }

                // Add updated process types
                foreach (var processTypeRequest in request.ProcessTypes)
                {
                    var processType = new ManufacturingProcessType
                    {
                        FactoryTypeId = factoryType.Id,
                        Name = processTypeRequest.Name,
                        Description = processTypeRequest.Description,
                        HasHazardousChemicals = processTypeRequest.HasHazardousChemicals,
                        HasDangerousOperations = processTypeRequest.HasDangerousOperations,
                        WorkerLimit = processTypeRequest.WorkerLimit
                    };

                    foreach (var docRequest in processTypeRequest.RequiredDocuments)
                    {
                        processType.RequiredDocuments.Add(new ProcessDocument
                        {
                            ManufacturingProcessTypeId = processType.Id,
                            DocumentTypeId = docRequest.DocumentTypeId,
                            IsRequired = docRequest.IsRequired,
                            ConditionalField = docRequest.ConditionalField,
                            ConditionalValue = docRequest.ConditionalValue
                        });
                    }

                    factoryType.AllowedProcessTypes.Add(processType);
                }

                await _context.SaveChangesAsync();

                // Reload with includes
                var updatedFactoryType = await _context.FactoryTypes_Old
                    .Include(ft => ft.RequiredDocuments)
                        .ThenInclude(ftd => ftd.DocumentType)
                    .Include(ft => ft.AllowedProcessTypes)
                        .ThenInclude(pt => pt.RequiredDocuments)
                    .FirstAsync(ft => ft.Id == factoryType.Id);

                return new ApiResponseDto<FactoryTypeOldDto>
                {
                    Success = true,
                    Data = MapToDto(updatedFactoryType),
                    Message = "Factory type updated successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<FactoryTypeOldDto>
                {
                    Success = false,
                    Message = $"Error updating factory type: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteFactoryTypeAsync(string id)
        {
            try
            {
                var factoryType = await _context.FactoryTypes_Old.FirstOrDefaultAsync(ft => ft.Id == id);

                if (factoryType == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Factory type not found"
                    };
                }

                factoryType.IsActive = false;
                factoryType.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Factory type deleted successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = $"Error deleting factory type: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<List<DocumentTypeDto>>> GetAllDocumentTypesAsync()
        {
            try
            {
                var documentTypes = await _context.DocumentTypes
                    .Where(dt => dt.IsActive)
                    .OrderBy(dt => dt.Name)
                    .ToListAsync();

                var documentTypeDtos = documentTypes.Select(dt => new DocumentTypeDto
                {
                    Id = dt.Id,
                    Name = dt.Name,
                    Description = dt.Description,
                    FileTypes = dt.FileTypes,
                    MaxSizeMB = dt.MaxSizeMB,
                    Module = dt.Module,
                    ServiceType = dt.ServiceType,
                    IsConditional = dt.IsConditional,
                    ConditionalField = dt.ConditionalField,
                    ConditionalValue = dt.ConditionalValue,
                    IsActive = dt.IsActive,
                    CreatedAt = dt.CreatedAt,
                    UpdatedAt = dt.UpdatedAt
                }).ToList();

                return new ApiResponseDto<List<DocumentTypeDto>>
                {
                    Success = true,
                    Data = documentTypeDtos,
                    Message = "Document types retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<List<DocumentTypeDto>>
                {
                    Success = false,
                    Message = $"Error retrieving document types: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<DocumentTypeDto>> CreateDocumentTypeAsync(CreateDocumentTypeRequest request)
        {
            try
            {
                var documentType = new DocumentType
                {
                    Name = request.Name,
                    Description = request.Description,
                    FileTypes = request.FileTypes,
                    MaxSizeMB = request.MaxSizeMB,
                    Module = request.Module,
                    ServiceType = request.ServiceType,
                    IsConditional = request.IsConditional,
                    ConditionalField = request.ConditionalField,
                    ConditionalValue = request.ConditionalValue,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.DocumentTypes.Add(documentType);
                await _context.SaveChangesAsync();

                var documentTypeDto = new DocumentTypeDto
                {
                    Id = documentType.Id,
                    Name = documentType.Name,
                    Description = documentType.Description,
                    FileTypes = documentType.FileTypes,
                    MaxSizeMB = documentType.MaxSizeMB,
                    Module = documentType.Module,
                    ServiceType = documentType.ServiceType,
                    IsConditional = documentType.IsConditional,
                    ConditionalField = documentType.ConditionalField,
                    ConditionalValue = documentType.ConditionalValue,
                    IsActive = documentType.IsActive,
                    CreatedAt = documentType.CreatedAt,
                    UpdatedAt = documentType.UpdatedAt
                };

                return new ApiResponseDto<DocumentTypeDto>
                {
                    Success = true,
                    Data = documentTypeDto,
                    Message = "Document type created successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<DocumentTypeDto>
                {
                    Success = false,
                    Message = $"Error creating document type: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<DocumentTypeDto>> UpdateDocumentTypeAsync(string id, CreateDocumentTypeRequest request)
        {
            try
            {
                var documentType = await _context.DocumentTypes.FirstOrDefaultAsync(dt => dt.Id == id);

                if (documentType == null)
                {
                    return new ApiResponseDto<DocumentTypeDto>
                    {
                        Success = false,
                        Message = "Document type not found"
                    };
                }

                documentType.Name = request.Name;
                documentType.Description = request.Description;
                documentType.FileTypes = request.FileTypes;
                documentType.MaxSizeMB = request.MaxSizeMB;
                documentType.Module = request.Module;
                documentType.ServiceType = request.ServiceType;
                documentType.IsConditional = request.IsConditional;
                documentType.ConditionalField = request.ConditionalField;
                documentType.ConditionalValue = request.ConditionalValue;
                documentType.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                var documentTypeDto = new DocumentTypeDto
                {
                    Id = documentType.Id,
                    Name = documentType.Name,
                    Description = documentType.Description,
                    FileTypes = documentType.FileTypes,
                    MaxSizeMB = documentType.MaxSizeMB,
                    Module = documentType.Module,
                    ServiceType = documentType.ServiceType,
                    IsConditional = documentType.IsConditional,
                    ConditionalField = documentType.ConditionalField,
                    ConditionalValue = documentType.ConditionalValue,
                    IsActive = documentType.IsActive,
                    CreatedAt = documentType.CreatedAt,
                    UpdatedAt = documentType.UpdatedAt
                };

                return new ApiResponseDto<DocumentTypeDto>
                {
                    Success = true,
                    Data = documentTypeDto,
                    Message = "Document type updated successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<DocumentTypeDto>
                {
                    Success = false,
                    Message = $"Error updating document type: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteDocumentTypeAsync(string id)
        {
            try
            {
                var documentType = await _context.DocumentTypes.FirstOrDefaultAsync(dt => dt.Id == id);

                if (documentType == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Document type not found"
                    };
                }

                documentType.IsActive = false;
                documentType.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Document type deleted successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = $"Error deleting document type: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<List<DocumentTypeDto>>> GetDocumentTypesByModuleAsync(string module)
        {
            try
            {
                var documentTypes = await _context.DocumentTypes
                    .Where(dt => dt.IsActive && dt.Module == module)
                    .OrderBy(dt => dt.Name)
                    .ToListAsync();

                var documentTypeDtos = documentTypes.Select(dt => new DocumentTypeDto
                {
                    Id = dt.Id,
                    Name = dt.Name,
                    Description = dt.Description,
                    FileTypes = dt.FileTypes,
                    MaxSizeMB = dt.MaxSizeMB,
                    Module = dt.Module,
                    ServiceType = dt.ServiceType,
                    IsConditional = dt.IsConditional,
                    ConditionalField = dt.ConditionalField,
                    ConditionalValue = dt.ConditionalValue,
                    IsActive = dt.IsActive,
                    CreatedAt = dt.CreatedAt,
                    UpdatedAt = dt.UpdatedAt
                }).ToList();

                return new ApiResponseDto<List<DocumentTypeDto>>
                {
                    Success = true,
                    Data = documentTypeDtos,
                    Message = $"Document types for module '{module}' retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<List<DocumentTypeDto>>
                {
                    Success = false,
                    Message = $"Error retrieving document types for module: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<List<DocumentTypeDto>>> GetDocumentTypesByModuleAndServiceAsync(string module, string serviceType)
        {
            try
            {
                var documentTypes = await _context.DocumentTypes
                    .Where(dt => dt.IsActive && dt.Module == module && dt.ServiceType == serviceType)
                    .OrderBy(dt => dt.Name)
                    .ToListAsync();

                var documentTypeDtos = documentTypes.Select(dt => new DocumentTypeDto
                {
                    Id = dt.Id,
                    Name = dt.Name,
                    Description = dt.Description,
                    FileTypes = dt.FileTypes,
                    MaxSizeMB = dt.MaxSizeMB,
                    Module = dt.Module,
                    ServiceType = dt.ServiceType,
                    IsConditional = dt.IsConditional,
                    ConditionalField = dt.ConditionalField,
                    ConditionalValue = dt.ConditionalValue,
                    IsActive = dt.IsActive,
                    CreatedAt = dt.CreatedAt,
                    UpdatedAt = dt.UpdatedAt
                }).ToList();

                return new ApiResponseDto<List<DocumentTypeDto>>
                {
                    Success = true,
                    Data = documentTypeDtos,
                    Message = $"Document types for module '{module}' and service '{serviceType}' retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<List<DocumentTypeDto>>
                {
                    Success = false,
                    Message = $"Error retrieving document types for module and service: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<List<BoilerDocumentTypeDto>>> GetBoilerDocumentTypesAsync(string serviceType)
        {
            try
            {
                // Normalize service type to handle various input formats
                var normalizedServiceType = NormalizeBoilerServiceType(serviceType);
                Console.WriteLine($"DEBUG: Querying BoilerDocumentTypes for service type: {normalizedServiceType}");
                
                // Query using a safe left join to avoid GUID->string casting issues
                var boilerDocTypeDtos = await (from bdt in _context.BoilerDocumentTypes
                                               join dt in _context.DocumentTypes on bdt.DocumentTypeId equals dt.Id into gj
                                               from dt in gj.DefaultIfEmpty()
                                               where bdt.BoilerServiceType == normalizedServiceType
                                               orderby bdt.OrderIndex
                                               select new BoilerDocumentTypeDto
                                               {
                                                   Id = bdt.Id,
                                                   BoilerServiceType = bdt.BoilerServiceType,
                                                   DocumentTypeId = bdt.DocumentTypeId,
                                                   DocumentTypeName = dt != null ? dt.Name : "",
                                                   DocumentTypeDescription = dt != null ? dt.Description : "",
                                                   IsRequired = bdt.IsRequired,
                                                   ConditionalField = bdt.ConditionalField,
                                                   ConditionalValue = bdt.ConditionalValue,
                                                   OrderIndex = bdt.OrderIndex,
                                                   FileTypes = dt != null ? dt.FileTypes : "",
                                                   MaxSizeMB = dt != null ? dt.MaxSizeMB : 25
                                               }).ToListAsync();

                // If no specific mappings found, fallback to DocumentTypes directly
                if (!boilerDocTypeDtos.Any())
                {
                    Console.WriteLine("DEBUG: No BoilerDocumentTypes found, trying DocumentTypes fallback");
                    
                    var fallbackDocTypes = await _context.DocumentTypes
                        .Where(dt => dt.IsActive && 
                                   dt.Module == "Boiler" && 
                                   dt.ServiceType == normalizedServiceType)
                        .OrderBy(dt => dt.Name)
                        .ToListAsync();

                    Console.WriteLine($"DEBUG: DocumentTypes fallback returned {fallbackDocTypes.Count} records");

                    boilerDocTypeDtos = fallbackDocTypes.Select((dt, index) => new BoilerDocumentTypeDto
                    {
                        Id = Guid.NewGuid().ToString(), // Temporary ID for unmapped items
                        BoilerServiceType = normalizedServiceType,
                        DocumentTypeId = dt.Id,
                        DocumentTypeName = dt.Name,
                        DocumentTypeDescription = dt.Description ?? "",
                        IsRequired = true, // Default to required for fallback
                        ConditionalField = dt.ConditionalField,
                        ConditionalValue = dt.ConditionalValue,
                        OrderIndex = index + 1,
                        FileTypes = dt.FileTypes ?? "",
                        MaxSizeMB = dt.MaxSizeMB
                    }).ToList();
                }

                return new ApiResponseDto<List<BoilerDocumentTypeDto>>
                {
                    Success = true,
                    Data = boilerDocTypeDtos,
                    Message = $"Boiler document types for service '{normalizedServiceType}' retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetBoilerDocumentTypesAsync: {ex.Message}");
                Console.WriteLine($"ERROR Stack Trace: {ex.StackTrace}");
                
                return new ApiResponseDto<List<BoilerDocumentTypeDto>>
                {
                    Success = false,
                    Message = $"Error retrieving boiler document types: {ex.Message}"
                };
            }
        }

        private static string NormalizeBoilerServiceType(string serviceType)
        {
            if (string.IsNullOrWhiteSpace(serviceType))
                return "Registration";

            // Remove "boiler-" prefix if present and normalize case
            var normalized = serviceType.Replace("boiler-", "", StringComparison.OrdinalIgnoreCase);
            
            // Convert to Title Case for consistency
            return normalized.ToLowerInvariant() switch
            {
                "registration" => "Registration",
                "renewal" => "Renewal", 
                "modification" => "Modification",
                "transfer" => "Transfer",
                _ => char.ToUpper(normalized[0]) + normalized[1..].ToLower()
            };
        }

        public async Task<ApiResponseDto<BoilerDocumentTypeDto>> CreateBoilerDocumentTypeAsync(CreateBoilerDocumentTypeRequest request)
        {
            try
            {
                // Check if document type exists
                var documentType = await _context.DocumentTypes.FirstOrDefaultAsync(dt => dt.Id == request.DocumentTypeId);
                if (documentType == null)
                {
                    return new ApiResponseDto<BoilerDocumentTypeDto>
                    {
                        Success = false,
                        Message = "Document type not found"
                    };
                }

                var boilerDocType = new BoilerDocumentType
                {
                    BoilerServiceType = request.BoilerServiceType,
                    DocumentTypeId = request.DocumentTypeId,
                    IsRequired = request.IsRequired,
                    ConditionalField = request.ConditionalField,
                    ConditionalValue = request.ConditionalValue,
                    OrderIndex = request.OrderIndex
                };

                _context.BoilerDocumentTypes.Add(boilerDocType);
                await _context.SaveChangesAsync();

                // Reload with includes
                var createdBoilerDocType = await _context.BoilerDocumentTypes
                    .Include(bdt => bdt.DocumentType)
                    .FirstAsync(bdt => bdt.Id == boilerDocType.Id);

                var boilerDocTypeDto = new BoilerDocumentTypeDto
                {
                    Id = createdBoilerDocType.Id,
                    BoilerServiceType = createdBoilerDocType.BoilerServiceType,
                    DocumentTypeId = createdBoilerDocType.DocumentTypeId,
                    DocumentTypeName = createdBoilerDocType.DocumentType?.Name ?? "",
                    DocumentTypeDescription = createdBoilerDocType.DocumentType?.Description ?? "",
                    IsRequired = createdBoilerDocType.IsRequired,
                    ConditionalField = createdBoilerDocType.ConditionalField,
                    ConditionalValue = createdBoilerDocType.ConditionalValue,
                    OrderIndex = createdBoilerDocType.OrderIndex,
                    FileTypes = createdBoilerDocType.DocumentType?.FileTypes ?? "",
                    MaxSizeMB = createdBoilerDocType.DocumentType?.MaxSizeMB ?? 25
                };

                return new ApiResponseDto<BoilerDocumentTypeDto>
                {
                    Success = true,
                    Data = boilerDocTypeDto,
                    Message = "Boiler document type created successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<BoilerDocumentTypeDto>
                {
                    Success = false,
                    Message = $"Error creating boiler document type: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteBoilerDocumentTypeAsync(string id)
        {
            try
            {
                var boilerDocType = await _context.BoilerDocumentTypes.FirstOrDefaultAsync(bdt => bdt.Id == id);

                if (boilerDocType == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Boiler document type not found"
                    };
                }

                _context.BoilerDocumentTypes.Remove(boilerDocType);
                await _context.SaveChangesAsync();

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Boiler document type deleted successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = $"Error deleting boiler document type: {ex.Message}"
                };
            }
        }

        private static FactoryTypeOldDto MapToDto(FactoryTypeOld factoryType)
        {
            return new FactoryTypeOldDto
            {
                Id = factoryType.Id,
                Name = factoryType.Name,
                Description = factoryType.Description,
                IsActive = factoryType.IsActive,
                CreatedAt = factoryType.CreatedAt,
                UpdatedAt = factoryType.UpdatedAt,
                RequiredDocuments = factoryType.RequiredDocuments.Select(ftd => new FactoryTypeDocumentDto
                {
                    Id = ftd.Id,
                    DocumentTypeId = ftd.DocumentTypeId,
                    DocumentTypeName = ftd.DocumentType?.Name ?? "",
                    DocumentTypeDescription = ftd.DocumentType?.Description ?? "",
                    IsRequired = ftd.IsRequired,
                    Order = ftd.Order,
                    FileTypes = ftd.DocumentType?.FileTypes ?? "",
                    MaxSizeMB = ftd.DocumentType?.MaxSizeMB ?? 25
                }).OrderBy(d => d.Order).ToList(),
                AllowedProcessTypes = factoryType.AllowedProcessTypes.Select(pt => new ManufacturingProcessTypeDto
                {
                    Id = pt.Id,
                    Name = pt.Name,
                    Description = pt.Description,
                    HasHazardousChemicals = pt.HasHazardousChemicals,
                    HasDangerousOperations = pt.HasDangerousOperations,
                    WorkerLimit = pt.WorkerLimit,
                    RequiredDocuments = pt.RequiredDocuments.Select(pd => new ProcessDocumentDto
                    {
                        Id = pd.Id,
                        DocumentTypeId = pd.DocumentTypeId,
                        IsRequired = pd.IsRequired,
                        ConditionalField = pd.ConditionalField,
                        ConditionalValue = pd.ConditionalValue
                    }).ToList()
                }).ToList()
            };
        }
    }
}