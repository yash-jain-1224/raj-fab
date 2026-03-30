using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RajFabAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddSeniorityOrderAndDirection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Posts.SeniorityOrder — default 0 for all existing rows, no data loss
            migrationBuilder.AddColumn<int>(
                name: "SeniorityOrder",
                table: "Posts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // ApplicationApprovalRequests.Direction — default 'Forward' for all existing rows
            migrationBuilder.AddColumn<string>(
                name: "Direction",
                table: "ApplicationApprovalRequests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Forward");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SeniorityOrder",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Direction",
                table: "ApplicationApprovalRequests");
        }
    }
}
