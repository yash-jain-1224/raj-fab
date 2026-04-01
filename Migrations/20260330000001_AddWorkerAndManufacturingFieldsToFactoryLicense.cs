using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RajFabAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkerAndManufacturingFieldsToFactoryLicense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkersProposedMale",
                table: "FactoryLicenses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkersProposedFemale",
                table: "FactoryLicenses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkersProposedTransgender",
                table: "FactoryLicenses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkersLastYearMale",
                table: "FactoryLicenses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkersLastYearFemale",
                table: "FactoryLicenses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkersLastYearTransgender",
                table: "FactoryLicenses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkersOrdinaryMale",
                table: "FactoryLicenses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkersOrdinaryFemale",
                table: "FactoryLicenses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkersOrdinaryTransgender",
                table: "FactoryLicenses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SanctionedLoad",
                table: "FactoryLicenses",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SanctionedLoadUnit",
                table: "FactoryLicenses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManufacturingProcessLast12Months",
                table: "FactoryLicenses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManufacturingProcessNext12Months",
                table: "FactoryLicenses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DateOfStartProduction",
                table: "FactoryLicenses",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "WorkersProposedMale", table: "FactoryLicenses");
            migrationBuilder.DropColumn(name: "WorkersProposedFemale", table: "FactoryLicenses");
            migrationBuilder.DropColumn(name: "WorkersProposedTransgender", table: "FactoryLicenses");
            migrationBuilder.DropColumn(name: "WorkersLastYearMale", table: "FactoryLicenses");
            migrationBuilder.DropColumn(name: "WorkersLastYearFemale", table: "FactoryLicenses");
            migrationBuilder.DropColumn(name: "WorkersLastYearTransgender", table: "FactoryLicenses");
            migrationBuilder.DropColumn(name: "WorkersOrdinaryMale", table: "FactoryLicenses");
            migrationBuilder.DropColumn(name: "WorkersOrdinaryFemale", table: "FactoryLicenses");
            migrationBuilder.DropColumn(name: "WorkersOrdinaryTransgender", table: "FactoryLicenses");
            migrationBuilder.DropColumn(name: "SanctionedLoad", table: "FactoryLicenses");
            migrationBuilder.DropColumn(name: "SanctionedLoadUnit", table: "FactoryLicenses");
            migrationBuilder.DropColumn(name: "ManufacturingProcessLast12Months", table: "FactoryLicenses");
            migrationBuilder.DropColumn(name: "ManufacturingProcessNext12Months", table: "FactoryLicenses");
            migrationBuilder.DropColumn(name: "DateOfStartProduction", table: "FactoryLicenses");
        }
    }
}
