using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RajFabAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTransgenderAndPremiseOwnerDetailsAndUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Areas_Districts_DistrictId1",
                table: "Areas");

            migrationBuilder.DropForeignKey(
                name: "FK_FactoryMapApprovals_FactoryTypes_Old_FactoryTypeId",
                table: "FactoryMapApprovals");

            migrationBuilder.DropForeignKey(
                name: "FK_FactoryTypeDocuments_FactoryTypes_Old_FactoryTypeId",
                table: "FactoryTypeDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProcessDocuments_ManufacturingProcessTypes_ManufacturingProcessTypeId",
                table: "ProcessDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "UserPrivileges");

            migrationBuilder.DropIndex(
                name: "IX_Users_RoleId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Roles_Name",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Privileges_Module_Action",
                table: "Privileges");

            migrationBuilder.DropIndex(
                name: "IX_Modules_Category",
                table: "Modules");

            migrationBuilder.DropIndex(
                name: "IX_FactoryTypeDocuments_FactoryTypeId",
                table: "FactoryTypeDocuments");

            migrationBuilder.DropIndex(
                name: "IX_FactoryMapApprovals_AcknowledgementNumber",
                table: "FactoryMapApprovals");

            migrationBuilder.DropIndex(
                name: "IX_FactoryMapApprovals_Email",
                table: "FactoryMapApprovals");

            migrationBuilder.DropIndex(
                name: "IX_FactoryMapApprovals_FactoryTypeId",
                table: "FactoryMapApprovals");

            migrationBuilder.DropIndex(
                name: "IX_FactoryMapApprovals_Status",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "ProcessTypeId",
                table: "ProcessDocuments");

            migrationBuilder.DropColumn(
                name: "Module",
                table: "Privileges");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "ApplicantName",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "BuildingArea",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "Comments",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "District",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "FactoryTypeId",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "MobileNo",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "Pincode",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "ReviewedBy",
                table: "FactoryMapApprovals");

            migrationBuilder.RenameColumn(
                name: "ReviewedAt",
                table: "FactoryMapApprovals",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "PlotArea",
                table: "FactoryMapApprovals",
                newName: "AreaFactoryPremise");

            migrationBuilder.RenameColumn(
                name: "FactoryName",
                table: "FactoryMapApprovals",
                newName: "PlantParticulars");

            migrationBuilder.RenameColumn(
                name: "DistrictId1",
                table: "Areas",
                newName: "CityId");

            migrationBuilder.RenameIndex(
                name: "IX_Areas_DistrictId1",
                table: "Areas",
                newName: "IX_Areas_CityId");

            migrationBuilder.AddColumn<string>(
                name: "BRNNumber",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CitizenCategory",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LINNumber",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserType",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "OfficeId",
                table: "Roles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PostId",
                table: "Roles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "ManufacturingProcessTypeId",
                table: "ProcessDocuments",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DocumentTypeId",
                table: "ProcessDocuments",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<Guid>(
                name: "ModuleId",
                table: "Privileges",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "PanCard",
                table: "Occupiers",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ActId",
                table: "Modules",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "RuleId",
                table: "Modules",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "FactoryTypeId",
                table: "FactoryTypeDocuments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "DocumentTypeId",
                table: "FactoryTypeDocuments",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "FactoryTypeOldId",
                table: "FactoryTypeDocuments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OccupierCityTown",
                table: "FactoryRegistrations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "LandOwnerCityTown",
                table: "FactoryRegistrations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FactoryManagerCityTown",
                table: "FactoryRegistrations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CityTown",
                table: "FactoryRegistrations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "AmendmentCount",
                table: "FactoryRegistrations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AssignedTo",
                table: "FactoryRegistrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedToName",
                table: "FactoryRegistrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentStage",
                table: "FactoryRegistrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FactoryManagerPanCard",
                table: "FactoryRegistrations",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OccupierPanCard",
                table: "FactoryRegistrations",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationPDFUrl",
                table: "FactoryMapApprovals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FactoryDetails",
                table: "FactoryMapApprovals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsESignCompleted",
                table: "FactoryMapApprovals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsNew",
                table: "FactoryMapApprovals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ManufacturingProcess",
                table: "FactoryMapApprovals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MaxWorkerFemale",
                table: "FactoryMapApprovals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxWorkerMale",
                table: "FactoryMapApprovals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxWorkerTransgender",
                table: "FactoryMapApprovals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NoOfFactoriesIfCommonPremise",
                table: "FactoryMapApprovals",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObjectionLetterUrl",
                table: "FactoryMapApprovals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OccupierDetails",
                table: "FactoryMapApprovals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Place",
                table: "FactoryMapApprovals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PremiseOwnerAddressCity",
                table: "FactoryMapApprovals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PremiseOwnerAddressDistrict",
                table: "FactoryMapApprovals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PremiseOwnerAddressPinCode",
                table: "FactoryMapApprovals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PremiseOwnerAddressPlotNo",
                table: "FactoryMapApprovals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PremiseOwnerAddressState",
                table: "FactoryMapApprovals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PremiseOwnerAddressStreet",
                table: "FactoryMapApprovals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PremiseOwnerContactNo",
                table: "FactoryMapApprovals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PremiseOwnerDetails",
                table: "FactoryMapApprovals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PremiseOwnerName",
                table: "FactoryMapApprovals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "FactoryMapApprovals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Version",
                table: "FactoryMapApprovals",
                type: "decimal(3,1)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "DocumentTypes",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "ConditionalField",
                table: "DocumentTypes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConditionalValue",
                table: "DocumentTypes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsConditional",
                table: "DocumentTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Module",
                table: "DocumentTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ServiceType",
                table: "DocumentTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Districts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "Acts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ImplementationYear = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Acts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AnnualReturns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FactoryRegistrationNumber = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    FormData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<decimal>(type: "decimal(3,1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnnualReturns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Appeals",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FactoryRegistrationNumber = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AppealRegistrationNumber = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AppealApplicationNumber = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DateOfAccident = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateOfInspection = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NoticeNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NoticeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrderNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FactsAndGrounds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReliefSought = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChallanNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnclosureDetails1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnclosureDetails2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SignatureOfOccupier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Signature = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Place = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<decimal>(type: "decimal(3,1)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ESignPrnNumberOccupier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ESignPrnNumberManager = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsESignCompletedOccupier = table.Column<bool>(type: "bit", nullable: false),
                    IsESignCompletedManager = table.Column<bool>(type: "bit", nullable: false),
                    ApplicationPDFUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ESignPrnNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsESignCompleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appeals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationApprovalRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationRegistrationId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApplicationWorkFlowLevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationApprovalRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApplicationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreviousStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionByName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ForwardedTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ForwardedToName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationHistories", x => x.Id);
                });

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
                    Version = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationObjectionLetters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationRegistrations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApplicationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApplicationRegistrationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ESignPrnNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationRegistrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AudioVisualWorks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubDivisionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TehsilId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Area = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pincode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxNumberOfWorkerAnyDay = table.Column<int>(type: "int", nullable: true),
                    DateOfCompletion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AudioVisualWorks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BeediCigarWorks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManufacturingType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManufacturingDetail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Situation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubDivisionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TehsilId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Area = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pincode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmployerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MaxNumberOfWorkerAnyDay = table.Column<int>(type: "int", nullable: true),
                    NumberOfHomeWorker = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeediCigarWorks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoilerApplicationStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentPart = table.Column<int>(type: "int", nullable: false),
                    CurrentLevel = table.Column<int>(type: "int", nullable: false),
                    AssignedInspectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AuthorityForwardedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InspectorActionsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ChiefCycleCount = table.Column<int>(type: "int", nullable: false),
                    LastChiefActionValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CertificatePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerApplicationStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoilerApplicationStates_Users_AssignedInspectorId",
                        column: x => x.AssignedInspectorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BoilerCertificates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CertificateNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CertificateType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InspectorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InspectorId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerCertificates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoilerDocumentTypes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: false),
                    BoilerServiceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DocumentTypeId = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    ConditionalField = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ConditionalValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerDocumentTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoilerDocumentTypes_DocumentTypes_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoilerDrawingApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BoilerDrawingRegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FactoryRegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FactoryDetailjson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MakerNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MakerNameAndAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeatingSurfaceArea = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EvaporationCapacity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IntendedWorkingPressure = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BoilerType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DrawingNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BoilerDrawing = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FeedPipelineDrawing = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PressurePartCalculation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidUpto = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Version = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerDrawingApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoilerDrawingClosures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BoilerDrawingRegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClosureReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClosureDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerDrawingClosures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoilerLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FactoryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FactoryLicenseNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PlotNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Locality = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Pincode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AreaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DistrictId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DivisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(10,8)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(11,8)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoilerLocations_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BoilerLocations_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BoilerLocations_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BoilerLocations_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoilerManufactureClosures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManufactureRegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClosureReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClosureDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerManufactureClosures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoilerManufactureRegistrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FactoryRegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApplicationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManufactureRegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BmClassification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidUpto = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CoveredArea = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstablishmentJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManufacturingFacilityjson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetailInternalQualityjson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OtherReleventInformationjson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Version = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerManufactureRegistrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoilerRegistrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FactoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApplicationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BoilerRegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldRegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldStateName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPaymentCompleted = table.Column<bool>(type: "bit", nullable: false),
                    IsESignCompleted = table.Column<bool>(type: "bit", nullable: false),
                    ApplicationPDFUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Version = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerRegistrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoilerRepairerClosures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RepairerRegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClosureReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClosureDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerRepairerClosures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoilerRepairerRegistrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FactoryRegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApplicationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RepairerRegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BrClassification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstablishmentJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidUpto = table.Column<DateTime>(type: "datetime2", nullable: true),
                    JobsExecutedJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentEvidence = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovalHistoryJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RejectedHistoryJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToolsAvailable = table.Column<bool>(type: "bit", nullable: true),
                    SimultaneousSites = table.Column<int>(type: "int", nullable: true),
                    AcceptsRegulations = table.Column<bool>(type: "bit", nullable: true),
                    AcceptsResponsibility = table.Column<bool>(type: "bit", nullable: true),
                    CanSupplyMaterial = table.Column<bool>(type: "bit", nullable: true),
                    QualityControlType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QualityControlDetailsjson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Version = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerRepairerRegistrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoilerSafetyFeatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SafetyValves = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WaterGauges = table.Column<int>(type: "int", nullable: false),
                    PressureGauges = table.Column<int>(type: "int", nullable: false),
                    FusiblePlugs = table.Column<int>(type: "int", nullable: true),
                    BlowdownValves = table.Column<int>(type: "int", nullable: false),
                    FeedwaterSystem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EmergencyShutoff = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerSafetyFeatures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoilerSpecifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoilerType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Manufacturer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    YearOfManufacture = table.Column<int>(type: "int", nullable: false),
                    WorkingPressure = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DesignPressure = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    SteamCapacity = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    FuelType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HeatingArea = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    SuperheaterArea = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    EconomiserArea = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    AirPreheaterArea = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerSpecifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoilerWorkflowLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Part = table.Column<int>(type: "int", nullable: false),
                    FromUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ToUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FromLevel = table.Column<int>(type: "int", nullable: true),
                    ToLevel = table.Column<int>(type: "int", nullable: true),
                    ActionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CycleNumber = table.Column<int>(type: "int", nullable: true),
                    ChiefActionValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerWorkflowLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BuildingAndConstructionWorks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProbablePeriodOfCommencementOfWork = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpectedPeriodOfCommencementOfWork = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocalAuthorityApprovalDetail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfCompletion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingAndConstructionWorks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CertificateVersion = table.Column<decimal>(type: "decimal(3,1)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CertificateUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssuedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsESignCompleted = table.Column<bool>(type: "bit", nullable: false),
                    ESignPrnNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChiefInspectionScrutinyRemarks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RemarkText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiefInspectionScrutinyRemarks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommencementCessationApplication",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    FactoryRegistrationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateOfCessation = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApproxDurationOfWork = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsESignCompleted = table.Column<bool>(type: "bit", nullable: false),
                    ApplicationPDFUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Version = table.Column<decimal>(type: "decimal(3,1)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommencementCessationApplication", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompetentEquipmentRegistrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompetentRegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompetentEquipmentRegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RenewalYears = table.Column<int>(type: "int", nullable: true),
                    ValidUpto = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetentEquipmentRegistrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompetentPersonRegistrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompetentRegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegistrationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RenewalYears = table.Column<int>(type: "int", nullable: false),
                    ValidUpto = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetentPersonRegistrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentUploads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleDocType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DocumentName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentSize = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentUploads", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EconomiserClosures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EconomiserRegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClosureReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClosureDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EconomiserClosures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EconomiserRegistrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EconomiserRegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FactoryRegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FactoryDetailJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MakersNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MakersName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MakersAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YearOfMake = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PressureFrom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PressureTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErectionType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OutletTemperature = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalHeatingSurfaceArea = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumberOfTubes = table.Column<int>(type: "int", nullable: true),
                    NumberOfHeaders = table.Column<int>(type: "int", nullable: true),
                    FormIB = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormIC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormIVA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormIVB = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormIVC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormIVD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormVA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormXV = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormXVI = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttendantCertificate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EngineerCertificate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Drawings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidUpto = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EconomiserRegistrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ESignTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EncryptedPrn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrnHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TxnId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SignedPdfPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ESignTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EstablishmentDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LinNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BrnNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PanNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FactoryTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstablishmentName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubDivisionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TehsilId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Area = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pincode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalNumberOfEmployee = table.Column<int>(type: "int", maxLength: 100, nullable: true),
                    TotalNumberOfContractEmployee = table.Column<int>(type: "int", maxLength: 100, nullable: true),
                    TotalNumberOfInterstateWorker = table.Column<int>(type: "int", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstablishmentDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EstablishmentRegistrations",
                columns: table => new
                {
                    EstablishmentRegistrationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EstablishmentDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MainOwnerDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ManagerOrAgentDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FactoryCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OccupierIdProof = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PartnershipDeed = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ManagerIdProof = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LoadSanctionCopy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Version = table.Column<decimal>(type: "decimal(3,1)", nullable: false),
                    Place = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Signature = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Date = table.Column<DateTime>(type: "datetime", nullable: true),
                    AutoRenewal = table.Column<bool>(type: "bit", nullable: false),
                    IsESignCompleted = table.Column<bool>(type: "bit", nullable: false),
                    IsPaymentCompleted = table.Column<bool>(type: "bit", nullable: false),
                    ApplicationPDFUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ObjectionLetterUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ESignPrnNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstablishmentRegistrations", x => x.EstablishmentRegistrationId);
                });

            migrationBuilder.CreateTable(
                name: "EstablishmentUserDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TypeOfEmployer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RelationType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelativeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    District = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Tehsil = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Area = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Pincode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Telephone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstablishmentUserDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FactoryClosures",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClosureNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactoryRegistrationId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FactoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccupierName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactoryAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FeesDue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LastRenewalDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosureDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReasonForClosure = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InspectingOfficerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InspectionRemarks = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InspectionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentStage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedToName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactoryClosures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactoryClosures_FactoryRegistrations_FactoryRegistrationId",
                        column: x => x.FactoryRegistrationId,
                        principalTable: "FactoryRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FactoryDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManufacturingType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManufacturingDetail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Situation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubDivisionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TehsilId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Area = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pincode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumberOfWorker = table.Column<int>(type: "int", nullable: true),
                    SanctionedLoad = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SanctionedLoadUnit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmployerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnershipType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnershipSector = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActivityAsPerNIC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NICCodeDetail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdentificationOfEstablishment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactoryDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FactoryLicenses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    FactoryLicenseNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FactoryRegistrationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NoOfYears = table.Column<int>(type: "int", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Place = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ManagerSignature = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OccupierSignature = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuthorisedSignature = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<decimal>(type: "decimal(3,1)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsESignCompletedOccupier = table.Column<bool>(type: "bit", nullable: false),
                    IsESignCompletedManager = table.Column<bool>(type: "bit", nullable: false),
                    ESignPrnNumberManager = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ESignPrnNumberOccupier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsPaymentCompleted = table.Column<bool>(type: "bit", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ApplicationPDFUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ObjectionLetterUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ESignPrnNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsESignCompleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactoryLicenses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FactoryMapApprovalChemicals",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FactoryMapApprovalId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TradeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ChemicalName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MaxStorageQuantity = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactoryMapApprovalChemicals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactoryMapApprovalChemicals_FactoryMapApprovals_FactoryMapApprovalId",
                        column: x => x.FactoryMapApprovalId,
                        principalTable: "FactoryMapApprovals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FactoryMapDangerousOperations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FactoryMapApprovalId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ChemicalName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OrganicInorganicDetails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactoryMapDangerousOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactoryMapDangerousOperations_FactoryMapApprovals_FactoryMapApprovalId",
                        column: x => x.FactoryMapApprovalId,
                        principalTable: "FactoryMapApprovals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FactoryMapFinishGoods",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FactoryMapApprovalId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    QuantityPerDay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaxStorageCapacity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    StorageMethod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactoryMapFinishGoods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactoryMapFinishGoods_FactoryMapApprovals_FactoryMapApprovalId",
                        column: x => x.FactoryMapApprovalId,
                        principalTable: "FactoryMapApprovals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FactoryMapIntermediateProducts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FactoryMapApprovalId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MaxStorageQuantity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactoryMapIntermediateProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactoryMapIntermediateProducts_FactoryMapApprovals_FactoryMapApprovalId",
                        column: x => x.FactoryMapApprovalId,
                        principalTable: "FactoryMapApprovals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FactoryMapRawMaterials",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FactoryMapApprovalId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaterialName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MaxStorageQuantity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactoryMapRawMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactoryMapRawMaterials_FactoryMapApprovals_FactoryMapApprovalId",
                        column: x => x.FactoryMapApprovalId,
                        principalTable: "FactoryMapApprovals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FactoryRegistrationFees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FactoryRegistrationId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TotalWorkers = table.Column<int>(type: "int", nullable: false),
                    TotalPowerHP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPowerKW = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FactoryFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ElectricityFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeeBreakdown = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CalculatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactoryRegistrationFees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactoryRegistrationFees_FactoryRegistrations_FactoryRegistrationId",
                        column: x => x.FactoryRegistrationId,
                        principalTable: "FactoryRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FactoryTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactoryTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeeResults",
                columns: table => new
                {
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "InspectionFormSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InspectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Photos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Documents = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GeneratedPdfPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ESignData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PreviewGeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionFormSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionFormSubmissions_Users_InspectorId",
                        column: x => x.InspectorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InspectionSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InspectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InspectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InspectionTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    PlaceAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InspectionType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstimatedDuration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InspectorNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionSchedules_Users_InspectorId",
                        column: x => x.InspectorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InspectorApplicationAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationRegistrationId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApplicationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApplicationTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApplicationRegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectorApplicationAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectorApplicationAssignments_Users_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InspectorApplicationAssignments_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LicenseRenewals",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RenewalNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalRegistrationId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalRegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RenewalYears = table.Column<int>(type: "int", nullable: false),
                    LicenseRenewalFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LicenseRenewalTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FactoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactoryRegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlotNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StreetLocality = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CityTown = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    District = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Area = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pincode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManufacturingProcess = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductionStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ManufacturingProcessLast12Months = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ManufacturingProcessNext12Months = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductDetailsLast12Months = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaxWorkersMaleProposed = table.Column<int>(type: "int", nullable: false),
                    MaxWorkersFemaleProposed = table.Column<int>(type: "int", nullable: false),
                    MaxWorkersTransgenderProposed = table.Column<int>(type: "int", nullable: false),
                    MaxWorkersMaleEmployed = table.Column<int>(type: "int", nullable: false),
                    MaxWorkersFemaleEmployed = table.Column<int>(type: "int", nullable: false),
                    MaxWorkersTransgenderEmployed = table.Column<int>(type: "int", nullable: false),
                    WorkersMaleOrdinary = table.Column<int>(type: "int", nullable: false),
                    WorkersFemaleOrdinary = table.Column<int>(type: "int", nullable: false),
                    WorkersTransgenderOrdinary = table.Column<int>(type: "int", nullable: false),
                    TotalRatedHorsePower = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaximumPowerToBeUsed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FactoryManagerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactoryManagerFatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactoryManagerAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccupierType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccupierName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccupierFatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccupierAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LandOwnerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LandOwnerAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BuildingPlanReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BuildingPlanApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WasteDisposalReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WasteDisposalApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WasteDisposalAuthority = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Place = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeclarationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Declaration1Accepted = table.Column<bool>(type: "bit", nullable: false),
                    Declaration2Accepted = table.Column<bool>(type: "bit", nullable: false),
                    Declaration3Accepted = table.Column<bool>(type: "bit", nullable: false),
                    PaymentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentTransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AmendmentCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseRenewals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ManagerChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FactoryRegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcknowledgementNumber = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    OldManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NewManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SignatureofOccupier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SignatureOfNewManager = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfAppointment = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<decimal>(type: "decimal(3,1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagerChanges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MapApprovalFactoryDetails",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FactoryMapApprovalId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FactoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactorySituation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactoryPlotNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DivisionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DistrictId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AreaId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactoryPincode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapApprovalFactoryDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MapApprovalFactoryDetails_FactoryMapApprovals_FactoryMapApprovalId",
                        column: x => x.FactoryMapApprovalId,
                        principalTable: "FactoryMapApprovals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MapApprovalOccupierDetails",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FactoryMapApprovalId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RelationTypeId = table.Column<int>(type: "int", nullable: false),
                    RelativeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OfficeAddressPlotno = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OfficeAddressStreet = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OfficeAddressCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OfficeAddressDistrict = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OfficeAddressState = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OfficeAddressPinCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResidentialAddressPlotno = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResidentialAddressStreet = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ResidentialAddressCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ResidentialAddressDistrict = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResidentialAddressState = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResidentialAddressPinCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    OccupierMobile = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    OccupierEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapApprovalOccupierDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MapApprovalOccupierDetails_FactoryMapApprovals_FactoryMapApprovalId",
                        column: x => x.FactoryMapApprovalId,
                        principalTable: "FactoryMapApprovals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Masters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComboName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    OptionId = table.Column<int>(type: "int", nullable: false),
                    OptionValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Masters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModulePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PermissionCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModulePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModulePermissions_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MotorTransportServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NatureOfService = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Situation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubDivisionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TehsilId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Area = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pincode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmployerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MaxNumberOfWorkerDuringRegistration = table.Column<int>(type: "int", nullable: true),
                    TotalNumberOfVehicles = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotorTransportServices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NewsPaperEstablishments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubDivisionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TehsilId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Area = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pincode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxNumberOfWorkerAnyDay = table.Column<int>(type: "int", nullable: true),
                    DateOfCompletion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsPaperEstablishments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NonHazardousFactoryRegistrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FactoryName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ApplicantName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RelationType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RelationName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ApplicantAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AreaId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DistrictId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DivisionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FactoryAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FactoryPincode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DeclarationAccepted = table.Column<bool>(type: "bit", nullable: false),
                    RequiredInfoAccepted = table.Column<bool>(type: "bit", nullable: false),
                    VerifyAccepted = table.Column<bool>(type: "bit", nullable: false),
                    WorkersLimitAccepted = table.Column<bool>(type: "bit", nullable: false),
                    ApplicationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApplicationPlace = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApplicantSignature = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    VerifyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VerifyPlace = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VerifierSignature = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NonHazardousFactoryRegistrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OfficeLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LevelOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfficeLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OfficePostLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfficeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfficeLevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfficePostLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Offices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Pincode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    LevelCount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsHeadOffice = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DistrictId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Offices_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Offices_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Plantations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubDivisionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TehsilId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Area = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pincode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxNumberOfWorkerAnyDay = table.Column<int>(type: "int", nullable: true),
                    DateOfCompletion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plantations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegisteredBoilerNews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FactoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactoryRegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OwnerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErectionType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DivisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DistrictId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AreaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BoilerRegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pincode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MobileNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MakerNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MakerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MakerAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YearOfMake = table.Column<int>(type: "int", nullable: true),
                    HeatingSurfaceArea = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EvaporationCapacity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IntendedWorkingPressure = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BoilerType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RepairModificationName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RepairModificationAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RepairModificationType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BoilerClosureOrTransferType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClosureOrTransferDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosureOrTransferDocument = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClosureOrTransferRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisteredBoilerNews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleLocationAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DivisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DistrictId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AreaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleLocationAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleLocationAssignments_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoleLocationAssignments_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoleLocationAssignments_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoleLocationAssignments_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleLocationAssignments_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePrivileges",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrivilegeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePrivileges", x => new { x.RoleId, x.PrivilegeId });
                    table.ForeignKey(
                        name: "FK_RolePrivileges_Privileges_PrivilegeId",
                        column: x => x.PrivilegeId,
                        principalTable: "Privileges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePrivileges_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleA_FactoryFees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MinWorkers = table.Column<int>(type: "int", nullable: false),
                    MaxWorkers = table.Column<int>(type: "int", nullable: false),
                    FeeUpTo9HP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeeUpTo20HP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeeUpTo50HP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeeUpTo100HP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeeUpTo250HP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeeUpTo500HP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeeUpTo750HP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeeUpTo1000HP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeeUpTo1500HP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeeUpTo2000HP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeeUpTo3000HP = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleA_FactoryFees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleB_ElectricityFees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CapacityKW = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GeneratingFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransformingFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransmittingFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleB_ElectricityFees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SMTCRegistrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SMTCRegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FactoryRegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrainingCenterAvailable = table.Column<bool>(type: "bit", nullable: false),
                    SeatingCapacity = table.Column<int>(type: "int", nullable: true),
                    TrainingCenterPhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AudioVideoFacility = table.Column<bool>(type: "bit", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ValidUpto = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SMTCRegistrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SteamPipeLineApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BoilerApplicationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SteamPipeLineRegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProposedLayoutDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsentLetterProvided = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SteamPipeLineDrawingNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BoilerMakerRegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErectorName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FactoryRegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Factorydetailjson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PipeLengthUpTo100mm = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PipeLengthAbove100mm = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NoOfDeSuperHeaters = table.Column<int>(type: "int", nullable: true),
                    NoOfSteamReceivers = table.Column<int>(type: "int", nullable: true),
                    NoOfFeedHeaters = table.Column<int>(type: "int", nullable: true),
                    NoOfSeparatelyFiredSuperHeaters = table.Column<int>(type: "int", nullable: true),
                    RenewalYears = table.Column<int>(type: "int", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidUpto = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FormIIPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormIIIPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormIIIAPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormIIIBPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormIVPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormIVAPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DrawingPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupportingDocumentsPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SteamPipeLineApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SteamPipeLineClosures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SteamPipeLineRegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReasonForClosure = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupportingDocumentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SteamPipeLineClosures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tehsils",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameHindi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DistrictId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tehsils", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tehsils_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrnNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReqTimeStamp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RPPTXNID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentReq = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentRes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ApplicationId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAreaAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AreaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAreaAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAreaAssignments_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAreaAssignments_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserAreaAssignments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserHierarchies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportsToId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmergencyReportToId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHierarchies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserHierarchies_Users_EmergencyReportToId",
                        column: x => x.EmergencyReportToId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserHierarchies_Users_ReportsToId",
                        column: x => x.ReportsToId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserHierarchies_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLocationAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DivisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DistrictId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AreaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLocationAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLocationAssignments_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserLocationAssignments_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserLocationAssignments_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserLocationAssignments_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserLocationAssignments_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserLocationAssignments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserModulePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Permissions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserModulePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserModulePermissions_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserModulePermissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JoiningDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JoiningDetail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JoiningType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsInspector = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WelderApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WelderRegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Version = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidUpto = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WelderApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WelderClosures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WelderRegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClosureReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClosureDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WelderClosures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkerRanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MinWorkers = table.Column<int>(type: "int", nullable: false),
                    MaxWorkers = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkerRanges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ImplementationYear = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ActId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rules_Acts_ActId",
                        column: x => x.ActId,
                        principalTable: "Acts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DesignFacilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoilerManufactureRegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DistrictId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubDivisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TehsilId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Area = table.Column<int>(type: "int", nullable: true),
                    PinCode = table.Column<int>(type: "int", nullable: true),
                    Document = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesignFacilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DesignFacilities_BoilerManufactureRegistrations_BoilerManufactureRegistrationId",
                        column: x => x.BoilerManufactureRegistrationId,
                        principalTable: "BoilerManufactureRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NDTPersonnels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoilerManufactureRegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Qualification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Certificate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NDTPersonnels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NDTPersonnels_BoilerManufactureRegistrations_BoilerManufactureRegistrationId",
                        column: x => x.BoilerManufactureRegistrationId,
                        principalTable: "BoilerManufactureRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QualifiedWelders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoilerManufactureRegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Qualification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Certificate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualifiedWelders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualifiedWelders_BoilerManufactureRegistrations_BoilerManufactureRegistrationId",
                        column: x => x.BoilerManufactureRegistrationId,
                        principalTable: "BoilerManufactureRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RDFacilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoilerManufactureRegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DistrictId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubDivisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TehsilId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Area = table.Column<int>(type: "int", nullable: true),
                    PinCode = table.Column<int>(type: "int", nullable: true),
                    RDFacilityJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RDFacilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RDFacilities_BoilerManufactureRegistrations_BoilerManufactureRegistrationId",
                        column: x => x.BoilerManufactureRegistrationId,
                        principalTable: "BoilerManufactureRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TechnicalManpowers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoilerManufactureRegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Qualification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MinimumFiveYearsExperienceDoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExperienceInErectionDoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExperienceInCommissioningDoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnicalManpowers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TechnicalManpowers_BoilerManufactureRegistrations_BoilerManufactureRegistrationId",
                        column: x => x.BoilerManufactureRegistrationId,
                        principalTable: "BoilerManufactureRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestingFacilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoilerManufactureRegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DistrictId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubDivisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TehsilId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Area = table.Column<int>(type: "int", nullable: true),
                    PinCode = table.Column<int>(type: "int", nullable: true),
                    TestingFacilityJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestingFacilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestingFacilities_BoilerManufactureRegistrations_BoilerManufactureRegistrationId",
                        column: x => x.BoilerManufactureRegistrationId,
                        principalTable: "BoilerManufactureRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoilerClosures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoilerRegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoilerRegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClosureType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClosureDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToStateName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reasons = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClosureReportPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerClosures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoilerClosures_BoilerRegistrations_BoilerRegistrationId",
                        column: x => x.BoilerRegistrationId,
                        principalTable: "BoilerRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoilerDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoilerRegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DistrictId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubDivisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TehsilId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Area = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PinCode = table.Column<int>(type: "int", nullable: true),
                    RenewalYears = table.Column<int>(type: "int", nullable: true),
                    ValidUpto = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErectionTypeId = table.Column<int>(type: "int", nullable: true),
                    MakerNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YearOfMake = table.Column<int>(type: "int", nullable: true),
                    HeatingSurfaceArea = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EvaporationCapacity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EvaporationUnit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IntendedWorkingPressure = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PressureUnit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BoilerType = table.Column<int>(type: "int", nullable: true),
                    BoilerCategory = table.Column<int>(type: "int", nullable: true),
                    Superheater = table.Column<bool>(type: "bit", nullable: true),
                    SuperheaterOutletTemp = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Economiser = table.Column<bool>(type: "bit", nullable: true),
                    EconomiserOutletTemp = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FurnaceType = table.Column<int>(type: "int", nullable: true),
                    DrawingsPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecificationPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormI_B_CPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormI_DPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormI_EPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormIV_APath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormV_APath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TestCertificatesPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WeldRepairChartsPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PipesCertificatesPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TubesCertificatesPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CastingCertificatePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ForgingCertificatePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeadersCertificatePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DishedEndsInspectionPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BoilerAttendantCertificatePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BoilerOperationEngineerCertificatePath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoilerDetails_BoilerRegistrations_BoilerRegistrationId",
                        column: x => x.BoilerRegistrationId,
                        principalTable: "BoilerRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoilerRegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TypeOfEmployer = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Designation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RelationType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelativeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    District = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Tehsil = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Area = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Pincode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonDetails_BoilerRegistrations_BoilerRegistrationId",
                        column: x => x.BoilerRegistrationId,
                        principalTable: "BoilerRegistrations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BoilerRepairerEngineers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoilerRepairerRegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Qualification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExperienceYears = table.Column<int>(type: "int", nullable: false),
                    DocumentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerRepairerEngineers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoilerRepairerEngineers_BoilerRepairerRegistrations_BoilerRepairerRegistrationId",
                        column: x => x.BoilerRepairerRegistrationId,
                        principalTable: "BoilerRepairerRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoilerRepairerWelders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoilerRepairerRegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExperienceYears = table.Column<int>(type: "int", nullable: false),
                    CertificatePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerRepairerWelders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoilerRepairerWelders_BoilerRepairerRegistrations_BoilerRepairerRegistrationId",
                        column: x => x.BoilerRepairerRegistrationId,
                        principalTable: "BoilerRepairerRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegisteredBoilers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SpecificationsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SafetyFeaturesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentCertificateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OwnerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OperatorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OperatorCertificateNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OperatorCertificateExpiry = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AreaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisteredBoilers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegisteredBoilers_BoilerCertificates_CurrentCertificateId",
                        column: x => x.CurrentCertificateId,
                        principalTable: "BoilerCertificates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegisteredBoilers_BoilerLocations_AreaId",
                        column: x => x.AreaId,
                        principalTable: "BoilerLocations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RegisteredBoilers_BoilerSafetyFeatures_SafetyFeaturesId",
                        column: x => x.SafetyFeaturesId,
                        principalTable: "BoilerSafetyFeatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegisteredBoilers_BoilerSpecifications_SpecificationsId",
                        column: x => x.SpecificationsId,
                        principalTable: "BoilerSpecifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompetentPersonEquipments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EquipmentRegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompetentPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EquipmentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EquipmentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdentificationNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CalibrationCertificateNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfCalibration = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CalibrationValidity = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CalibrationCertificatePath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetentPersonEquipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompetentPersonEquipments_CompetentEquipmentRegistrations_EquipmentRegistrationId",
                        column: x => x.EquipmentRegistrationId,
                        principalTable: "CompetentEquipmentRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompetantEstablishmentDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstablishmentName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DistrictId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TehsilId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SdoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Area = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pincode = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetantEstablishmentDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompetantEstablishmentDetails_CompetentPersonRegistrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalTable: "CompetentPersonRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompetantOccupierDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Relation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DistrictId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TehsilId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SdoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pincode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetantOccupierDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompetantOccupierDetails_CompetentPersonRegistrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalTable: "CompetentPersonRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompetantPersonDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DOB = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Experience = table.Column<int>(type: "int", nullable: true),
                    Qualification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Engineering = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SignPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttachmentPath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetantPersonDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompetantPersonDetails_CompetentPersonRegistrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalTable: "CompetentPersonRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EstablishmentEntityMapping",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EstablishmentRegistrationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstablishmentEntityMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EstablishmentEntityMapping_EstablishmentRegistrations_EstablishmentRegistrationId",
                        column: x => x.EstablishmentRegistrationId,
                        principalTable: "EstablishmentRegistrations",
                        principalColumn: "EstablishmentRegistrationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EstablishmentRegistrationDocuments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EstablishmentRegistrationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstablishmentRegistrationId1 = table.Column<string>(type: "nvarchar(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstablishmentRegistrationDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EstablishmentRegistrationDocuments_EstablishmentRegistrations_EstablishmentRegistrationId",
                        column: x => x.EstablishmentRegistrationId,
                        principalTable: "EstablishmentRegistrations",
                        principalColumn: "EstablishmentRegistrationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EstablishmentRegistrationDocuments_EstablishmentRegistrations_EstablishmentRegistrationId1",
                        column: x => x.EstablishmentRegistrationId1,
                        principalTable: "EstablishmentRegistrations",
                        principalColumn: "EstablishmentRegistrationId");
                });

            migrationBuilder.CreateTable(
                name: "FactoryClosureDocuments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FactoryClosureId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactoryClosureDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactoryClosureDocuments_FactoryClosures_FactoryClosureId",
                        column: x => x.FactoryClosureId,
                        principalTable: "FactoryClosures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InspectorApplicationInspections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InspectorApplicationAssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InspectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BoilerCondition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaxAllowableWorkingPressure = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Observations = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefectsFound = table.Column<bool>(type: "bit", nullable: false),
                    DefectDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InspectionReportNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HydraulicTestPressure = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HydraulicTestDuration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JointsCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RivetsCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlatingCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StaysCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CrownCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FireboxCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FusiblePlugCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FireTubesCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FlueFurnaceCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SmokeBoxCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SteamDrumCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SafetyValveCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PressureGaugeCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FeedCheckCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StopValveCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BlowDownCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EconomiserCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SuperheaterCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AirPressureGaugeCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AllowedWorkingPressure = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProvisionalOrderNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProvisionalOrderDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BoilerAttendantName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BoilerAttendantCertNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FeeAmount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChallanNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectorApplicationInspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectorApplicationInspections_InspectorApplicationAssignments_InspectorApplicationAssignmentId",
                        column: x => x.InspectorApplicationAssignmentId,
                        principalTable: "InspectorApplicationAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LicenseRenewalDocuments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RenewalId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseRenewalDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LicenseRenewalDocuments_LicenseRenewals_RenewalId",
                        column: x => x.RenewalId,
                        principalTable: "LicenseRenewals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InspectionScrutinyWorkflows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfficeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LevelCount = table.Column<int>(type: "int", nullable: false),
                    IsBidirectional = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionScrutinyWorkflows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionScrutinyWorkflows_Offices_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "Offices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OfficeApplicationAreas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfficeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfficeApplicationAreas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfficeApplicationAreas_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OfficeApplicationAreas_Offices_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "Offices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OfficeInspectionAreas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfficeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfficeInspectionAreas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfficeInspectionAreas_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OfficeInspectionAreas_Offices_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "Offices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SMTCTrainerDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SMTCRegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalYearsExperience = table.Column<int>(type: "int", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DegreeDocumentPath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SMTCTrainerDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SMTCTrainerDetails_SMTCRegistrations_SMTCRegistrationId",
                        column: x => x.SMTCRegistrationId,
                        principalTable: "SMTCRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WelderDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WelderApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DOB = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdentificationMark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Weight = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Height = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    District = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tehsil = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Area = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pincode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExperienceYears = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExperienceDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExperienceCertificate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TestType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Radiography = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Materials = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfTest = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TypePosition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaterialType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaterialGrouping = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessOfWelding = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WeldWithBacking = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ElectrodeGrouping = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TestPieceXrayed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Photo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Thumb = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WelderSign = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmployerSign = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WelderDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WelderDetails_WelderApplications_WelderApplicationId",
                        column: x => x.WelderApplicationId,
                        principalTable: "WelderApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WelderEmployers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WelderApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployerType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmployerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirmName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    District = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tehsil = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Area = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pincode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmployedFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmployedTo = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WelderEmployers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WelderEmployers_WelderApplications_WelderApplicationId",
                        column: x => x.WelderApplicationId,
                        principalTable: "WelderApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FactoryCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactoryTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkerRangeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactoryCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactoryCategories_FactoryTypes_FactoryTypeId",
                        column: x => x.FactoryTypeId,
                        principalTable: "FactoryTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FactoryCategories_WorkerRanges_WorkerRangeId",
                        column: x => x.WorkerRangeId,
                        principalTable: "WorkerRanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoilerRepairModifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoilerRegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BoilerRegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PersonDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RenewalApplicationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RepairType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AttendantCertificatePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OperationEngineerCertificatePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RepairDocumentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerRepairModifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoilerRepairModifications_PersonDetails_PersonDetailId",
                        column: x => x.PersonDetailId,
                        principalTable: "PersonDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractorDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractorPersonalDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NameOfWork = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxContractWorkerCountMale = table.Column<int>(type: "int", nullable: true),
                    MaxContractWorkerCountFemale = table.Column<int>(type: "int", nullable: true),
                    MaxContractWorkerCountTransgender = table.Column<int>(type: "int", nullable: true),
                    DateOfCommencement = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateOfCompletion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractorDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractorDetails_PersonDetails_ContractorPersonalDetailId",
                        column: x => x.ContractorPersonalDetailId,
                        principalTable: "PersonDetails",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FactoryContractorMapping",
                columns: table => new
                {
                    EstablishmentRegistrationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContractorDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactoryContractorMapping", x => new { x.EstablishmentRegistrationId, x.ContractorDetailId });
                    table.ForeignKey(
                        name: "FK_FactoryContractorMapping_EstablishmentRegistrations_EstablishmentRegistrationId",
                        column: x => x.EstablishmentRegistrationId,
                        principalTable: "EstablishmentRegistrations",
                        principalColumn: "EstablishmentRegistrationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FactoryContractorMapping_PersonDetails_ContractorDetailId",
                        column: x => x.ContractorDetailId,
                        principalTable: "PersonDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoilerApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApplicationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BoilerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApplicantName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OrganizationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApplicationData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentPaths = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoilerApplications_RegisteredBoilers_BoilerId",
                        column: x => x.BoilerId,
                        principalTable: "RegisteredBoilers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BoilerInspectionHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InspectionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InspectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InspectionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InspectorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Findings = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Recommendations = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NextInspectionDue = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CertificateIssued = table.Column<bool>(type: "bit", nullable: false),
                    BoilerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerInspectionHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoilerInspectionHistories_RegisteredBoilers_BoilerId",
                        column: x => x.BoilerId,
                        principalTable: "RegisteredBoilers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InspectionScrutinyLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LevelNumber = table.Column<int>(type: "int", nullable: false),
                    OfficePostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsPrefilled = table.Column<bool>(type: "bit", nullable: false),
                    PrefillSource = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionScrutinyLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionScrutinyLevels_InspectionScrutinyWorkflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "InspectionScrutinyWorkflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SMTCTrainerEducationDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EducationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Course = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Degree = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UniversityCollege = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PassingYear = table.Column<int>(type: "int", nullable: true),
                    Specialization = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SMTCTrainerEducationDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SMTCTrainerEducationDetails_SMTCTrainerDetails_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "SMTCTrainerDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationWorkFlows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfficeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FactoryCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LevelCount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationWorkFlows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationWorkFlows_FactoryCategories_FactoryCategoryId",
                        column: x => x.FactoryCategoryId,
                        principalTable: "FactoryCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationWorkFlows_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationWorkFlows_Offices_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "Offices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleInspectionPrivileges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FactoryCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleInspectionPrivileges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleInspectionPrivileges_FactoryCategories_FactoryCategoryId",
                        column: x => x.FactoryCategoryId,
                        principalTable: "FactoryCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleInspectionPrivileges_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationWorkFlowLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationWorkFlowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LevelNumber = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationWorkFlowLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationWorkFlowLevels_ApplicationWorkFlows_ApplicationWorkFlowId",
                        column: x => x.ApplicationWorkFlowId,
                        principalTable: "ApplicationWorkFlows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_OfficeId",
                table: "Roles",
                column: "OfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_PostId",
                table: "Roles",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_Privileges_ModuleId_Action",
                table: "Privileges",
                columns: new[] { "ModuleId", "Action" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Modules_ActId",
                table: "Modules",
                column: "ActId");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_Name",
                table: "Modules",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Modules_RuleId",
                table: "Modules",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryTypeDocuments_FactoryTypeOldId",
                table: "FactoryTypeDocuments",
                column: "FactoryTypeOldId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_Module_ServiceType",
                table: "DocumentTypes",
                columns: new[] { "Module", "ServiceType" });

            migrationBuilder.CreateIndex(
                name: "IX_Districts_Name",
                table: "Districts",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationWorkFlowLevels_ApplicationWorkFlowId",
                table: "ApplicationWorkFlowLevels",
                column: "ApplicationWorkFlowId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationWorkFlows_FactoryCategoryId",
                table: "ApplicationWorkFlows",
                column: "FactoryCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationWorkFlows_ModuleId",
                table: "ApplicationWorkFlows",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationWorkFlows_OfficeId",
                table: "ApplicationWorkFlows",
                column: "OfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_BoilerApplications_BoilerId",
                table: "BoilerApplications",
                column: "BoilerId");

            migrationBuilder.CreateIndex(
                name: "IX_BoilerApplicationStates_AssignedInspectorId",
                table: "BoilerApplicationStates",
                column: "AssignedInspectorId");

            migrationBuilder.CreateIndex(
                name: "IX_BoilerClosures_BoilerRegistrationId",
                table: "BoilerClosures",
                column: "BoilerRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_BoilerDetails_BoilerRegistrationId",
                table: "BoilerDetails",
                column: "BoilerRegistrationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BoilerDocumentTypes_BoilerServiceType",
                table: "BoilerDocumentTypes",
                column: "BoilerServiceType");

            migrationBuilder.CreateIndex(
                name: "IX_BoilerDocumentTypes_DocumentTypeId",
                table: "BoilerDocumentTypes",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BoilerInspectionHistories_BoilerId",
                table: "BoilerInspectionHistories",
                column: "BoilerId");

            migrationBuilder.CreateIndex(
                name: "IX_BoilerLocations_AreaId",
                table: "BoilerLocations",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_BoilerLocations_CityId",
                table: "BoilerLocations",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_BoilerLocations_DistrictId",
                table: "BoilerLocations",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_BoilerLocations_DivisionId",
                table: "BoilerLocations",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_BoilerRepairerEngineers_BoilerRepairerRegistrationId",
                table: "BoilerRepairerEngineers",
                column: "BoilerRepairerRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_BoilerRepairerWelders_BoilerRepairerRegistrationId",
                table: "BoilerRepairerWelders",
                column: "BoilerRepairerRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_BoilerRepairModifications_PersonDetailId",
                table: "BoilerRepairModifications",
                column: "PersonDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetantEstablishmentDetails_RegistrationId",
                table: "CompetantEstablishmentDetails",
                column: "RegistrationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompetantOccupierDetails_RegistrationId",
                table: "CompetantOccupierDetails",
                column: "RegistrationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompetantPersonDetails_RegistrationId",
                table: "CompetantPersonDetails",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetentPersonEquipments_EquipmentRegistrationId",
                table: "CompetentPersonEquipments",
                column: "EquipmentRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractorDetails_ContractorPersonalDetailId",
                table: "ContractorDetails",
                column: "ContractorPersonalDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_DesignFacilities_BoilerManufactureRegistrationId",
                table: "DesignFacilities",
                column: "BoilerManufactureRegistrationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstablishmentEntityMapping_EstablishmentRegistrationId",
                table: "EstablishmentEntityMapping",
                column: "EstablishmentRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_EstablishmentRegistrationDocuments_EstablishmentRegistrationId",
                table: "EstablishmentRegistrationDocuments",
                column: "EstablishmentRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_EstablishmentRegistrationDocuments_EstablishmentRegistrationId1",
                table: "EstablishmentRegistrationDocuments",
                column: "EstablishmentRegistrationId1");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryCategories_FactoryTypeId",
                table: "FactoryCategories",
                column: "FactoryTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryCategories_WorkerRangeId",
                table: "FactoryCategories",
                column: "WorkerRangeId");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryClosureDocuments_FactoryClosureId",
                table: "FactoryClosureDocuments",
                column: "FactoryClosureId");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryClosures_FactoryRegistrationId",
                table: "FactoryClosures",
                column: "FactoryRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryContractorMapping_ContractorDetailId",
                table: "FactoryContractorMapping",
                column: "ContractorDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryMapApprovalChemicals_FactoryMapApprovalId",
                table: "FactoryMapApprovalChemicals",
                column: "FactoryMapApprovalId");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryMapDangerousOperations_FactoryMapApprovalId",
                table: "FactoryMapDangerousOperations",
                column: "FactoryMapApprovalId");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryMapFinishGoods_FactoryMapApprovalId",
                table: "FactoryMapFinishGoods",
                column: "FactoryMapApprovalId");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryMapIntermediateProducts_FactoryMapApprovalId",
                table: "FactoryMapIntermediateProducts",
                column: "FactoryMapApprovalId");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryMapRawMaterials_FactoryMapApprovalId",
                table: "FactoryMapRawMaterials",
                column: "FactoryMapApprovalId");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryRegistrationFees_FactoryRegistrationId",
                table: "FactoryRegistrationFees",
                column: "FactoryRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionFormSubmissions_InspectorId",
                table: "InspectionFormSubmissions",
                column: "InspectorId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionSchedules_InspectorId",
                table: "InspectionSchedules",
                column: "InspectorId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionScrutinyLevels_WorkflowId",
                table: "InspectionScrutinyLevels",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionScrutinyWorkflows_OfficeId",
                table: "InspectionScrutinyWorkflows",
                column: "OfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectorApplicationAssignments_AssignedByUserId",
                table: "InspectorApplicationAssignments",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectorApplicationAssignments_AssignedToUserId",
                table: "InspectorApplicationAssignments",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectorApplicationInspections_InspectorApplicationAssignmentId",
                table: "InspectorApplicationInspections",
                column: "InspectorApplicationAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseRenewalDocuments_RenewalId",
                table: "LicenseRenewalDocuments",
                column: "RenewalId");

            migrationBuilder.CreateIndex(
                name: "IX_MapApprovalFactoryDetails_FactoryMapApprovalId",
                table: "MapApprovalFactoryDetails",
                column: "FactoryMapApprovalId");

            migrationBuilder.CreateIndex(
                name: "IX_MapApprovalOccupierDetails_FactoryMapApprovalId",
                table: "MapApprovalOccupierDetails",
                column: "FactoryMapApprovalId");

            migrationBuilder.CreateIndex(
                name: "IX_ModulePermissions_ModuleId_PermissionCode",
                table: "ModulePermissions",
                columns: new[] { "ModuleId", "PermissionCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NDTPersonnels_BoilerManufactureRegistrationId",
                table: "NDTPersonnels",
                column: "BoilerManufactureRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_OfficeApplicationAreas_CityId",
                table: "OfficeApplicationAreas",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_OfficeApplicationAreas_OfficeId",
                table: "OfficeApplicationAreas",
                column: "OfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_OfficeInspectionAreas_CityId",
                table: "OfficeInspectionAreas",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_OfficeInspectionAreas_OfficeId",
                table: "OfficeInspectionAreas",
                column: "OfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_Offices_CityId",
                table: "Offices",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Offices_DistrictId",
                table: "Offices",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Offices_Name_CityId",
                table: "Offices",
                columns: new[] { "Name", "CityId" });

            migrationBuilder.CreateIndex(
                name: "IX_PersonDetails_BoilerRegistrationId",
                table: "PersonDetails",
                column: "BoilerRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_Name",
                table: "Posts",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QualifiedWelders_BoilerManufactureRegistrationId",
                table: "QualifiedWelders",
                column: "BoilerManufactureRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_RDFacilities_BoilerManufactureRegistrationId",
                table: "RDFacilities",
                column: "BoilerManufactureRegistrationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredBoilers_AreaId",
                table: "RegisteredBoilers",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredBoilers_CurrentCertificateId",
                table: "RegisteredBoilers",
                column: "CurrentCertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredBoilers_SafetyFeaturesId",
                table: "RegisteredBoilers",
                column: "SafetyFeaturesId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredBoilers_SpecificationsId",
                table: "RegisteredBoilers",
                column: "SpecificationsId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleInspectionPrivileges_FactoryCategoryId",
                table: "RoleInspectionPrivileges",
                column: "FactoryCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleInspectionPrivileges_RoleId",
                table: "RoleInspectionPrivileges",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleLocationAssignments_AreaId",
                table: "RoleLocationAssignments",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleLocationAssignments_DistrictId",
                table: "RoleLocationAssignments",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleLocationAssignments_DivisionId",
                table: "RoleLocationAssignments",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleLocationAssignments_ModuleId",
                table: "RoleLocationAssignments",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleLocationAssignments_RoleId",
                table: "RoleLocationAssignments",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePrivileges_PrivilegeId",
                table: "RolePrivileges",
                column: "PrivilegeId");

            migrationBuilder.CreateIndex(
                name: "IX_Rules_ActId",
                table: "Rules",
                column: "ActId");

            migrationBuilder.CreateIndex(
                name: "IX_SMTCTrainerDetails_SMTCRegistrationId",
                table: "SMTCTrainerDetails",
                column: "SMTCRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_SMTCTrainerEducationDetails_TrainerId",
                table: "SMTCTrainerEducationDetails",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalManpowers_BoilerManufactureRegistrationId",
                table: "TechnicalManpowers",
                column: "BoilerManufactureRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_Tehsils_DistrictId",
                table: "Tehsils",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_TestingFacilities_BoilerManufactureRegistrationId",
                table: "TestingFacilities",
                column: "BoilerManufactureRegistrationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAreaAssignments_AreaId",
                table: "UserAreaAssignments",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAreaAssignments_ModuleId",
                table: "UserAreaAssignments",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAreaAssignments_UserId_AreaId_ModuleId",
                table: "UserAreaAssignments",
                columns: new[] { "UserId", "AreaId", "ModuleId" },
                unique: true,
                filter: "[ModuleId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserHierarchies_EmergencyReportToId",
                table: "UserHierarchies",
                column: "EmergencyReportToId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHierarchies_ReportsToId",
                table: "UserHierarchies",
                column: "ReportsToId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHierarchies_UserId",
                table: "UserHierarchies",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserLocationAssignments_AreaId",
                table: "UserLocationAssignments",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLocationAssignments_DistrictId",
                table: "UserLocationAssignments",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLocationAssignments_DivisionId",
                table: "UserLocationAssignments",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLocationAssignments_ModuleId",
                table: "UserLocationAssignments",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLocationAssignments_RoleId",
                table: "UserLocationAssignments",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLocationAssignments_UserId",
                table: "UserLocationAssignments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserModulePermissions_ModuleId",
                table: "UserModulePermissions",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserModulePermissions_UserId_ModuleId",
                table: "UserModulePermissions",
                columns: new[] { "UserId", "ModuleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WelderDetails_WelderApplicationId",
                table: "WelderDetails",
                column: "WelderApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WelderEmployers_WelderApplicationId",
                table: "WelderEmployers",
                column: "WelderApplicationId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Areas_Cities_CityId",
                table: "Areas",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FactoryTypeDocuments_FactoryTypes_Old_FactoryTypeOldId",
                table: "FactoryTypeDocuments",
                column: "FactoryTypeOldId",
                principalTable: "FactoryTypes_Old",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Modules_Acts_ActId",
                table: "Modules",
                column: "ActId",
                principalTable: "Acts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Modules_Rules_RuleId",
                table: "Modules",
                column: "RuleId",
                principalTable: "Rules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Privileges_Modules_ModuleId",
                table: "Privileges",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProcessDocuments_ManufacturingProcessTypes_ManufacturingProcessTypeId",
                table: "ProcessDocuments",
                column: "ManufacturingProcessTypeId",
                principalTable: "ManufacturingProcessTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Offices_OfficeId",
                table: "Roles",
                column: "OfficeId",
                principalTable: "Offices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Posts_PostId",
                table: "Roles",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Areas_Cities_CityId",
                table: "Areas");

            migrationBuilder.DropForeignKey(
                name: "FK_FactoryTypeDocuments_FactoryTypes_Old_FactoryTypeOldId",
                table: "FactoryTypeDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_Modules_Acts_ActId",
                table: "Modules");

            migrationBuilder.DropForeignKey(
                name: "FK_Modules_Rules_RuleId",
                table: "Modules");

            migrationBuilder.DropForeignKey(
                name: "FK_Privileges_Modules_ModuleId",
                table: "Privileges");

            migrationBuilder.DropForeignKey(
                name: "FK_ProcessDocuments_ManufacturingProcessTypes_ManufacturingProcessTypeId",
                table: "ProcessDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Offices_OfficeId",
                table: "Roles");

            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Posts_PostId",
                table: "Roles");

            migrationBuilder.DropTable(
                name: "AnnualReturns");

            migrationBuilder.DropTable(
                name: "Appeals");

            migrationBuilder.DropTable(
                name: "ApplicationApprovalRequests");

            migrationBuilder.DropTable(
                name: "ApplicationHistories");

            migrationBuilder.DropTable(
                name: "ApplicationObjectionLetters");

            migrationBuilder.DropTable(
                name: "ApplicationRegistrations");

            migrationBuilder.DropTable(
                name: "ApplicationWorkFlowLevels");

            migrationBuilder.DropTable(
                name: "AudioVisualWorks");

            migrationBuilder.DropTable(
                name: "BeediCigarWorks");

            migrationBuilder.DropTable(
                name: "BoilerApplications");

            migrationBuilder.DropTable(
                name: "BoilerApplicationStates");

            migrationBuilder.DropTable(
                name: "BoilerClosures");

            migrationBuilder.DropTable(
                name: "BoilerDetails");

            migrationBuilder.DropTable(
                name: "BoilerDocumentTypes");

            migrationBuilder.DropTable(
                name: "BoilerDrawingApplications");

            migrationBuilder.DropTable(
                name: "BoilerDrawingClosures");

            migrationBuilder.DropTable(
                name: "BoilerInspectionHistories");

            migrationBuilder.DropTable(
                name: "BoilerManufactureClosures");

            migrationBuilder.DropTable(
                name: "BoilerRepairerClosures");

            migrationBuilder.DropTable(
                name: "BoilerRepairerEngineers");

            migrationBuilder.DropTable(
                name: "BoilerRepairerWelders");

            migrationBuilder.DropTable(
                name: "BoilerRepairModifications");

            migrationBuilder.DropTable(
                name: "BoilerWorkflowLogs");

            migrationBuilder.DropTable(
                name: "BuildingAndConstructionWorks");

            migrationBuilder.DropTable(
                name: "Certificates");

            migrationBuilder.DropTable(
                name: "ChiefInspectionScrutinyRemarks");

            migrationBuilder.DropTable(
                name: "CommencementCessationApplication");

            migrationBuilder.DropTable(
                name: "CompetantEstablishmentDetails");

            migrationBuilder.DropTable(
                name: "CompetantOccupierDetails");

            migrationBuilder.DropTable(
                name: "CompetantPersonDetails");

            migrationBuilder.DropTable(
                name: "CompetentPersonEquipments");

            migrationBuilder.DropTable(
                name: "ContractorDetails");

            migrationBuilder.DropTable(
                name: "DesignFacilities");

            migrationBuilder.DropTable(
                name: "DocumentUploads");

            migrationBuilder.DropTable(
                name: "EconomiserClosures");

            migrationBuilder.DropTable(
                name: "EconomiserRegistrations");

            migrationBuilder.DropTable(
                name: "ESignTransactions");

            migrationBuilder.DropTable(
                name: "EstablishmentDetails");

            migrationBuilder.DropTable(
                name: "EstablishmentEntityMapping");

            migrationBuilder.DropTable(
                name: "EstablishmentRegistrationDocuments");

            migrationBuilder.DropTable(
                name: "EstablishmentUserDetails");

            migrationBuilder.DropTable(
                name: "FactoryClosureDocuments");

            migrationBuilder.DropTable(
                name: "FactoryContractorMapping");

            migrationBuilder.DropTable(
                name: "FactoryDetails");

            migrationBuilder.DropTable(
                name: "FactoryLicenses");

            migrationBuilder.DropTable(
                name: "FactoryMapApprovalChemicals");

            migrationBuilder.DropTable(
                name: "FactoryMapDangerousOperations");

            migrationBuilder.DropTable(
                name: "FactoryMapFinishGoods");

            migrationBuilder.DropTable(
                name: "FactoryMapIntermediateProducts");

            migrationBuilder.DropTable(
                name: "FactoryMapRawMaterials");

            migrationBuilder.DropTable(
                name: "FactoryRegistrationFees");

            migrationBuilder.DropTable(
                name: "FeeResults");

            migrationBuilder.DropTable(
                name: "InspectionFormSubmissions");

            migrationBuilder.DropTable(
                name: "InspectionSchedules");

            migrationBuilder.DropTable(
                name: "InspectionScrutinyLevels");

            migrationBuilder.DropTable(
                name: "InspectorApplicationInspections");

            migrationBuilder.DropTable(
                name: "LicenseRenewalDocuments");

            migrationBuilder.DropTable(
                name: "ManagerChanges");

            migrationBuilder.DropTable(
                name: "MapApprovalFactoryDetails");

            migrationBuilder.DropTable(
                name: "MapApprovalOccupierDetails");

            migrationBuilder.DropTable(
                name: "Masters");

            migrationBuilder.DropTable(
                name: "ModulePermissions");

            migrationBuilder.DropTable(
                name: "MotorTransportServices");

            migrationBuilder.DropTable(
                name: "NDTPersonnels");

            migrationBuilder.DropTable(
                name: "NewsPaperEstablishments");

            migrationBuilder.DropTable(
                name: "NonHazardousFactoryRegistrations");

            migrationBuilder.DropTable(
                name: "OfficeApplicationAreas");

            migrationBuilder.DropTable(
                name: "OfficeInspectionAreas");

            migrationBuilder.DropTable(
                name: "OfficeLevels");

            migrationBuilder.DropTable(
                name: "OfficePostLevels");

            migrationBuilder.DropTable(
                name: "Plantations");

            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropTable(
                name: "QualifiedWelders");

            migrationBuilder.DropTable(
                name: "RDFacilities");

            migrationBuilder.DropTable(
                name: "RegisteredBoilerNews");

            migrationBuilder.DropTable(
                name: "RoleInspectionPrivileges");

            migrationBuilder.DropTable(
                name: "RoleLocationAssignments");

            migrationBuilder.DropTable(
                name: "RolePrivileges");

            migrationBuilder.DropTable(
                name: "Rules");

            migrationBuilder.DropTable(
                name: "ScheduleA_FactoryFees");

            migrationBuilder.DropTable(
                name: "ScheduleB_ElectricityFees");

            migrationBuilder.DropTable(
                name: "SMTCTrainerEducationDetails");

            migrationBuilder.DropTable(
                name: "SteamPipeLineApplications");

            migrationBuilder.DropTable(
                name: "SteamPipeLineClosures");

            migrationBuilder.DropTable(
                name: "TechnicalManpowers");

            migrationBuilder.DropTable(
                name: "Tehsils");

            migrationBuilder.DropTable(
                name: "TestingFacilities");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "UserAreaAssignments");

            migrationBuilder.DropTable(
                name: "UserHierarchies");

            migrationBuilder.DropTable(
                name: "UserLocationAssignments");

            migrationBuilder.DropTable(
                name: "UserModulePermissions");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "WelderClosures");

            migrationBuilder.DropTable(
                name: "WelderDetails");

            migrationBuilder.DropTable(
                name: "WelderEmployers");

            migrationBuilder.DropTable(
                name: "ApplicationWorkFlows");

            migrationBuilder.DropTable(
                name: "RegisteredBoilers");

            migrationBuilder.DropTable(
                name: "BoilerRepairerRegistrations");

            migrationBuilder.DropTable(
                name: "CompetentPersonRegistrations");

            migrationBuilder.DropTable(
                name: "CompetentEquipmentRegistrations");

            migrationBuilder.DropTable(
                name: "FactoryClosures");

            migrationBuilder.DropTable(
                name: "EstablishmentRegistrations");

            migrationBuilder.DropTable(
                name: "PersonDetails");

            migrationBuilder.DropTable(
                name: "InspectionScrutinyWorkflows");

            migrationBuilder.DropTable(
                name: "InspectorApplicationAssignments");

            migrationBuilder.DropTable(
                name: "LicenseRenewals");

            migrationBuilder.DropTable(
                name: "Acts");

            migrationBuilder.DropTable(
                name: "SMTCTrainerDetails");

            migrationBuilder.DropTable(
                name: "BoilerManufactureRegistrations");

            migrationBuilder.DropTable(
                name: "WelderApplications");

            migrationBuilder.DropTable(
                name: "FactoryCategories");

            migrationBuilder.DropTable(
                name: "BoilerCertificates");

            migrationBuilder.DropTable(
                name: "BoilerLocations");

            migrationBuilder.DropTable(
                name: "BoilerSafetyFeatures");

            migrationBuilder.DropTable(
                name: "BoilerSpecifications");

            migrationBuilder.DropTable(
                name: "BoilerRegistrations");

            migrationBuilder.DropTable(
                name: "Offices");

            migrationBuilder.DropTable(
                name: "SMTCRegistrations");

            migrationBuilder.DropTable(
                name: "FactoryTypes");

            migrationBuilder.DropTable(
                name: "WorkerRanges");

            migrationBuilder.DropIndex(
                name: "IX_Roles_OfficeId",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Roles_PostId",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Privileges_ModuleId_Action",
                table: "Privileges");

            migrationBuilder.DropIndex(
                name: "IX_Modules_ActId",
                table: "Modules");

            migrationBuilder.DropIndex(
                name: "IX_Modules_Name",
                table: "Modules");

            migrationBuilder.DropIndex(
                name: "IX_Modules_RuleId",
                table: "Modules");

            migrationBuilder.DropIndex(
                name: "IX_FactoryTypeDocuments_FactoryTypeOldId",
                table: "FactoryTypeDocuments");

            migrationBuilder.DropIndex(
                name: "IX_DocumentTypes_Module_ServiceType",
                table: "DocumentTypes");

            migrationBuilder.DropIndex(
                name: "IX_Districts_Name",
                table: "Districts");

            migrationBuilder.DropColumn(
                name: "BRNNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CitizenCategory",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LINNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserType",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "OfficeId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "PostId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "ModuleId",
                table: "Privileges");

            migrationBuilder.DropColumn(
                name: "PanCard",
                table: "Occupiers");

            migrationBuilder.DropColumn(
                name: "ActId",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "RuleId",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "FactoryTypeOldId",
                table: "FactoryTypeDocuments");

            migrationBuilder.DropColumn(
                name: "AmendmentCount",
                table: "FactoryRegistrations");

            migrationBuilder.DropColumn(
                name: "AssignedTo",
                table: "FactoryRegistrations");

            migrationBuilder.DropColumn(
                name: "AssignedToName",
                table: "FactoryRegistrations");

            migrationBuilder.DropColumn(
                name: "CurrentStage",
                table: "FactoryRegistrations");

            migrationBuilder.DropColumn(
                name: "FactoryManagerPanCard",
                table: "FactoryRegistrations");

            migrationBuilder.DropColumn(
                name: "OccupierPanCard",
                table: "FactoryRegistrations");

            migrationBuilder.DropColumn(
                name: "ApplicationPDFUrl",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "FactoryDetails",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "IsESignCompleted",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "IsNew",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "ManufacturingProcess",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "MaxWorkerFemale",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "MaxWorkerMale",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "MaxWorkerTransgender",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "NoOfFactoriesIfCommonPremise",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "ObjectionLetterUrl",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "OccupierDetails",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "Place",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "PremiseOwnerAddressCity",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "PremiseOwnerAddressDistrict",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "PremiseOwnerAddressPinCode",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "PremiseOwnerAddressPlotNo",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "PremiseOwnerAddressState",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "PremiseOwnerAddressStreet",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "PremiseOwnerContactNo",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "PremiseOwnerDetails",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "PremiseOwnerName",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "FactoryMapApprovals");

            migrationBuilder.DropColumn(
                name: "ConditionalField",
                table: "DocumentTypes");

            migrationBuilder.DropColumn(
                name: "ConditionalValue",
                table: "DocumentTypes");

            migrationBuilder.DropColumn(
                name: "IsConditional",
                table: "DocumentTypes");

            migrationBuilder.DropColumn(
                name: "Module",
                table: "DocumentTypes");

            migrationBuilder.DropColumn(
                name: "ServiceType",
                table: "DocumentTypes");

            migrationBuilder.RenameColumn(
                name: "PlantParticulars",
                table: "FactoryMapApprovals",
                newName: "FactoryName");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "FactoryMapApprovals",
                newName: "ReviewedAt");

            migrationBuilder.RenameColumn(
                name: "AreaFactoryPremise",
                table: "FactoryMapApprovals",
                newName: "PlotArea");

            migrationBuilder.RenameColumn(
                name: "CityId",
                table: "Areas",
                newName: "DistrictId1");

            migrationBuilder.RenameIndex(
                name: "IX_Areas_CityId",
                table: "Areas",
                newName: "IX_Areas_DistrictId1");

            migrationBuilder.AddColumn<Guid>(
                name: "RoleId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Roles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ManufacturingProcessTypeId",
                table: "ProcessDocuments",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "DocumentTypeId",
                table: "ProcessDocuments",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(36)",
                oldUnicode: false,
                oldMaxLength: 36);

            migrationBuilder.AddColumn<string>(
                name: "ProcessTypeId",
                table: "ProcessDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Module",
                table: "Privileges",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "FactoryTypeId",
                table: "FactoryTypeDocuments",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DocumentTypeId",
                table: "FactoryTypeDocuments",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(36)",
                oldUnicode: false,
                oldMaxLength: 36);

            migrationBuilder.AlterColumn<string>(
                name: "OccupierCityTown",
                table: "FactoryRegistrations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LandOwnerCityTown",
                table: "FactoryRegistrations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FactoryManagerCityTown",
                table: "FactoryRegistrations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CityTown",
                table: "FactoryRegistrations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "FactoryMapApprovals",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApplicantName",
                table: "FactoryMapApprovals",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "BuildingArea",
                table: "FactoryMapApprovals",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "FactoryMapApprovals",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "FactoryMapApprovals",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "FactoryMapApprovals",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FactoryTypeId",
                table: "FactoryMapApprovals",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobileNo",
                table: "FactoryMapApprovals",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Pincode",
                table: "FactoryMapApprovals",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReviewedBy",
                table: "FactoryMapApprovals",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "DocumentTypes",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(36)",
                oldUnicode: false,
                oldMaxLength: 36);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Districts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateTable(
                name: "UserPrivileges",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrivilegeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPrivileges", x => new { x.UserId, x.PrivilegeId });
                    table.ForeignKey(
                        name: "FK_UserPrivileges_Privileges_PrivilegeId",
                        column: x => x.PrivilegeId,
                        principalTable: "Privileges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPrivileges_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPrivileges_Users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Privileges_Module_Action",
                table: "Privileges",
                columns: new[] { "Module", "Action" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Modules_Category",
                table: "Modules",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryTypeDocuments_FactoryTypeId",
                table: "FactoryTypeDocuments",
                column: "FactoryTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryMapApprovals_AcknowledgementNumber",
                table: "FactoryMapApprovals",
                column: "AcknowledgementNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FactoryMapApprovals_Email",
                table: "FactoryMapApprovals",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryMapApprovals_FactoryTypeId",
                table: "FactoryMapApprovals",
                column: "FactoryTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FactoryMapApprovals_Status",
                table: "FactoryMapApprovals",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UserPrivileges_PrivilegeId",
                table: "UserPrivileges",
                column: "PrivilegeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPrivileges_UserId1",
                table: "UserPrivileges",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Areas_Districts_DistrictId1",
                table: "Areas",
                column: "DistrictId1",
                principalTable: "Districts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FactoryMapApprovals_FactoryTypes_Old_FactoryTypeId",
                table: "FactoryMapApprovals",
                column: "FactoryTypeId",
                principalTable: "FactoryTypes_Old",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FactoryTypeDocuments_FactoryTypes_Old_FactoryTypeId",
                table: "FactoryTypeDocuments",
                column: "FactoryTypeId",
                principalTable: "FactoryTypes_Old",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProcessDocuments_ManufacturingProcessTypes_ManufacturingProcessTypeId",
                table: "ProcessDocuments",
                column: "ManufacturingProcessTypeId",
                principalTable: "ManufacturingProcessTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
