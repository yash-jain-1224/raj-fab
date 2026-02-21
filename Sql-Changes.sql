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