using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services
{
    public partial class LicenseRenewalService
    {
        public async Task<ApiResponseDto<LicenseRenewalDto>> AmendRenewalAsync(string id, CreateLicenseRenewalRequest request)
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

                // Allow amendments for renewals that are:
                // 1. Returned to Applicant (for corrections)
                // 2. Under Review (before final decision)
                // 3. Submitted (before review starts)
                // 4. Approved (post-approval amendments)
                var normalizedStatus = renewal.Status?.ToLower().Trim() ?? "";
                var allowedStatuses = new[] { "return", "under review", "submitted", "approved" };
                bool canAmend = allowedStatuses.Any(status => normalizedStatus.Contains(status));
                
                if (!canAmend)
                {
                    return new ApiResponseDto<LicenseRenewalDto>
                    {
                        Success = false,
                        Message = $"Renewals with status '{renewal.Status}' cannot be amended. Only renewals that are Returned, Under Review, Submitted, or Approved can be amended."
                    };
                }

                // Update renewal period
                renewal.RenewalYears = request.RenewalYears;
                renewal.LicenseRenewalFrom = request.LicenseRenewalFrom;
                renewal.LicenseRenewalTo = request.LicenseRenewalTo;

                // Update factory information
                renewal.FactoryName = request.FactoryName;
                renewal.PlotNumber = request.PlotNumber;
                renewal.StreetLocality = request.StreetLocality;
                renewal.CityTown = request.CityTown;
                renewal.District = request.District;
                renewal.Area = request.Area;
                renewal.Pincode = request.Pincode;
                renewal.Mobile = request.Mobile;
                renewal.Email = request.Email;

                // Update manufacturing details
                renewal.ManufacturingProcess = request.ManufacturingProcess;
                renewal.ProductionStartDate = request.ProductionStartDate;
                renewal.ManufacturingProcessLast12Months = request.ManufacturingProcessLast12Months;
                renewal.ManufacturingProcessNext12Months = request.ManufacturingProcessNext12Months;
                renewal.ProductDetailsLast12Months = request.ProductDetailsLast12Months;

                // Update workers
                renewal.MaxWorkersMaleProposed = request.MaxWorkersMaleProposed;
                renewal.MaxWorkersFemaleProposed = request.MaxWorkersFemaleProposed;
                renewal.MaxWorkersTransgenderProposed = request.MaxWorkersTransgenderProposed;
                renewal.MaxWorkersMaleEmployed = request.MaxWorkersMaleEmployed;
                renewal.MaxWorkersFemaleEmployed = request.MaxWorkersFemaleEmployed;
                renewal.MaxWorkersTransgenderEmployed = request.MaxWorkersTransgenderEmployed;
                renewal.WorkersMaleOrdinary = request.WorkersMaleOrdinary;
                renewal.WorkersFemaleOrdinary = request.WorkersFemaleOrdinary;
                renewal.WorkersTransgenderOrdinary = request.WorkersTransgenderOrdinary;

                // Update power
                renewal.TotalRatedHorsePower = request.TotalRatedHorsePower;
                renewal.MaximumPowerToBeUsed = request.MaximumPowerToBeUsed;

                // Update manager details
                renewal.FactoryManagerName = request.FactoryManagerName;
                renewal.FactoryManagerFatherName = request.FactoryManagerFatherName;
                renewal.FactoryManagerAddress = request.FactoryManagerAddress;

                // Update occupier details
                renewal.OccupierType = request.OccupierType;
                renewal.OccupierName = request.OccupierName;
                renewal.OccupierFatherName = request.OccupierFatherName;
                renewal.OccupierAddress = request.OccupierAddress;

                // Update land owner
                renewal.LandOwnerName = request.LandOwnerName;
                renewal.LandOwnerAddress = request.LandOwnerAddress;

                // Update building plan
                renewal.BuildingPlanReferenceNumber = request.BuildingPlanReferenceNumber;
                renewal.BuildingPlanApprovalDate = request.BuildingPlanApprovalDate;

                // Update waste disposal
                renewal.WasteDisposalReferenceNumber = request.WasteDisposalReferenceNumber;
                renewal.WasteDisposalApprovalDate = request.WasteDisposalApprovalDate;
                renewal.WasteDisposalAuthority = request.WasteDisposalAuthority;

                // Update declaration
                renewal.Place = request.Place;
                renewal.DeclarationDate = request.DeclarationDate;
                renewal.Declaration1Accepted = request.Declaration1Accepted;
                renewal.Declaration2Accepted = request.Declaration2Accepted;
                renewal.Declaration3Accepted = request.Declaration3Accepted;

                // Increment amendment count and save history
                renewal.AmendmentCount++;
                
                var history = new Models.ApplicationHistory
                {
                    ApplicationId = renewal.Id,
                    ApplicationType = "LicenseRenewal",
                    Action = "Amended",
                    PreviousStatus = renewal.Status,
                    NewStatus = "Under Review",
                    Comments = $"Renewal amended (Amendment #{renewal.AmendmentCount})",
                    ActionBy = "Applicant",
                    ActionByName = renewal.OccupierName,
                    ActionDate = DateTime.Now
                };
                
                _context.ApplicationHistories.Add(history);
                
                // Change status back to "Under Review"
                renewal.Status = "Under Review";
                renewal.Comments = null; // Clear previous comments
                renewal.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                // Reload with related data
                renewal = await _context.LicenseRenewals
                    .Include(r => r.Documents)
                    .FirstAsync(r => r.Id == renewal.Id);

                var districts = await LoadDistricts(new[] { renewal.District });
                var areas = await LoadAreas(new[] { renewal.Area });

                return new ApiResponseDto<LicenseRenewalDto>
                {
                    Success = true,
                    Message = "Renewal amended and resubmitted successfully",
                    Data = MapToDto(renewal, districts, areas)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error amending renewal {Id}", id);
                return new ApiResponseDto<LicenseRenewalDto>
                {
                    Success = false,
                    Message = $"Error amending renewal: {ex.Message}"
                };
            }
        }
    }
}
