-- Drop old table if it exists
IF OBJECT_ID('dbo.ManagerChanges', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.ManagerChanges;
END
GO

-- Create new table with Version column
CREATE TABLE dbo.ManagerChanges
(
    Id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT PK_ManagerChanges PRIMARY KEY
        DEFAULT NEWID(),

    FactoryRegistrationId UNIQUEIDENTIFIER NOT NULL,
    OldManagerId UNIQUEIDENTIFIER NOT NULL,
    NewManagerId UNIQUEIDENTIFIER NOT NULL,

    AcknowledgementNumber NVARCHAR(255) NOT NULL,

    SignatureofOccupier NVARCHAR(MAX) NULL,
    SignatureOfNewManager NVARCHAR(MAX) NULL,

    DateOfAppointment DATETIME2 NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',

    Version DECIMAL(3,1) NOT NULL DEFAULT 1.0,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1
);
GO

alter table ManagerChanges add IsActive NVARCHAR(50) NOT NULL

ALTER TABLE EstablishmentRegistrations
ADD Version DECIMAL(3,1) NOT NULL
    CONSTRAINT DF_EstablishmentRegistrations_Version DEFAULT (1.0);


 CREATE TABLE Certificate
(
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    
    RegistrationNumber NVARCHAR(100) NOT NULL,
    
    CertificateVersion INT NOT NULL,
    
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NOT NULL,
    
    CertificateUrl NVARCHAR(Max) NOT NULL,
    
    IssuedByUserId UNIQUEIDENTIFIER NOT NULL,
    IssuedAt DATETIME2 NOT NULL,
    
    Place NVARCHAR(150) NULL,
    Signature NVARCHAR(250) NULL,
    
    Status NVARCHAR(20) NOT NULL,
    
    ModuleId UNIQUEIDENTIFIER NOT NULL,
    
    Remarks NVARCHAR(500) NULL
);


CREATE TABLE FactoryLicenses
(
    Id UNIQUEIDENTIFIER NOT NULL 
        CONSTRAINT PK_FactoryLicenses PRIMARY KEY
        DEFAULT NEWID(),

    FactoryLicenseNumber NVARCHAR(50) NOT NULL,

    FactoryRegistrationNumber NVARCHAR(20) NULL,

    NoOfYears INT NOT NULL 
        CONSTRAINT DF_FactoryLicenses_NoOfYears DEFAULT 1,

    ValidFrom DATETIME2 NOT NULL,
    ValidTo DATETIME2 NOT NULL,

    Place NVARCHAR(255) NOT NULL,

    Date DATETIME2 NOT NULL,

    ManagerSignature NVARCHAR(MAX) NULL,
    OccupierSignature NVARCHAR(MAX) NULL,
    AuthorisedSignature NVARCHAR(MAX) NULL,

    CreatedAt DATETIME2 NOT NULL 
        CONSTRAINT DF_FactoryLicenses_CreatedAt DEFAULT GETDATE(),

    UpdatedAt DATETIME2 NOT NULL 
        CONSTRAINT DF_FactoryLicenses_UpdatedAt DEFAULT GETDATE(),

    IsActive BIT NOT NULL 
        CONSTRAINT DF_FactoryLicenses_IsActive DEFAULT 1,

    Version DECIMAL(3,1) NOT NULL 
        CONSTRAINT DF_FactoryLicenses_Version DEFAULT 1.0,

    Status NVARCHAR(50) NOT NULL 
        CONSTRAINT DF_FactoryLicenses_Status DEFAULT 'Pending',

    Type NVARCHAR(50) NOT NULL 
        CONSTRAINT DF_FactoryLicenses_Type DEFAULT 'New'
);

ALTER TABLE EstablishmentRegistrations
ADD Signature VARCHAR(MAX);

  ALTER TABLE [ApplicationApprovalRequests] ALTER COLUMN [CreatedBy] nvarchar(max) NULL;
  ALTER TABLE [ApplicationRegistrations] ALTER COLUMN [ApplicationRegistrationNumber] nvarchar(200) NULL;

  CREATE TABLE Appeals (
    Id NVARCHAR(36) NOT NULL PRIMARY KEY,
    FactoryRegistrationNumber NVARCHAR(255) NOT NULL,
    DateOfAccident DATETIME NULL,
    DateOfInspection DATETIME NULL,
    NoticeNumber NVARCHAR(100) NULL,
    NoticeDate DATETIME NULL,
    OrderNumber NVARCHAR(100) NULL,
    OrderDate DATETIME NULL,
    FactsAndGrounds NVARCHAR(MAX) NULL,
    ReliefSought NVARCHAR(MAX) NULL,
    ChallanNumber NVARCHAR(100) NULL,
    EnclosureDetails1 NVARCHAR(MAX) NULL,
    EnclosureDetails2 NVARCHAR(MAX) NULL,
    SignatureOfOccupier NVARCHAR(MAX) NULL,
    Signature NVARCHAR(MAX) NULL,
    Place NVARCHAR(100) NULL,
    Date DATETIME NULL,
    Version DECIMAL(3,1) NOT NULL DEFAULT 1.0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE CommencementCessationApplication (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    ApplicationType NVARCHAR(50) NOT NULL,
    FactoryRegistrationNumber NVARCHAR(100) NOT NULL,
    CessationIntimationDate DATETIME NOT NULL,
    CessationIntimationEffectiveDate DATETIME NOT NULL,
    ApproxDurationOfWork NVARCHAR(50) NULL,
    OccupierSignature NVARCHAR(MAX) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    Version DECIMAL(3,1) NOT NULL DEFAULT 1.0,
    IsActive BIT NOT NULL DEFAULT 1
);


 DROP TABLE FactoryMapApprovals
CREATE TABLE FactoryMapApprovals
(
    Id NVARCHAR(50) NOT NULL PRIMARY KEY,
    
    AcknowledgementNumber NVARCHAR(255) NOT NULL,
    PlantParticulars NVARCHAR(200) NOT NULL,
    ProductName NVARCHAR(MAX) NOT NULL,
    ManufacturingProcess NVARCHAR(MAX) NOT NULL,
    
    MaxWorkerMale INT NOT NULL,
    MaxWorkerFemale INT NOT NULL,
    
    Version DECIMAL(3,1) NOT NULL DEFAULT 1.0,
    IsNew BIT NOT NULL DEFAULT 1,
    
    AreaFactoryPremise DECIMAL(18,2) NOT NULL DEFAULT 0,
    NoOfFactoriesIfCommonPremise INT NULL,
    
    PremiseOwnerName NVARCHAR(MAX) NULL,
    PremiseOwnerContactNo NVARCHAR(50) NULL,
    PremiseOwnerAddressPlotNo NVARCHAR(100) NULL,
    PremiseOwnerAddressStreet NVARCHAR(200) NULL,
    PremiseOwnerAddressCity NVARCHAR(100) NULL,
    PremiseOwnerAddressDistrict NVARCHAR(100) NULL,
    PremiseOwnerAddressState NVARCHAR(100) NULL,
    PremiseOwnerAddressPinCode NVARCHAR(20) NULL,
    
    Place NVARCHAR(100) NULL,
    Date DATETIME NULL,
    IsESignCompleted BIT NOT NULL DEFAULT 0,
    ApplicationPDFUrl NVARCHAR(MAX) NULL DEFAULT '',
    
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    
    FactoryDetails NVARCHAR(MAX) NOT NULL DEFAULT '',
    OccupierDetails NVARCHAR(MAX) NOT NULL DEFAULT '',
    
);

ALTER TABLE EstablishmentRegistrations
ADD ApplicationPDFUrl NVARCHAR(MAX) NULL DEFAULT '',

ALTER TABLE CommencementCessationApplications
ADD IsESignCompleted BIT NOT NULL CONSTRAINT DF_CommencementCessation_IsESignCompleted DEFAULT 0,
    ApplicationPDFUrl NVARCHAR(MAX) NULL;


CREATE TABLE [dbo].[BoilerRegistrations](
	[Id] [uniqueidentifier] NOT NULL,
	[FactoryId] [uniqueidentifier] NULL,
	[ApplicationId] [nvarchar](50) NULL,
	[Status] [nvarchar](50) NULL,
	[Type] [nvarchar](50) NULL,
	[Version] [decimal](5, 2) NULL,
	[IsActive] [bit] NULL,
	[CreatedAt] [datetime] NULL,
	[UpdatedAt] [datetime] NULL,
	[BoilerRegistrationNo] [nvarchar](100) NULL,
 CONSTRAINT [PK_BoilerRegistrations] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


------------------------------------

CREATE TABLE [dbo].[BoilerRepairModifications](
	[Id] [uniqueidentifier] NOT NULL,
	[BoilerRegistrationId] [uniqueidentifier] NOT NULL,
	[BoilerRegistrationNo] [nvarchar](50) NOT NULL,
	[PersonDetailId] [uniqueidentifier] NOT NULL,
	[ApplicationId] [nvarchar](50) NOT NULL,
	[RenewalApplicationId] [nvarchar](50) NOT NULL,
	[RepairType] [nvarchar](20) NOT NULL,
	[AttendantCertificatePath] [nvarchar](500) NULL,
	[OperationEngineerCertificatePath] [nvarchar](500) NULL,
	[RepairDocumentPath] [nvarchar](500) NULL,
	[Status] [nvarchar](20) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[UpdatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[BoilerRepairModifications] ADD  DEFAULT ('Pending') FOR [Status]
GO

ALTER TABLE [dbo].[BoilerRepairModifications]  WITH CHECK ADD  CONSTRAINT [FK_Repair_Boiler] FOREIGN KEY([BoilerRegistrationId])
REFERENCES [dbo].[BoilerRegistrations] ([Id])
GO

ALTER TABLE [dbo].[BoilerRepairModifications] CHECK CONSTRAINT [FK_Repair_Boiler]
GO

ALTER TABLE [dbo].[BoilerRepairModifications]  WITH CHECK ADD  CONSTRAINT [FK_Repair_Person] FOREIGN KEY([PersonDetailId])
REFERENCES [dbo].[PersonDetails] ([Id])
GO

ALTER TABLE [dbo].[BoilerRepairModifications] CHECK CONSTRAINT [FK_Repair_Person]
GO

-------------------------------------


CREATE TABLE [dbo].[BoilerClosures](
	[Id] [uniqueidentifier] NOT NULL,
	[BoilerRegistrationId] [uniqueidentifier] NOT NULL,
	[BoilerRegistrationNo] [nvarchar](50) NOT NULL,
	[ApplicationId] [nvarchar](50) NOT NULL,
	[ClosureType] [nvarchar](20) NOT NULL,
	[ClosureDate] [date] NOT NULL,
	[ToStateName] [nvarchar](100) NULL,
	[Reasons] [nvarchar](max) NULL,
	[Remarks] [nvarchar](max) NULL,
	[ClosureReportPath] [nvarchar](500) NULL,
	[Status] [nvarchar](20) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[UpdatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[BoilerClosures]  WITH CHECK ADD  CONSTRAINT [FK_BoilerClosures_BoilerRegistrations] FOREIGN KEY([BoilerRegistrationId])
REFERENCES [dbo].[BoilerRegistrations] ([Id])
GO

ALTER TABLE [dbo].[BoilerClosures] CHECK CONSTRAINT [FK_BoilerClosures_BoilerRegistrations]
GO


CREATE TABLE [dbo].[BoilerDetails](
	[Id] [uniqueidentifier] NOT NULL,
	[AddressLine1] [nvarchar](300) NULL,
	[AddressLine2] [nvarchar](300) NULL,
	[Area] [NVARCHAR](100) NULL,
	[PinCode] [int] NULL,
	[Telephone] [nvarchar](20) NULL,
	[Mobile] [nvarchar](20) NULL,
	[Email] [nvarchar](200) NULL,
	[ErectionTypeId] [int] NULL,
	[MakerNumber] [nvarchar](100) NULL,
	[YearOfMake] [int] NULL,
	[HeatingSurfaceArea] [decimal](18, 2) NULL,
	[EvaporationCapacity] [decimal](18, 2) NULL,
	[EvaporationUnit] [nvarchar](50) NULL,
	[IntendedWorkingPressure] [decimal](18, 2) NULL,
	[PressureUnit] [nvarchar](50) NULL,
	[BoilerType] [int] NULL,
	[BoilerCategory] [int] NULL,
	[Superheater] [bit] NULL,
	[SuperheaterOutletTemp] [decimal](18, 2) NULL,
	[Economiser] [bit] NULL,
	[EconomiserOutletTemp] [decimal](18, 2) NULL,
	[FurnaceType] [int] NULL,
	[DrawingsPath] [nvarchar](500) NULL,
	[SpecificationPath] [nvarchar](500) NULL,
	[FormI_B_CPath] [nvarchar](500) NULL,
	[FormI_DPath] [nvarchar](500) NULL,
	[FormI_EPath] [nvarchar](500) NULL,
	[FormIV_APath] [nvarchar](500) NULL,
	[FormV_APath] [nvarchar](500) NULL,
	[TestCertificatesPath] [nvarchar](500) NULL,
	[WeldRepairChartsPath] [nvarchar](500) NULL,
	[PipesCertificatesPath] [nvarchar](500) NULL,
	[TubesCertificatesPath] [nvarchar](500) NULL,
	[CastingCertificatePath] [nvarchar](500) NULL,
	[ForgingCertificatePath] [nvarchar](500) NULL,
	[HeadersCertificatePath] [nvarchar](500) NULL,
	[DishedEndsInspectionPath] [nvarchar](500) NULL,
	[BoilerAttendantCertificatePath] [nvarchar](500) NULL,
	[BoilerOperationEngineerCertificatePath] [nvarchar](500) NULL,
	[DistrictId] [uniqueidentifier] NULL,
	[SubDivisionId] [uniqueidentifier] NULL,
	[TehsilId] [uniqueidentifier] NULL,
	[BoilerRegistrationId] [uniqueidentifier] NULL,
	[RenewalYears] [int] NULL,
	[ValidUpto] [datetime] NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[BoilerDetails]  WITH CHECK ADD  CONSTRAINT [FK_BoilerDetails_BoilerRegistrations] FOREIGN KEY([BoilerRegistrationId])
REFERENCES [dbo].[BoilerRegistrations] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[BoilerDetails] CHECK CONSTRAINT [FK_BoilerDetails_BoilerRegistrations]
GO

ALTER TABLE PersonDetails
ADD BoilerRegistrationId UNIQUEIDENTIFIER NULL;

ALTER TABLE BoilerDetails
ALTER COLUMN Area NVARCHAR(100) NULL;


CREATE TABLE SteamPipeLineClosures
(
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,

    ApplicationId NVARCHAR(100) NOT NULL,

    SteamPipeLineRegistrationNo NVARCHAR(100) NOT NULL,

    ReasonForClosure NVARCHAR(MAX) NULL,

    SupportingDocumentPath NVARCHAR(500) NULL,

    Type NVARCHAR(20) NOT NULL DEFAULT 'close',

    Version DECIMAL(5,2) NOT NULL DEFAULT 1.0,

    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE()
);


CREATE TABLE [dbo].[SteamPipeLineApplications](
	[Id] [uniqueidentifier] NOT NULL,
	[ApplicationId] [nvarchar](50) NOT NULL,
	[BoilerApplicationNo] [nvarchar](100) NULL,
	[ProposedLayoutDescription] [nvarchar](500) NULL,
	[ConsentLetterProvided] [nvarchar](500) NULL,
	[SteamPipeLineDrawingNo] [nvarchar](100) NULL,
	[BoilerMakerRegistrationNo] [nvarchar](100) NULL,
	[ErectorName] [nvarchar](200) NULL,
	[FactoryRegistrationNumber] [nvarchar](100) NULL,
	[Factorydetailjson] [nvarchar](max) NULL,
	[PipeLengthUpTo100mm] [decimal](18, 2) NULL,
	[PipeLengthAbove100mm] [decimal](18, 2) NULL,
	[NoOfDeSuperHeaters] [int] NULL,
	[NoOfSteamReceivers] [int] NULL,
	[NoOfFeedHeaters] [int] NULL,
	[NoOfSeparatelyFiredSuperHeaters] [int] NULL,
	[FormIIPath] [nvarchar](500) NULL,
	[FormIIIPath] [nvarchar](500) NULL,
	[FormIIIAPath] [nvarchar](500) NULL,
	[FormIIIBPath] [nvarchar](500) NULL,
	[FormIVPath] [nvarchar](500) NULL,
	[FormIVAPath] [nvarchar](500) NULL,
	[DrawingPath] [nvarchar](500) NULL,
	[SupportingDocumentsPath] [nvarchar](500) NULL,
	[Status] [nvarchar](50) NOT NULL,
	[Version] [decimal](5, 2) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Type] [nvarchar](20) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[UpdatedAt] [datetime] NOT NULL,
	[SteamPipeLineRegistrationNo] [nvarchar](20) NOT NULL,
	[RenewalYears] [int] NULL,
	[ValidFrom] [datetime] NULL,
	[ValidUpto] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[SteamPipeLineApplications] ADD  DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE [dbo].[SteamPipeLineApplications] ADD  DEFAULT ('Pending') FOR [Status]
GO

ALTER TABLE [dbo].[SteamPipeLineApplications] ADD  DEFAULT ((1.0)) FOR [Version]
GO

ALTER TABLE [dbo].[SteamPipeLineApplications] ADD  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[SteamPipeLineApplications] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO

ALTER TABLE [dbo].[SteamPipeLineApplications] ADD  DEFAULT (getdate()) FOR [UpdatedAt]
GO

ALTER TABLE [dbo].[SteamPipeLineApplications] ADD  DEFAULT ('') FOR [SteamPipeLineRegistrationNo]
GO



CREATE TABLE BoilerManufactureRegistrations (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    FactoryRegistrationNo NVARCHAR(100) NULL,
    ApplicationId NVARCHAR(100) NULL,
    ManufactureRegistrationNo NVARCHAR(100) NOT NULL,
    BmClassification NVARCHAR(255) NULL,
    
    -- Renewal & Validity
    ValidFrom DATETIME2 NULL,
    ValidUpto DATETIME2 NULL,
    CoveredArea NVARCHAR(500) NULL,
    
    -- JSON blobs for unstructured data
    EstablishmentJson NVARCHAR(MAX) NULL,
    ManufacturingFacilityjson NVARCHAR(MAX) NULL,
    DetailInternalQualityjson NVARCHAR(MAX) NULL,
    OtherReleventInformationjson NVARCHAR(MAX) NULL,

    -- Metadata
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    [Type] NVARCHAR(50) NULL DEFAULT 'new',
    [Version] DECIMAL(18, 2) NOT NULL DEFAULT 1.0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);



-- Design Facility Table
CREATE TABLE DesignFacilities (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    BoilerManufactureRegistrationId UNIQUEIDENTIFIER UNIQUE NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    AddressLine1 NVARCHAR(255) NULL,
    AddressLine2 NVARCHAR(255) NULL,
    DistrictId UNIQUEIDENTIFIER NULL,
    SubDivisionId UNIQUEIDENTIFIER NULL,
    TehsilId UNIQUEIDENTIFIER NULL,
    Area INT NULL,
    PinCode INT NULL,
    [Document] NVARCHAR(MAX) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_DesignFacility_Main FOREIGN KEY (BoilerManufactureRegistrationId) 
        REFERENCES BoilerManufactureRegistrations(Id) ON DELETE CASCADE
);

-- Testing Facility Table
CREATE TABLE TestingFacilities (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    BoilerManufactureRegistrationId UNIQUEIDENTIFIER UNIQUE NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    AddressLine1 NVARCHAR(255) NULL,
    AddressLine2 NVARCHAR(255) NULL,
    DistrictId UNIQUEIDENTIFIER NULL,
    SubDivisionId UNIQUEIDENTIFIER NULL,
    TehsilId UNIQUEIDENTIFIER NULL,
    Area INT NULL,
    PinCode INT NULL,
    TestingFacilityJson NVARCHAR(MAX) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_TestingFacility_Main FOREIGN KEY (BoilerManufactureRegistrationId) 
        REFERENCES BoilerManufactureRegistrations(Id) ON DELETE CASCADE
);

-- R&D Facility Table
CREATE TABLE RDFacilities (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    BoilerManufactureRegistrationId UNIQUEIDENTIFIER UNIQUE NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    AddressLine1 NVARCHAR(255) NULL,
    AddressLine2 NVARCHAR(255) NULL,
    DistrictId UNIQUEIDENTIFIER NULL,
    SubDivisionId UNIQUEIDENTIFIER NULL,
    TehsilId UNIQUEIDENTIFIER NULL,
    Area INT NULL,
    PinCode INT NULL,
    RDFacilityJson NVARCHAR(MAX) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_RDFacility_Main FOREIGN KEY (BoilerManufactureRegistrationId) 
        REFERENCES BoilerManufactureRegistrations(Id) ON DELETE CASCADE
);





-- 1. Drop the existing table if it exists
IF OBJECT_ID('dbo.NDTPersonnels', 'U') IS NOT NULL
    DROP TABLE dbo.NDTPersonnels;
GO

-- 2. Create the table as NDTPersonnels
CREATE TABLE NDTPersonnels (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    BoilerManufactureRegistrationId UNIQUEIDENTIFIER NOT NULL,
    
    [Name] NVARCHAR(255) NULL,
    Qualification NVARCHAR(255) NULL,
    Certificate NVARCHAR(MAX) NULL, -- Stores document path or link

    -- Logic & Metadata
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    -- Relationship
    CONSTRAINT FK_NDTPersonnels_BoilerMain FOREIGN KEY (BoilerManufactureRegistrationId) 
        REFERENCES BoilerManufactureRegistrations(Id) ON DELETE CASCADE
);
GO

IF OBJECT_ID('dbo.QualifiedWelders', 'U') IS NOT NULL
    DROP TABLE dbo.QualifiedWelders;
GO
-- Qualified Welders Table
CREATE TABLE QualifiedWelders (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    BoilerManufactureRegistrationId UNIQUEIDENTIFIER NOT NULL,
    [Name] NVARCHAR(255) NULL,
    Qualification NVARCHAR(255) NULL,
    Certificate NVARCHAR(MAX) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Welders_Main FOREIGN KEY (BoilerManufactureRegistrationId) 
        REFERENCES BoilerManufactureRegistrations(Id) ON DELETE CASCADE
);



-- 1. Drop the existing table if it exists
IF OBJECT_ID('dbo.TechnicalManpowers', 'U') IS NOT NULL
    DROP TABLE dbo.TechnicalManpowers;
GO

-- 2. Create the table as TechnicalManpowers
CREATE TABLE TechnicalManpowers (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    BoilerManufactureRegistrationId UNIQUEIDENTIFIER NOT NULL,

    [Name] NVARCHAR(255) NULL,
    FatherName NVARCHAR(255) NULL,
    Qualification NVARCHAR(255) NULL,

    -- Documents / File Paths
    MinimumFiveYearsExperienceDoc NVARCHAR(MAX) NULL,
    ExperienceInErectionDoc NVARCHAR(MAX) NULL,
    ExperienceInCommissioningDoc NVARCHAR(MAX) NULL,

    -- Logic & Metadata
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    -- Relationship
    CONSTRAINT FK_TechnicalManpowers_BoilerMain FOREIGN KEY (BoilerManufactureRegistrationId) 
        REFERENCES BoilerManufactureRegistrations(Id) ON DELETE CASCADE
);
GO


--  Boiler Repairer Registrations table sql 
CREATE TABLE BoilerRepairerRegistrations (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),

    FactoryRegistrationNo NVARCHAR(100) NULL,
    ApplicationId NVARCHAR(100) NULL,
    RepairerRegistrationNo NVARCHAR(100) NOT NULL,
    BrClassification NVARCHAR(100) NULL,

    EstablishmentJson NVARCHAR(MAX) NULL,

    -- 🔥 RENEWAL TRACKING
    ValidFrom DATETIME2 NULL,
    ValidUpto DATETIME2 NULL,

    JobsExecutedJson NVARCHAR(MAX) NULL,
    DocumentEvidence NVARCHAR(MAX) NULL,

    ApprovalHistoryJson NVARCHAR(MAX) NULL,
    RejectedHistoryJson NVARCHAR(MAX) NULL,

    ToolsAvailable BIT NULL,
    SimultaneousSites INT NULL,

    AcceptsRegulations BIT NULL,
    AcceptsResponsibility BIT NULL,
    CanSupplyMaterial BIT NULL,

    QualityControlType NVARCHAR(200) NULL,
    QualityControlDetailsjson NVARCHAR(MAX) NULL,

    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    Type NVARCHAR(50) NULL DEFAULT 'new',

    Version DECIMAL(5,2) NOT NULL DEFAULT 1.00,
    IsActive BIT NOT NULL DEFAULT 1,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
CREATE TABLE BoilerRepairerEngineers (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),

    BoilerRepairerRegistrationId UNIQUEIDENTIFIER NOT NULL,

    Name NVARCHAR(200) NOT NULL,
    Designation NVARCHAR(200) NOT NULL,
    Qualification NVARCHAR(300) NOT NULL,
    ExperienceYears INT NOT NULL,

    DocumentPath NVARCHAR(500) NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_BoilerRepairerEngineer_Registration
        FOREIGN KEY (BoilerRepairerRegistrationId)
        REFERENCES BoilerRepairerRegistrations(Id)
        ON DELETE CASCADE
);
CREATE INDEX IX_BoilerRepairerEngineers_RegistrationId
ON BoilerRepairerEngineers(BoilerRepairerRegistrationId);

CREATE TABLE BoilerRepairerWelders (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),

    BoilerRepairerRegistrationId UNIQUEIDENTIFIER NOT NULL,

    Name NVARCHAR(200) NOT NULL,
    Designation NVARCHAR(200) NOT NULL,
    ExperienceYears INT NOT NULL,

    CertificatePath NVARCHAR(500) NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_BoilerRepairerWelder_Registration
        FOREIGN KEY (BoilerRepairerRegistrationId)
        REFERENCES BoilerRepairerRegistrations(Id)
        ON DELETE CASCADE
);
CREATE INDEX IX_BoilerRepairerWelders_RegistrationId
ON BoilerRepairerWelders(BoilerRepairerRegistrationId);
ALTER TABLE BoilerRepairerWelders
ADD CONSTRAINT CK_Welder_ExperienceYears
CHECK (ExperienceYears >= 0);

-- steam pipe line table squery
CREATE TABLE SteamPipeLineApplications (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),

    ApplicationId NVARCHAR(100) NOT NULL, -- e.g. 2026/47/STPL/41628
    BoilerApplicationNo NVARCHAR(100) NULL,
    SteamPipeLineRegistrationNo NVARCHAR(100) NOT NULL,

    ProposedLayoutDescription NVARCHAR(MAX) NULL,
    ConsentLetterProvided NVARCHAR(500) NULL,
    SteamPipeLineDrawingNo NVARCHAR(200) NULL,
    BoilerMakerRegistrationNo NVARCHAR(100) NULL,
    ErectorName NVARCHAR(200) NULL,
    FactoryRegistrationNumber NVARCHAR(100) NULL,

    Factorydetailjson NVARCHAR(MAX) NULL,

    PipeLengthUpTo100mm DECIMAL(18,2) NULL,
    PipeLengthAbove100mm DECIMAL(18,2) NULL,

    NoOfDeSuperHeaters INT NULL,
    NoOfSteamReceivers INT NULL,
    NoOfFeedHeaters INT NULL,
    NoOfSeparatelyFiredSuperHeaters INT NULL,

    RenewalYears INT NULL,
    ValidFrom DATETIME2 NULL,
    ValidUpto DATETIME2 NULL,

    FormIIPath NVARCHAR(500) NULL,
    FormIIIPath NVARCHAR(500) NULL,
    FormIIIAPath NVARCHAR(500) NULL,
    FormIIIBPath NVARCHAR(500) NULL,
    FormIVPath NVARCHAR(500) NULL,
    FormIVAPath NVARCHAR(500) NULL,
    DrawingPath NVARCHAR(500) NULL,
    SupportingDocumentsPath NVARCHAR(500) NULL,

    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',

    Version DECIMAL(5,2) NOT NULL DEFAULT 1.00,
    IsActive BIT NOT NULL DEFAULT 1,

    Type NVARCHAR(50) NOT NULL, -- new / amendment / renew

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

ALTER TABLE SteamPipeLineApplications
ADD CONSTRAINT UQ_SteamPipeLineApplications_ApplicationId
UNIQUE (ApplicationId);

ALTER TABLE SteamPipeLineApplications
ADD CONSTRAINT CK_SteamPipeLine_PipeLength
CHECK (
    (PipeLengthUpTo100mm IS NULL OR PipeLengthUpTo100mm >= 0) AND
    (PipeLengthAbove100mm IS NULL OR PipeLengthAbove100mm >= 0)
);

ALTER TABLE SteamPipeLineApplications
ADD CONSTRAINT CK_SteamPipeLine_EquipmentCounts
CHECK (
    (NoOfDeSuperHeaters IS NULL OR NoOfDeSuperHeaters >= 0) AND
    (NoOfSteamReceivers IS NULL OR NoOfSteamReceivers >= 0) AND
    (NoOfFeedHeaters IS NULL OR NoOfFeedHeaters >= 0) AND
    (NoOfSeparatelyFiredSuperHeaters IS NULL OR NoOfSeparatelyFiredSuperHeaters >= 0)
);

ALTER TABLE SteamPipeLineApplications
ADD CONSTRAINT CK_SteamPipeLine_ValidDates
CHECK (
    ValidFrom IS NULL OR
    ValidUpto IS NULL OR
    ValidUpto >= ValidFrom
);

CREATE INDEX IX_SteamPipeLineApplications_Status
ON SteamPipeLineApplications(Status);

CREATE INDEX IX_SteamPipeLineApplications_IsActive
ON SteamPipeLineApplications(IsActive);


-- for ApplicationHistories table
ALTER TABLE ApplicationHistories
ADD ApplicationId NVARCHAR(255) NOT NULL,
    IsESignCompleted BIT NOT NULL DEFAULT 0,
    ESignPrnNumber NVARCHAR(100) NULL;

CREATE UNIQUE NONCLUSTERED INDEX IX_Certificates_ApplicationId
ON Certificates (ApplicationId);




date -06-03-2026 ---------------------------------------------------------------------

CREATE TABLE [dbo].[EconomiserRegistrations](
	[Id] [uniqueidentifier] NOT NULL,
	[ApplicationId] [nvarchar](50) NOT NULL,
	[Economiserregistrationno] [nvarchar](100) NULL,
	[FactoryRegistrationNumber] [nvarchar](100) NULL,
	[FactoryDetailjson] [nvarchar](max) NULL,
	[MakersNumber] [nvarchar](100) NULL,
	[MakersName] [nvarchar](200) NULL,
	[MakersAddress] [nvarchar](300) NULL,
	[YearOfMake] [nvarchar](10) NULL,
	[PressureFrom] [nvarchar](50) NULL,
	[PressureTo] [nvarchar](50) NULL,
	[ErectionType] [nvarchar](100) NULL,
	[OutletTemperature] [nvarchar](50) NULL,
	[TotalHeatingSurfaceArea] [nvarchar](100) NULL,
	[NumberOfTubes] [int] NULL,
	[NumberOfHeaders] [int] NULL,
	[FormIB] [nvarchar](500) NULL,
	[FormIC] [nvarchar](500) NULL,
	[FormIVA] [nvarchar](500) NULL,
	[FormIVB] [nvarchar](500) NULL,
	[FormIVC] [nvarchar](500) NULL,
	[FormIVD] [nvarchar](500) NULL,
	[FormVA] [nvarchar](500) NULL,
	[FormXV] [nvarchar](500) NULL,
	[FormXVI] [nvarchar](500) NULL,
	[AttendantCertificate] [nvarchar](500) NULL,
	[EngineerCertificate] [nvarchar](500) NULL,
	[Drawings] [nvarchar](500) NULL,
	[CreatedDate] [datetime] NULL,
	[UpdatedDate] [datetime] NULL,
	[Type] [nvarchar](20) NOT NULL,
	[Version] [decimal](5, 2) NOT NULL,
	[Status] [nvarchar](50) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[ValidFrom] [datetime] NULL,
	[ValidUpto] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[EconomiserRegistrations] ADD  DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE [dbo].[EconomiserRegistrations] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO

ALTER TABLE [dbo].[EconomiserRegistrations] ADD  DEFAULT (getdate()) FOR [UpdatedDate]
GO

ALTER TABLE [dbo].[EconomiserRegistrations] ADD  DEFAULT ((1.0)) FOR [Version]
GO

ALTER TABLE [dbo].[EconomiserRegistrations] ADD  DEFAULT ('Pending') FOR [Status]
GO

ALTER TABLE [dbo].[EconomiserRegistrations] ADD  DEFAULT ((1)) FOR [IsActive]
GO
-------------------------------------------------------------------------

CREATE TABLE [dbo].[EconomiserClosures](
	[Id] [uniqueidentifier] NOT NULL,
	[ApplicationId] [nvarchar](50) NULL,
	[EconomiserRegistrationNo] [nvarchar](100) NULL,
	[ClosureReason] [nvarchar](max) NULL,
	[ClosureDate] [datetime] NULL,
	[Remarks] [nvarchar](max) NULL,
	[DocumentPath] [nvarchar](500) NULL,
	[Type] [nvarchar](20) NULL,
	[Status] [nvarchar](20) NULL,
	[IsActive] [bit] NULL,
	[CreatedDate] [datetime] NULL,
	[UpdatedDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[EconomiserClosures] ADD  DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE [dbo].[EconomiserClosures] ADD  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[EconomiserClosures] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO

ALTER TABLE [dbo].[EconomiserClosures] ADD  DEFAULT (getdate()) FOR [UpdatedDate]
GO



---------------------------------------------
CREATE TABLE [dbo].[WelderApplications](
	[Id] [uniqueidentifier] NOT NULL,
	[ApplicationId] [nvarchar](50) NULL,
	[WelderRegistrationNo] [nvarchar](100) NULL,
	[Type] [nvarchar](20) NULL,
	[Version] [decimal](5, 2) NULL,
	[Status] [nvarchar](50) NULL,
	[ValidFrom] [datetime] NULL,
	[ValidUpto] [datetime] NULL,
	[CreatedDate] [datetime] NULL,
	[UpdatedDate] [datetime] NULL,
	[IsActive] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[WelderApplications] ADD  DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE [dbo].[WelderApplications] ADD  DEFAULT ((1.0)) FOR [Version]
GO

ALTER TABLE [dbo].[WelderApplications] ADD  DEFAULT ('Pending') FOR [Status]
GO

ALTER TABLE [dbo].[WelderApplications] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO

ALTER TABLE [dbo].[WelderApplications] ADD  DEFAULT (getdate()) FOR [UpdatedDate]
GO

ALTER TABLE [dbo].[WelderApplications] ADD  DEFAULT ((1)) FOR [IsActive]
GO











CREATE TABLE [dbo].[WelderDetails](
	[Id] [uniqueidentifier] NOT NULL,
	[WelderApplicationId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](200) NULL,
	[FatherName] [nvarchar](200) NULL,
	[DOB] [date] NULL,
	[IdentificationMark] [nvarchar](200) NULL,
	[Weight] [nvarchar](20) NULL,
	[Height] [nvarchar](20) NULL,
	[AddressLine1] [nvarchar](200) NULL,
	[AddressLine2] [nvarchar](200) NULL,
	[District] [nvarchar](100) NULL,
	[Tehsil] [nvarchar](100) NULL,
	[Area] [nvarchar](100) NULL,
	[Pincode] [nvarchar](20) NULL,
	[Telephone] [nvarchar](20) NULL,
	[Mobile] [nvarchar](20) NULL,
	[Email] [nvarchar](50) NULL,
	[ExperienceYears] [nvarchar](20) NULL,
	[ExperienceDetails] [nvarchar](max) NULL,
	[ExperienceCertificate] [nvarchar](500) NULL,
	[TestType] [nvarchar](100) NULL,
	[Radiography] [nvarchar](100) NULL,
	[Materials] [nvarchar](max) NULL,
	[DateOfTest] [date] NULL,
	[TypePosition] [nvarchar](100) NULL,
	[MaterialType] [nvarchar](100) NULL,
	[MaterialGrouping] [nvarchar](100) NULL,
	[ProcessOfWelding] [nvarchar](100) NULL,
	[WeldWithBacking] [nvarchar](100) NULL,
	[ElectrodeGrouping] [nvarchar](100) NULL,
	[TestPieceXrayed] [nvarchar](100) NULL,
	[Photo] [nvarchar](500) NULL,
	[Thumb] [nvarchar](500) NULL,
	[WelderSign] [nvarchar](500) NULL,
	[EmployerSign] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[WelderDetails] ADD  DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE [dbo].[WelderDetails]  WITH CHECK ADD  CONSTRAINT [FK_WelderDetails_Application] FOREIGN KEY([WelderApplicationId])
REFERENCES [dbo].[WelderApplications] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[WelderDetails] CHECK CONSTRAINT [FK_WelderDetails_Application]
GO


CREATE TABLE [dbo].[WelderEmployers](
	[Id] [uniqueidentifier] NOT NULL,
	[WelderApplicationId] [uniqueidentifier] NOT NULL,
	[EmployerType] [nvarchar](100) NULL,
	[EmployerName] [nvarchar](200) NULL,
	[FirmName] [nvarchar](200) NULL,
	[AddressLine1] [nvarchar](200) NULL,
	[AddressLine2] [nvarchar](200) NULL,
	[District] [nvarchar](100) NULL,
	[Tehsil] [nvarchar](100) NULL,
	[Area] [nvarchar](100) NULL,
	[Pincode] [nvarchar](20) NULL,
	[Telephone] [nvarchar](20) NULL,
	[Mobile] [nvarchar](20) NULL,
	[Email] [nvarchar](50) NULL,
	[EmployedFrom] [date] NULL,
	[EmployedTo] [date] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[WelderEmployers] ADD  DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE [dbo].[WelderEmployers]  WITH CHECK ADD  CONSTRAINT [FK_WelderEmployers_Application] FOREIGN KEY([WelderApplicationId])
REFERENCES [dbo].[WelderApplications] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[WelderEmployers] CHECK CONSTRAINT [FK_WelderEmployers_Application]
GO

-------------------------------------
CREATE TABLE WelderClosures
(
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),

    ApplicationId NVARCHAR(50),

    WelderRegistrationNo NVARCHAR(100),

    ClosureReason NVARCHAR(MAX),

    ClosureDate DATE,

    Remarks NVARCHAR(MAX),

    DocumentPath NVARCHAR(500),

    Type NVARCHAR(20) DEFAULT 'close',

    Status NVARCHAR(50) DEFAULT 'Pending',

    CreatedDate DATETIME DEFAULT GETDATE(),

    UpdatedDate DATETIME DEFAULT GETDATE(),

    IsActive BIT DEFAULT 1
)

ALTER TABLE FactoryMapApprovals
ALTER COLUMN Status NVARCHAR(50);

UPDATE Certificates
SET Signature = ''
WHERE Signature IS NULL;

ALTER TABLE Certificates
ADD CONSTRAINT DF_Certificates_Signature
DEFAULT '' FOR Signature;

-------------------------------------------------------------
09-03-2026

CREATE TABLE [dbo].[BoilerDrawingApplications](
	[Id] [uniqueidentifier] NOT NULL,
	[ApplicationId] [nvarchar](50) NULL,
	[BoilerDrawingRegistrationNo] [nvarchar](100) NULL,
	[FactoryRegistrationNumber] [nvarchar](100) NULL,
	[FactoryDetailjson] [nvarchar](max) NULL,
	[MakerNumber] [nvarchar](100) NULL,
	[MakerNameAndAddress] [nvarchar](300) NULL,
	[HeatingSurfaceArea] [nvarchar](100) NULL,
	[EvaporationCapacity] [nvarchar](100) NULL,
	[IntendedWorkingPressure] [nvarchar](100) NULL,
	[BoilerType] [nvarchar](100) NULL,
	[DrawingNo] [nvarchar](100) NULL,
	[BoilerDrawing] [nvarchar](500) NULL,
	[FeedPipelineDrawing] [nvarchar](500) NULL,
	[PressurePartCalculation] [nvarchar](500) NULL,
	[ValidFrom] [datetime] NULL,
	[ValidUpto] [datetime] NULL,
	[Type] [nvarchar](20) NULL,
	[Version] [decimal](5, 2) NULL,
	[Status] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NULL,
	[UpdatedDate] [datetime] NULL,
	[IsActive] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[BoilerDrawingApplications] ADD  DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE [dbo].[BoilerDrawingApplications] ADD  DEFAULT ((1.0)) FOR [Version]
GO

ALTER TABLE [dbo].[BoilerDrawingApplications] ADD  DEFAULT ('Pending') FOR [Status]
GO

ALTER TABLE [dbo].[BoilerDrawingApplications] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO

ALTER TABLE [dbo].[BoilerDrawingApplications] ADD  DEFAULT (getdate()) FOR [UpdatedDate]
GO

ALTER TABLE [dbo].[BoilerDrawingApplications] ADD  DEFAULT ((1)) FOR [IsActive]
GO




CREATE TABLE [dbo].[BoilerDrawingClosures](
	[Id] [uniqueidentifier] NOT NULL,
	[ApplicationId] [nvarchar](50) NULL,
	[BoilerDrawingRegistrationNo] [nvarchar](100) NULL,
	[ClosureReason] [nvarchar](max) NULL,
	[ClosureDate] [datetime] NULL,
	[Remarks] [nvarchar](max) NULL,
	[DocumentPath] [nvarchar](500) NULL,
	[Type] [nvarchar](20) NULL,
	[Status] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NULL,
	[UpdatedDate] [datetime] NULL,
	[IsActive] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[BoilerDrawingClosures] ADD  DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE [dbo].[BoilerDrawingClosures] ADD  DEFAULT ('close') FOR [Type]
GO

ALTER TABLE [dbo].[BoilerDrawingClosures] ADD  DEFAULT ('Pending') FOR [Status]
GO

ALTER TABLE [dbo].[BoilerDrawingClosures] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO

ALTER TABLE [dbo].[BoilerDrawingClosures] ADD  DEFAULT (getdate()) FOR [UpdatedDate]
GO

ALTER TABLE [dbo].[BoilerDrawingClosures] ADD  DEFAULT ((1)) FOR [IsActive]
GO


ALTER TABLE BoilerRegistrations
ADD 
    OldRegistrationNo NVARCHAR(100) NULL,
    OldStateName NVARCHAR(100) NULL,
    Amount DECIMAL(18,2) NOT NULL DEFAULT 0,
    IsPaymentCompleted BIT NOT NULL DEFAULT 0,
    IsESignCompleted BIT NOT NULL DEFAULT 0,
    ApplicationPDFUrl NVARCHAR(500) NULL;

ALTER TABLE UserRoles
ADD IsInspector BIT NOT NULL DEFAULT 0;

CREATE TABLE InspectorApplicationAssignments (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),

    ApplicationRegistrationId NVARCHAR(100) NOT NULL,
    ApplicationType NVARCHAR(200) NOT NULL,
    ApplicationTitle NVARCHAR(500) NOT NULL,
    ApplicationRegistrationNumber NVARCHAR(100) NOT NULL,

    AssignedToUserId UNIQUEIDENTIFIER NOT NULL,
    AssignedByUserId UNIQUEIDENTIFIER NOT NULL,

    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    Remarks NVARCHAR(MAX) NULL,

    AssignedDate DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedDate DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_InspectorAssignments_AssignedTo
        FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id),

    CONSTRAINT FK_InspectorAssignments_AssignedBy
        FOREIGN KEY (AssignedByUserId) REFERENCES Users(Id)
);
CREATE INDEX IX_InspectorAssignments_AssignedToUserId
ON InspectorApplicationAssignments(AssignedToUserId);

CREATE INDEX IX_InspectorAssignments_Status
ON InspectorApplicationAssignments(Status);


CREATE TABLE InspectorApplicationInspections (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),

    InspectorApplicationAssignmentId UNIQUEIDENTIFIER NOT NULL,

    InspectionDate DATETIME2 NOT NULL,

    BoilerCondition NVARCHAR(MAX) NOT NULL,

    MaxAllowableWorkingPressure NVARCHAR(MAX) NULL,

    Observations NVARCHAR(MAX) NOT NULL,

    DefectsFound BIT NOT NULL DEFAULT 0,

    DefectDetails NVARCHAR(MAX) NULL,

    InspectionReportNumber NVARCHAR(MAX) NULL,

    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_InspectorApplicationInspections_Assignment
    FOREIGN KEY (InspectorApplicationAssignmentId)
    REFERENCES InspectorApplicationAssignments(Id)
);

CREATE INDEX IX_Inspection_AssignmentId
ON InspectorApplicationInspections(InspectorApplicationAssignmentId);

------------------------------------------------------------------------
10-03-2026

CREATE TABLE CompetentPersonRegistrations
(
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),

    ApplicationId NVARCHAR(50) NOT NULL,
    CompetentRegistrationNo NVARCHAR(50) NULL,

    RegistrationType NVARCHAR(20) NOT NULL,   -- Individual / Institution
    Type NVARCHAR(20) NOT NULL,               -- New / Amend / Renewal

    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',

    Version DECIMAL(4,1) NOT NULL DEFAULT 1.0,

    IsActive BIT NOT NULL DEFAULT 1,

    RenewalYears INT NULL,
    ValidUpto DATETIME NULL,

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE CompetantEstablishmentDetails
(
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),

    RegistrationId UNIQUEIDENTIFIER NOT NULL,

    EstablishmentName NVARCHAR(200) NOT NULL,
    Email NVARCHAR(150) NULL,
    Mobile NVARCHAR(20) NULL,
    Telephone NVARCHAR(20) NULL,

    AddressLine1 NVARCHAR(250) NULL,
    AddressLine2 NVARCHAR(250) NULL,

    DistrictId UNIQUEIDENTIFIER NULL,
    TehsilId UNIQUEIDENTIFIER NULL,
    SdoId UNIQUEIDENTIFIER NULL,

    Area NVARCHAR(150) NULL,
    Pincode NVARCHAR(10) NULL,

    CONSTRAINT FK_Establishment_Registration
    FOREIGN KEY (RegistrationId)
    REFERENCES CompetentPersonRegistrations(Id)
    ON DELETE CASCADE
);

CREATE TABLE CompetantOccupierDetails
(
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),

    RegistrationId UNIQUEIDENTIFIER NOT NULL,

    Name NVARCHAR(150) NOT NULL,
    Designation NVARCHAR(150) NULL,
    Relation NVARCHAR(100) NULL,

    AddressLine1 NVARCHAR(250) NULL,
    AddressLine2 NVARCHAR(250) NULL,

    DistrictId UNIQUEIDENTIFIER NULL,
    TehsilId UNIQUEIDENTIFIER NULL,
    SdoId UNIQUEIDENTIFIER NULL,

    City NVARCHAR(100) NULL,
    Pincode NVARCHAR(10) NULL,

    Email NVARCHAR(150) NULL,
    Mobile NVARCHAR(20) NULL,
    Telephone NVARCHAR(20) NULL,

    CONSTRAINT FK_Occupier_Registration
    FOREIGN KEY (RegistrationId)
    REFERENCES CompetentPersonRegistrations(Id)
    ON DELETE CASCADE
);

CREATE TABLE CompetantPersonDetails
(
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),

    RegistrationId UNIQUEIDENTIFIER NOT NULL,

    Name NVARCHAR(150) NOT NULL,
    FatherName NVARCHAR(150) NULL,

    DOB DATE NULL,
    Address NVARCHAR(300) NULL,

    Email NVARCHAR(150) NULL,
    Mobile NVARCHAR(20) NULL,

    Experience INT NULL,

    Qualification NVARCHAR(150) NULL,
    Engineering NVARCHAR(150) NULL,

    PhotoPath NVARCHAR(300) NULL,
    SignPath NVARCHAR(300) NULL,
    AttachmentPath NVARCHAR(300) NULL,

    CONSTRAINT FK_CompetentPerson_Registration
    FOREIGN KEY (RegistrationId)
    REFERENCES CompetentPersonRegistrations(Id)
    ON DELETE CASCADE
);


CREATE TABLE [dbo].[CompetentPersonEquipments](
	[Id] [uniqueidentifier] NOT NULL,
	[EquipmentRegistrationId] [uniqueidentifier] NOT NULL,
	[CompetentPersonId] [uniqueidentifier] NOT NULL,
	[EquipmentType] [nvarchar](200) NULL,
	[EquipmentName] [nvarchar](200) NULL,
	[IdentificationNumber] [nvarchar](100) NULL,
	[CalibrationCertificateNumber] [nvarchar](100) NULL,
	[DateOfCalibration] [datetime] NULL,
	[CalibrationValidity] [datetime] NULL,
	[CalibrationCertificatePath] [nvarchar](300) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CompetentPersonEquipments] ADD  DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE [dbo].[CompetentPersonEquipments]  WITH CHECK ADD  CONSTRAINT [FK_EquipmentPerson] FOREIGN KEY([CompetentPersonId])
REFERENCES [dbo].[CompetantPersonDetails] ([Id])
GO

ALTER TABLE [dbo].[CompetentPersonEquipments] CHECK CONSTRAINT [FK_EquipmentPerson]
GO

ALTER TABLE [dbo].[CompetentPersonEquipments]  WITH CHECK ADD  CONSTRAINT [FK_EquipmentRegistration] FOREIGN KEY([EquipmentRegistrationId])
REFERENCES [dbo].[CompetentEquipmentRegistrations] ([Id])
GO

ALTER TABLE [dbo].[CompetentPersonEquipments] CHECK CONSTRAINT [FK_EquipmentRegistration]
GO

CREATE TABLE [dbo].[CompetentEquipmentRegistrations](
	[Id] [uniqueidentifier] NOT NULL,
	[ApplicationId] [nvarchar](50) NOT NULL,
	[CompetentEquipmentRegistrationNo] [nvarchar](50) NULL,
	[Type] [nvarchar](20) NOT NULL,
	[Status] [nvarchar](20) NOT NULL,
	[Version] [decimal](4, 1) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[RenewalYears] [int] NULL,
	[ValidUpto] [datetime] NULL,
	[CreatedAt] [datetime] NOT NULL,
	[UpdatedAt] [datetime] NOT NULL,
	[CompetentRegistrationNo] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CompetentEquipmentRegistrations] ADD  DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE [dbo].[CompetentEquipmentRegistrations] ADD  DEFAULT ('Pending') FOR [Status]
GO

ALTER TABLE [dbo].[CompetentEquipmentRegistrations] ADD  DEFAULT ((1.0)) FOR [Version]
GO

ALTER TABLE [dbo].[CompetentEquipmentRegistrations] ADD  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[CompetentEquipmentRegistrations] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO

ALTER TABLE [dbo].[CompetentEquipmentRegistrations] ADD  DEFAULT (getdate()) FOR [UpdatedAt]
GO
ALTER TABLE EstablishmentRegistrations
DROP COLUMN ContractorDetailId;

ALTER TABLE EstablishmentRegistrations
ADD FactoryCategoryId UNIQUEIDENTIFIER;

ALTER TABLE EstablishmentRegistrations
ADD CONSTRAINT FK_EstablishmentRegistrations_FactoryCategories
FOREIGN KEY (FactoryCategoryId)
REFERENCES FactoryCategories(Id);

ALTER TABLE InspectorApplicationInspections
ADD
    HydraulicTestPressure NVARCHAR(100) NULL,
    HydraulicTestDuration NVARCHAR(100) NULL,

    -- Shell / component conditions
    JointsCondition NVARCHAR(100) NULL,
    RivetsCondition NVARCHAR(100) NULL,
    PlatingCondition NVARCHAR(100) NULL,
    StaysCondition NVARCHAR(100) NULL,
    CrownCondition NVARCHAR(100) NULL,
    FireboxCondition NVARCHAR(100) NULL,
    FusiblePlugCondition NVARCHAR(100) NULL,
    FireTubesCondition NVARCHAR(100) NULL,
    FlueFurnaceCondition NVARCHAR(100) NULL,
    SmokeBoxCondition NVARCHAR(100) NULL,
    SteamDrumCondition NVARCHAR(100) NULL,

    -- Mountings conditions
    SafetyValveCondition NVARCHAR(100) NULL,
    PressureGaugeCondition NVARCHAR(100) NULL,
    FeedCheckCondition NVARCHAR(100) NULL,
    StopValveCondition NVARCHAR(100) NULL,
    BlowDownCondition NVARCHAR(100) NULL,
    EconomiserCondition NVARCHAR(100) NULL,
    SuperheaterCondition NVARCHAR(100) NULL,
    AirPressureGaugeCondition NVARCHAR(100) NULL,

    -- Working pressure allowed
    AllowedWorkingPressure NVARCHAR(100) NULL,

    -- Provisional order
    ProvisionalOrderNumber NVARCHAR(100) NULL,
    ProvisionalOrderDate DATETIME NULL,

    -- Boiler attendant details
    BoilerAttendantName NVARCHAR(200) NULL,
    BoilerAttendantCertNo NVARCHAR(100) NULL,

    -- Fee and challan
    FeeAmount NVARCHAR(100) NULL,
    ChallanNumber NVARCHAR(100) NULL;


CREATE TABLE BoilerWorkflowLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),

    ApplicationId NVARCHAR(100) NOT NULL,

    Part INT NOT NULL,

    FromUserId UNIQUEIDENTIFIER NULL,
    ToUserId UNIQUEIDENTIFIER NULL,

    FromLevel INT NULL,
    ToLevel INT NULL,

    ActionType NVARCHAR(100) NOT NULL,

    Remarks NVARCHAR(MAX) NULL,

    CycleNumber INT NULL,

    ChiefActionValue NVARCHAR(200) NULL,

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_BoilerWorkflowLogs_FromUser
        FOREIGN KEY (FromUserId) REFERENCES Users(Id),

    CONSTRAINT FK_BoilerWorkflowLogs_ToUser
        FOREIGN KEY (ToUserId) REFERENCES Users(Id)
);
CREATE TABLE BoilerApplicationStates (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),

    ApplicationId NVARCHAR(100) NOT NULL,

    CurrentStatus NVARCHAR(50) NOT NULL DEFAULT 'Draft',

    CurrentPart INT NOT NULL DEFAULT 1,

    CurrentLevel INT NOT NULL DEFAULT 1,

    AssignedInspectorId UNIQUEIDENTIFIER NULL,

    AuthorityForwardedAt DATETIME NULL,

    InspectorActionsEnabled BIT NOT NULL DEFAULT 0,

    ChiefCycleCount INT NOT NULL DEFAULT 0,

    LastChiefActionValue NVARCHAR(100) NULL,

    RegistrationNumber NVARCHAR(100) NULL,

    CertificatePath NVARCHAR(500) NULL,

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_BoilerApplicationStates_AssignedInspector
        FOREIGN KEY (AssignedInspectorId) REFERENCES Users(Id)
);
CREATE TABLE ChiefInspectionScrutinyRemarks (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),

    RemarkText NVARCHAR(500) NOT NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    DisplayOrder INT NOT NULL DEFAULT 0,

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
CREATE TABLE InspectionFormSubmissions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),

    ApplicationId NVARCHAR(100) NOT NULL,

    InspectorId UNIQUEIDENTIFIER NOT NULL,

    FormData NVARCHAR(MAX) NULL,

    Photos NVARCHAR(MAX) NULL,

    Documents NVARCHAR(MAX) NULL,

    GeneratedPdfPath NVARCHAR(500) NULL,

    ESignData NVARCHAR(MAX) NULL,

    SubmittedAt DATETIME NULL,

    PreviewGeneratedAt DATETIME NULL,

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_InspectionFormSubmissions_Inspector
        FOREIGN KEY (InspectorId) REFERENCES Users(Id)
);
CREATE TABLE InspectionSchedules (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),

    ApplicationId NVARCHAR(100) NOT NULL,

    InspectorId UNIQUEIDENTIFIER NOT NULL,

    InspectionDate DATETIME NOT NULL,

    InspectionTime TIME NOT NULL,

    PlaceAddress NVARCHAR(500) NOT NULL,

    InspectionType NVARCHAR(100) NULL,

    EstimatedDuration NVARCHAR(100) NULL,

    InspectorNotes NVARCHAR(MAX) NULL,

    IsLocked BIT NOT NULL DEFAULT 0,

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_InspectionSchedules_Inspector
        FOREIGN KEY (InspectorId) REFERENCES Users(Id)
);

CREATE TABLE InspectionScrutinyWorkflows (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),

    OfficeId UNIQUEIDENTIFIER NOT NULL,

    LevelCount INT NOT NULL CHECK (LevelCount BETWEEN 2 AND 3),

    IsBidirectional BIT NOT NULL DEFAULT 1,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_InspectionScrutinyWorkflows_Office
        FOREIGN KEY (OfficeId) REFERENCES Offices(Id)
);

CREATE TABLE InspectionScrutinyLevels (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),

    WorkflowId UNIQUEIDENTIFIER NOT NULL,

    LevelNumber INT NOT NULL,

    OfficePostId UNIQUEIDENTIFIER NOT NULL,

    IsPrefilled BIT NOT NULL DEFAULT 0,

    PrefillSource NVARCHAR(100) NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_InspectionScrutinyLevels_Workflow
        FOREIGN KEY (WorkflowId) REFERENCES InspectionScrutinyWorkflows(Id)
);


CREATE INDEX IX_BoilerWorkflowLogs_ApplicationId
ON BoilerWorkflowLogs(ApplicationId);

CREATE INDEX IX_BoilerApplicationStates_ApplicationId
ON BoilerApplicationStates(ApplicationId);

CREATE INDEX IX_InspectionSchedules_ApplicationId
ON InspectionSchedules(ApplicationId);

CREATE INDEX IX_InspectionFormSubmissions_ApplicationId
ON InspectionFormSubmissions(ApplicationId);

ALTER TABLE EstablishmentRegistrations 
ADD ApplicationId NVARCHAR(50) NOT NULL DEFAULT '';

----------------------------------------------------------------
13.03.2026
CREATE TABLE [dbo].[SMTCTrainerDetails](
	[Id] [uniqueidentifier] NOT NULL,
	[SMTCRegistrationId] [uniqueidentifier] NOT NULL,
	[TrainerName] [nvarchar](200) NOT NULL,
	[TotalYearsExperience] [int] NULL,
	[Mobile] [nvarchar](20) NULL,
	[PhotoPath] [nvarchar](500) NULL,
	[DegreeDocumentPath] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[SMTCTrainerDetails] ADD  DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE [dbo].[SMTCTrainerDetails]  WITH CHECK ADD  CONSTRAINT [FK_SMTCTrainerRegistration] FOREIGN KEY([SMTCRegistrationId])
REFERENCES [dbo].[SMTCRegistrations] ([Id])
GO

ALTER TABLE [dbo].[SMTCTrainerDetails] CHECK CONSTRAINT [FK_SMTCTrainerRegistration]
GO

CREATE TABLE [dbo].[SMTCTrainerDetails](
	[Id] [uniqueidentifier] NOT NULL,
	[SMTCRegistrationId] [uniqueidentifier] NOT NULL,
	[TrainerName] [nvarchar](200) NOT NULL,
	[TotalYearsExperience] [int] NULL,
	[Mobile] [nvarchar](20) NULL,
	[PhotoPath] [nvarchar](500) NULL,
	[DegreeDocumentPath] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[SMTCTrainerDetails] ADD  DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE [dbo].[SMTCTrainerDetails]  WITH CHECK ADD  CONSTRAINT [FK_SMTCTrainerRegistration] FOREIGN KEY([SMTCRegistrationId])
REFERENCES [dbo].[SMTCRegistrations] ([Id])
GO

ALTER TABLE [dbo].[SMTCTrainerDetails] CHECK CONSTRAINT [FK_SMTCTrainerRegistration]
GO

CREATE TABLE [dbo].[SMTCTrainerEducationDetails](
	[Id] [uniqueidentifier] NOT NULL,
	[TrainerId] [uniqueidentifier] NOT NULL,
	[EducationType] [nvarchar](20) NOT NULL,
	[Course] [nvarchar](200) NULL,
	[Degree] [nvarchar](200) NULL,
	[UniversityCollege] [nvarchar](300) NULL,
	[PassingYear] [int] NULL,
	[Specialization] [nvarchar](200) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[SMTCTrainerEducationDetails] ADD  DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE [dbo].[SMTCTrainerEducationDetails]  WITH CHECK ADD  CONSTRAINT [FK_SMTCTrainerEducation] FOREIGN KEY([TrainerId])
REFERENCES [dbo].[SMTCTrainerDetails] ([Id])
GO

ALTER TABLE [dbo].[SMTCTrainerEducationDetails] CHECK CONSTRAINT [FK_SMTCTrainerEducation]
GO

drop table DocumentUploads
/****** Object:  Table [dbo].[DocumentUploads]    Script Date: 16/03/2026 18:25:32 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[DocumentUploads](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[ModuleId] [uniqueidentifier] NOT NULL,
	[DocumentType] [nvarchar](100) NOT NULL,
	[DocumentName] [nvarchar](255) NOT NULL,
	[DocumentUrl] [nvarchar](500) NOT NULL,
	[DocumentSize] [bigint] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
	[ModuleDocType] [nvarchar](100) NOT NULL,
	[Version] [decimal](5, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[DocumentUploads] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO

ALTER TABLE [dbo].[DocumentUploads] ADD  DEFAULT (getutcdate()) FOR [UpdatedAt]
GO

ALTER TABLE [dbo].[DocumentUploads] ADD  DEFAULT ('') FOR [ModuleDocType]
GO

ALTER TABLE [dbo].[DocumentUploads] ADD  DEFAULT ((1.0)) FOR [Version]
GO


CREATE INDEX IX_DocumentUploads_User_Module_DocType
ON DocumentUploads(UserId, ModuleId, ModuleDocType, Version);

ALTER TABLE EstablishmentRegistrations
ADD OccupierIdProof NVARCHAR(500) NOT NULL DEFAULT '',
    PartnershipDeed NVARCHAR(500) NOT NULL DEFAULT '',
    ManagerIdProof NVARCHAR(500) NOT NULL DEFAULT '',
    LoadSanctionCopy NVARCHAR(500) NOT NULL DEFAULT '';

ALTER TABLE Certificates
DROP CONSTRAINT DF_Certificates_Signature;
    ALTER TABLE Certificates
DROP COLUMN Place, Signature;

ALTER TABLE Certificates
ALTER COLUMN EndDate DATETIME NULL;

ALTER TABLE EstablishmentRegistrations
ADD ObjectionLetterUrl NVARCHAR(500) NOT NULL DEFAULT '';

CREATE TABLE ApplicationObjectionLetters (
    Id INT IDENTITY(1,1) PRIMARY KEY,

    ApplicationId NVARCHAR(100) NOT NULL,

    ModuleName NVARCHAR(100) NOT NULL,

    FileUrl NVARCHAR(MAX) NOT NULL,

    Subject NVARCHAR(MAX) NULL,

    GeneratedBy NVARCHAR(100) NULL,

    GeneratedByName NVARCHAR(200) NULL,

    SignatoryDesignation NVARCHAR(200) NULL,

    SignatoryLocation NVARCHAR(200) NULL,

    Version INT NOT NULL DEFAULT 1,

    CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE INDEX IX_ApplicationObjectionLetters_ApplicationId
ON ApplicationObjectionLetters (ApplicationId);

CREATE INDEX IX_AppObjection_AppId_Version
ON ApplicationObjectionLetters (ApplicationId, Version);

CREATE UNIQUE INDEX UX_AppObjection_AppId_Version
ON ApplicationObjectionLetters (ApplicationId, Version);

CREATE INDEX IX_ApplicationObjectionLetters_AppId_Version
ON ApplicationObjectionLetters (ApplicationId, Version);

CREATE UNIQUE INDEX UX_ApplicationObjectionLetters_AppId_Version
ON ApplicationObjectionLetters (ApplicationId, Version);

ALTER TABLE EstablishmentRegistrations
ADD AutoRenewal BIT NOT NULL DEFAULT 0;

ALTER TABLE EstablishmentDetails
ADD PanNumber NVARCHAR(20) NOT NULL DEFAULT '';

ALTER TABLE FactoryDetails
ADD ManufacturingType NVARCHAR(50) NOT NULL DEFAULT '';

ALTER TABLE BeediCigarWorks
ADD ManufacturingType NVARCHAR(50) NOT NULL DEFAULT '';

ALTER TABLE ContractorDetails
ADD MaxContractWorkerCountTransgender INT NULL;

ALTER TABLE EstablishmentUserDetails
ADD RelationType NVARCHAR(50) NULL,
    RelativeName NVARCHAR(50) NULL;

ALTER TABLE FactoryMapApprovals
    ADD ObjectionLetterUrl NVARCHAR(500) NULL;

-- Remove old column
ALTER TABLE FactoryDetails
    DROP COLUMN OwnershipTypeSector;

-- Add new columns
ALTER TABLE FactoryDetails
    ADD OwnershipType NVARCHAR(255) NULL,
        OwnershipSector NVARCHAR(255) NULL;

ALTER TABLE FactoryMapApprovals
    ADD MaxWorkerTransgender INT NOT NULL DEFAULT 0,
        PremiseOwnerDetails NVARCHAR(MAX) NULL;

ALTER TABLE FactoryMapRawMaterials
    ADD Unit NVARCHAR(50) NULL;

ALTER TABLE FactoryMapIntermediateProducts
    ADD Unit NVARCHAR(50) NULL;

ALTER TABLE FactoryMapApprovalChemicals
    ADD Unit NVARCHAR(50) NULL;
    
--------------------------------------------------------------------
18-03-2026
CREATE TABLE [dbo].[FactoryMapApprovalFiles] (
    [Id] NVARCHAR(50) NOT NULL PRIMARY KEY,

    [FactoryMapApprovalId] NVARCHAR(50) NOT NULL,  -- Reference to main table

    LandOwnershipDocumentUrl NVARCHAR(500) NULL,
    ApprovedLandPlanUrl NVARCHAR(500) NULL,
    ManufacturingProcessDescriptionUrl NVARCHAR(500) NULL,
    ProcessFlowChartUrl NVARCHAR(500) NULL,
    RawMaterialsListUrl NVARCHAR(500) NULL,
    HazardousProcessesListUrl NVARCHAR(500) NULL,
    EmergencyPlanUrl NVARCHAR(500) NULL,
    SafetyHealthPolicyUrl NVARCHAR(500) NULL,
    FactoryPlanDrawingUrl NVARCHAR(500) NULL,
    SafetyPolicyApplicableUrl NVARCHAR(500) NULL,
    OccupierPhotoIdProofUrl NVARCHAR(500) NULL,
    OccupierAddressProofUrl NVARCHAR(500) NULL,

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,

   
);

---------------
23-03-26
ALTER TABLE BoilerManufactureRegistrations
ADD 
    Amount DECIMAL(18,2),
    IsPaymentCompleted BIT DEFAULT 0,
    IsESignCompleted BIT DEFAULT 0,
    ApplicationPDFUrl NVARCHAR(500);
  ----------------------------------------------
    alter table  SteamPipeLineApplications 
ADD     Amount DECIMAL(18,2)

-----------------------
alter table  BoilerRepairerRegistrations 
ADD 
    Amount DECIMAL(18,2)

    ----------------------------------

    alter table  EconomiserRegistrations 
ADD 
    Amount DECIMAL(18,2)

    -------------------------------

    	ALTER TABLE WelderApplications
add  Amount DECIMAL(18,2);


CREATE TABLE BoilerDrawingRegistrations (
    Id UNIQUEIDENTIFIER PRIMARY KEY,

    ApplicationId NVARCHAR(100) NULL,
    BoilerDrawingRegistrationNo NVARCHAR(100) NULL,

    Amount DECIMAL(18,2) NOT NULL,

    /* ================= FACTORY ================= */
    FactoryRegistrationNumber NVARCHAR(100) NULL,
    FactoryDetailjson NVARCHAR(MAX) NULL,

    /* ================= BOILER DRAWING ================= */
    MakerNumber NVARCHAR(100) NULL,
    MakerNameAndAddress NVARCHAR(500) NULL,
    HeatingSurfaceArea NVARCHAR(100) NULL,
    EvaporationCapacity NVARCHAR(100) NULL,
    IntendedWorkingPressure NVARCHAR(100) NULL,
    BoilerType NVARCHAR(100) NULL,
    DrawingNo NVARCHAR(100) NULL,

    /* ================= DOCUMENTS ================= */
    BoilerDrawing NVARCHAR(500) NULL,
    FeedPipelineDrawing NVARCHAR(500) NULL,
    PressurePartCalculation NVARCHAR(500) NULL,

    /* ================= VALIDITY ================= */
    ValidFrom DATETIME NULL,
    ValidUpto DATETIME NULL,

    /* ================= WORKFLOW ================= */
    Type NVARCHAR(50) NULL,
    Version DECIMAL(10,2) NOT NULL,
    Status NVARCHAR(50) NULL,

    CreatedDate DATETIME NOT NULL,
    UpdatedDate DATETIME NOT NULL,

    IsActive BIT NOT NULL
);


CREATE TABLE BoilerDrawingClosures (
    Id UNIQUEIDENTIFIER PRIMARY KEY,

    ApplicationId NVARCHAR(100) NULL,
    BoilerDrawingRegistrationNo NVARCHAR(100) NULL,

    ClosureReason NVARCHAR(500) NULL,
    ClosureDate DATETIME NULL,
    Remarks NVARCHAR(1000) NULL,

    DocumentPath NVARCHAR(500) NULL,

    Type NVARCHAR(50) NULL,
    Status NVARCHAR(50) NULL,

    CreatedDate DATETIME NOT NULL,
    UpdatedDate DATETIME NOT NULL,

    IsActive BIT NOT NULL
);
--------------------------------------
30-03-2026

CREATE TABLE BoilerCategories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    HeatingSurfaceArea DECIMAL(10,2) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

ALTER TABLE Posts
ADD SeniorityOrder INT NOT NULL DEFAULT 0;

ALTER TABLE ApplicationApprovalRequests
ADD Direction NVARCHAR(20) NOT NULL DEFAULT 'Forward';

ALTER TABLE FactoryMapApprovals
ADD FactoryRegistrationNumber VARCHAR(100) NULL;

ALTER TABLE FactoryLicenses
ADD ObjectionLetterUrl NVARCHAR(500) NOT NULL DEFAULT '';

ALTER TABLE FactoryMapApprovals
ADD NoOfShifts INT NOT NULL DEFAULT 1;

ALTER TABLE FactoryMapApprovals
ADD CONSTRAINT CK_FactoryMapApprovals_NoOfShifts
CHECK (NoOfShifts BETWEEN 1 AND 5);


ALTER TABLE FactoryLicenses
ADD 
    WorkersProposedMale INT NULL,
    WorkersProposedFemale INT NULL,
    WorkersProposedTransgender INT NULL,

    WorkersLastYearMale INT NULL,
    WorkersLastYearFemale INT NULL,
    WorkersLastYearTransgender INT NULL,

    WorkersOrdinaryMale INT NULL,
    WorkersOrdinaryFemale INT NULL,
    WorkersOrdinaryTransgender INT NULL,

    SanctionedLoad DECIMAL(18,2) NULL,
    SanctionedLoadUnit NVARCHAR(50) NULL,

    ManufacturingProcessLast12Months NVARCHAR(MAX) NULL,
    ManufacturingProcessNext12Months NVARCHAR(MAX) NULL,
    DateOfStartProduction NVARCHAR(50) NULL,

    FactoryData NVARCHAR(MAX) NULL,
    MapApprovalData NVARCHAR(MAX) NULL;

-- boiler fees table 
CREATE TABLE BoilerFees (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MaxHeatingSurfaceArea DECIMAL(10,2) NOT NULL,
    Fees DECIMAL(10,2) NOT NULL
);

INSERT INTO BoilerFees (MaxHeatingSurfaceArea, Fees)
VALUES
(10, 1800),
(30, 2400),
(50, 2700),
(70, 3300),
(90, 3900),
(110, 4500),
(200, 5100),
(400, 5700),
(600, 6600),
(800, 7200),
(1000, 8100),
(1200, 9600),
(1400, 10800),
(1600, 12600),
(1800, 13500),
(2000, 15000),
(2200, 16200),
(2400, 18000),
(2600, 18900),
(2800, 20400),
(3000, 21600);
