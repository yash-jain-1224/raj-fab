using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;
using System.Text.Json;

namespace RajFabAPI.Services
{
    public class AnnualReturnService : IAnnualReturnService
    {
        private readonly ApplicationDbContext _context;

        public AnnualReturnService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponseDto<List<AnnualReturnDto>>> GetAllAnnualReturnsAsync()
        {
            try
            {
                var annualReturns = await _context.AnnualReturns
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                var dtos = annualReturns.Select(MapToDto).ToList();

                return new ApiResponseDto<List<AnnualReturnDto>>
                {
                    Success = true,
                    Message = "Annual returns retrieved successfully",
                    Data = dtos
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<List<AnnualReturnDto>>
                {
                    Success = false,
                    Message = $"Error retrieving annual returns: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<List<AnnualReturnDto>>> GetAnnualReturnsByFactoryRegistrationNumberAsync(string factoryRegistrationNumber)
        {
            try
            {
                var annualReturns = await _context.AnnualReturns
                    .Where(a => a.FactoryRegistrationNumber == factoryRegistrationNumber)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                if (!annualReturns.Any())
                {
                    return new ApiResponseDto<List<AnnualReturnDto>>
                    {
                        Success = false,
                        Message = $"No annual returns found for factory registration number: {factoryRegistrationNumber}",
                        Data = new List<AnnualReturnDto>()
                    };
                }

                var dtos = annualReturns.Select(MapToDto).ToList();

                return new ApiResponseDto<List<AnnualReturnDto>>
                {
                    Success = true,
                    Message = "Annual returns retrieved successfully",
                    Data = dtos
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<List<AnnualReturnDto>>
                {
                    Success = false,
                    Message = $"Error retrieving annual returns: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<AnnualReturnDto>> GetAnnualReturnByIdAsync(string id)
        {
            try
            {
                var annualReturn = await _context.AnnualReturns
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (annualReturn == null)
                {
                    return new ApiResponseDto<AnnualReturnDto>
                    {
                        Success = false,
                        Message = $"Annual return with id {id} not found"
                    };
                }

                return new ApiResponseDto<AnnualReturnDto>
                {
                    Success = true,
                    Message = "Annual return retrieved successfully",
                    Data = MapToDto(annualReturn)
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<AnnualReturnDto>
                {
                    Success = false,
                    Message = $"Error retrieving annual return: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<AnnualReturnDto>> CreateAnnualReturnAsync(CreateAnnualReturnRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.FactoryRegistrationNumber))
                {
                    return new ApiResponseDto<AnnualReturnDto>
                    {
                        Success = false,
                        Message = "Factory registration number is required"
                    };
                }

                // Get the highest version for this factory registration number
                var latestRecord = await _context.AnnualReturns
                    .Where(a => a.FactoryRegistrationNumber == request.FactoryRegistrationNumber)
                    .OrderByDescending(a => a.Version)
                    .FirstOrDefaultAsync();

                decimal newVersion = 1.0m;
                if (latestRecord != null)
                {
                    // Increment the version by 0.1
                    newVersion = latestRecord.Version + 0.1m;
                }

                var annualReturn = new AnnualReturn
                {
                    Id = Guid.NewGuid().ToString(),
                    FactoryRegistrationNumber = request.FactoryRegistrationNumber,
                    IsActive = request.IsActive,
                    FormData = request.FormData.GetRawText(),
                    Version = newVersion,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.AnnualReturns.Add(annualReturn);
                await _context.SaveChangesAsync();

                return new ApiResponseDto<AnnualReturnDto>
                {
                    Success = true,
                    Message = "Annual return created successfully",
                    Data = MapToDto(annualReturn)
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<AnnualReturnDto>
                {
                    Success = false,
                    Message = $"Error creating annual return: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<AnnualReturnDto>> UpdateAnnualReturnAsync(string id, UpdateAnnualReturnRequest request)
        {
            try
            {
                var annualReturn = await _context.AnnualReturns
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (annualReturn == null)
                {
                    return new ApiResponseDto<AnnualReturnDto>
                    {
                        Success = false,
                        Message = $"Annual return with id {id} not found"
                    };
                }

                if (request.IsActive.HasValue)
                {
                    annualReturn.IsActive = request.IsActive.Value;
                }

                if (request.FormData.HasValue)
                {
                    annualReturn.FormData = request.FormData.Value.GetRawText();
                }

                if (request.Version.HasValue)
                {
                    annualReturn.Version = request.Version.Value;
                }

                annualReturn.UpdatedAt = DateTime.Now;

                _context.AnnualReturns.Update(annualReturn);
                await _context.SaveChangesAsync();

                return new ApiResponseDto<AnnualReturnDto>
                {
                    Success = true,
                    Message = "Annual return updated successfully",
                    Data = MapToDto(annualReturn)
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<AnnualReturnDto>
                {
                    Success = false,
                    Message = $"Error updating annual return: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteAnnualReturnAsync(string id)
        {
            try
            {
                var annualReturn = await _context.AnnualReturns
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (annualReturn == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = $"Annual return with id {id} not found"
                    };
                }

                _context.AnnualReturns.Remove(annualReturn);
                await _context.SaveChangesAsync();

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Annual return deleted successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = $"Error deleting annual return: {ex.Message}"
                };
            }
        }

        private AnnualReturnDto MapToDto(AnnualReturn annualReturn)
        {
            return new AnnualReturnDto
            {
                Id = annualReturn.Id,
                FactoryRegistrationNumber = annualReturn.FactoryRegistrationNumber,
                IsActive = annualReturn.IsActive,
                FormData = JsonDocument.Parse(annualReturn.FormData ?? "{}").RootElement,
                Version = annualReturn.Version,
                CreatedAt = annualReturn.CreatedAt,
                UpdatedAt = annualReturn.UpdatedAt
            };
        }
    }
}
