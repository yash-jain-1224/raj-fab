-- Fix IIS App Pool database permissions
-- Run this in SQL Server Management Studio

-- Step 1: Create login for IIS App Pool user
USE [master]
GO
CREATE LOGIN [IIS APPPOOL\rajfabbackend] FROM WINDOWS WITH DEFAULT_DATABASE=[RajFabDB]
GO

-- Step 2: Create user in your database and grant permissions
USE [RajFabDB]
GO
CREATE USER [IIS APPPOOL\rajfabbackend] FOR LOGIN [IIS APPPOOL\rajfabbackend]
GO

-- Step 3: Grant necessary permissions
ALTER ROLE [db_datareader] ADD MEMBER [IIS APPPOOL\rajfabbackend]
GO
ALTER ROLE [db_datawriter] ADD MEMBER [IIS APPPOOL\rajfabbackend]
GO
ALTER ROLE [db_ddladmin] ADD MEMBER [IIS APPPOOL\rajfabbackend]
GO

-- Step 4: Verify the user was created
SELECT 
    dp.name AS UserName,
    dp.type_desc AS UserType,
    r.name AS RoleName
FROM sys.database_principals dp
LEFT JOIN sys.database_role_members drm ON dp.principal_id = drm.member_principal_id
LEFT JOIN sys.database_principals r ON drm.role_principal_id = r.principal_id
WHERE dp.name = 'IIS APPPOOL\rajfabbackend'
ORDER BY dp.name, r.name;

PRINT 'Permissions granted successfully!'
