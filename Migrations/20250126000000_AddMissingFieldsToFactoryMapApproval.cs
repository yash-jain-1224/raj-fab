using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RajFabAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingFieldsToFactoryMapApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "FactoryMapApprovals",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PoliceStation",
                table: "FactoryMapApprovals",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RailwayStation",
                table: "FactoryMapApprovals",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessRegistrationNumber",
                table: "FactoryMapApprovals",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OccupierDesignation",
                table: "FactoryMapApprovals",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "PoliceStation",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "RailwayStation",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "BusinessRegistrationNumber",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "OccupierDesignation",
                table: "FactoryMapApprovals");
        }
    }
}
