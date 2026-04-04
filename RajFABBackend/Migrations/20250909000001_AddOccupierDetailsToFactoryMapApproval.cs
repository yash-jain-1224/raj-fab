using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RajFabAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddOccupierDetailsToFactoryMapApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OccupierType",
                table: "FactoryMapApprovals",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OccupierName",
                table: "FactoryMapApprovals",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OccupierFatherName",
                table: "FactoryMapApprovals",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OccupierPlotNumber",
                table: "FactoryMapApprovals",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OccupierStreetLocality",
                table: "FactoryMapApprovals",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OccupierCityTown",
                table: "FactoryMapApprovals",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OccupierDistrict",
                table: "FactoryMapApprovals",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OccupierArea",
                table: "FactoryMapApprovals",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OccupierPincode",
                table: "FactoryMapApprovals",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OccupierMobile",
                table: "FactoryMapApprovals",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OccupierEmail",
                table: "FactoryMapApprovals",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OccupierType",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "OccupierName",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "OccupierFatherName",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "OccupierPlotNumber",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "OccupierStreetLocality",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "OccupierCityTown",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "OccupierDistrict",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "OccupierArea",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "OccupierPincode",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "OccupierMobile",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "OccupierEmail",
                table: "FactoryMapApprovals");
        }
    }
}