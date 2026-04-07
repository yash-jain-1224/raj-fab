using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RajFabAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPanNumberToEstablishmentDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PanNumber",
                table: "EstablishmentDetails",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PanNumber",
                table: "EstablishmentDetails");
        }
    }
}
