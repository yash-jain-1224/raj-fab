using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RajFabAPI.Models;
using RajFabAPI.Models.FactoryModels;
using System.Data;
using System.Text.Json;
using RajFabAPI.Models.BoilerModels;
using static RajFabAPI.Services.EstablishmentRegistrationService;

namespace RajFabAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<FormModule> Modules { get; set; }
        public DbSet<DynamicForm> Forms { get; set; }
        public DbSet<FormSubmission> Submissions { get; set; }
        public DbSet<FormSection> FormSections { get; set; }
        public DbSet<WorkflowConfig> WorkflowConfigs { get; set; }
        public DbSet<RoleLocationAssignment> RoleLocationAssignments { get; set; }
        public DbSet<Master> Masters { get; set; }
        public DbSet<EstablishmentRegistration> EstablishmentRegistrations { get; set; }
        public DbSet<EstablishmentUserDetail> EstablishmentUserDetails { get; set; }
        public DbSet<FactoryDetail> FactoryDetails { get; set; }
        public DbSet<AudioVisualWork> AudioVisualWorks { get; set; }
        public DbSet<NewsPaperEstablishment> NewsPaperEstablishments { get; set; }
        public DbSet<BuildingAndConstructionWork> BuildingAndConstructionWorks { get; set; }
        public DbSet<MotorTransportService> MotorTransportServices { get; set; }
        public DbSet<BeediCigarWork> BeediCigarWorks { get; set; }
        public DbSet<PersonDetail> PersonDetails { get; set; }
        public DbSet<EstablishmentEntityMapping> EstablishmentEntityMapping { get; set; }
        public DbSet<FactoryContractorMapping> FactoryContractorMapping { get; set; }
        public DbSet<ApplicationApprovalRequest> ApplicationApprovalRequests { get; set; }
        public DbSet<ApplicationRegistration> ApplicationRegistrations { get; set; }
        public DbSet<Certificate> Certificates { get; set; }


        public DbSet<CommencementCessationApplication> CommencementCessationApplication { get; set; }

        // Factory Related
        public DbSet<EstablishmentDetail> EstablishmentDetails { get; set; }
        public DbSet<EstablishmentRegistrationDocument> EstablishmentRegistrationDocuments { get; set; }
        public DbSet<Plantation> Plantations { get; set; }
        public DbSet<ContractorDetail> ContractorDetails { get; set; }

        // Location Masters
        public DbSet<Division> Divisions { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<DocumentUpload> DocumentUploads { get; set; }
        public DbSet<Area> Areas { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Tehsil> Tehsils { get; set; }
        public DbSet<Office> Offices { get; set; }
        public DbSet<PoliceStation> PoliceStations { get; set; }
        public DbSet<RailwayStation> RailwayStations { get; set; }
        public DbSet<Act> Acts { get; set; }
        public DbSet<Models.Rule> Rules { get; set; }

        // User Management
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserHierarchy> UserHierarchies { get; set; }
        public DbSet<UserLocationAssignment> UserLocationAssignments { get; set; }
        public DbSet<Privilege> Privileges { get; set; }
        public DbSet<RolePrivilege> RolePrivileges { get; set; }

        // Enhanced Privilege System
        public DbSet<ModulePermission> ModulePermissions { get; set; }
        public DbSet<UserModulePermission> UserModulePermissions { get; set; }
        public DbSet<UserAreaAssignment> UserAreaAssignments { get; set; }

        // Factory Type related entities
        public DbSet<FactoryTypeOld> FactoryTypes_Old { get; set; }
        public DbSet<FactoryTypeDocument> FactoryTypeDocuments { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }
        public DbSet<ManufacturingProcessType> ManufacturingProcessTypes { get; set; }
        public DbSet<ProcessDocument> ProcessDocuments { get; set; }

        // Occupier Master
        public DbSet<Occupier> Occupiers { get; set; }

        // Factory Applications
        public DbSet<FactoryMapApproval> FactoryMapApprovals { get; set; }
        public DbSet<MapApprovalFactoryDetail> MapApprovalFactoryDetails { get; set; }
        public DbSet<MapApprovalOccupierDetail> MapApprovalOccupierDetails { get; set; }
        public DbSet<FactoryMapDocument> FactoryMapDocuments { get; set; }
        public DbSet<FactoryMapRawMaterial> FactoryMapRawMaterials { get; set; }
        public DbSet<FactoryMapIntermediateProduct> FactoryMapIntermediateProducts { get; set; }
        public DbSet<FactoryMapFinishGood> FactoryMapFinishGoods { get; set; }
        public DbSet<FactoryMapDangerousOperation> FactoryMapDangerousOperations { get; set; }
        public DbSet<FactoryMapApprovalChemical> FactoryMapApprovalChemicals { get; set; }
        public DbSet<FactoryRegistration> FactoryRegistrations { get; set; }
        public DbSet<FactoryRegistrationDocument> FactoryRegistrationDocuments { get; set; }
        public DbSet<LicenseRenewal> LicenseRenewals { get; set; }
        public DbSet<LicenseRenewalDocument> LicenseRenewalDocuments { get; set; }
        public DbSet<FactoryClosure> FactoryClosures { get; set; }
        public DbSet<FactoryClosureDocument> FactoryClosureDocuments { get; set; }
        public DbSet<ManagerChange> ManagerChanges { get; set; }
        public DbSet<AnnualReturn> AnnualReturns { get; set; }
        public DbSet<Appeal> Appeals { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<ApplicationHistory> ApplicationHistories { get; set; }

        // Boiler entities
        public DbSet<RegisteredBoiler> RegisteredBoilers { get; set; }
        public DbSet<BoilerSpecifications> BoilerSpecifications { get; set; }
        public DbSet<BoilerLocation> BoilerLocations { get; set; }
        public DbSet<BoilerSafetyFeatures> BoilerSafetyFeatures { get; set; }
        public DbSet<BoilerCertificate> BoilerCertificates { get; set; }
        public DbSet<BoilerInspectionHistory> BoilerInspectionHistories { get; set; }
        public DbSet<BoilerApplication> BoilerApplications { get; set; }
        public DbSet<BoilerDocumentType> BoilerDocumentTypes { get; set; }

        public DbSet<BoilerDetail> BoilerDetails { get; set; }
        public DbSet<BoilerRegistration> BoilerRegistrations { get; set; }
        public DbSet<RegisteredBoilerNew> RegisteredBoilerNews { get; set; }
        public DbSet<SteamPipeLineApplication> SteamPipeLineApplications { get; set; }
      
        // Fee Calculation entities
        public DbSet<ScheduleA_FactoryFees> ScheduleA_FactoryFees { get; set; }
        public DbSet<ScheduleB_ElectricityFees> ScheduleB_ElectricityFees { get; set; }
        public DbSet<FactoryRegistrationFee> FactoryRegistrationFees { get; set; }
        public DbSet<NonHazardousFactoryRegistration> NonHazardousFactoryRegistrations { get; set; }
        public DbSet<FactoryType> FactoryTypes { get; set; } = null!;
        public DbSet<FactoryCategory> FactoryCategories { get; set; } = null!;
        public DbSet<RoleInspectionPrivilege> RoleInspectionPrivileges { get; set; } = null!;
        public DbSet<OfficeLevel> OfficeLevels { get; set; } = null!;
        public DbSet<OfficePostLevel> OfficePostLevels { get; set; } = null!;
        public DbSet<Post> Posts { get; set; } = null!;
        public DbSet<ApplicationWorkFlow> ApplicationWorkFlows { get; set; } = null!;
        public DbSet<ApplicationWorkFlowLevel> ApplicationWorkFlowLevels { get; set; } = null!;
        public DbSet<OfficeApplicationArea> OfficeApplicationAreas { get; set; } = null!;
        public DbSet<OfficeInspectionArea> OfficeInspectionAreas { get; set; } = null!;
        public DbSet<WorkerRange> WorkerRanges { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;
        public DbSet<FactoryLicense> FactoryLicenses { get; set; }
        public DbSet<ESignTransaction> ESignTransactions { get; set; }
        public DbSet<BoilerClosure> BoilerClosures { get; set; }

        public DbSet<BoilerRepairModification> BoilerRepairModifications { get; set; }
        public DbSet<BoilerManufactureRegistration> BoilerManufactureRegistrations { get; set; }

        public DbSet<DesignFacility> DesignFacilities { get; set; }
        public DbSet<TestingFacility> TestingFacilities { get; set; }
        public DbSet<RDFacility> RDFacilities { get; set; }
        public DbSet<NDTPersonnel> NDTPersonnels { get; set; }
        public DbSet<QualifiedWelder> QualifiedWelders { get; set; }
        public DbSet<TechnicalManpower> TechnicalManpowers { get; set; }
        public DbSet<BoilerManufactureClosure> BoilerManufactureClosures { get; set; }
        public DbSet<BoilerRepairerRegistration> BoilerRepairerRegistrations { get; set; }
        public DbSet<BoilerRepairerEngineer> BoilerRepairerEngineers { get; set; }
        public DbSet<BoilerRepairerWelder> BoilerRepairerWelders { get; set; }
        public DbSet<BoilerRepairerClosure> BoilerRepairerClosures { get; set; }
        public DbSet<SteamPipeLineClosure> SteamPipeLineClosures { get; set; }
        public DbSet<EconomiserRegistration> EconomiserRegistrations { get; set; }
        public DbSet<EconomiserClosure> EconomiserClosures { get; set; }
        public DbSet<WelderApplication> WelderApplications { get; set; }

        public DbSet<WelderDetail> WelderDetails { get; set; }

        public DbSet<WelderEmployer> WelderEmployers { get; set; }
        public DbSet<WelderClosure> WelderClosures { get; set; }


        public DbSet<FeeResult> FeeResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Value converters for string<->Guid to align with DB uniqueidentifier columns
            var stringToGuid = new ValueConverter<string, Guid>(v => Guid.Parse(v), v => v.ToString());
            modelBuilder.Entity<User>()
                    .ToTable(tb => tb.UseSqlOutputClause(false));
            // Configure FormModule
            // modelBuilder.Entity<FormModule>(entity =>
            // {
            //     entity.HasKey(e => e.Id);
            //     entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            //     entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
            //     entity.Property(e => e.Description).HasMaxLength(500);
            //     entity.HasIndex(e => e.Category);
            // });

            // Configure DynamicForm
            modelBuilder.Entity<DynamicForm>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.FieldsJson).HasColumnType("nvarchar(max)");

                entity.HasOne(e => e.Module)
                      .WithMany(m => m.Forms)
                      .HasForeignKey(e => e.ModuleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure FormSection
            modelBuilder.Entity<FormSection>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.HasOne(e => e.Form)
                      .WithMany(f => f.Sections)
                      .HasForeignKey(e => e.FormId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure WorkflowConfig
            modelBuilder.Entity<WorkflowConfig>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OnSubmitApiEndpoint).HasMaxLength(500);
                entity.Property(e => e.OnSubmitMethod).HasMaxLength(10);
                entity.Property(e => e.OnSubmitNotificationEmail).HasMaxLength(200);
                entity.Property(e => e.OnSubmitRedirectUrl).HasMaxLength(500);
                entity.Property(e => e.OnSubmitCustomActions).HasColumnType("nvarchar(max)");
                entity.Property(e => e.OnApprovalApiEndpoint).HasMaxLength(500);
                entity.Property(e => e.OnApprovalNotificationEmail).HasMaxLength(200);
                entity.Property(e => e.OnApprovalCustomActions).HasColumnType("nvarchar(max)");

                entity.HasOne(e => e.Form)
                      .WithOne(f => f.WorkflowConfig)
                      .HasForeignKey<WorkflowConfig>(e => e.FormId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure FormSubmission
            modelBuilder.Entity<FormSubmission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.DataJson).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.Comments).HasMaxLength(1000);

                entity.HasOne(e => e.Form)
                      .WithMany(f => f.Submissions)
                      .HasForeignKey(e => e.FormId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Status);
            });

            // Configure Occupier
            modelBuilder.Entity<Occupier>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FatherName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Gender).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.MobileNo).IsRequired().HasMaxLength(15);
                entity.Property(e => e.PlotNo).IsRequired().HasMaxLength(50);
                entity.Property(e => e.StreetLocality).IsRequired().HasMaxLength(200);
                entity.Property(e => e.VillageTownCity).IsRequired().HasMaxLength(100);
                entity.Property(e => e.District).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Pincode).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Designation).HasMaxLength(100);

                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.MobileNo);
            });

            // Configure FactoryMapApproval
            modelBuilder.Entity<FactoryMapApproval>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AcknowledgementNumber).IsRequired().HasMaxLength(50);
                //entity.Property(e => e.MapApprovalFactoryDetail.FactoryName).IsRequired().HasMaxLength(200);
                //entity.Property(e => e.MapApprovalFactoryDetail.AreaId).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Status).HasMaxLength(20);
            });

            // Configure FactoryMapDocument
            modelBuilder.Entity<FactoryMapDocument>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DocumentType).IsRequired().HasMaxLength(200);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(500);
                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
                entity.Property(e => e.FileSize).HasMaxLength(100);
                entity.Property(e => e.FileExtension).HasMaxLength(50);
            });

            // Configure FactoryMapRawMaterial
            modelBuilder.Entity<FactoryMapRawMaterial>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MaterialName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.MaxStorageQuantity).HasMaxLength(100);
            });

            // Configure FactoryMapIntermediateProduct
            modelBuilder.Entity<FactoryMapIntermediateProduct>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.MaxStorageQuantity).IsRequired().HasMaxLength(50);
            });

            modelBuilder.Entity<City>()
               .HasOne(c => c.District)
               .WithMany(d => d.Cities)
               .HasForeignKey(c => c.DistrictId)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PoliceStation>()
                .HasOne(p => p.District)
                .WithMany()
                .HasForeignKey(p => p.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PoliceStation>()
                .HasOne(p => p.City)
                .WithMany()
                .HasForeignKey(p => p.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RailwayStation>()
                .HasOne(r => r.District)
                .WithMany()
                .HasForeignKey(r => r.DistrictId);

            modelBuilder.Entity<RailwayStation>()
                .HasOne(r => r.City)
                .WithMany()
                .HasForeignKey(r => r.CityId);

            // Configure Office
            modelBuilder.Entity<Office>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Pincode).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(500);

                entity.HasOne(e => e.District)
                      .WithMany()
                      .HasForeignKey(e => e.DistrictId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.City)
                      .WithMany()
                      .HasForeignKey(e => e.CityId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.Name, e.CityId });
            });

            // Configure FactoryType
            modelBuilder.Entity<FactoryTypeOld>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.HasIndex(e => e.Name);
            });

            // Configure DocumentType
            modelBuilder.Entity<DocumentType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(36).IsUnicode(false).HasColumnType("varchar(36)");
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.FileTypes).HasMaxLength(200);
                entity.Property(e => e.Module).HasMaxLength(50);
                entity.Property(e => e.ServiceType).HasMaxLength(50);
                entity.Property(e => e.ConditionalField).HasMaxLength(100);
                entity.Property(e => e.ConditionalValue).HasMaxLength(100);
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => new { e.Module, e.ServiceType });
            });

            // Configure BoilerDocumentType
            modelBuilder.Entity<BoilerDocumentType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(36).IsUnicode(false).HasColumnType("varchar(36)");
                entity.Property(e => e.BoilerServiceType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ConditionalField).HasMaxLength(100);
                entity.Property(e => e.ConditionalValue).HasMaxLength(100);
                entity.Property(e => e.DocumentTypeId).HasMaxLength(36).IsUnicode(false).HasColumnType("varchar(36)");
                entity.HasOne(e => e.DocumentType)
                      .WithMany()
                      .HasForeignKey(e => e.DocumentTypeId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.BoilerServiceType);
            });

            // Configure FactoryTypeDocument
            modelBuilder.Entity<FactoryTypeDocument>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DocumentTypeId).HasMaxLength(36).IsUnicode(false).HasColumnType("varchar(36)");
                entity.HasOne(e => e.DocumentType)
                      .WithMany()
                      .HasForeignKey(e => e.DocumentTypeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ManufacturingProcessType
            modelBuilder.Entity<ManufacturingProcessType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.HasOne<FactoryTypeOld>()
                      .WithMany(f => f.AllowedProcessTypes)
                      .HasForeignKey(e => e.FactoryTypeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ProcessDocument
            modelBuilder.Entity<ProcessDocument>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ConditionalField).HasMaxLength(100);
                entity.Property(e => e.ConditionalValue).HasMaxLength(100);
                entity.Property(e => e.DocumentTypeId).HasMaxLength(36).IsUnicode(false).HasColumnType("varchar(36)");
                entity.Property(e => e.ManufacturingProcessTypeId).IsRequired();

                entity.HasOne<DocumentType>()
                      .WithMany()
                      .HasForeignKey(e => e.DocumentTypeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<ManufacturingProcessType>()
                      .WithMany(m => m.RequiredDocuments)
                      .HasForeignKey(e => e.ManufacturingProcessTypeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Division
            modelBuilder.Entity<Division>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Name);
            });

            // Configure District
            modelBuilder.Entity<District>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasOne(e => e.Division)
                      .WithMany(d => d.Districts)
                      .HasForeignKey(e => e.DivisionId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.Name);
            });

            // Configure Area
            modelBuilder.Entity<Area>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasOne(e => e.District)
                      .WithMany(d => d.Areas)
                      .HasForeignKey(e => e.DistrictId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.City)
                      .WithMany()
                      .HasForeignKey(e => e.CityId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Role
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Post)
                    .WithMany()
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Office)
                    .WithMany()
                    .HasForeignKey(e => e.OfficeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.HasIndex(e => e.Name)
                      .IsUnique();
            });


            // Configure User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Mobile)
                    .IsRequired()
                    .HasMaxLength(15);

                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(ur => ur.Id);

                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.Role)
                    .WithMany()
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();
            });


            // Configure Privilege
            modelBuilder.Entity<Privilege>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Action)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.HasOne(e => e.Module)
                      .WithMany()
                      .HasForeignKey(e => e.ModuleId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.ModuleId, e.Action })
                      .IsUnique();
            });
            modelBuilder.Entity<FormModule>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.HasIndex(e => e.Name)
                      .IsUnique();
            });

            // Configure RolePrivilege
            modelBuilder.Entity<RolePrivilege>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.PrivilegeId });
                entity.HasOne(e => e.Role)
                      .WithMany(r => r.Privileges)
                      .HasForeignKey(e => e.RoleId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Privilege)
                      .WithMany()
                      .HasForeignKey(e => e.PrivilegeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure UserHierarchy
            modelBuilder.Entity<UserHierarchy>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.ReportsTo)
                      .WithMany()
                      .HasForeignKey(e => e.ReportsToId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.EmergencyReportTo)
                      .WithMany()
                      .HasForeignKey(e => e.EmergencyReportToId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasIndex(e => e.UserId).IsUnique(); // One hierarchy per user
            });

            // Configure UserLocationAssignment
            modelBuilder.Entity<UserLocationAssignment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Role)
                      .WithMany()
                      .HasForeignKey(e => e.RoleId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.Module)
                      .WithMany()
                      .HasForeignKey(e => e.ModuleId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.Division)
                      .WithMany()
                      .HasForeignKey(e => e.DivisionId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.District)
                      .WithMany()
                      .HasForeignKey(e => e.DistrictId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.Area)
                      .WithMany()
                      .HasForeignKey(e => e.AreaId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure FactoryRegistration
            modelBuilder.Entity<FactoryRegistration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalRatedHorsePower)
                      .HasColumnType("decimal(18,2)");

                entity.HasMany(e => e.Documents)
                      .WithOne(d => d.FactoryRegistration)
                      .HasForeignKey(d => d.FactoryRegistrationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ScheduleA_FactoryFees
            modelBuilder.Entity<ScheduleA_FactoryFees>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FeeUpTo9HP).HasColumnType("decimal(18,2)");
                entity.Property(e => e.FeeUpTo20HP).HasColumnType("decimal(18,2)");
                entity.Property(e => e.FeeUpTo50HP).HasColumnType("decimal(18,2)");
                entity.Property(e => e.FeeUpTo100HP).HasColumnType("decimal(18,2)");
                entity.Property(e => e.FeeUpTo250HP).HasColumnType("decimal(18,2)");
                entity.Property(e => e.FeeUpTo500HP).HasColumnType("decimal(18,2)");
                entity.Property(e => e.FeeUpTo750HP).HasColumnType("decimal(18,2)");
                entity.Property(e => e.FeeUpTo1000HP).HasColumnType("decimal(18,2)");
                entity.Property(e => e.FeeUpTo1500HP).HasColumnType("decimal(18,2)");
                entity.Property(e => e.FeeUpTo2000HP).HasColumnType("decimal(18,2)");
                entity.Property(e => e.FeeUpTo3000HP).HasColumnType("decimal(18,2)");
            });

            // Configure ScheduleB_ElectricityFees
            modelBuilder.Entity<ScheduleB_ElectricityFees>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CapacityKW).HasColumnType("decimal(18,2)");
                entity.Property(e => e.GeneratingFee).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TransformingFee).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TransmittingFee).HasColumnType("decimal(18,2)");
            });

            // Configure FactoryRegistrationFee
            modelBuilder.Entity<FactoryRegistrationFee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalPowerHP).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalPowerKW).HasColumnType("decimal(18,2)");
                entity.Property(e => e.FactoryFee).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ElectricityFee).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalFee).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.FactoryRegistration)
                      .WithMany()
                      .HasForeignKey(e => e.FactoryRegistrationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Enhanced Privilege System
            modelBuilder.Entity<ModulePermission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PermissionName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PermissionCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasOne(e => e.Module)
                      .WithMany(m => m.AvailablePermissions)
                      .HasForeignKey(e => e.ModuleId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.ModuleId, e.PermissionCode }).IsUnique();
            });

            modelBuilder.Entity<UserModulePermission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Permissions).IsRequired().HasColumnType("nvarchar(max)");
                entity.HasOne(e => e.User)
                      .WithMany(u => u.ModulePermissions)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Module)
                      .WithMany(m => m.UserPermissions)
                      .HasForeignKey(e => e.ModuleId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.UserId, e.ModuleId }).IsUnique();
            });

            modelBuilder.Entity<UserAreaAssignment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.AreaAssignments)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Area)
                      .WithMany(a => a.UserAssignments)
                      .HasForeignKey(e => e.AreaId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Module)
                      .WithMany(m => m.AreaAssignments)
                      .HasForeignKey(e => e.ModuleId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasIndex(e => new { e.UserId, e.AreaId, e.ModuleId }).IsUnique();
            });

            // Conversion for Establishment and Application entities to handle uniqueidentifier columns as strings
            // ------------------- EstablishmentRegistration -------------------
            modelBuilder.Entity<EstablishmentRegistration>(entity =>
            {
                // Primary key
                entity.HasKey(e => e.EstablishmentRegistrationId);

                entity.Property(e => e.EstablishmentRegistrationId)
                      .HasMaxLength(100)
                      .IsRequired();

                // Other optional GUIDs
                entity.Property(e => e.EstablishmentDetailId)
                      .HasColumnType("uniqueidentifier");

                entity.Property(e => e.MainOwnerDetailId)
                      .HasColumnType("uniqueidentifier");

                entity.Property(e => e.ManagerOrAgentDetailId)
                      .HasColumnType("uniqueidentifier");

                entity.Property(e => e.ContractorDetailId)
                      .HasColumnType("uniqueidentifier");

                // nvarchar columns
                entity.Property(e => e.Status)
                      .HasMaxLength(50);

                entity.Property(e => e.Place)
                      .HasMaxLength(200);

                entity.Property(e => e.RegistrationNumber)
                      .HasMaxLength(100);

                entity.Property(e => e.Type)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.Signature)
                      .HasMaxLength(200);

                entity.Property(e => e.ApplicationPDFUrl)
                      .HasMaxLength(500);

                // datetime columns
                entity.Property(e => e.CreatedDate)
                      .HasColumnType("datetime2");

                entity.Property(e => e.UpdatedDate)
                      .HasColumnType("datetime2");

                entity.Property(e => e.Date)
                      .HasColumnType("datetime");

                // decimal columns
                entity.Property(e => e.Version)
                      .HasColumnType("decimal(3,1)")
                      .IsRequired();

                entity.Property(e => e.Amount)
                      .HasColumnType("decimal(18,2)");

                // bit columns
                entity.Property(e => e.IsPaymentCompleted)
                      .HasColumnType("bit");

                entity.Property(e => e.IsESignCompleted)
                      .HasColumnType("bit");
            });

            // ------------------- EstablishmentRegistrationDocument -------------------
            modelBuilder.Entity<EstablishmentRegistrationDocument>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.EstablishmentRegistrationId)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.DocumentType)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(e => e.FileName)
                      .HasMaxLength(500)
                      .IsRequired();

                entity.Property(e => e.FilePath)
                      .HasMaxLength(500)
                      .IsRequired();

                entity.Property(e => e.FileExtension)
                      .HasMaxLength(50);

                entity.HasOne(d => d.EstablishmentRegistration)
                      .WithMany()
                      .HasForeignKey(d => d.EstablishmentRegistrationId)
                      .HasPrincipalKey(p => p.EstablishmentRegistrationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ------------------- EstablishmentEntityMapping -------------------
            modelBuilder.Entity<EstablishmentEntityMapping>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.EstablishmentRegistrationId)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.HasOne<EstablishmentRegistration>()
                      .WithMany()
                      .HasForeignKey(e => e.EstablishmentRegistrationId)
                      .HasPrincipalKey(r => r.EstablishmentRegistrationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ------------------- ApplicationRegistration -------------------
            modelBuilder.Entity<ApplicationRegistration>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.ApplicationId)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.ApplicationRegistrationNumber)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.CreatedDate)
                      .HasColumnType("datetime2");

                entity.Property(e => e.UpdatedDate)
                      .HasColumnType("datetime2");

                entity.Property(e => e.ModuleId)
                      .HasColumnType("uniqueidentifier");

                entity.Property(e => e.UserId)
                      .HasColumnType("uniqueidentifier");
            });
            modelBuilder.Entity<FactoryContractorMapping>(entity =>
            {
                  // Composite Primary Key
                  entity.HasKey(e => new 
                  { 
                        e.EstablishmentRegistrationId, 
                        e.ContractorDetailId 
                  });

                  entity.Property(e => e.EstablishmentRegistrationId)
                        .HasMaxLength(100)
                        .IsRequired();

                  entity.Property(e => e.ContractorDetailId)
                        .IsRequired();

                  // Foreign Key to EstablishmentRegistration
                  entity.HasOne<EstablishmentRegistration>()
                        .WithMany()
                        .HasForeignKey(e => e.EstablishmentRegistrationId)
                        .HasPrincipalKey(r => r.EstablishmentRegistrationId)
                        .OnDelete(DeleteBehavior.Cascade);

                  // Optional: Foreign Key to ContractorDetail (if exists)
                  entity.HasOne<PersonDetail>() // replace with correct entity
                        .WithMany()
                        .HasForeignKey(e => e.ContractorDetailId)
                        .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<FeeResult>().HasNoKey();
            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is FormModule || e.Entity is DynamicForm || e.Entity is WorkflowConfig ||
                           e.Entity is FactoryMapApproval || e.Entity is FactoryTypeOld || e.Entity is DocumentType ||
                           e.Entity is FactoryRegistration)
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    if (entry.Entity is FormModule module)
                    {
                        module.CreatedAt = DateTime.Now;
                        module.UpdatedAt = DateTime.Now;
                    }
                    else if (entry.Entity is DynamicForm form)
                    {
                        form.CreatedAt = DateTime.Now;
                        form.UpdatedAt = DateTime.Now;
                    }
                    else if (entry.Entity is WorkflowConfig workflow)
                    {
                        workflow.CreatedAt = DateTime.Now;
                        workflow.UpdatedAt = DateTime.Now;
                    }
                    else if (entry.Entity is FactoryMapApproval approval)
                    {
                        approval.CreatedAt = DateTime.Now;
                        approval.UpdatedAt = DateTime.Now;
                    }
                    else if (entry.Entity is FactoryTypeOld factoryType)
                    {
                        factoryType.CreatedAt = DateTime.Now;
                        factoryType.UpdatedAt = DateTime.Now;
                    }
                    else if (entry.Entity is DocumentType documentType)
                    {
                        documentType.CreatedAt = DateTime.Now;
                        documentType.UpdatedAt = DateTime.Now;
                    }
                    else if (entry.Entity is FactoryRegistration registration)
                    {
                        registration.CreatedAt = DateTime.Now;
                        registration.UpdatedAt = DateTime.Now;
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    if (entry.Entity is FormModule module)
                    {
                        module.UpdatedAt = DateTime.Now;
                    }
                    else if (entry.Entity is DynamicForm form)
                    {
                        form.UpdatedAt = DateTime.Now;
                    }
                    else if (entry.Entity is WorkflowConfig workflow)
                    {
                        workflow.UpdatedAt = DateTime.Now;
                    }
                    else if (entry.Entity is FactoryMapApproval approval)
                    {
                        approval.UpdatedAt = DateTime.Now;
                    }
                    else if (entry.Entity is FactoryTypeOld factoryType)
                    {
                        factoryType.UpdatedAt = DateTime.Now;
                    }
                    else if (entry.Entity is DocumentType documentType)
                    {
                        documentType.UpdatedAt = DateTime.Now;
                    }
                    else if (entry.Entity is FactoryRegistration registration)
                    {
                        registration.UpdatedAt = DateTime.Now;
                    }
                }
            }
        }
    }
}