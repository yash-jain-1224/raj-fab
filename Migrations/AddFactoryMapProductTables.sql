-- Migration: Create FactoryMapFinishGoods, FactoryMapDangerousOperations, and FactoryMapHazardousChemicals tables
-- Run this query directly on your database

-- Create FactoryMapFinishGoods table
CREATE TABLE [FactoryMapFinishGoods] (
    [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
    [FactoryMapApprovalId] NVARCHAR(450) NOT NULL,
    [ProductName] NVARCHAR(200) NOT NULL,
    [QuantityPerDay] DECIMAL(18,2) NOT NULL,
    [Unit] NVARCHAR(50) NOT NULL,
    [MaxStorageCapacity] DECIMAL(18,2) NULL,
    [StorageMethod] NVARCHAR(100) NULL,
    [Remarks] NVARCHAR(500) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT [FK_FactoryMapFinishGoods_FactoryMapApprovals] 
        FOREIGN KEY ([FactoryMapApprovalId]) 
        REFERENCES [FactoryMapApprovals]([Id]) 
        ON DELETE CASCADE
);

-- Create index for FactoryMapFinishGoods
CREATE NONCLUSTERED INDEX [IX_FactoryMapFinishGoods_FactoryMapApprovalId] 
ON [FactoryMapFinishGoods] ([FactoryMapApprovalId]);

-- Create FactoryMapDangerousOperations table
CREATE TABLE [FactoryMapDangerousOperations] (
    [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
    [FactoryMapApprovalId] NVARCHAR(450) NOT NULL,
    [ChemicalName] NVARCHAR(500) NOT NULL,
    [OrganicInorganicDetails] NVARCHAR(500) NOT NULL,
    [Comments] NVARCHAR(1000) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT [FK_FactoryMapDangerousOperations_FactoryMapApprovals] 
        FOREIGN KEY ([FactoryMapApprovalId]) 
        REFERENCES [FactoryMapApprovals]([Id]) 
        ON DELETE CASCADE
);

-- Create index for FactoryMapDangerousOperations
CREATE NONCLUSTERED INDEX [IX_FactoryMapDangerousOperations_FactoryMapApprovalId] 
ON [FactoryMapDangerousOperations] ([FactoryMapApprovalId]);

-- Create FactoryMapHazardousChemicals table
CREATE TABLE [FactoryMapHazardousChemicals] (
    [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
    [FactoryMapApprovalId] NVARCHAR(450) NOT NULL,
    [ChemicalName] NVARCHAR(200) NOT NULL,
    [ChemicalType] NVARCHAR(200) NOT NULL,
    [Comments] NVARCHAR(1000) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT [FK_FactoryMapHazardousChemicals_FactoryMapApprovals] 
        FOREIGN KEY ([FactoryMapApprovalId]) 
        REFERENCES [FactoryMapApprovals]([Id]) 
        ON DELETE CASCADE
);

-- Create index for FactoryMapHazardousChemicals
CREATE NONCLUSTERED INDEX [IX_FactoryMapHazardousChemicals_FactoryMapApprovalId] 
ON [FactoryMapHazardousChemicals] ([FactoryMapApprovalId]);

-- Add extended properties for documentation
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Finished/Final products manufactured by the factory', 
    @level0type = N'SCHEMA', @level0name = 'dbo',
    @level1type = N'TABLE',  @level1name = 'FactoryMapFinishGoods';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Dangerous operations involving chemicals in the factory', 
    @level0type = N'SCHEMA', @level0name = 'dbo',
    @level1type = N'TABLE',  @level1name = 'FactoryMapDangerousOperations';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Hazardous chemicals used or stored in the factory', 
    @level0type = N'SCHEMA', @level0name = 'dbo',
    @level1type = N'TABLE',  @level1name = 'FactoryMapHazardousChemicals';

-- Verify the tables were created
SELECT 
    TABLE_NAME,
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME) AS ColumnCount
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_NAME IN ('FactoryMapFinishGoods', 'FactoryMapDangerousOperations', 'FactoryMapHazardousChemicals')
ORDER BY TABLE_NAME;
