-- Migration: Add FactoryRegistrationFinishGood, FactoryRegistrationDangerousOperation, and FactoryRegistrationHazardousChemical tables

-- Create FactoryRegistrationFinishGoods table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FactoryRegistrationFinishGoods]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[FactoryRegistrationFinishGoods] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [FactoryRegistrationId] NVARCHAR(450) NOT NULL,
        [ProductName] NVARCHAR(200) NOT NULL,
        [MaxStorageCapacity] DECIMAL(18,2) NULL,
        [Unit] NVARCHAR(50) NULL,
        [Remarks] NVARCHAR(500) NULL,
        CONSTRAINT [FK_FactoryRegistrationFinishGoods_FactoryRegistrations] 
            FOREIGN KEY ([FactoryRegistrationId]) 
            REFERENCES [dbo].[FactoryRegistrations] ([Id]) 
            ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_FactoryRegistrationFinishGoods_FactoryRegistrationId] 
        ON [dbo].[FactoryRegistrationFinishGoods] ([FactoryRegistrationId]);
END
GO

-- Create FactoryRegistrationDangerousOperations table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FactoryRegistrationDangerousOperations]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[FactoryRegistrationDangerousOperations] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [FactoryRegistrationId] NVARCHAR(450) NOT NULL,
        [OperationName] NVARCHAR(200) NOT NULL,
        [ChemicalInvolved] NVARCHAR(200) NULL,
        [Remarks] NVARCHAR(500) NULL,
        CONSTRAINT [FK_FactoryRegistrationDangerousOperations_FactoryRegistrations] 
            FOREIGN KEY ([FactoryRegistrationId]) 
            REFERENCES [dbo].[FactoryRegistrations] ([Id]) 
            ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_FactoryRegistrationDangerousOperations_FactoryRegistrationId] 
        ON [dbo].[FactoryRegistrationDangerousOperations] ([FactoryRegistrationId]);
END
GO

-- Create FactoryRegistrationHazardousChemicals table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FactoryRegistrationHazardousChemicals]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[FactoryRegistrationHazardousChemicals] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [FactoryRegistrationId] NVARCHAR(450) NOT NULL,
        [ChemicalName] NVARCHAR(200) NOT NULL,
        [CASNumber] NVARCHAR(50) NULL,
        [Quantity] DECIMAL(18,2) NULL,
        [Unit] NVARCHAR(50) NULL,
        [Remarks] NVARCHAR(500) NULL,
        CONSTRAINT [FK_FactoryRegistrationHazardousChemicals_FactoryRegistrations] 
            FOREIGN KEY ([FactoryRegistrationId]) 
            REFERENCES [dbo].[FactoryRegistrations] ([Id]) 
            ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_FactoryRegistrationHazardousChemicals_FactoryRegistrationId] 
        ON [dbo].[FactoryRegistrationHazardousChemicals] ([FactoryRegistrationId]);
END
GO

PRINT 'Migration completed: Added FactoryRegistrationFinishGoods, FactoryRegistrationDangerousOperations, and FactoryRegistrationHazardousChemicals tables';
