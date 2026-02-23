using FluentValidation;
using FluentValidation.AspNetCore;
using RajFabAPI.DTOs;
using RajFabAPI.Validators;
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.Services;
using RajFabAPI.Services.Interface;
using RajFabAPI.Middlewares;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<FileUploadOperationFilter>();

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
            ),

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddHttpClient();
builder.Services.AddAuthorization();
builder.Services.AddMemoryCache();
// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<RailwayStationService>();   // <-- required
builder.Services.AddScoped<PoliceStationService>();
builder.Services.AddScoped<IDivisionService, DivisionService>();
builder.Services.AddScoped<IDistrictService, DistrictService>();
builder.Services.AddScoped<IAreaService, AreaService>();
builder.Services.AddScoped<IApplicationApprovalRequestService, ApplicationApprovalRequestService>();
builder.Services.AddScoped<IApplicationRegistrationService, ApplicationRegistrationService>();
// builder.Services.AddScoped<IOfficeService, OfficeService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<UserHierarchyService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IEnhancedPrivilegeService, EnhancedPrivilegeService>();
builder.Services.AddScoped<IModuleService, ModuleService>();
// Add custom services
builder.Services.AddScoped<IModuleService, ModuleService>();
builder.Services.AddScoped<IFormService, FormService>();
builder.Services.AddScoped<ISubmissionService, SubmissionService>();
builder.Services.AddScoped<IDynamicTableService, DynamicTableService>();
builder.Services.AddScoped<IFactoryTypeService, FactoryTypeService>();
builder.Services.AddScoped<IFactoryTypeNewService, FactoryTypeNewService>();

builder.Services.AddScoped<IOccupierService, OccupierService>();
builder.Services.AddScoped<IFactoryMapApprovalService, FactoryMapApprovalService>();
builder.Services.AddScoped<IFactoryRegistrationService, FactoryRegistrationService>();
builder.Services.AddScoped<ILicenseRenewalService, LicenseRenewalService>();
builder.Services.AddScoped<IFactoryClosureService, FactoryClosureService>();
builder.Services.AddScoped<IManagerChangeService, ManagerChangeService>();
builder.Services.AddScoped<IApplicationReviewService, ApplicationReviewService>();
builder.Services.AddScoped<IBoilerService, BoilerService>();
builder.Services.AddScoped<IFeeCalculationService, FeeCalculationService>();
builder.Services.AddScoped<IAnnualReturnService, AnnualReturnService>();
builder.Services.AddScoped<IAppealService, AppealService>();
// Register ActService
builder.Services.AddScoped<IMasterService, MasterService>();
builder.Services.AddScoped<IEstablishmentRegistrationService, EstablishmentRegistrationService>();
builder.Services.AddScoped<IDocumentUploadService, DocumentUploadService>();
builder.Services.AddScoped<ICommencementCessationService, CommencementCessationService>();
builder.Services.AddScoped<INonHazardousFactoryRegistrationService, NonHazardousFactoryRegistrationService>();
builder.Services.AddScoped<IGetFactoryDetailsByBRNService, GetFactoryDetailsByBRNService>();

builder.Services.AddScoped<IActService, ActService>();
builder.Services.AddScoped<IRuleService, RuleService>();
builder.Services.AddScoped<IOfficeService, OfficeService>();
builder.Services.AddScoped<IUserRoleAssignmentService, UserRoleAssignmentService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IWorkerRangeService, WorkerRangeService>();
builder.Services.AddScoped<IFactoryCategoryService, FactoryCategoryService>();
builder.Services.AddScoped<IRoleInspectionPrivilegeService, RoleInspectionPrivilegeService>();
builder.Services.AddScoped<IOfficeLevelService, OfficeLevelService>();
builder.Services.AddScoped<IOfficePostLevelService, OfficePostLevelService>();
builder.Services.AddScoped<IApplicationWorkFlowService, ApplicationWorkFlowService>();
builder.Services.AddScoped<IUserServiceNew, UserService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<IFactoryLicenseService, FactoryLicenseService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IESignService, ESignService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

builder.Services.AddScoped<IBoilerRegistartionService, BoilerRegistrationService>();
builder.Services.AddScoped<IBoilerNewService, BoilerNewService>();
builder.Services.AddScoped<ISteamPipeLineApplicationService, SteamPipeLineApplicationService>();
builder.Services.AddScoped<IBoilerManufactureService, BoilerManufactureService>();


builder.Services.AddScoped<IDynamicPDFGenerationFormService, DynamicPDFGenerationFormService>();
builder.Services.AddHttpContextAccessor();


// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
               "http://10.68.108.29",
                "http://10.68.108.29:8080",
               "http://10.68.211.24",
               "http://10.68.211.24:8080",
               "http://10.70.234.214",
               "http://10.70.234.214:8080",
               "http://localhost:8080"
           )
           .AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials();
    });
});

// FluentValidation registration
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateEstablishmentRegistrationDtoValidator>();
builder.Services.AddHttpClient();

var app = builder.Build();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // Apply pending migrations with error handling
    try
    {
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration failed: {ex.Message}. Proceeding with safety net...");
    }
    #region
    /* // Ensure UserHierarchies table exists (for environments without new migrations)
   context.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'[dbo].[UserHierarchies]', N'U') IS NULL
BEGIN
   CREATE TABLE [dbo].[UserHierarchies](
       [Id] uniqueidentifier NOT NULL,
       [UserId] uniqueidentifier NOT NULL,
       [ReportsToId] uniqueidentifier NULL,
       [EmergencyReportToId] uniqueidentifier NULL,
       [CreatedAt] datetime2 NOT NULL,
       [UpdatedAt] datetime2 NOT NULL,
       CONSTRAINT [PK_UserHierarchies] PRIMARY KEY CLUSTERED ([Id] ASC)
   );
END

IF OBJECT_ID(N'[FK_UserHierarchies_Users_UserId]', N'F') IS NULL
BEGIN
   ALTER TABLE [dbo].[UserHierarchies] WITH CHECK 
   ADD CONSTRAINT [FK_UserHierarchies_Users_UserId] FOREIGN KEY([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE;
END

IF OBJECT_ID(N'[FK_UserHierarchies_Users_ReportsToId]', N'F') IS NULL
BEGIN
   ALTER TABLE [dbo].[UserHierarchies] WITH CHECK 
   ADD CONSTRAINT [FK_UserHierarchies_Users_ReportsToId] FOREIGN KEY([ReportsToId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE NO ACTION;
END

IF OBJECT_ID(N'[FK_UserHierarchies_Users_EmergencyReportToId]', N'F') IS NULL
BEGIN
   ALTER TABLE [dbo].[UserHierarchies] WITH CHECK 
   ADD CONSTRAINT [FK_UserHierarchies_Users_EmergencyReportToId] FOREIGN KEY([EmergencyReportToId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE NO ACTION;
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_UserHierarchies_UserId' AND object_id = OBJECT_ID(N'[dbo].[UserHierarchies]'))
BEGIN
   CREATE UNIQUE NONCLUSTERED INDEX [IX_UserHierarchies_UserId] ON [dbo].[UserHierarchies] ([UserId] ASC);
END
");

   
    // Ensure Enhanced Privilege System tables exist
   context.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'[dbo].[ModulePermissions]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ModulePermissions](
        [Id] uniqueidentifier NOT NULL,
        [ModuleId] uniqueidentifier NOT NULL,
        [PermissionName] nvarchar(100) NOT NULL,
        [PermissionCode] nvarchar(50) NOT NULL,
        [Description] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ModulePermissions] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ModulePermissions_Modules_ModuleId] FOREIGN KEY([ModuleId]) REFERENCES [dbo].[Modules] ([Id]) ON DELETE CASCADE
    );
    CREATE UNIQUE NONCLUSTERED INDEX [IX_ModulePermissions_ModuleId_PermissionCode] ON [dbo].[ModulePermissions] ([ModuleId] ASC, [PermissionCode] ASC);
END

IF OBJECT_ID(N'[dbo].[UserModulePermissions]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[UserModulePermissions](
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [ModuleId] uniqueidentifier NOT NULL,
        [Permissions] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_UserModulePermissions] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_UserModulePermissions_Users_UserId] FOREIGN KEY([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserModulePermissions_Modules_ModuleId] FOREIGN KEY([ModuleId]) REFERENCES [dbo].[Modules] ([Id]) ON DELETE CASCADE
    );
    CREATE UNIQUE NONCLUSTERED INDEX [IX_UserModulePermissions_UserId_ModuleId] ON [dbo].[UserModulePermissions] ([UserId] ASC, [ModuleId] ASC);
END

IF OBJECT_ID(N'[dbo].[UserAreaAssignments]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[UserAreaAssignments](
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [AreaId] uniqueidentifier NOT NULL,
        [ModuleId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_UserAreaAssignments] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_UserAreaAssignments_Users_UserId] FOREIGN KEY([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserAreaAssignments_Areas_AreaId] FOREIGN KEY([AreaId]) REFERENCES [dbo].[Areas] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserAreaAssignments_Modules_ModuleId] FOREIGN KEY([ModuleId]) REFERENCES [dbo].[Modules] ([Id]) ON DELETE SET NULL
    );
    CREATE UNIQUE NONCLUSTERED INDEX [IX_UserAreaAssignments_UserId_AreaId_ModuleId] ON [dbo].[UserAreaAssignments] ([UserId] ASC, [AreaId] ASC, [ModuleId] ASC);
END
");

    // Ensure ApplicationHistory table exists
    context.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'[dbo].[ApplicationHistories]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ApplicationHistories](
        [Id] uniqueidentifier NOT NULL,
        [ApplicationId] nvarchar(450) NOT NULL,
        [ApplicationType] nvarchar(50) NOT NULL,
        [Action] nvarchar(50) NOT NULL,
        [PreviousStatus] nvarchar(50) NULL,
        [NewStatus] nvarchar(50) NOT NULL,
        [Comments] nvarchar(max) NULL,
        [ActionBy] nvarchar(450) NOT NULL,
        [ActionByName] nvarchar(200) NOT NULL,
        [ForwardedTo] nvarchar(450) NULL,
        [ForwardedToName] nvarchar(200) NULL,
        [ActionDate] datetime2 NOT NULL,
        CONSTRAINT [PK_ApplicationHistories] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    CREATE NONCLUSTERED INDEX [IX_ApplicationHistories_ApplicationId] ON [dbo].[ApplicationHistories] ([ApplicationId] ASC);
    CREATE NONCLUSTERED INDEX [IX_ApplicationHistories_ActionDate] ON [dbo].[ApplicationHistories] ([ActionDate] DESC);
END
");

    // Add workflow columns to FactoryRegistrations
    context.Database.ExecuteSqlRaw(@"
IF COL_LENGTH(N'[dbo].[FactoryRegistrations]', 'CurrentStage') IS NULL
    ALTER TABLE [dbo].[FactoryRegistrations] ADD [CurrentStage] nvarchar(50) NULL;
IF COL_LENGTH(N'[dbo].[FactoryRegistrations]', 'AssignedTo') IS NULL
    ALTER TABLE [dbo].[FactoryRegistrations] ADD [AssignedTo] nvarchar(450) NULL;
IF COL_LENGTH(N'[dbo].[FactoryRegistrations]', 'AssignedToName') IS NULL
    ALTER TABLE [dbo].[FactoryRegistrations] ADD [AssignedToName] nvarchar(200) NULL;
");*/
    #endregion
    #region
    // Ensure DocumentTypes columns and BoilerDocumentTypes table (safety net)
    try
    {
        context.Database.ExecuteSqlRaw(@"
BEGIN TRY
    -- Ensure DocumentTypes columns exist
    IF COL_LENGTH(N'[dbo].[DocumentTypes]', 'Module') IS NULL
        ALTER TABLE [dbo].[DocumentTypes] ADD [Module] nvarchar(100) NULL;
    IF COL_LENGTH(N'[dbo].[DocumentTypes]', 'ServiceType') IS NULL
        ALTER TABLE [dbo].[DocumentTypes] ADD [ServiceType] nvarchar(100) NULL;
    IF COL_LENGTH(N'[dbo].[DocumentTypes]', 'IsConditional') IS NULL
        ALTER TABLE [dbo].[DocumentTypes] ADD [IsConditional] bit NOT NULL CONSTRAINT [DF_DocumentTypes_IsConditional] DEFAULT(0);
    IF COL_LENGTH(N'[dbo].[DocumentTypes]', 'ConditionalField') IS NULL
        ALTER TABLE [dbo].[DocumentTypes] ADD [ConditionalField] nvarchar(200) NULL;
    IF COL_LENGTH(N'[dbo].[DocumentTypes]', 'ConditionalValue') IS NULL
        ALTER TABLE [dbo].[DocumentTypes] ADD [ConditionalValue] nvarchar(200) NULL;
    IF COL_LENGTH(N'[dbo].[DocumentTypes]', 'IsActive') IS NULL
        ALTER TABLE [dbo].[DocumentTypes] ADD [IsActive] bit NOT NULL CONSTRAINT [DF_DocumentTypes_IsActive] DEFAULT(1);
    IF COL_LENGTH(N'[dbo].[DocumentTypes]', 'CreatedAt') IS NULL
        ALTER TABLE [dbo].[DocumentTypes] ADD [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_DocumentTypes_CreatedAt] DEFAULT(SYSUTCDATETIME());
    IF COL_LENGTH(N'[dbo].[DocumentTypes]', 'UpdatedAt') IS NULL
        ALTER TABLE [dbo].[DocumentTypes] ADD [UpdatedAt] datetime2 NOT NULL CONSTRAINT [DF_DocumentTypes_UpdatedAt] DEFAULT(SYSUTCDATETIME());
END TRY
BEGIN CATCH
    PRINT 'Error updating DocumentTypes: ' + ERROR_MESSAGE();
END CATCH
");

        // Create BoilerDocumentTypes with dynamic data type detection
        context.Database.ExecuteSqlRaw(@"
BEGIN TRY
    IF OBJECT_ID(N'[dbo].[BoilerDocumentTypes]', N'U') IS NULL
    BEGIN
        DECLARE @DocumentTypeIdDataType NVARCHAR(128);
        SELECT @DocumentTypeIdDataType = DATA_TYPE + 
            CASE 
                WHEN DATA_TYPE IN ('varchar', 'nvarchar', 'char', 'nchar') THEN '(' + CAST(CHARACTER_MAXIMUM_LENGTH AS VARCHAR(10)) + ')'
                WHEN DATA_TYPE IN ('decimal', 'numeric') THEN '(' + CAST(NUMERIC_PRECISION AS VARCHAR(10)) + ',' + CAST(NUMERIC_SCALE AS VARCHAR(10)) + ')'
                ELSE ''
            END
        FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'DocumentTypes' AND COLUMN_NAME = 'Id';
        
        IF @DocumentTypeIdDataType IS NULL
            SET @DocumentTypeIdDataType = 'uniqueidentifier';
            
        DECLARE @SQL NVARCHAR(MAX) = '
        CREATE TABLE [dbo].[BoilerDocumentTypes](
            [Id] uniqueidentifier NOT NULL,
            [BoilerServiceType] nvarchar(100) NOT NULL,
            [DocumentTypeId] ' + @DocumentTypeIdDataType + ' NOT NULL,
            [IsRequired] bit NOT NULL CONSTRAINT [DF_BoilerDocumentTypes_IsRequired] DEFAULT(1),
            [OrderIndex] int NOT NULL CONSTRAINT [DF_BoilerDocumentTypes_OrderIndex] DEFAULT(0),
            [ConditionalField] nvarchar(100) NULL,
            [ConditionalValue] nvarchar(100) NULL,
            CONSTRAINT [PK_BoilerDocumentTypes] PRIMARY KEY CLUSTERED ([Id] ASC),
            CONSTRAINT [FK_BoilerDocumentTypes_DocumentTypes_DocumentTypeId] FOREIGN KEY([DocumentTypeId]) REFERENCES [dbo].[DocumentTypes] ([Id]) ON DELETE CASCADE
        );
        CREATE NONCLUSTERED INDEX [IX_BoilerDocumentTypes_DocumentTypeId] ON [dbo].[BoilerDocumentTypes] ([DocumentTypeId] ASC);
        CREATE NONCLUSTERED INDEX [IX_BoilerDocumentTypes_BoilerServiceType] ON [dbo].[BoilerDocumentTypes] ([BoilerServiceType] ASC);';
        
        EXEC sp_executesql @SQL;
        PRINT 'BoilerDocumentTypes table created with FK constraint';
    END
    ELSE
    BEGIN
        -- Verify and recreate foreign key constraint if missing
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_BoilerDocumentTypes_DocumentTypes_DocumentTypeId')
        BEGIN
            DECLARE @DocumentTypeIdDataType2 NVARCHAR(128);
            SELECT @DocumentTypeIdDataType2 = DATA_TYPE + 
                CASE 
                    WHEN DATA_TYPE IN ('varchar', 'nvarchar', 'char', 'nchar') THEN '(' + CAST(CHARACTER_MAXIMUM_LENGTH AS VARCHAR(10)) + ')'
                    WHEN DATA_TYPE IN ('decimal', 'numeric') THEN '(' + CAST(NUMERIC_PRECISION AS VARCHAR(10)) + ',' + CAST(NUMERIC_SCALE AS VARCHAR(10)) + ')'
                    ELSE ''
                END
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = 'DocumentTypes' AND COLUMN_NAME = 'Id';
            
            -- Ensure data types match
            DECLARE @BoilerDocTypeIdDataType NVARCHAR(128);
            SELECT @BoilerDocTypeIdDataType = DATA_TYPE + 
                CASE 
                    WHEN DATA_TYPE IN ('varchar', 'nvarchar', 'char', 'nchar') THEN '(' + CAST(CHARACTER_MAXIMUM_LENGTH AS VARCHAR(10)) + ')'
                    WHEN DATA_TYPE IN ('decimal', 'numeric') THEN '(' + CAST(NUMERIC_PRECISION AS VARCHAR(10)) + ',' + CAST(NUMERIC_SCALE AS VARCHAR(10)) + ')'
                    ELSE ''
                END
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = 'BoilerDocumentTypes' AND COLUMN_NAME = 'DocumentTypeId';
            
            IF @DocumentTypeIdDataType2 = @BoilerDocTypeIdDataType
            BEGIN
                ALTER TABLE [dbo].[BoilerDocumentTypes] 
                ADD CONSTRAINT [FK_BoilerDocumentTypes_DocumentTypes_DocumentTypeId] 
                FOREIGN KEY([DocumentTypeId]) REFERENCES [dbo].[DocumentTypes] ([Id]) ON DELETE CASCADE;
                PRINT 'Foreign key constraint FK_BoilerDocumentTypes_DocumentTypes_DocumentTypeId created';
            END
            ELSE
            BEGIN
                PRINT 'Data type mismatch: DocumentTypes.Id = ' + @DocumentTypeIdDataType2 + ', BoilerDocumentTypes.DocumentTypeId = ' + @BoilerDocTypeIdDataType;
            END
        END
        ELSE
        BEGIN
            PRINT 'Foreign key constraint FK_BoilerDocumentTypes_DocumentTypes_DocumentTypeId already exists';
        END
    END
    
    -- Add missing columns to existing BoilerDocumentTypes table
    IF COL_LENGTH(N'[dbo].[BoilerDocumentTypes]', 'OrderIndex') IS NULL
        ALTER TABLE [dbo].[BoilerDocumentTypes] ADD [OrderIndex] int NOT NULL CONSTRAINT [DF_BoilerDocumentTypes_OrderIndex_2] DEFAULT(0);
    IF COL_LENGTH(N'[dbo].[BoilerDocumentTypes]', 'ConditionalField') IS NULL
        ALTER TABLE [dbo].[BoilerDocumentTypes] ADD [ConditionalField] nvarchar(100) NULL;
    IF COL_LENGTH(N'[dbo].[BoilerDocumentTypes]', 'ConditionalValue') IS NULL
        ALTER TABLE [dbo].[BoilerDocumentTypes] ADD [ConditionalValue] nvarchar(100) NULL;
        
END TRY
BEGIN CATCH
    PRINT 'Error creating/updating BoilerDocumentTypes: ' + ERROR_MESSAGE();
END CATCH
");

        // Fix DocumentTypes.Id data type mismatch (GUID to string)
        context.Database.ExecuteSqlRaw(@"
BEGIN TRY
    -- Check if DocumentTypes.Id is uniqueidentifier and convert to nvarchar(450)
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'DocumentTypes' AND COLUMN_NAME = 'Id' AND DATA_TYPE = 'uniqueidentifier')
    BEGIN
        PRINT 'Converting DocumentTypes.Id from uniqueidentifier to nvarchar(450)...';
        
        -- Drop foreign key constraints first
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
                   WHERE CONSTRAINT_NAME = 'FK_BoilerDocumentTypes_DocumentTypes_DocumentTypeId')
        BEGIN
            ALTER TABLE [BoilerDocumentTypes] DROP CONSTRAINT [FK_BoilerDocumentTypes_DocumentTypes_DocumentTypeId];
        END
        
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
                   WHERE CONSTRAINT_NAME = 'FK_FactoryTypeDocuments_DocumentTypes_DocumentTypeId')
        BEGIN
            ALTER TABLE [FactoryTypeDocuments] DROP CONSTRAINT [FK_FactoryTypeDocuments_DocumentTypes_DocumentTypeId];
        END
        
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
                   WHERE CONSTRAINT_NAME = 'FK_ProcessDocuments_DocumentTypes_DocumentTypeId')
        BEGIN
            ALTER TABLE [ProcessDocuments] DROP CONSTRAINT [FK_ProcessDocuments_DocumentTypes_DocumentTypeId];
        END
        
        -- Convert DocumentTypes.Id from uniqueidentifier to nvarchar(450)
        -- First add a temporary column
        ALTER TABLE [DocumentTypes] ADD [Id_Temp] nvarchar(450);
        
        -- Convert GUID values to string representation
        UPDATE [DocumentTypes] SET [Id_Temp] = CAST([Id] AS nvarchar(450));
        
        -- Update all foreign key references
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'BoilerDocumentTypes' AND COLUMN_NAME = 'DocumentTypeId')
        BEGIN
            UPDATE [BoilerDocumentTypes] 
            SET [DocumentTypeId] = dt.[Id_Temp]
            FROM [BoilerDocumentTypes] bdt
            INNER JOIN [DocumentTypes] dt ON CAST(bdt.[DocumentTypeId] AS uniqueidentifier) = dt.[Id]
            WHERE TRY_CAST(bdt.[DocumentTypeId] AS uniqueidentifier) IS NOT NULL;
        END
        
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'FactoryTypeDocuments' AND COLUMN_NAME = 'DocumentTypeId')
        BEGIN
            UPDATE [FactoryTypeDocuments] 
            SET [DocumentTypeId] = dt.[Id_Temp]
            FROM [FactoryTypeDocuments] ftd
            INNER JOIN [DocumentTypes] dt ON CAST(ftd.[DocumentTypeId] AS uniqueidentifier) = dt.[Id]
            WHERE TRY_CAST(ftd.[DocumentTypeId] AS uniqueidentifier) IS NOT NULL;
        END
        
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'ProcessDocuments' AND COLUMN_NAME = 'DocumentTypeId')
        BEGIN
            UPDATE [ProcessDocuments] 
            SET [DocumentTypeId] = dt.[Id_Temp]
            FROM [ProcessDocuments] pd
            INNER JOIN [DocumentTypes] dt ON CAST(pd.[DocumentTypeId] AS uniqueidentifier) = dt.[Id]
            WHERE TRY_CAST(pd.[DocumentTypeId] AS uniqueidentifier) IS NOT NULL;
        END
        
        -- Drop the old column and rename temp column
        ALTER TABLE [DocumentTypes] DROP CONSTRAINT [PK_DocumentTypes];
        ALTER TABLE [DocumentTypes] DROP COLUMN [Id];
        EXEC sp_rename 'DocumentTypes.Id_Temp', 'Id', 'COLUMN';
        ALTER TABLE [DocumentTypes] ALTER COLUMN [Id] nvarchar(450) NOT NULL;
        ALTER TABLE [DocumentTypes] ADD CONSTRAINT [PK_DocumentTypes] PRIMARY KEY ([Id]);
        
        -- Recreate foreign key constraints
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BoilerDocumentTypes')
        BEGIN
            ALTER TABLE [BoilerDocumentTypes] ADD CONSTRAINT [FK_BoilerDocumentTypes_DocumentTypes_DocumentTypeId] 
            FOREIGN KEY ([DocumentTypeId]) REFERENCES [DocumentTypes] ([Id]) ON DELETE CASCADE;
        END
        
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'FactoryTypeDocuments')
        BEGIN
            ALTER TABLE [FactoryTypeDocuments] ADD CONSTRAINT [FK_FactoryTypeDocuments_DocumentTypes_DocumentTypeId] 
            FOREIGN KEY ([DocumentTypeId]) REFERENCES [DocumentTypes] ([Id]) ON DELETE CASCADE;
        END
        
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ProcessDocuments')
        BEGIN
            ALTER TABLE [ProcessDocuments] ADD CONSTRAINT [FK_ProcessDocuments_DocumentTypes_DocumentTypeId] 
            FOREIGN KEY ([DocumentTypeId]) REFERENCES [DocumentTypes] ([Id]) ON DELETE CASCADE;
        END
        
        PRINT 'DocumentTypes.Id conversion completed successfully.';
    END
END TRY
BEGIN CATCH
    PRINT 'Error converting DocumentTypes.Id: ' + ERROR_MESSAGE();
END CATCH
");

        // Normalize BoilerDocumentTypes identifiers and FK types (safety net)
        context.Database.ExecuteSqlRaw(@"
BEGIN TRY
    IF OBJECT_ID(N'[dbo].[BoilerDocumentTypes]', N'U') IS NOT NULL
    BEGIN
        -- Ensure BoilerDocumentTypes.Id is nvarchar(450)
        IF EXISTS (
            SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = 'BoilerDocumentTypes' AND COLUMN_NAME = 'Id' AND DATA_TYPE = 'uniqueidentifier'
        )
        BEGIN
            PRINT 'Converting BoilerDocumentTypes.Id from uniqueidentifier to nvarchar(450)...';
            -- Drop PK
            IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_NAME='BoilerDocumentTypes' AND CONSTRAINT_TYPE='PRIMARY KEY')
            BEGIN
                DECLARE @pkName NVARCHAR(128);
                SELECT @pkName = kc.name
                FROM sys.key_constraints kc
                JOIN sys.tables t ON kc.parent_object_id = t.object_id
                WHERE t.name = 'BoilerDocumentTypes' AND kc.[type] = 'PK';
                IF @pkName IS NOT NULL EXEC('ALTER TABLE [BoilerDocumentTypes] DROP CONSTRAINT [' + @pkName + ']');
            END

            -- Convert Id
            ALTER TABLE [BoilerDocumentTypes] ADD [Id_Temp] nvarchar(450);
            UPDATE [BoilerDocumentTypes] SET [Id_Temp] = CAST([Id] AS nvarchar(450));
            ALTER TABLE [BoilerDocumentTypes] DROP COLUMN [Id];
            EXEC sp_rename 'BoilerDocumentTypes.Id_Temp', 'Id', 'COLUMN';
            ALTER TABLE [BoilerDocumentTypes] ALTER COLUMN [Id] nvarchar(450) NOT NULL;
            ALTER TABLE [BoilerDocumentTypes] ADD CONSTRAINT [PK_BoilerDocumentTypes] PRIMARY KEY ([Id]);
        END

        -- Ensure BoilerDocumentTypes.DocumentTypeId matches DocumentTypes.Id (nvarchar(450))
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='DocumentTypes' AND COLUMN_NAME='Id' AND DATA_TYPE='nvarchar')
        BEGIN
            -- Drop FK if exists
            IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_BoilerDocumentTypes_DocumentTypes_DocumentTypeId')
            BEGIN
                ALTER TABLE [BoilerDocumentTypes] DROP CONSTRAINT [FK_BoilerDocumentTypes_DocumentTypes_DocumentTypeId];
            END
            -- Alter column type if needed
            IF EXISTS (
                SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME='BoilerDocumentTypes' AND COLUMN_NAME='DocumentTypeId' AND DATA_TYPE <> 'nvarchar'
            )
            BEGIN
                PRINT 'Converting BoilerDocumentTypes.DocumentTypeId to nvarchar(450)...';
                ALTER TABLE [BoilerDocumentTypes] ALTER COLUMN [DocumentTypeId] nvarchar(450) NOT NULL;
            END
            -- Recreate FK
            ALTER TABLE [BoilerDocumentTypes] 
                ADD CONSTRAINT [FK_BoilerDocumentTypes_DocumentTypes_DocumentTypeId]
                FOREIGN KEY([DocumentTypeId]) REFERENCES [DocumentTypes]([Id]) ON DELETE CASCADE;
        END
    END
END TRY
BEGIN CATCH
    PRINT 'Error normalizing BoilerDocumentTypes types: ' + ERROR_MESSAGE();
END CATCH
");

        // Ensure all boiler-related tables exist
        context.Database.ExecuteSqlRaw(@"
BEGIN TRY
    -- 1. Create BoilerSpecifications table if not exists
    IF OBJECT_ID(N'[dbo].[BoilerSpecifications]', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[BoilerSpecifications](
            [Id] uniqueidentifier NOT NULL,
            [BoilerId] uniqueidentifier NOT NULL,
            [SerialNumber] nvarchar(100) NULL,
            [ManufacturerName] nvarchar(200) NULL,
            [ManufacturedDate] datetime2 NULL,
            [Model] nvarchar(100) NULL,
            [WorkingPressure] decimal(18,2) NULL,
            [SuperheatedSteamPressure] decimal(18,2) NULL,
            [SuperheatedSteamTemperature] decimal(18,2) NULL,
            [EvaporationCapacity] decimal(18,2) NULL,
            [HeatingArea] decimal(18,2) NULL,
            [BoilerType] nvarchar(100) NULL,
            [FuelType] nvarchar(100) NULL,
            [RatedCapacity] decimal(18,2) NULL,
            CONSTRAINT [PK_BoilerSpecifications] PRIMARY KEY CLUSTERED ([Id] ASC)
        );
        CREATE NONCLUSTERED INDEX [IX_BoilerSpecifications_BoilerId] ON [dbo].[BoilerSpecifications] ([BoilerId] ASC);
    END
    
    -- 2. Create BoilerLocations table if not exists  
    IF OBJECT_ID(N'[dbo].[BoilerLocations]', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[BoilerLocations](
            [Id] uniqueidentifier NOT NULL,
            [BoilerId] uniqueidentifier NOT NULL,
            [Address] nvarchar(500) NULL,
            [District] nvarchar(100) NULL,
            [Pincode] nvarchar(20) NULL,
            [State] nvarchar(100) NULL,
            [Latitude] decimal(10,7) NULL,
            [Longitude] decimal(10,7) NULL,
            [NearestRailwayStation] nvarchar(200) NULL,
            [NearestPoliceStation] nvarchar(200) NULL,
            CONSTRAINT [PK_BoilerLocations] PRIMARY KEY CLUSTERED ([Id] ASC)
        );
        CREATE NONCLUSTERED INDEX [IX_BoilerLocations_BoilerId] ON [dbo].[BoilerLocations] ([BoilerId] ASC);
    END
    
    -- 3. Create BoilerSafetyFeatures table if not exists
    IF OBJECT_ID(N'[dbo].[BoilerSafetyFeatures]', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[BoilerSafetyFeatures](
            [Id] uniqueidentifier NOT NULL,
            [BoilerId] uniqueidentifier NOT NULL,
            [SafetyValves] nvarchar(max) NULL,
            [PressureGauges] nvarchar(max) NULL,
            [WaterLevelIndicators] nvarchar(max) NULL,
            [BlowDownValves] nvarchar(max) NULL,
            [FireExtinguishers] nvarchar(max) NULL,
            [EmergencyShutoffs] nvarchar(max) NULL,
            [AlarmSystems] nvarchar(max) NULL,
            [OtherSafetyDevices] nvarchar(max) NULL,
            CONSTRAINT [PK_BoilerSafetyFeatures] PRIMARY KEY CLUSTERED ([Id] ASC)
        );
        CREATE NONCLUSTERED INDEX [IX_BoilerSafetyFeatures_BoilerId] ON [dbo].[BoilerSafetyFeatures] ([BoilerId] ASC);
    END
    
    -- 4. Create BoilerCertificates table if not exists
    IF OBJECT_ID(N'[dbo].[BoilerCertificates]', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[BoilerCertificates](
            [Id] uniqueidentifier NOT NULL,
            [BoilerId] uniqueidentifier NOT NULL,
            [CertificateNumber] nvarchar(100) NOT NULL,
            [CertificateType] nvarchar(100) NOT NULL,
            [IssueDate] datetime2 NOT NULL,
            [ExpiryDate] datetime2 NOT NULL,
            [IssuedBy] nvarchar(200) NOT NULL,
            [Status] nvarchar(50) NOT NULL,
            [FilePath] nvarchar(500) NULL,
            CONSTRAINT [PK_BoilerCertificates] PRIMARY KEY CLUSTERED ([Id] ASC)
        );
        CREATE NONCLUSTERED INDEX [IX_BoilerCertificates_BoilerId] ON [dbo].[BoilerCertificates] ([BoilerId] ASC);
        CREATE NONCLUSTERED INDEX [IX_BoilerCertificates_CertificateNumber] ON [dbo].[BoilerCertificates] ([CertificateNumber] ASC);
    END
    
    -- 5. Create BoilerInspectionHistories table if not exists
    IF OBJECT_ID(N'[dbo].[BoilerInspectionHistories]', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[BoilerInspectionHistories](
            [Id] uniqueidentifier NOT NULL,
            [BoilerId] uniqueidentifier NOT NULL,
            [InspectionDate] datetime2 NOT NULL,
            [InspectionType] nvarchar(100) NOT NULL,
            [InspectorName] nvarchar(200) NOT NULL,
            [InspectorId] nvarchar(100) NULL,
            [Findings] nvarchar(max) NULL,
            [Recommendations] nvarchar(max) NULL,
            [Status] nvarchar(50) NOT NULL,
            [NextInspectionDate] datetime2 NULL,
            [ReportFilePath] nvarchar(500) NULL,
            CONSTRAINT [PK_BoilerInspectionHistories] PRIMARY KEY CLUSTERED ([Id] ASC)
        );
        CREATE NONCLUSTERED INDEX [IX_BoilerInspectionHistories_BoilerId] ON [dbo].[BoilerInspectionHistories] ([BoilerId] ASC);
        CREATE NONCLUSTERED INDEX [IX_BoilerInspectionHistories_InspectionDate] ON [dbo].[BoilerInspectionHistories] ([InspectionDate] DESC);
    END
    
    -- 6. Create RegisteredBoilers table if not exists
    IF OBJECT_ID(N'[dbo].[RegisteredBoilers]', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[RegisteredBoilers](
            [Id] uniqueidentifier NOT NULL,
            [RegistrationNumber] nvarchar(100) NOT NULL,
            [OwnerName] nvarchar(200) NOT NULL,
            [OperatorName] nvarchar(200) NULL,
            [RegistrationDate] datetime2 NOT NULL,
            [Status] nvarchar(50) NOT NULL,
            [CreatedAt] datetime2 NOT NULL DEFAULT(SYSUTCDATETIME()),
            [UpdatedAt] datetime2 NOT NULL DEFAULT(SYSUTCDATETIME()),
            CONSTRAINT [PK_RegisteredBoilers] PRIMARY KEY CLUSTERED ([Id] ASC)
        );
        CREATE UNIQUE NONCLUSTERED INDEX [IX_RegisteredBoilers_RegistrationNumber] ON [dbo].[RegisteredBoilers] ([RegistrationNumber] ASC);
        CREATE NONCLUSTERED INDEX [IX_RegisteredBoilers_Status] ON [dbo].[RegisteredBoilers] ([Status] ASC);
        
        -- Add foreign key relationships after RegisteredBoilers is created
        ALTER TABLE [dbo].[BoilerSpecifications] ADD CONSTRAINT [FK_BoilerSpecifications_RegisteredBoilers_BoilerId] 
            FOREIGN KEY([BoilerId]) REFERENCES [dbo].[RegisteredBoilers] ([Id]) ON DELETE CASCADE;
        ALTER TABLE [dbo].[BoilerLocations] ADD CONSTRAINT [FK_BoilerLocations_RegisteredBoilers_BoilerId] 
            FOREIGN KEY([BoilerId]) REFERENCES [dbo].[RegisteredBoilers] ([Id]) ON DELETE CASCADE;
        ALTER TABLE [dbo].[BoilerSafetyFeatures] ADD CONSTRAINT [FK_BoilerSafetyFeatures_RegisteredBoilers_BoilerId] 
            FOREIGN KEY([BoilerId]) REFERENCES [dbo].[RegisteredBoilers] ([Id]) ON DELETE CASCADE;
        ALTER TABLE [dbo].[BoilerCertificates] ADD CONSTRAINT [FK_BoilerCertificates_RegisteredBoilers_BoilerId] 
            FOREIGN KEY([BoilerId]) REFERENCES [dbo].[RegisteredBoilers] ([Id]) ON DELETE CASCADE;
        ALTER TABLE [dbo].[BoilerInspectionHistories] ADD CONSTRAINT [FK_BoilerInspectionHistories_RegisteredBoilers_BoilerId] 
            FOREIGN KEY([BoilerId]) REFERENCES [dbo].[RegisteredBoilers] ([Id]) ON DELETE CASCADE;
    END
    
    -- 7. Create BoilerApplications table if not exists
    IF OBJECT_ID(N'[dbo].[BoilerApplications]', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[BoilerApplications](
            [Id] uniqueidentifier NOT NULL,
            [ApplicationNumber] nvarchar(100) NOT NULL,
            [ApplicationType] nvarchar(100) NOT NULL,
            [Status] nvarchar(50) NOT NULL,
            [ApplicantName] nvarchar(200) NOT NULL,
            [OrganizationName] nvarchar(200) NULL,
            [ContactPerson] nvarchar(200) NULL,
            [Mobile] nvarchar(15) NULL,
            [Email] nvarchar(200) NULL,
            [Address] nvarchar(500) NULL,
            [SubmissionDate] datetime2 NOT NULL,
            [ProcessingDate] datetime2 NULL,
            [CompletionDate] datetime2 NULL,
            [ProcessedBy] nvarchar(200) NULL,
            [Comments] nvarchar(max) NULL,
            [CreatedAt] datetime2 NOT NULL DEFAULT(SYSUTCDATETIME()),
            [UpdatedAt] datetime2 NOT NULL DEFAULT(SYSUTCDATETIME()),
            CONSTRAINT [PK_BoilerApplications] PRIMARY KEY CLUSTERED ([Id] ASC)
        );
        CREATE UNIQUE NONCLUSTERED INDEX [IX_BoilerApplications_ApplicationNumber] ON [dbo].[BoilerApplications] ([ApplicationNumber] ASC);
        CREATE NONCLUSTERED INDEX [IX_BoilerApplications_Status] ON [dbo].[BoilerApplications] ([Status] ASC);
        CREATE NONCLUSTERED INDEX [IX_BoilerApplications_ApplicationType] ON [dbo].[BoilerApplications] ([ApplicationType] ASC);
    END
    
END TRY
BEGIN CATCH
    PRINT 'Error creating boiler tables: ' + ERROR_MESSAGE();
END CATCH
");

        // Ensure Offices table exists
        context.Database.ExecuteSqlRaw(@"
BEGIN TRY
    IF OBJECT_ID(N'[dbo].[Offices]', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[Offices](
            [Id] uniqueidentifier NOT NULL,
            [Name] nvarchar(200) NOT NULL,
            [DistrictId] uniqueidentifier NOT NULL,
            [CityId] uniqueidentifier NOT NULL,
            [Pincode] nvarchar(10) NOT NULL,
            [Address] nvarchar(500) NOT NULL,
            [CreatedAt] datetime2 NOT NULL DEFAULT(SYSUTCDATETIME()),
            [UpdatedAt] datetime2 NOT NULL DEFAULT(SYSUTCDATETIME()),
            CONSTRAINT [PK_Offices] PRIMARY KEY CLUSTERED ([Id] ASC),
            CONSTRAINT [FK_Offices_Districts_DistrictId] 
                FOREIGN KEY([DistrictId]) REFERENCES [dbo].[Districts] ([Id]) ON DELETE NO ACTION,
            CONSTRAINT [FK_Offices_Cities_CityId] 
                FOREIGN KEY([CityId]) REFERENCES [dbo].[Cities] ([Id]) ON DELETE NO ACTION
        );
        CREATE NONCLUSTERED INDEX [IX_Offices_DistrictId] ON [dbo].[Offices] ([DistrictId] ASC);
        CREATE NONCLUSTERED INDEX [IX_Offices_CityId] ON [dbo].[Offices] ([CityId] ASC);
        CREATE NONCLUSTERED INDEX [IX_Offices_Name_CityId] ON [dbo].[Offices] ([Name] ASC, [CityId] ASC);
        PRINT 'Offices table created successfully';
    END
    ELSE
    BEGIN
        PRINT 'Offices table already exists';
    END
END TRY
BEGIN CATCH
    PRINT 'Error creating Offices table: ' + ERROR_MESSAGE();
END CATCH
");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Safety net execution failed: {ex.Message}");
    }
    #endregion
}

var env = app.Services.GetRequiredService<IWebHostEnvironment>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var webRootPath = env.WebRootPath;

if (string.IsNullOrWhiteSpace(webRootPath))
{
    throw new InvalidOperationException("wwwroot path is not configured.");
}

var folders = new[]
{
    "documents",
    "certificates",
    "factory-establishment-forms",
    "factory-map-forms",
    "factory-license-forms"
};

foreach (var folder in folders)
{
    var fullPath = Path.Combine(webRootPath, folder);

    if (!Directory.Exists(fullPath))
    {
        Directory.CreateDirectory(fullPath);
    }
}

app.UseStaticFiles();

//app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();