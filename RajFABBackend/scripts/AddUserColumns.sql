-- Add missing columns to Users table
-- Run this script on your database to fix the schema mismatch

-- Check if FullName column exists, if not add it
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'FullName')
BEGIN
    ALTER TABLE [Users]
    ADD [FullName] NVARCHAR(200) NOT NULL DEFAULT '';
    PRINT 'FullName column added successfully';
END
ELSE
BEGIN
    PRINT 'FullName column already exists';
END
GO

-- Check if Mobile column exists, if not add it
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'Mobile')
BEGIN
    ALTER TABLE [Users]
    ADD [Mobile] NVARCHAR(15) NOT NULL DEFAULT '';
    PRINT 'Mobile column added successfully';
END
ELSE
BEGIN
    PRINT 'Mobile column already exists';
END
GO

-- Verify the columns were added
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Users'
ORDER BY ORDINAL_POSITION;
GO
