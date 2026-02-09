using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RajFabAPI.Migrations
{
    public partial class CreateManagerChangeTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ManagerChanges",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NoticeNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FactoryRegistrationId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FactoryRegistrationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FactoryName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FactoryAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactoryArea = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FactoryDistrict = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OutgoingManagerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NewManagerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NewManagerFatherName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DateOfAppointment = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResidencePlot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ResidenceDistrict = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ResidenceArea = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ResidenceStreet = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ResidenceCity = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ResidencePincode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ResidenceMobile = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    ReviewComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmittedBy = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagerChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManagerChanges_FactoryRegistrations_FactoryRegistrationId",
                        column: x => x.FactoryRegistrationId,
                        principalTable: "FactoryRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ManagerChangeDocuments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ManagerChangeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagerChangeDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManagerChangeDocuments_ManagerChanges_ManagerChangeId",
                        column: x => x.ManagerChangeId,
                        principalTable: "ManagerChanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ManagerChanges_FactoryRegistrationId",
                table: "ManagerChanges",
                column: "FactoryRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_ManagerChanges_NoticeNumber",
                table: "ManagerChanges",
                column: "NoticeNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ManagerChanges_Status",
                table: "ManagerChanges",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ManagerChangeDocuments_ManagerChangeId",
                table: "ManagerChangeDocuments",
                column: "ManagerChangeId");

            migrationBuilder.CreateIndex(
                name: "IX_ManagerChangeDocuments_DocumentType",
                table: "ManagerChangeDocuments",
                column: "DocumentType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ManagerChangeDocuments");

            migrationBuilder.DropTable(
                name: "ManagerChanges");
        }
    }
}
