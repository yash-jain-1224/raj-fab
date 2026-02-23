using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
namespace RajFabAPI.Services
{
    public partial class FactoryRegistrationService
    {
        public async Task<ApiResponseDto<FactoryRegistrationDto>> AmendRegistrationAsync(string id, CreateFactoryRegistrationRequest request)
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

                // Allow amendments for registrations that are:
                // 1. Returned to Applicant (for corrections)
                // 2. Under Review (before final decision)
                // 3. Submitted (before review starts)
                // 4. Approved (post-approval amendments)
                var normalizedStatus = registration.Status?.ToLower().Trim() ?? "";
                var allowedStatuses = new[] { "return", "under review", "submitted", "approved" };
                bool canAmend = allowedStatuses.Any(status => normalizedStatus.Contains(status));
                
                if (!canAmend)
                {
                    return new ApiResponseDto<FactoryRegistrationDto>
                    {
                        Success = false,
                        Message = $"Registrations with status '{registration.Status}' cannot be amended. Only registrations that are Returned, Under Review, Submitted, or Approved can be amended.",
                        Data = null
                    };
                }

                // Update all fields with new data
                registration.MapApprovalAcknowledgementNumber = request.MapApprovalAcknowledgementNumber;
                
                // Period of License
                registration.LicenseFromDate = request.LicenseFromDate;
                registration.LicenseToDate = request.LicenseToDate;
                registration.LicenseYears = request.LicenseYears;
                
                // General Information
                registration.FactoryName = request.FactoryName;
                registration.FactoryRegistrationNumber = request.FactoryRegistrationNumber;
                
                // Factory Address and Contact Information
                registration.PlotNumber = request.PlotNumber;
                registration.StreetLocality = request.StreetLocality;
                registration.District = request.District;
                registration.CityTown = request.CityTown;
                registration.Area = request.Area;
                registration.Pincode = request.Pincode;
                registration.Mobile = request.Mobile;
                registration.Email = request.Email;
                
                // Nature of manufacturing process
                registration.ManufacturingProcess = request.ManufacturingProcess;
                registration.ProductionStartDate = request.ProductionStartDate;
                registration.ManufacturingProcessLast12Months = request.ManufacturingProcessLast12Months;
                registration.ManufacturingProcessNext12Months = request.ManufacturingProcessNext12Months;
                
                // Workers Employed
                registration.MaxWorkersMaleProposed = request.MaxWorkersMaleProposed;
                registration.MaxWorkersFemaleProposed = request.MaxWorkersFemaleProposed;
                registration.MaxWorkersTransgenderProposed = request.MaxWorkersTransgenderProposed;
                registration.MaxWorkersMaleEmployed = request.MaxWorkersMaleEmployed;
                registration.MaxWorkersFemaleEmployed = request.MaxWorkersFemaleEmployed;
                registration.MaxWorkersTransgenderEmployed = request.MaxWorkersTransgenderEmployed;
                registration.WorkersMaleOrdinary = request.WorkersMaleOrdinary;
                registration.WorkersFemaleOrdinary = request.WorkersFemaleOrdinary;
                registration.WorkersTransgenderOrdinary = request.WorkersTransgenderOrdinary;
                
                // Power Installed
                registration.TotalRatedHorsePower = request.TotalRatedHorsePower;
                registration.PowerUnit = request.PowerUnit;
                registration.KNumber = request.KNumber;
                
                // Particulars of Factory Manager
                registration.FactoryManagerName = request.FactoryManagerName;
                registration.FactoryManagerFatherName = request.FactoryManagerFatherName;
                registration.FactoryManagerPlotNumber = request.FactoryManagerPlotNumber;
                registration.FactoryManagerStreetLocality = request.FactoryManagerStreetLocality;
                registration.FactoryManagerDistrict = request.FactoryManagerDistrict;
                registration.FactoryManagerArea = request.FactoryManagerArea;
                registration.FactoryManagerCityTown = request.FactoryManagerCityTown;
                registration.FactoryManagerPincode = request.FactoryManagerPincode;
                registration.FactoryManagerMobile = request.FactoryManagerMobile;
                registration.FactoryManagerEmail = request.FactoryManagerEmail;
                registration.FactoryManagerPanCard = request.FactoryManagerPanCard;
                
                // Particulars of Occupier
                registration.OccupierType = request.OccupierType;
                registration.OccupierName = request.OccupierName;
                registration.OccupierFatherName = request.OccupierFatherName;
                registration.OccupierPlotNumber = request.OccupierPlotNumber;
                registration.OccupierStreetLocality = request.OccupierStreetLocality;
                registration.OccupierCityTown = request.OccupierCityTown;
                registration.OccupierDistrict = request.OccupierDistrict;
                registration.OccupierArea = request.OccupierArea;
                registration.OccupierPincode = request.OccupierPincode;
                registration.OccupierMobile = request.OccupierMobile;
                registration.OccupierEmail = request.OccupierEmail;
                registration.OccupierPanCard = request.OccupierPanCard;
                
                // Land and Building
                registration.LandOwnerName = request.LandOwnerName;
                registration.LandOwnerPlotNumber = request.LandOwnerPlotNumber;
                registration.LandOwnerStreetLocality = request.LandOwnerStreetLocality;
                registration.LandOwnerDistrict = request.LandOwnerDistrict;
                registration.LandOwnerArea = request.LandOwnerArea;
                registration.LandOwnerCityTown = request.LandOwnerCityTown;
                registration.LandOwnerPincode = request.LandOwnerPincode;
                registration.LandOwnerMobile = request.LandOwnerMobile;
                registration.LandOwnerEmail = request.LandOwnerEmail;
                
                // Building Plan Approval
                registration.BuildingPlanReferenceNumber = request.BuildingPlanReferenceNumber;
                registration.BuildingPlanApprovalDate = request.BuildingPlanApprovalDate;
                
                // Disposal of wastes and effluents
                registration.WasteDisposalReferenceNumber = request.WasteDisposalReferenceNumber;
                registration.WasteDisposalApprovalDate = request.WasteDisposalApprovalDate;
                registration.WasteDisposalAuthority = request.WasteDisposalAuthority;
                
                // Payment
                registration.WantToMakePaymentNow = request.WantToMakePaymentNow;
                
                // Declaration
                registration.DeclarationAccepted = request.DeclarationAccepted;

                // Increment amendment count and save history
                registration.AmendmentCount++;
                
                var history = new Models.ApplicationHistory
                {
                    ApplicationId = registration.Id,
                    ApplicationType = "FactoryRegistration",
                    Action = "Amended",
                    PreviousStatus = registration.Status,
                    NewStatus = "Under Review",
                    Comments = $"Application amended (Amendment #{registration.AmendmentCount})",
                    ActionBy = "Applicant",
                    ActionByName = registration.OccupierName,
                    ActionDate = DateTime.Now
                };
                
                _context.ApplicationHistories.Add(history);

                // Change status back to "Under Review"
                registration.Status = "Under Review";
                registration.Comments = null; // Clear previous comments
                registration.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                var districts = await LoadDistricts(new[] { registration.District, registration.FactoryManagerDistrict, registration.OccupierDistrict, registration.LandOwnerDistrict });
                var areas = await LoadAreas(new[] { registration.Area, registration.FactoryManagerArea, registration.OccupierArea, registration.LandOwnerArea });

                return new ApiResponseDto<FactoryRegistrationDto>
                {
                    Success = true,
                    Message = "Factory registration amended and resubmitted successfully",
                    Data = MapToDto(registration, districts, areas)
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<FactoryRegistrationDto>
                {
                    Success = false,
                    Message = $"Error amending factory registration: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}
