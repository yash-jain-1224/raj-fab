using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
    public class OccupierService : IOccupierService
    {
        private readonly ApplicationDbContext _context;

        public OccupierService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponseDto<List<OccupierDto>>> GetAllOccupiersAsync()
        {
            try
            {
                var occupiers = await _context.Occupiers.ToListAsync();
                var occupierDtos = occupiers.Select(MapToDto).ToList();

                return new ApiResponseDto<List<OccupierDto>>
                {
                    Success = true,
                    Data = occupierDtos,
                    Message = "Occupiers retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<List<OccupierDto>>
                {
                    Success = false,
                    Message = $"Error retrieving occupiers: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<OccupierDto>> GetOccupierByIdAsync(string id)
        {
            try
            {
                var occupier = await _context.Occupiers.FindAsync(id);
                if (occupier == null)
                {
                    return new ApiResponseDto<OccupierDto>
                    {
                        Success = false,
                        Message = "Occupier not found"
                    };
                }

                return new ApiResponseDto<OccupierDto>
                {
                    Success = true,
                    Data = MapToDto(occupier),
                    Message = "Occupier retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<OccupierDto>
                {
                    Success = false,
                    Message = $"Error retrieving occupier: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<OccupierDto>> GetOccupierByEmailAsync(string email)
        {
            try
            {
                var occupier = await _context.Occupiers
                    .FirstOrDefaultAsync(o => o.Email.ToLower() == email.ToLower());
                
                if (occupier == null)
                {
                    return new ApiResponseDto<OccupierDto>
                    {
                        Success = false,
                        Message = "Occupier not found with this email"
                    };
                }

                return new ApiResponseDto<OccupierDto>
                {
                    Success = true,
                    Data = MapToDto(occupier),
                    Message = "Occupier retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<OccupierDto>
                {
                    Success = false,
                    Message = $"Error retrieving occupier by email: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<OccupierDto>> CreateOccupierAsync(CreateOccupierRequest request)
        {
            try
            {
                var occupier = new Occupier
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    FatherName = request.FatherName,
                    DateOfBirth = request.DateOfBirth,
                    Gender = request.Gender,
                    Email = request.Email,
                    MobileNo = request.MobileNo,
                    PlotNo = request.PlotNo,
                    StreetLocality = request.StreetLocality,
                    VillageTownCity = request.VillageTownCity,
                    District = request.District,
                    Pincode = request.Pincode,
                    Designation = request.Designation,
                    PanCard = request.PanCard,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Occupiers.Add(occupier);
                await _context.SaveChangesAsync();

                return new ApiResponseDto<OccupierDto>
                {
                    Success = true,
                    Data = MapToDto(occupier),
                    Message = "Occupier created successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<OccupierDto>
                {
                    Success = false,
                    Message = $"Error creating occupier: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<OccupierDto>> UpdateOccupierAsync(string id, CreateOccupierRequest request)
        {
            try
            {
                var occupier = await _context.Occupiers.FindAsync(id);
                if (occupier == null)
                {
                    return new ApiResponseDto<OccupierDto>
                    {
                        Success = false,
                        Message = "Occupier not found"
                    };
                }

                occupier.FirstName = request.FirstName;
                occupier.LastName = request.LastName;
                occupier.FatherName = request.FatherName;
                occupier.DateOfBirth = request.DateOfBirth;
                occupier.Gender = request.Gender;
                occupier.Email = request.Email;
                occupier.MobileNo = request.MobileNo;
                occupier.PlotNo = request.PlotNo;
                occupier.StreetLocality = request.StreetLocality;
                occupier.VillageTownCity = request.VillageTownCity;
                occupier.District = request.District;
                occupier.Pincode = request.Pincode;
                occupier.Designation = request.Designation;
                occupier.PanCard = request.PanCard;
                occupier.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return new ApiResponseDto<OccupierDto>
                {
                    Success = true,
                    Data = MapToDto(occupier),
                    Message = "Occupier updated successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<OccupierDto>
                {
                    Success = false,
                    Message = $"Error updating occupier: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteOccupierAsync(string id)
        {
            try
            {
                var occupier = await _context.Occupiers.FindAsync(id);
                if (occupier == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Occupier not found"
                    };
                }

                _context.Occupiers.Remove(occupier);
                await _context.SaveChangesAsync();

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Occupier deleted successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = $"Error deleting occupier: {ex.Message}"
                };
            }
        }

        private static OccupierDto MapToDto(Occupier occupier)
        {
            return new OccupierDto
            {
                Id = occupier.Id,
                FirstName = occupier.FirstName,
                LastName = occupier.LastName,
                FatherName = occupier.FatherName,
                DateOfBirth = occupier.DateOfBirth,
                Gender = occupier.Gender,
                Email = occupier.Email,
                MobileNo = occupier.MobileNo,
                PlotNo = occupier.PlotNo,
                StreetLocality = occupier.StreetLocality,
                VillageTownCity = occupier.VillageTownCity,
                District = occupier.District,
                Pincode = occupier.Pincode,
                Designation = occupier.Designation,
                PanCard = occupier.PanCard,
                CreatedAt = occupier.CreatedAt,
                UpdatedAt = occupier.UpdatedAt
            };
        }
    }
}