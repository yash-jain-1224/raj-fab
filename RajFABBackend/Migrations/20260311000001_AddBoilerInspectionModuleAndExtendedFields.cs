using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RajFabAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddBoilerInspectionModuleAndExtendedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add extended inspection fields to InspectorApplicationInspections
            migrationBuilder.AddColumn<string>(
                name: "HydraulicTestPressure",
                table: "InspectorApplicationInspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HydraulicTestDuration",
                table: "InspectorApplicationInspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JointsCondition",
                table: "InspectorApplicationInspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RivetsCondition",
                table: "InspectorApplicationInspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlatingCondition",
                table: "InspectorApplicationInspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StaysCondition",
                table: "InspectorApplicationInspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CrownCondition",
                table: "InspectorApplicationInspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FireboxCondition",
                table: "InspectorApplicationInspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FusiblePlugCondition",
                table: "InspectorApplicationInspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FireTubesCondition",
                table: "InspectorApplicationInspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlueFurnaceCondition",
                table: "InspectorApplicationInspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmokeBoxCondition",
                table: "InspectorApplicationInspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SteamDrumCondition",
                table: "InspectorApplicationInspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SafetyValveCondition",
                table: "InspectorApplicationInspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PressureGaugeCondition",
                table: "InspectorApplicationInspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeedCheckCondition",
                table: "InspectorApplicationInspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StopValveCondition",
                table: "InspectorApplicationInspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BlowDownCondition",
                table: "InspectorApplicationInspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EconomiserCondition",
                table: "InspectorApplicationInspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuperheaterCondition",
                table: "InspectorApplicationInspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AirPressureGaugeCondition",
                table: "InspectorApplicationInspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AllowedWorkingPressure",
                table: "InspectorApplicationInspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProvisionalOrderNumber",
                table: "InspectorApplicationInspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProvisionalOrderDate",
                table: "InspectorApplicationInspections",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BoilerAttendantName",
                table: "InspectorApplicationInspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BoilerAttendantCertNo",
                table: "InspectorApplicationInspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeeAmount",
                table: "InspectorApplicationInspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChallanNumber",
                table: "InspectorApplicationInspections",
                type: "nvarchar(max)",
                nullable: true);

            // Seed Boiler Inspection module
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Modules WHERE Name = 'Boiler Inspection')
                BEGIN
                    INSERT INTO Modules (Id, Name, CreatedAt, UpdatedAt)
                    VALUES (NEWID(), 'Boiler Inspection', GETDATE(), GETDATE())
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "HydraulicTestPressure", table: "InspectorApplicationInspections");
            migrationBuilder.DropColumn(name: "HydraulicTestDuration", table: "InspectorApplicationInspections");
            migrationBuilder.DropColumn(name: "JointsCondition", table: "InspectorApplicationInspections");
            migrationBuilder.DropColumn(name: "RivetsCondition", table: "InspectorApplicationInspections");
            migrationBuilder.DropColumn(name: "PlatingCondition", table: "InspectorApplicationInspections");
            migrationBuilder.DropColumn(name: "StaysCondition", table: "InspectorApplicationInspections");
            migrationBuilder.DropColumn(name: "CrownCondition", table: "InspectorApplicationInspections");
            migrationBuilder.DropColumn(name: "FireboxCondition", table: "InspectorApplicationInspections");
            migrationBuilder.DropColumn(name: "FusiblePlugCondition", table: "InspectorApplicationInspections");
            migrationBuilder.DropColumn(name: "FireTubesCondition", table: "InspectorApplicationInspections");
            migrationBuilder.DropColumn(name: "FlueFurnaceCondition", table: "InspectorApplicationInspections");
            migrationBuilder.DropColumn(name: "SmokeBoxCondition", table: "InspectorApplicationInspections");
            migrationBuilder.DropColumn(name: "SteamDrumCondition", table: "InspectorApplicationInspections");
            migrationBuilder.DropColumn(name: "SafetyValveCondition", table: "InspectorApplicationInspections");
            migrationBuilder.DropColumn(name: "PressureGaugeCondition", table: "InspectorApplicationInspections");
            migrationBuilder.DropColumn(name: "FeedCheckCondition", table: "InspectorApplicationInspections");
            migrationBuilder.DropColumn(name: "StopValveCondition", table: "InspectorApplicationInspections");
            migrationBuilder.DropColumn(name: "BlowDownCondition", table: "InspectorApplicationInspections");
            migrationBuilder.DropColumn(name: "EconomiserCondition", table: "InspectorApplicationInspections");
            migrationBuilder.DropColumn(name: "SuperheaterCondition", table: "InspectorApplicationInspections");
            migrationBuilder.DropColumn(name: "AirPressureGaugeCondition", table: "InspectorApplicationInspections");
            migrationBuilder.DropColumn(name: "AllowedWorkingPressure", table: "InspectorApplicationInspections");
            migrationBuilder.DropColumn(name: "ProvisionalOrderNumber", table: "InspectorApplicationInspections");
            migrationBuilder.DropColumn(name: "ProvisionalOrderDate", table: "InspectorApplicationInspections");
            migrationBuilder.DropColumn(name: "BoilerAttendantName", table: "InspectorApplicationInspections");
            migrationBuilder.DropColumn(name: "BoilerAttendantCertNo", table: "InspectorApplicationInspections");
            migrationBuilder.DropColumn(name: "FeeAmount", table: "InspectorApplicationInspections");
            migrationBuilder.DropColumn(name: "ChallanNumber", table: "InspectorApplicationInspections");

            migrationBuilder.Sql("DELETE FROM Modules WHERE Name = 'Boiler Inspection'");
        }
    }
}
