using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RajFabAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAmountToBoilerRegistrationAndBoilerModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add Amount column to BoilerRegistrations table
            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "BoilerRegistrations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            // Add IsPaymentCompleted column to BoilerRegistrations table
            migrationBuilder.AddColumn<bool>(
                name: "IsPaymentCompleted",
                table: "BoilerRegistrations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Add IsESignCompleted column to BoilerRegistrations table
            migrationBuilder.AddColumn<bool>(
                name: "IsESignCompleted",
                table: "BoilerRegistrations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Add ApplicationPDFUrl column to BoilerRegistrations table
            migrationBuilder.AddColumn<string>(
                name: "ApplicationPDFUrl",
                table: "BoilerRegistrations",
                type: "nvarchar(max)",
                nullable: true);

            // Insert Boiler Registration module into Modules (FormModules) table if not already present
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Modules WHERE Name = 'Boiler Registration')
                BEGIN
                    INSERT INTO Modules (Id, Name, CreatedAt, UpdatedAt)
                    VALUES (NEWID(), 'Boiler Registration', GETDATE(), GETDATE())
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationPDFUrl",
                table: "BoilerRegistrations");

            migrationBuilder.DropColumn(
                name: "IsESignCompleted",
                table: "BoilerRegistrations");

            migrationBuilder.DropColumn(
                name: "IsPaymentCompleted",
                table: "BoilerRegistrations");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "BoilerRegistrations");

            migrationBuilder.Sql(@"
                DELETE FROM Modules WHERE Name = 'Boiler Registration'
            ");
        }
    }
}
