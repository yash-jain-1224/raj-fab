-- DEV ONLY: Drop and recreate the database. USE WITH CAUTION.
-- 1) Replace the database name below, then run in SQL Server Management Studio
-- 2) After recreation, run `dotnet ef database update` from the backend folder

DECLARE @DBName sysname = N'YourDatabaseNameHere';

IF DB_ID(@DBName) IS NOT NULL
BEGIN
    DECLARE @sql nvarchar(max) = N'ALTER DATABASE [' + @DBName + N'] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [' + @DBName + N']';
    EXEC sp_executesql @sql;
END

DECLARE @create nvarchar(max) = N'CREATE DATABASE [' + @DBName + N']';
EXEC sp_executesql @create;

PRINT 'Database dropped and recreated. Now run migrations: dotnet ef database update';
