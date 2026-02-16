using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services
{
    public partial class FactoryMapApprovalService
    {
        public async Task<ApiResponseDto<FactoryMapApprovalDto>> AmendApplicationAsync(string id, CreateFactoryMapApprovalRequest request)
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

                // Allow amendments for applications that are:
                // 1. Returned to Applicant (for corrections)
                // 2. Under Review (before final decision)
                // 3. Submitted (before review starts)
                // 4. Approved (post-approval amendments)
                var normalizedStatus = application.Status?.ToLower().Trim() ?? "";
                var allowedStatuses = new[] { "return", "under review", "submitted", "approved" };
                bool canAmend = allowedStatuses.Any(status => normalizedStatus.Contains(status));
                
                if (!canAmend)
                {
                    return new ApiResponseDto<FactoryMapApprovalDto>
                    {
                        Success = false,
                        Message = $"Applications with status '{application.Status}' cannot be amended. Only applications that are Returned, Under Review, Submitted, or Approved can be amended."
                    };
                }

                // Update basic fields
                //application.FactoryName = request.FactoryName;
                //application.ApplicantName = request.ApplicantName;
                //application.Email = request.Email;
                //application.MobileNo = request.MobileNo;
                //application.Address = request.Address;
                //application.District = request.District;
                //application.Pincode = request.Pincode;
                //application.PlotArea = request.PlotArea;
                //application.BuildingArea = request.BuildingArea;

                // Update FactoryTypeId
                //string? factoryTypeId = string.IsNullOrWhiteSpace(request.FactoryTypeId) ? null : request.FactoryTypeId;
                //if (factoryTypeId != null)
                //{
                //    var exists = await _context.FactoryTypes_Old.AnyAsync(ft => ft.Id == factoryTypeId);
                //    if (!exists)
                //    {
                //        factoryTypeId = null;
                //    }
                //}
                //application.FactoryTypeId = factoryTypeId;

                // Update Occupier details
                //application.OccupierType = request.OccupierType;
                //application.Name = request.Name;
                //application.OccupierFatherName = request.OccupierFatherName;
                //application.OccupierPlotNumber = request.OccupierPlotNumber;
                //application.OccupierStreetLocality = request.OccupierStreetLocality;
                //application.OccupierCityTown = request.OccupierCityTown;
                //application.OccupierDistrict = request.OccupierDistrict;
                //application.OccupierArea = request.OccupierArea;
                //application.OccupierPincode = request.OccupierPincode;
                //application.OccupierMobile = request.OccupierMobile;
                //application.OccupierEmail = request.OccupierEmail;
                //application.OccupierPanCard = request.OccupierPanCard;

                // Remove existing raw materials and add new ones
                _context.FactoryMapRawMaterials.RemoveRange(application.RawMaterials);
                if (request.RawMaterials != null && request.RawMaterials.Any())
                {
                    foreach (var rawMaterial in request.RawMaterials)
                    {
                        application.RawMaterials.Add(new FactoryMapRawMaterial
                        {
                            MaterialName = rawMaterial.MaterialName,
                            MaxStorageQuantity = rawMaterial.MaxStorageQuantity,
                        });
                    }
                }

                // Remove existing intermediate products and add new ones
                _context.FactoryMapIntermediateProducts.RemoveRange(application.IntermediateProducts);
                if (request.IntermediateProducts != null && request.IntermediateProducts.Any())
                {
                    foreach (var product in request.IntermediateProducts)
                    {
                        application.IntermediateProducts.Add(new FactoryMapIntermediateProduct
                        {
                            ProductName = product.ProductName,
                            MaxStorageQuantity = product.MaxStorageQuantity,
                        });
                    }
                }

                // Increment amendment count and save history
                //application.AmendmentCount++;
                
                var history = new Models.ApplicationHistory
                {
                    ApplicationId = application.Id,
                    ApplicationType = "FactoryMapApproval",
                    Action = "Amended",
                    PreviousStatus = application.Status,
                    NewStatus = "Under Review",
                    //Comments = $"Application amended (Amendment #{application.AmendmentCount})",
                    ActionBy = "Applicant",
                    //ActionByName = application.ApplicantName,
                    ActionDate = DateTime.Now
                };
                
                _context.ApplicationHistories.Add(history);
                
                // Change status back to "Under Review"
                application.Status = "Under Review";
                //application.Comments = null; // Clear previous comments
                application.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                // Reload with related data
                application = await _context.FactoryMapApprovals
                    //.Include(f => f.Documents)
                    .Include(f => f.RawMaterials)
                    .Include(f => f.IntermediateProducts)
                    .Include(f => f.FinishGoods)
                    .Include(f => f.Chemicals)
                    .FirstAsync(f => f.Id == application.Id);

                var districts = await LoadDistricts(new[] { application.MapApprovalOccupierDetails.OfficeAddressDistrict, application.MapApprovalOccupierDetails.ResidentialAddressDistrict });
                var areas = await LoadAreas(new[] { application.MapApprovalFactoryDetails.AreaId});

                return new ApiResponseDto<FactoryMapApprovalDto>
                {
                    Success = true,
                    Message = "Application amended and resubmitted successfully",
                    Data = MapToDto(application, districts, areas)
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<FactoryMapApprovalDto>
                {
                    Success = false,
                    Message = $"Error amending application: {ex.Message}"
                };
            }
        }
    }
}
