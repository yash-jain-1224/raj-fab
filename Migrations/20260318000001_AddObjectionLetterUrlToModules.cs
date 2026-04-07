using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RajFabAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddObjectionLetterUrlToModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // EstablishmentRegistrations
            migrationBuilder.AddColumn<string>(
                name: "ObjectionLetterUrl",
                table: "EstablishmentRegistrations",
                type: "nvarchar(max)",
                nullable: true);

            // FactoryMapApprovals
            migrationBuilder.AddColumn<string>(
                name: "ObjectionLetterUrl",
                table: "FactoryMapApprovals",
                type: "nvarchar(max)",
                nullable: true);

            // FactoryLicenses
            migrationBuilder.AddColumn<string>(
                name: "ObjectionLetterUrl",
                table: "FactoryLicenses",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ObjectionLetterUrl",
                table: "EstablishmentRegistrations");

            migrationBuilder.DropColumn(
                name: "ObjectionLetterUrl",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "ObjectionLetterUrl",
                table: "FactoryLicenses");
        }
    }
}
