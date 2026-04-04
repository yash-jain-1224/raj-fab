using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RajFabAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Divisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Divisions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FileTypes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MaxSizeMB = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FactoryRegistrations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MapApprovalAcknowledgementNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LicenseFromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LicenseToDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LicenseYears = table.Column<int>(type: "int", nullable: false),
                    FactoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactoryRegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlotNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StreetLocality = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    District = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CityTown = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Area = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pincode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ManufacturingProcess = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductionStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ManufacturingProcessLast12Months = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ManufacturingProcessNext12Months = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaxWorkersMaleProposed = table.Column<int>(type: "int", nullable: false),
                    MaxWorkersFemaleProposed = table.Column<int>(type: "int", nullable: false),
                    MaxWorkersTransgenderProposed = table.Column<int>(type: "int", nullable: false),
                    MaxWorkersMaleEmployed = table.Column<int>(type: "int", nullable: false),
                    MaxWorkersFemaleEmployed = table.Column<int>(type: "int", nullable: false),
                    MaxWorkersTransgenderEmployed = table.Column<int>(type: "int", nullable: false),
                    WorkersMaleOrdinary = table.Column<int>(type: "int", nullable: false),
                    WorkersFemaleOrdinary = table.Column<int>(type: "int", nullable: false),
                    WorkersTransgenderOrdinary = table.Column<int>(type: "int", nullable: false),
                    TotalRatedHorsePower = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PowerUnit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FactoryManagerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactoryManagerFatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactoryManagerPlotNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactoryManagerStreetLocality = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactoryManagerDistrict = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactoryManagerArea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactoryManagerCityTown = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactoryManagerPincode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactoryManagerMobile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactoryManagerEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccupierType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccupierName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccupierFatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccupierPlotNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccupierStreetLocality = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccupierCityTown = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccupierDistrict = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccupierArea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccupierPincode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccupierMobile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccupierEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LandOwnerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LandOwnerPlotNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LandOwnerStreetLocality = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LandOwnerDistrict = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LandOwnerArea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LandOwnerCityTown = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LandOwnerPincode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LandOwnerMobile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LandOwnerEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BuildingPlanReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BuildingPlanApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WasteDisposalReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WasteDisposalApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WasteDisposalAuthority = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WantToMakePaymentNow = table.Column<bool>(type: "bit", nullable: false),
                    DeclarationAccepted = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactoryRegistrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FactoryTypes_Old",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactoryTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Occupiers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MobileNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    PlotNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StreetLocality = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    VillageTownCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    District = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Pincode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Occupiers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Privileges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Module = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Privileges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DivisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Districts_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FactoryRegistrationDocuments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FactoryRegistrationId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactoryRegistrationDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactoryRegistrationDocuments_FactoryRegistrations_FactoryRegistrationId",
                        column: x => x.FactoryRegistrationId,
                        principalTable: "FactoryRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FactoryMapApprovals",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AcknowledgementNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FactoryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ApplicantName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MobileNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    District = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Pincode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FactoryTypeId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    PlotArea = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BuildingArea = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReviewedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactoryMapApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactoryMapApprovals_FactoryTypes_FactoryTypeId",
                        column: x => x.FactoryTypeId,
                        principalTable: "FactoryTypes_Old",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "FactoryTypeDocuments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FactoryTypeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DocumentTypeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactoryTypeDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactoryTypeDocuments_DocumentTypes_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FactoryTypeDocuments_FactoryTypes_FactoryTypeId",
                        column: x => x.FactoryTypeId,
                        principalTable: "FactoryTypes_Old",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ManufacturingProcessTypes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FactoryTypeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    HasHazardousChemicals = table.Column<bool>(type: "bit", nullable: false),
                    HasDangerousOperations = table.Column<bool>(type: "bit", nullable: false),
                    WorkerLimit = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManufacturingProcessTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManufacturingProcessTypes_FactoryTypes_FactoryTypeId",
                        column: x => x.FactoryTypeId,
                        principalTable: "FactoryTypes_Old",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Forms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FieldsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Forms_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Areas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DistrictId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DistrictId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Areas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Areas_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Areas_Districts_DistrictId1",
                        column: x => x.DistrictId1,
                        principalTable: "Districts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DistrictId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cities_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FactoryMapDocuments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FactoryMapApprovalId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FileExtension = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactoryMapDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactoryMapDocuments_FactoryMapApprovals_FactoryMapApprovalId",
                        column: x => x.FactoryMapApprovalId,
                        principalTable: "FactoryMapApprovals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcessDocuments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProcessTypeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentTypeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    ConditionalField = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ConditionalValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ManufacturingProcessTypeId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessDocuments_DocumentTypes_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcessDocuments_ManufacturingProcessTypes_ManufacturingProcessTypeId",
                        column: x => x.ManufacturingProcessTypeId,
                        principalTable: "ManufacturingProcessTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FormSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Collapsible = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormSections_Forms_FormId",
                        column: x => x.FormId,
                        principalTable: "Forms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Submissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DataJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Submissions_Forms_FormId",
                        column: x => x.FormId,
                        principalTable: "Forms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OnSubmitApiEndpoint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OnSubmitMethod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    OnSubmitNotificationEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OnSubmitRedirectUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OnSubmitCustomActions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OnApprovalApiEndpoint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OnApprovalNotificationEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OnApprovalCustomActions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowConfigs_Forms_FormId",
                        column: x => x.FormId,
                        principalTable: "Forms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPrivileges",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrivilegeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPrivileges", x => new { x.UserId, x.PrivilegeId });
                    table.ForeignKey(
                        name: "FK_UserPrivileges_Privileges_PrivilegeId",
                        column: x => x.PrivilegeId,
                        principalTable: "Privileges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPrivileges_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPrivileges_Users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PoliceStations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DistrictId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliceStations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PoliceStations_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PoliceStations_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RailwayStations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DistrictId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RailwayStations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RailwayStations_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RailwayStations_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Areas_DistrictId",
                table: "Areas",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Areas_DistrictId1",
                table: "Areas",
                column: "DistrictId1");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_DistrictId",
                table: "Cities",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_DivisionId",
                table: "Districts",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Divisions_Name",
                table: "Divisions",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_Name",
                table: "DocumentTypes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryMapApprovals_AcknowledgementNumber",
                table: "FactoryMapApprovals",
                column: "AcknowledgementNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FactoryMapApprovals_Email",
                table: "FactoryMapApprovals",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryMapApprovals_FactoryTypeId",
                table: "FactoryMapApprovals",
                column: "FactoryTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryMapApprovals_Status",
                table: "FactoryMapApprovals",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryMapDocuments_FactoryMapApprovalId",
                table: "FactoryMapDocuments",
                column: "FactoryMapApprovalId");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryRegistrationDocuments_FactoryRegistrationId",
                table: "FactoryRegistrationDocuments",
                column: "FactoryRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryTypeDocuments_DocumentTypeId",
                table: "FactoryTypeDocuments",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryTypeDocuments_FactoryTypeId",
                table: "FactoryTypeDocuments",
                column: "FactoryTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryTypes_Name",
                table: "FactoryTypes_Old",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Forms_ModuleId",
                table: "Forms",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_FormSections_FormId",
                table: "FormSections",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingProcessTypes_FactoryTypeId",
                table: "ManufacturingProcessTypes",
                column: "FactoryTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_Category",
                table: "Modules",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Occupiers_Email",
                table: "Occupiers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Occupiers_MobileNo",
                table: "Occupiers",
                column: "MobileNo");

            migrationBuilder.CreateIndex(
                name: "IX_PoliceStations_CityId",
                table: "PoliceStations",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliceStations_DistrictId",
                table: "PoliceStations",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Privileges_Module_Action",
                table: "Privileges",
                columns: new[] { "Module", "Action" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessDocuments_DocumentTypeId",
                table: "ProcessDocuments",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessDocuments_ManufacturingProcessTypeId",
                table: "ProcessDocuments",
                column: "ManufacturingProcessTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RailwayStations_CityId",
                table: "RailwayStations",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_RailwayStations_DistrictId",
                table: "RailwayStations",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_FormId",
                table: "Submissions",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_Status",
                table: "Submissions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_UserId",
                table: "Submissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPrivileges_PrivilegeId",
                table: "UserPrivileges",
                column: "PrivilegeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPrivileges_UserId1",
                table: "UserPrivileges",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowConfigs_FormId",
                table: "WorkflowConfigs",
                column: "FormId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Areas");

            migrationBuilder.DropTable(
                name: "FactoryMapDocuments");

            migrationBuilder.DropTable(
                name: "FactoryRegistrationDocuments");

            migrationBuilder.DropTable(
                name: "FactoryTypeDocuments");

            migrationBuilder.DropTable(
                name: "FormSections");

            migrationBuilder.DropTable(
                name: "Occupiers");

            migrationBuilder.DropTable(
                name: "PoliceStations");

            migrationBuilder.DropTable(
                name: "ProcessDocuments");

            migrationBuilder.DropTable(
                name: "RailwayStations");

            migrationBuilder.DropTable(
                name: "Submissions");

            migrationBuilder.DropTable(
                name: "UserPrivileges");

            migrationBuilder.DropTable(
                name: "WorkflowConfigs");

            migrationBuilder.DropTable(
                name: "FactoryMapApprovals");

            migrationBuilder.DropTable(
                name: "FactoryRegistrations");

            migrationBuilder.DropTable(
                name: "DocumentTypes");

            migrationBuilder.DropTable(
                name: "ManufacturingProcessTypes");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "Privileges");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Forms");

            migrationBuilder.DropTable(
                name: "FactoryTypes_Old");

            migrationBuilder.DropTable(
                name: "Districts");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Modules");

            migrationBuilder.DropTable(
                name: "Divisions");
        }
    }
}
