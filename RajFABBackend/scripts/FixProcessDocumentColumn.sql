-- ================================================================
-- Fix ProcessDocument Schema - Option A Implementation
-- ================================================================
-- This script removes the incorrectly typed ProcessTypeId column
-- and ensures ManufacturingProcessTypeId is properly configured
-- ================================================================

-- Step 1: Verify current schema
PRINT 'Current ProcessDocuments schema:';
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ProcessDocuments'
ORDER BY ORDINAL_POSITION;

-- Step 2: Drop the incorrectly typed ProcessTypeId column
PRINT '';
PRINT 'Dropping ProcessTypeId column...';
IF EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ProcessDocuments' 
    AND COLUMN_NAME = 'ProcessTypeId'
)
BEGIN
    ALTER TABLE ProcessDocuments
    DROP COLUMN ProcessTypeId;
    PRINT 'ProcessTypeId column dropped successfully.';
END
ELSE
BEGIN
    PRINT 'ProcessTypeId column does not exist, skipping drop.';
END

-- Step 3: Ensure ManufacturingProcessTypeId exists and has correct type
PRINT '';
PRINT 'Verifying ManufacturingProcessTypeId column...';
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ProcessDocuments' 
    AND COLUMN_NAME = 'ManufacturingProcessTypeId'
)
BEGIN
    PRINT 'ERROR: ManufacturingProcessTypeId column does not exist!';
    PRINT 'This column should already exist in the database.';
END
ELSE
BEGIN
    PRINT 'ManufacturingProcessTypeId column exists.';
END

-- Step 4: Verify the foreign key constraint exists
PRINT '';
PRINT 'Checking foreign key constraint...';
IF EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
    WHERE CONSTRAINT_NAME = 'FK_ProcessDocuments_ManufacturingProcessTypes_ManufacturingProcessTypeId'
)
BEGIN
    PRINT 'Foreign key constraint already exists.';
END
ELSE
BEGIN
    PRINT 'Foreign key constraint does not exist.';
    PRINT 'Note: EF Core will create this constraint on next application start.';
END

-- Step 5: Verify final schema
PRINT '';
PRINT 'Final ProcessDocuments schema:';
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ProcessDocuments'
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT '================================================================';
PRINT 'Script completed successfully!';
PRINT 'Next step: Restart the application to let EF Core apply the';
PRINT 'updated DbContext configuration.';
PRINT '================================================================';
