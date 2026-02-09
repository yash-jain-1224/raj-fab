using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
    public class ApplicationReviewService : IApplicationReviewService
    {
        private readonly ApplicationDbContext _context;

        public ApplicationReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ApplicationSummaryDto>> GetAssignedApplicationsAsync(string userId, Guid? moduleId = null)
        {
            var applications = new List<ApplicationSummaryDto>();

            // Parse userId to Guid if possible, otherwise use empty guid
            var userGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

            // Get user area assignments with names
            var userAreaData = await _context.UserAreaAssignments
                .Where(ua => ua.UserId == userGuid)
                .Join(_context.Areas,
                    ua => ua.AreaId,
                    a => a.Id,
                    (ua, a) => new { Area = a })
                .Include(x => x.Area.District)
                .ToListAsync();

            var userAreaNames = userAreaData.Select(x => x.Area.Name).Distinct().ToList();
            var userDistrictNames = userAreaData.Select(x => x.Area.District?.Name ?? "").Where(n => !string.IsNullOrEmpty(n)).Distinct().ToList();

            //// Get Factory Map Approvals assigned to user or in user's district
            //var mapApprovals = await _context.FactoryMapApprovals
            //    .Include(f => f.Documents)
            //    .Where(f => f.AssignedTo == userId || 
            //               (userDistrictNames.Count > 0 && userDistrictNames.Contains(f.District)))
            //    .ToListAsync();

            //applications.AddRange(mapApprovals.Select(app => new ApplicationSummaryDto
            //{
            //    Id = app.Id,
            //    ApplicationNumber = app.AcknowledgementNumber,
            //    ApplicationType = "Factory Map Approval",
            //    ApplicantName = app.ApplicantName,
            //    FactoryName = app.FactoryName,
            //    Status = app.Status,
            //    CurrentStage = app.CurrentStage,
            //    SubmittedDate = app.CreatedAt,
            //    DaysPending = (DateTime.Now - app.CreatedAt).Days,
            //    AssignedTo = app.AssignedTo,
            //    AssignedToName = app.AssignedToName,
            //    HasDocuments = app.Documents.Any(),
            //    TotalDocuments = app.Documents.Count,
            //    AreaId = app.District,
            //    AreaName = app.District
            //}));

            // Get Factory Registrations assigned to user or in user's area
            var registrations = await _context.FactoryRegistrations
                .Include(f => f.Documents)
                .Where(f => f.AssignedTo == userId || 
                           (userAreaNames.Count > 0 && userAreaNames.Contains(f.Area)))
                .ToListAsync();

            applications.AddRange(registrations.Select(app => new ApplicationSummaryDto
            {
                Id = app.Id,
                ApplicationNumber = app.RegistrationNumber,
                ApplicationType = "Factory Registration",
                ApplicantName = app.FactoryName,
                FactoryName = app.FactoryName,
                Status = app.Status,
                CurrentStage = app.CurrentStage,
                SubmittedDate = app.CreatedAt,
                DaysPending = (DateTime.Now - app.CreatedAt).Days,
                AssignedTo = app.AssignedTo,
                AssignedToName = app.AssignedToName,
                HasDocuments = app.Documents.Any(),
                TotalDocuments = app.Documents.Count,
                AreaId = app.Area,
                AreaName = app.Area
            }));

            return applications.OrderByDescending(a => a.SubmittedDate).ToList();
        }

        public async Task<List<ApplicationSummaryDto>> GetApplicationsByAreaAsync(Guid areaId)
        {
            var applications = new List<ApplicationSummaryDto>();

            // Get area and district names for the specified area
            var areaData = await _context.Areas
                .Include(a => a.District)
                .FirstOrDefaultAsync(a => a.Id == areaId);

            if (areaData == null)
                return applications;

            var areaName = areaData.Name;
            var districtName = areaData.District?.Name ?? "";

            // Get Factory Map Approvals in this district
            var mapApprovals = await _context.FactoryMapApprovals
                //.Where(f => f.District == districtName)
                .ToListAsync();

            applications.AddRange(mapApprovals.Select(app => new ApplicationSummaryDto
            {
                Id = app.Id,
                ApplicationNumber = app.AcknowledgementNumber,
                ApplicationType = "Factory Map Approval",
                //ApplicantName = app.ApplicantName,
                //FactoryName = app.FactoryName,
                Status = app.Status,
                //CurrentStage = app.CurrentStage,
                SubmittedDate = app.CreatedAt,
                DaysPending = (DateTime.Now - app.CreatedAt).Days,
                //AssignedTo = app.AssignedTo,
                //AssignedToName = app.AssignedToName,
                //HasDocuments = app.Documents.Any(),
                //TotalDocuments = app.Documents.Count,
                //AreaId = app.District,
                //AreaName = app.District
            }));

            // Get Factory Registrations in this area
            var registrations = await _context.FactoryRegistrations
                .Include(f => f.Documents)
                .Where(f => f.Area == areaName)
                .ToListAsync();

            applications.AddRange(registrations.Select(app => new ApplicationSummaryDto
            {
                Id = app.Id,
                ApplicationNumber = app.RegistrationNumber,
                ApplicationType = "Factory Registration",
                ApplicantName = app.FactoryName,
                FactoryName = app.FactoryName,
                Status = app.Status,
                CurrentStage = app.CurrentStage,
                SubmittedDate = app.CreatedAt,
                DaysPending = (DateTime.Now - app.CreatedAt).Days,
                AssignedTo = app.AssignedTo,
                AssignedToName = app.AssignedToName,
                HasDocuments = app.Documents.Any(),
                TotalDocuments = app.Documents.Count,
                AreaId = app.Area,
                AreaName = app.Area
            }));

            return applications.OrderByDescending(a => a.SubmittedDate).ToList();
        }

        public async Task<List<ApplicationSummaryDto>> GetAllApplicationsAsync()
        {
            var applications = new List<ApplicationSummaryDto>();

            // Get all Factory Map Approvals
            var mapApprovals = await _context.FactoryMapApprovals
                //.Include(f => f.Documents)
                .ToListAsync();

            applications.AddRange(mapApprovals.Select(app => new ApplicationSummaryDto
            {
                Id = app.Id,
                ApplicationNumber = app.AcknowledgementNumber,
                ApplicationType = "Factory Map Approval",
                //ApplicantName = app.ApplicantName,
                //FactoryName = app.FactoryName,
                Status = app.Status,
                //CurrentStage = app.CurrentStage,
                SubmittedDate = app.CreatedAt,
                DaysPending = (DateTime.Now - app.CreatedAt).Days,
                //AssignedTo = app.AssignedTo,
                //AssignedToName = app.AssignedToName,
                //HasDocuments = app.Documents.Any(),
                //TotalDocuments = app.Documents.Count,
                //AreaId = app.District,
                //AreaName = app.District
            }));

            // Get all Factory Registrations
            var registrations = await _context.FactoryRegistrations
                .Include(f => f.Documents)
                .ToListAsync();

            applications.AddRange(registrations.Select(app => new ApplicationSummaryDto
            {
                Id = app.Id,
                ApplicationNumber = app.RegistrationNumber,
                ApplicationType = "Factory Registration",
                ApplicantName = app.FactoryName,
                FactoryName = app.FactoryName,
                Status = app.Status,
                CurrentStage = app.CurrentStage,
                SubmittedDate = app.CreatedAt,
                DaysPending = (DateTime.Now - app.CreatedAt).Days,
                AssignedTo = app.AssignedTo,
                AssignedToName = app.AssignedToName,
                HasDocuments = app.Documents.Any(),
                TotalDocuments = app.Documents.Count,
                AreaId = app.Area,
                AreaName = app.Area
            }));

            return applications.OrderByDescending(a => a.SubmittedDate).ToList();
        }

        public async Task<ApplicationDetailDto?> GetApplicationDetailAsync(string applicationType, string applicationId, string userId)
        {
            // Parse userId to Guid if possible
            var userGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

            // Get user permissions
            var userModulePermissions = await _context.UserModulePermissions
                .Where(ump => ump.UserId == userGuid)
                .ToListAsync();

            var userPermissions = userModulePermissions
                .SelectMany(ump => System.Text.Json.JsonSerializer.Deserialize<string[]>(ump.Permissions) ?? Array.Empty<string>())
                .Distinct()
                .ToList();

            if (applicationType.ToLower().Contains("map"))
            {
                var application = await _context.FactoryMapApprovals
                    .Include(f => f.RawMaterials)
                    .Include(f => f.IntermediateProducts)
                    .FirstOrDefaultAsync(f => f.Id == applicationId);

                if (application == null) return null;

                var history = await GetApplicationHistoryAsync(applicationType, applicationId);

                // Map to clean object without circular references
                //var cleanApplicationData = new
                //{
                //    application.Id,
                //    application.AcknowledgementNumber,
                //    application.FactoryName,
                //    application.ApplicantName,
                //    application.Email,
                //    application.MobileNo,
                //    application.Address,
                //    application.District,
                //    application.Pincode,
                //    application.FactoryTypeId,
                //    application.PlotArea,
                //    application.BuildingArea,
                //    application.TotalNoOfWorkersMale,
                //    application.TotalNoOfWorkersFemale,
                //    application.TotalNoOfWorkersTransgender,
                //    application.TotalWorkers,
                //    application.TotalNoOfShifts,
                //    application.ManufacturingProcessName,
                //    application.Status,
                //    application.CreatedAt,
                //    application.UpdatedAt,
                //    application.OccupierType,
                //    application.Name,
                //    application.OccupierFatherName,
                //    application.OccupierPlotNumber,
                //    application.OccupierStreetLocality,
                //    application.OccupierCityTown,
                //    application.OccupierDistrict,
                //    application.OccupierArea,
                //    application.OccupierPincode,
                //    application.OccupierMobile,
                //    application.OccupierEmail,
                //    application.CurrentStage,
                //    application.AssignedTo,
                //    application.AssignedToName,
                //    application.Comments,
                //    application.ReviewedBy,
                //    application.ReviewedAt,
                //    FactoryType = application.FactoryType?.Name,
                //    Documents = application.Documents.Select(d => new DocumentDto
                //    {
                //        Id = d.Id,
                //        DocumentType = d.DocumentType,
                //        FileName = d.FileName,
                //        FilePath = d.FilePath,
                //        FileSize = d.FileSize ?? "0",
                //        FileExtension = d.FileExtension,
                //        UploadedAt = d.UploadedAt
                //    }).ToList(),
                //    RawMaterials = application.RawMaterials.Select(r => new
                //    {
                //        r.Id,
                //        r.MaterialName,
                //        r.CASNumber,
                //        r.QuantityPerDay,
                //        r.Unit,
                //        r.StorageMethod,
                //        r.Remarks
                //    }).ToList(),
                //    IntermediateProducts = application.IntermediateProducts.Select(p => new
                //    {
                //        p.Id,
                //        p.ProductName,
                //        p.QuantityPerDay,
                //        p.Unit,
                //        p.ProcessStage,
                //        p.Remarks
                //    }).ToList()
                //};

                return new ApplicationDetailDto
                {
                    ApplicationType = "Factory Map Approval",
                    //ApplicationData = cleanApplicationData,
                    History = history,
                    AvailableActions = userPermissions
                };
            }
            else
            {
                var application = await _context.FactoryRegistrations
                    .Include(f => f.Documents)
                    .FirstOrDefaultAsync(f => f.Id == applicationId);

                if (application == null) return null;

                var history = await GetApplicationHistoryAsync(applicationType, applicationId);

                // Map to clean object without circular references
                var cleanApplicationData = new
                {
                    application.Id,
                    application.RegistrationNumber,
                    application.MapApprovalAcknowledgementNumber,
                    application.LicenseFromDate,
                    application.LicenseToDate,
                    application.LicenseYears,
                    application.FactoryName,
                    application.FactoryRegistrationNumber,
                    application.PlotNumber,
                    application.StreetLocality,
                    application.District,
                    application.CityTown,
                    application.Area,
                    application.Pincode,
                    application.Mobile,
                    application.Email,
                    application.ManufacturingProcess,
                    application.ProductionStartDate,
                    application.ManufacturingProcessLast12Months,
                    application.ManufacturingProcessNext12Months,
                    application.MaxWorkersMaleProposed,
                    application.MaxWorkersFemaleProposed,
                    application.MaxWorkersTransgenderProposed,
                    application.MaxWorkersMaleEmployed,
                    application.MaxWorkersFemaleEmployed,
                    application.MaxWorkersTransgenderEmployed,
                    application.WorkersMaleOrdinary,
                    application.WorkersFemaleOrdinary,
                    application.WorkersTransgenderOrdinary,
                    application.TotalRatedHorsePower,
                    application.PowerUnit,
                    application.KNumber,
                    application.FactoryManagerName,
                    application.FactoryManagerFatherName,
                    application.FactoryManagerPlotNumber,
                    application.FactoryManagerStreetLocality,
                    application.FactoryManagerDistrict,
                    application.FactoryManagerArea,
                    application.FactoryManagerCityTown,
                    application.FactoryManagerPincode,
                    application.FactoryManagerMobile,
                    application.FactoryManagerEmail,
                    application.OccupierType,
                    application.OccupierName,
                    application.OccupierFatherName,
                    application.OccupierPlotNumber,
                    application.OccupierStreetLocality,
                    application.OccupierCityTown,
                    application.OccupierDistrict,
                    application.OccupierArea,
                    application.OccupierPincode,
                    application.OccupierMobile,
                    application.OccupierEmail,
                    application.LandOwnerName,
                    application.LandOwnerPlotNumber,
                    application.LandOwnerStreetLocality,
                    application.LandOwnerDistrict,
                    application.LandOwnerArea,
                    application.LandOwnerCityTown,
                    application.LandOwnerPincode,
                    application.LandOwnerMobile,
                    application.LandOwnerEmail,
                    application.BuildingPlanReferenceNumber,
                    application.BuildingPlanApprovalDate,
                    application.WasteDisposalReferenceNumber,
                    application.WasteDisposalApprovalDate,
                    application.WasteDisposalAuthority,
                    application.WantToMakePaymentNow,
                    application.DeclarationAccepted,
                    application.Status,
                    application.CurrentStage,
                    application.AssignedTo,
                    application.AssignedToName,
                    application.Comments,
                    application.ReviewedBy,
                    application.ReviewedAt,
                    application.CreatedAt,
                    application.UpdatedAt,
                    Documents = application.Documents.Select(d => new DocumentDto
                    {
                        Id = d.Id.ToString(),
                        DocumentType = d.DocumentType,
                        FileName = d.FileName,
                        FilePath = d.FilePath,
                        FileSize = d.FileSize.ToString(),
                        FileExtension = d.FileExtension,
                        UploadedAt = d.UploadedAt
                    }).ToList()
                };

                return new ApplicationDetailDto
                {
                    ApplicationType = "Factory Registration",
                    ApplicationData = cleanApplicationData,
                    History = history,
                    AvailableActions = userPermissions
                };
            }
        }

        public async Task<bool> ForwardApplicationAsync(string applicationType, string applicationId, ForwardApplicationRequest request, string userId)
        {
            // Parse userId to Guid if possible
            var userGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

            // Get forward to user details
            var forwardToUser = await _context.Users.FindAsync(Guid.Parse(request.ForwardToUserId));
            if (forwardToUser == null) return false;

            var currentUser = await _context.Users.FindAsync(userGuid);
            var currentUserName = currentUser?.FullName ?? "Unknown User";

            if (applicationType.ToLower().Contains("map"))
            {
                var application = await _context.FactoryMapApprovals.FindAsync(applicationId);
                if (application == null) return false;

                var previousStatus = application.Status;
                application.Status = "Forwarded";
                //application.CurrentStage = "Under Review";
                //application.AssignedTo = request.ForwardToUserId;
                //application.AssignedToName = forwardToUser.FullName;
                application.UpdatedAt = DateTime.Now;

                // Add history
                var history = new ApplicationHistory
                {
                    ApplicationId = applicationId,
                    ApplicationType = "FactoryMapApproval",
                    Action = "Forwarded",
                    PreviousStatus = previousStatus,
                    NewStatus = "Forwarded",
                    Comments = request.Comments,
                    ActionBy = userId,
                    ActionByName = currentUserName,
                    ForwardedTo = request.ForwardToUserId,
                    ForwardedToName = forwardToUser.FullName,
                    ActionDate = DateTime.Now
                };

                _context.ApplicationHistories.Add(history);
            }
            else
            {
                var application = await _context.FactoryRegistrations.FindAsync(applicationId);
                if (application == null) return false;

                var previousStatus = application.Status;
                application.Status = "Forwarded";
                application.CurrentStage = "Under Review";
                application.AssignedTo = request.ForwardToUserId;
                application.AssignedToName = forwardToUser.FullName;
                application.UpdatedAt = DateTime.Now;

                // Add history
                var history = new ApplicationHistory
                {
                    ApplicationId = applicationId,
                    ApplicationType = "FactoryRegistration",
                    Action = "Forwarded",
                    PreviousStatus = previousStatus,
                    NewStatus = "Forwarded",
                    Comments = request.Comments,
                    ActionBy = userId,
                    ActionByName = currentUserName,
                    ForwardedTo = request.ForwardToUserId,
                    ForwardedToName = forwardToUser.FullName,
                    ActionDate = DateTime.Now
                };

                _context.ApplicationHistories.Add(history);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddRemarkAsync(string applicationType, string applicationId, AddRemarkRequest request, string userId)
        {
            // Parse userId to Guid if possible
            var userGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

            var currentUser = await _context.Users.FindAsync(userGuid);
            var currentUserName = currentUser?.FullName ?? "Unknown User";

            var remarkType = request.IsInternal ? "Internal Note" : "Remark";

            var history = new ApplicationHistory
            {
                ApplicationId = applicationId,
                ApplicationType = applicationType.ToLower().Contains("map") ? "FactoryMapApproval" : "FactoryRegistration",
                Action = "Remarked",
                PreviousStatus = null,
                NewStatus = "Remark Added",
                Comments = $"[{remarkType}] {request.Remark}",
                ActionBy = userId,
                ActionByName = currentUserName,
                ActionDate = DateTime.Now
            };

            _context.ApplicationHistories.Add(history);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ApproveApplicationAsync(string applicationType, string applicationId, ApproveApplicationRequest request, string userId)
        {
            // Parse userId to Guid if possible
            var userGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

            var currentUser = await _context.Users.FindAsync(userGuid);
            var currentUserName = currentUser?.FullName ?? "Unknown User";

            if (applicationType.ToLower().Contains("map"))
            {
                var application = await _context.FactoryMapApprovals.FindAsync(applicationId);
                if (application == null) return false;

                var previousStatus = application.Status;
                application.Status = "Approved";
                //application.CurrentStage = "Completed";
                //application.ReviewedBy = currentUserName;
                //application.ReviewedAt = DateTime.Now;
                //application.Comments = request.ApprovalComments;
                application.UpdatedAt = DateTime.Now;

                var history = new ApplicationHistory
                {
                    ApplicationId = applicationId,
                    ApplicationType = "FactoryMapApproval",
                    Action = "Approved",
                    PreviousStatus = previousStatus,
                    NewStatus = "Approved",
                    Comments = request.ApprovalComments,
                    ActionBy = userId,
                    ActionByName = currentUserName,
                    ActionDate = DateTime.Now
                };

                _context.ApplicationHistories.Add(history);
            }
            else
            {
                var application = await _context.FactoryRegistrations.FindAsync(applicationId);
                if (application == null) return false;

                var previousStatus = application.Status;
                application.Status = "Approved";
                application.CurrentStage = "Completed";
                application.ReviewedBy = currentUserName;
                application.ReviewedAt = DateTime.Now;
                application.Comments = request.ApprovalComments;
                application.UpdatedAt = DateTime.Now;

                var history = new ApplicationHistory
                {
                    ApplicationId = applicationId,
                    ApplicationType = "FactoryRegistration",
                    Action = "Approved",
                    PreviousStatus = previousStatus,
                    NewStatus = "Approved",
                    Comments = request.ApprovalComments,
                    ActionBy = userId,
                    ActionByName = currentUserName,
                    ActionDate = DateTime.Now
                };

                _context.ApplicationHistories.Add(history);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectApplicationAsync(string applicationType, string applicationId, RejectApplicationRequest request, string userId)
        {
            // Parse userId to Guid if possible
            var userGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

            var currentUser = await _context.Users.FindAsync(userGuid);
            var currentUserName = currentUser?.FullName ?? "Unknown User";

            if (applicationType.ToLower().Contains("map"))
            {
                var application = await _context.FactoryMapApprovals.FindAsync(applicationId);
                if (application == null) return false;

                var previousStatus = application.Status;
                application.Status = "Rejected";
                //application.CurrentStage = "Rejected";
                //application.ReviewedBy = currentUserName;
                //application.ReviewedAt = DateTime.Now;
                //application.Comments = request.RejectionReason;
                application.UpdatedAt = DateTime.Now;

                var history = new ApplicationHistory
                {
                    ApplicationId = applicationId,
                    ApplicationType = "FactoryMapApproval",
                    Action = "Rejected",
                    PreviousStatus = previousStatus,
                    NewStatus = "Rejected",
                    Comments = request.RejectionReason,
                    ActionBy = userId,
                    ActionByName = currentUserName,
                    ActionDate = DateTime.Now
                };

                _context.ApplicationHistories.Add(history);
            }
            else
            {
                var application = await _context.FactoryRegistrations.FindAsync(applicationId);
                if (application == null) return false;

                var previousStatus = application.Status;
                application.Status = "Rejected";
                application.CurrentStage = "Rejected";
                application.ReviewedBy = currentUserName;
                application.ReviewedAt = DateTime.Now;
                application.Comments = request.RejectionReason;
                application.UpdatedAt = DateTime.Now;

                var history = new ApplicationHistory
                {
                    ApplicationId = applicationId,
                    ApplicationType = "FactoryRegistration",
                    Action = "Rejected",
                    PreviousStatus = previousStatus,
                    NewStatus = "Rejected",
                    Comments = request.RejectionReason,
                    ActionBy = userId,
                    ActionByName = currentUserName,
                    ActionDate = DateTime.Now
                };

                _context.ApplicationHistories.Add(history);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReturnToApplicantAsync(string applicationType, string applicationId, ReturnApplicationRequest request, string userId)
        {
            // Parse userId to Guid if possible
            var userGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

            var currentUser = await _context.Users.FindAsync(userGuid);
            var currentUserName = currentUser?.FullName ?? "Unknown User";

            var correctionsText = request.RequiredCorrections.Any() 
                ? $"\n\nRequired Corrections:\n- {string.Join("\n- ", request.RequiredCorrections)}"
                : "";

            var fullComments = request.Reason + correctionsText;

            if (applicationType.ToLower().Contains("map"))
            {
                var application = await _context.FactoryMapApprovals.FindAsync(applicationId);
                if (application == null) return false;

                var previousStatus = application.Status;
                application.Status = "Returned";
                //application.CurrentStage = "Requires Corrections";
                //application.Comments = fullComments;
                application.UpdatedAt = DateTime.Now;

                var history = new ApplicationHistory
                {
                    ApplicationId = applicationId,
                    ApplicationType = "FactoryMapApproval",
                    Action = "Returned to Applicant",
                    PreviousStatus = previousStatus,
                    NewStatus = "Returned",
                    Comments = fullComments,
                    ActionBy = userId,
                    ActionByName = currentUserName,
                    ActionDate = DateTime.Now
                };

                _context.ApplicationHistories.Add(history);
            }
            else
            {
                var application = await _context.FactoryRegistrations.FindAsync(applicationId);
                if (application == null) return false;

                var previousStatus = application.Status;
                application.Status = "Returned";
                application.CurrentStage = "Requires Corrections";
                application.Comments = fullComments;
                application.UpdatedAt = DateTime.Now;

                var history = new ApplicationHistory
                {
                    ApplicationId = applicationId,
                    ApplicationType = "FactoryRegistration",
                    Action = "Returned to Applicant",
                    PreviousStatus = previousStatus,
                    NewStatus = "Returned",
                    Comments = fullComments,
                    ActionBy = userId,
                    ActionByName = currentUserName,
                    ActionDate = DateTime.Now
                };

                _context.ApplicationHistories.Add(history);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ApplicationHistoryDto>> GetApplicationHistoryAsync(string applicationType, string applicationId)
        {
            var history = await _context.ApplicationHistories
                .Where(h => h.ApplicationId == applicationId)
                .OrderByDescending(h => h.ActionDate)
                .Select(h => new ApplicationHistoryDto
                {
                    Id = h.Id,
                    Action = h.Action,
                    PreviousStatus = h.PreviousStatus,
                    NewStatus = h.NewStatus,
                    Comments = h.Comments,
                    ActionByName = h.ActionByName,
                    ForwardedToName = h.ForwardedToName,
                    ActionDate = h.ActionDate
                })
                .ToListAsync();

            return history;
        }

        public async Task<List<EligibleReviewerDto>> GetEligibleReviewersAsync(string applicationType, string applicationId)
        {
            string? districtName = null;
            string? areaName = null;

            // Get application location
            if (applicationType.ToLower().Contains("map"))
            {
                var application = await _context.FactoryMapApprovals
                    .Where(f => f.Id == applicationId)
                    //.Select(f => new { f.District })
                    .FirstOrDefaultAsync();

                if (application == null) return new List<EligibleReviewerDto>();
                //districtName = application.District;
            }
            else
            {
                var application = await _context.FactoryRegistrations
                    .Where(f => f.Id == applicationId)
                    .Select(f => new { f.District, f.Area })
                    .FirstOrDefaultAsync();

                if (application == null) return new List<EligibleReviewerDto>();
                districtName = application.District;
                areaName = application.Area;
            }

            // Find eligible reviewers based on location assignment
          var query = _context.UserAreaAssignments
                        .Include(uaa => uaa.User)
                            .ThenInclude(u => u.UserRoles)
                                .ThenInclude(ur => ur.Role)
                                    .ThenInclude(r => r.Post)
                        .Include(uaa => uaa.Area)
                            .ThenInclude(a => a.District)
                        .Where(uaa => uaa.User.IsActive);

            // Filter by district name
            if (!string.IsNullOrEmpty(districtName))
            {
                query = query.Where(uaa => uaa.Area.District != null && uaa.Area.District.Name == districtName);
            }

            // Filter by area name if available (for registrations)
            if (!string.IsNullOrEmpty(areaName))
            {
                query = query.Where(uaa => uaa.Area.Name == areaName);
            }

            var eligibleReviewers = await query
                .Select(uaa => new EligibleReviewerDto
                {
                    UserId = uaa.UserId,
                    FullName = uaa.User.FullName,
                    Username = uaa.User.Username,
                    Email = uaa.User.Email,
                    RoleName = uaa.User.UserRoles.Select(ur => ur.Role.Post.Name).FirstOrDefault() ?? "Unknown",
                    DistrictName = uaa.Area.District != null ? uaa.Area.District.Name : null,
                    AreaName = uaa.Area.Name
                })
                .Distinct()
                .OrderBy(r => r.FullName)
                .ToListAsync();

            return eligibleReviewers;
        }
    }
}
