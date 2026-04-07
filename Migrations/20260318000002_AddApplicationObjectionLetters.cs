using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RajFabAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationObjectionLetters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationObjectionLetters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModuleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GeneratedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GeneratedByName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SignatoryDesignation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SignatoryLocation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationObjectionLetters", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationObjectionLetters_ApplicationId",
                table: "ApplicationObjectionLetters",
                column: "ApplicationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ApplicationObjectionLetters");
        }
    }
}
