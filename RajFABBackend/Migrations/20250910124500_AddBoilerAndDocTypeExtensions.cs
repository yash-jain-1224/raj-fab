using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RajFabAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddBoilerAndDocTypeExtensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add missing columns to DocumentTypes table
            migrationBuilder.AddColumn<string>(
                name: "Module",
                table: "DocumentTypes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Factory");

            migrationBuilder.AddColumn<string>(
                name: "ServiceType", 
                table: "DocumentTypes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Registration");

            migrationBuilder.AddColumn<bool>(
                name: "IsConditional",
                table: "DocumentTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ConditionalField",
                table: "DocumentTypes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConditionalValue",
                table: "DocumentTypes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            // Create BoilerSpecifications table
            migrationBuilder.CreateTable(
                name: "BoilerSpecifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoilerType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Manufacturer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    YearOfManufacture = table.Column<int>(type: "int", nullable: false),
                    WorkingPressure = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DesignPressure = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    SteamCapacity = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    FuelType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HeatingArea = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    SuperheaterArea = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    EconomiserArea = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    AirPreheaterArea = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerSpecifications", x => x.Id);
                });

            // Create BoilerLocation table
            migrationBuilder.CreateTable(
                name: "BoilerLocation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FactoryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FactoryLicenseNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PlotNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Locality = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Pincode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AreaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DistrictId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DivisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(10,8)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(11,8)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerLocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoilerLocation_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BoilerLocation_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BoilerLocation_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BoilerLocation_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create BoilerSafetyFeatures table
            migrationBuilder.CreateTable(
                name: "BoilerSafetyFeatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SafetyValves = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WaterGauges = table.Column<int>(type: "int", nullable: false),
                    PressureGauges = table.Column<int>(type: "int", nullable: false),
                    FusiblePlugs = table.Column<int>(type: "int", nullable: true),
                    BlowdownValves = table.Column<int>(type: "int", nullable: false),
                    FeedwaterSystem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EmergencyShutoff = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerSafetyFeatures", x => x.Id);
                });

            // Create BoilerCertificate table
            migrationBuilder.CreateTable(
                name: "BoilerCertificate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CertificateNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CertificateType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InspectorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InspectorId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerCertificate", x => x.Id);
                });

            // Create RegisteredBoilers table
            migrationBuilder.CreateTable(
                name: "RegisteredBoilers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SpecificationsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SafetyFeaturesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentCertificateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OwnerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OperatorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OperatorCertificateNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OperatorCertificateExpiry = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisteredBoilers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegisteredBoilers_BoilerCertificate_CurrentCertificateId",
                        column: x => x.CurrentCertificateId,
                        principalTable: "BoilerCertificate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegisteredBoilers_BoilerLocation_LocationId",
                        column: x => x.LocationId,
                        principalTable: "BoilerLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegisteredBoilers_BoilerSafetyFeatures_SafetyFeaturesId",
                        column: x => x.SafetyFeaturesId,
                        principalTable: "BoilerSafetyFeatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegisteredBoilers_BoilerSpecifications_SpecificationsId",
                        column: x => x.SpecificationsId,
                        principalTable: "BoilerSpecifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create BoilerInspectionHistory table
            migrationBuilder.CreateTable(
                name: "BoilerInspectionHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InspectionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InspectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InspectionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InspectorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Findings = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Recommendations = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NextInspectionDue = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CertificateIssued = table.Column<bool>(type: "bit", nullable: false),
                    BoilerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerInspectionHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoilerInspectionHistory_RegisteredBoilers_BoilerId",
                        column: x => x.BoilerId,
                        principalTable: "RegisteredBoilers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create BoilerApplications table
            migrationBuilder.CreateTable(
                name: "BoilerApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApplicationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BoilerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApplicantName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OrganizationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApplicationData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentPaths = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoilerApplications_RegisteredBoilers_BoilerId",
                        column: x => x.BoilerId,
                        principalTable: "RegisteredBoilers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create BoilerDocumentTypes table
            migrationBuilder.CreateTable(
                name: "BoilerDocumentTypes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BoilerServiceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DocumentTypeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    ConditionalField = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ConditionalValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerDocumentTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoilerDocumentTypes_DocumentTypes_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes for performance
            migrationBuilder.CreateIndex(
                name: "IX_BoilerApplications_BoilerId",
                table: "BoilerApplications",
                column: "BoilerId");

            migrationBuilder.CreateIndex(
                name: "IX_BoilerApplications_ApplicationNumber",
                table: "BoilerApplications",
                column: "ApplicationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BoilerDocumentTypes_DocumentTypeId",
                table: "BoilerDocumentTypes",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BoilerDocumentTypes_BoilerServiceType",
                table: "BoilerDocumentTypes",
                column: "BoilerServiceType");

            migrationBuilder.CreateIndex(
                name: "IX_BoilerInspectionHistory_BoilerId",
                table: "BoilerInspectionHistory",
                column: "BoilerId");

            migrationBuilder.CreateIndex(
                name: "IX_BoilerLocation_AreaId",
                table: "BoilerLocation",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_BoilerLocation_CityId",
                table: "BoilerLocation",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_BoilerLocation_DistrictId",
                table: "BoilerLocation",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_BoilerLocation_DivisionId",
                table: "BoilerLocation",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredBoilers_CurrentCertificateId",
                table: "RegisteredBoilers",
                column: "CurrentCertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredBoilers_LocationId",
                table: "RegisteredBoilers",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredBoilers_SafetyFeaturesId",
                table: "RegisteredBoilers",
                column: "SafetyFeaturesId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredBoilers_SpecificationsId",
                table: "RegisteredBoilers",
                column: "SpecificationsId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredBoilers_RegistrationNumber",
                table: "RegisteredBoilers",
                column: "RegistrationNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop all boiler-related tables
            migrationBuilder.DropTable(
                name: "BoilerApplications");

            migrationBuilder.DropTable(
                name: "BoilerDocumentTypes");

            migrationBuilder.DropTable(
                name: "BoilerInspectionHistory");

            migrationBuilder.DropTable(
                name: "RegisteredBoilers");

            migrationBuilder.DropTable(
                name: "BoilerCertificate");

            migrationBuilder.DropTable(
                name: "BoilerLocation");

            migrationBuilder.DropTable(
                name: "BoilerSafetyFeatures");

            migrationBuilder.DropTable(
                name: "BoilerSpecifications");

            // Remove added columns from DocumentTypes
            migrationBuilder.DropColumn(
                name: "ConditionalValue",
                table: "DocumentTypes");

            migrationBuilder.DropColumn(
                name: "ConditionalField",
                table: "DocumentTypes");

            migrationBuilder.DropColumn(
                name: "IsConditional",
                table: "DocumentTypes");

            migrationBuilder.DropColumn(
                name: "ServiceType",
                table: "DocumentTypes");

            migrationBuilder.DropColumn(
                name: "Module",
                table: "DocumentTypes");
        }
    }
}