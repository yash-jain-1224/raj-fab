using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RajFabAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddBoilerWorkflowTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── InspectionScrutinyWorkflows ──────────────────────────────────
            migrationBuilder.CreateTable(
                name: "InspectionScrutinyWorkflows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfficeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LevelCount = table.Column<int>(type: "int", nullable: false),
                    IsBidirectional = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionScrutinyWorkflows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionScrutinyWorkflows_Offices_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "Offices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionScrutinyWorkflows_OfficeId",
                table: "InspectionScrutinyWorkflows",
                column: "OfficeId");

            // ── InspectionScrutinyLevels ─────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "InspectionScrutinyLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LevelNumber = table.Column<int>(type: "int", nullable: false),
                    OfficePostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsPrefilled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PrefillSource = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionScrutinyLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionScrutinyLevels_InspectionScrutinyWorkflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "InspectionScrutinyWorkflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionScrutinyLevels_WorkflowId",
                table: "InspectionScrutinyLevels",
                column: "WorkflowId");

            // ── BoilerApplicationStates ──────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "BoilerApplicationStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CurrentStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "Draft"),
                    CurrentPart = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    CurrentLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    AssignedInspectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AuthorityForwardedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InspectorActionsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ChiefCycleCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastChiefActionValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CertificatePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerApplicationStates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoilerApplicationStates_ApplicationId",
                table: "BoilerApplicationStates",
                column: "ApplicationId");

            // ── InspectionSchedules ──────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "InspectionSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InspectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InspectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InspectionTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    PlaceAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InspectionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EstimatedDuration = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InspectorNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionSchedules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionSchedules_ApplicationId",
                table: "InspectionSchedules",
                column: "ApplicationId");

            // ── InspectionFormSubmissions ────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "InspectionFormSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InspectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Photos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Documents = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GeneratedPdfPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ESignData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PreviewGeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionFormSubmissions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionFormSubmissions_ApplicationId",
                table: "InspectionFormSubmissions",
                column: "ApplicationId");

            // ── ChiefInspectionScrutinyRemarks ───────────────────────────────
            migrationBuilder.CreateTable(
                name: "ChiefInspectionScrutinyRemarks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RemarkText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiefInspectionScrutinyRemarks", x => x.Id);
                });

            // ── BoilerWorkflowLogs ───────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "BoilerWorkflowLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Part = table.Column<int>(type: "int", nullable: false),
                    FromUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ToUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FromLevel = table.Column<int>(type: "int", nullable: true),
                    ToLevel = table.Column<int>(type: "int", nullable: true),
                    ActionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CycleNumber = table.Column<int>(type: "int", nullable: true),
                    ChiefActionValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerWorkflowLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoilerWorkflowLogs_ApplicationId",
                table: "BoilerWorkflowLogs",
                column: "ApplicationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "BoilerWorkflowLogs");
            migrationBuilder.DropTable(name: "ChiefInspectionScrutinyRemarks");
            migrationBuilder.DropTable(name: "InspectionFormSubmissions");
            migrationBuilder.DropTable(name: "InspectionSchedules");
            migrationBuilder.DropTable(name: "BoilerApplicationStates");
            migrationBuilder.DropTable(name: "InspectionScrutinyLevels");
            migrationBuilder.DropTable(name: "InspectionScrutinyWorkflows");
        }
    }
}
