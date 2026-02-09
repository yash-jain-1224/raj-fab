-- Migration: Add missing fields to FactoryMapApprovals table
-- Date: 2025-01-XX
-- Description: Adds Area (Post Office), Police Station, Railway Station, Business Registration Number, and Occupier Designation fields

-- Add new columns to FactoryMapApprovals table
ALTER TABLE FactoryMapApprovals
ADD COLUMN Area NVARCHAR(100) NULL,
    PoliceStation NVARCHAR(100) NULL,
    RailwayStation NVARCHAR(100) NULL,
    BusinessRegistrationNumber NVARCHAR(100) NULL,
    OccupierDesignation NVARCHAR(100) NULL;

-- Add comments/descriptions for the new columns
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Post Office Area Name from Pin Code lookup', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'FactoryMapApprovals',
    @level2type = N'COLUMN', @level2name = N'Area';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Police Station name or ID', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'FactoryMapApprovals',
    @level2type = N'COLUMN', @level2name = N'PoliceStation';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Railway Station name or ID', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'FactoryMapApprovals',
    @level2type = N'COLUMN', @level2name = N'RailwayStation';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Business Registration Number / BRN / SAN Number', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'FactoryMapApprovals',
    @level2type = N'COLUMN', @level2name = N'BusinessRegistrationNumber';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Occupier designation (e.g., Director, Manager, Partner)', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'FactoryMapApprovals',
    @level2type = N'COLUMN', @level2name = N'OccupierDesignation';

-- Create indexes for lookup fields
CREATE INDEX IX_FactoryMapApprovals_PoliceStation ON FactoryMapApprovals(PoliceStation);
CREATE INDEX IX_FactoryMapApprovals_RailwayStation ON FactoryMapApprovals(RailwayStation);
CREATE INDEX IX_FactoryMapApprovals_BusinessRegistrationNumber ON FactoryMapApprovals(BusinessRegistrationNumber);

PRINT 'Migration completed: Added Area, PoliceStation, RailwayStation, BusinessRegistrationNumber, and OccupierDesignation fields to FactoryMapApprovals table';
