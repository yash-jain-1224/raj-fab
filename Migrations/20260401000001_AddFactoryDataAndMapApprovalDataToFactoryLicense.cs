using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RajFabAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddFactoryDataAndMapApprovalDataToFactoryLicense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FactoryData",
                table: "FactoryLicenses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MapApprovalData",
                table: "FactoryLicenses",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "FactoryData", table: "FactoryLicenses");
            migrationBuilder.DropColumn(name: "MapApprovalData", table: "FactoryLicenses");
        }
    }
}
