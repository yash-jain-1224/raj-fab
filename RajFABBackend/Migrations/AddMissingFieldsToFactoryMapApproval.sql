-- Migration: Add missing fields to FactoryMapApprovals table
-- Run this query directly on your database

-- Add missing columns
ALTER TABLE [FactoryMapApprovals] 
ADD [Area] NVARCHAR(100) NULL;

ALTER TABLE [FactoryMapApprovals] 
ADD [PoliceStation] NVARCHAR(100) NULL;

ALTER TABLE [FactoryMapApprovals] 
ADD [RailwayStation] NVARCHAR(100) NULL;

ALTER TABLE [FactoryMapApprovals] 
ADD [BusinessRegistrationNumber] NVARCHAR(100) NULL;

ALTER TABLE [FactoryMapApprovals] 
ADD [OccupierDesignation] NVARCHAR(100) NULL;

-- Add extended properties for documentation
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Post Office Area Name', 
    @level0type = N'SCHEMA', @level0name = 'dbo',
    @level1type = N'TABLE',  @level1name = 'FactoryMapApprovals',
    @level2type = N'COLUMN', @level2name = 'Area';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Police Station associated with the factory', 
    @level0type = N'SCHEMA', @level0name = 'dbo',
    @level1type = N'TABLE',  @level1name = 'FactoryMapApprovals',
    @level2type = N'COLUMN', @level2name = 'PoliceStation';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Railway Station nearest to the factory', 
    @level0type = N'SCHEMA', @level0name = 'dbo',
    @level1type = N'TABLE',  @level1name = 'FactoryMapApprovals',
    @level2type = N'COLUMN', @level2name = 'RailwayStation';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Business Registration Number of the factory', 
    @level0type = N'SCHEMA', @level0name = 'dbo',
    @level1type = N'TABLE',  @level1name = 'FactoryMapApprovals',
    @level2type = N'COLUMN', @level2name = 'BusinessRegistrationNumber';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Specific designation of the occupier', 
    @level0type = N'SCHEMA', @level0name = 'dbo',
    @level1type = N'TABLE',  @level1name = 'FactoryMapApprovals',
    @level2type = N'COLUMN', @level2name = 'OccupierDesignation';

-- Create indexes for better query performance
CREATE NONCLUSTERED INDEX [IX_FactoryMapApprovals_PoliceStation] 
ON [FactoryMapApprovals] ([PoliceStation]);

CREATE NONCLUSTERED INDEX [IX_FactoryMapApprovals_RailwayStation] 
ON [FactoryMapApprovals] ([RailwayStation]);

CREATE NONCLUSTERED INDEX [IX_FactoryMapApprovals_BusinessRegistrationNumber] 
ON [FactoryMapApprovals] ([BusinessRegistrationNumber]);

-- Verify the changes
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    CHARACTER_MAXIMUM_LENGTH, 
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'FactoryMapApprovals'
AND COLUMN_NAME IN ('Area', 'PoliceStation', 'RailwayStation', 'BusinessRegistrationNumber', 'OccupierDesignation')
ORDER BY COLUMN_NAME;
