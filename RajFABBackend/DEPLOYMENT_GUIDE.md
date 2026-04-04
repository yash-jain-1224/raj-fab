# IIS Deployment & Database Configuration Guide

## Issue: Database Connection Errors in IIS

If you see errors like:
```
Cannot open database "RajFabDBNew" requested by the login
Login failed for user 'IIS APPPOOL\rajfabbackend'
```

Follow these steps to resolve:

---

## Step 1: Verify Database Name

1. Open **SQL Server Management Studio (SSMS)**
2. Check if your database is named `RajFabDB` or `RajFabDBNew`
3. If it's `RajFabDBNew`, update `appsettings.json` to match:
   ```json
   "DefaultConnection": "Server=localhost;Database=RajFabDBNew;..."
   ```

---

## Step 2: Grant IIS App Pool Permissions

Run the SQL script in `sql/fix_iis_permissions.sql`:

1. Open **SSMS**
2. Connect to your SQL Server instance
3. Open `backend/sql/fix_iis_permissions.sql`
4. **Important**: If your database is named `RajFabDBNew`, replace all instances of `RajFabDB` with `RajFabDBNew` in the script
5. Execute the script (F5)

This creates a login for `IIS APPPOOL\rajfabbackend` and grants it read/write/DDL permissions.

---

## Step 3: Environment-Specific Configuration

The app uses different `appsettings` based on environment:

- **Development**: `appsettings.Development.json` (when debugging in Visual Studio)
- **Production**: `appsettings.Production.json` (when deployed to IIS)

### For IIS Deployment:

1. Ensure `appsettings.Production.json` exists in your publish folder
2. Verify the connection string matches your database name
3. Set the `ASPNETCORE_ENVIRONMENT` environment variable in IIS:
   - Open **IIS Manager**
   - Select your application
   - Click **Configuration Editor**
   - Navigate to: `system.webServer/aspNetCore`
   - Add environment variable:
     - Name: `ASPNETCORE_ENVIRONMENT`
     - Value: `Production`

---

## Step 4: Create Database (if it doesn't exist)

If the database doesn't exist, run the SQL creation script:

```sql
-- In SQL Server Management Studio
CREATE DATABASE [RajFabDB]
GO

-- Then run the table creation script from sql/create_all_tables.sql
```

Or use EF Core migrations:

```bash
cd backend
dotnet ef database update
```

---

## Step 5: Verify IIS Configuration

### Application Pool Settings:

1. Open **IIS Manager**
2. Go to **Application Pools**
3. Find your app pool (e.g., `rajfabbackend`)
4. Right-click → **Advanced Settings**
5. Verify:
   - **.NET CLR Version**: No Managed Code
   - **Identity**: ApplicationPoolIdentity (default)
   - **Start Mode**: AlwaysRunning (optional for production)

### Application Settings:

1. Select your application in IIS
2. Right-click → **Manage Application** → **Advanced Settings**
3. Verify **Application Pool** matches your pool name

---

## Step 6: Test the Connection

1. Restart the IIS Application Pool:
   ```powershell
   Restart-WebAppPool -Name "rajfabbackend"
   ```

2. Check the application logs in:
   - `F:\My Final Working Code\rajfab-regulator-hub\backend\logs\`

3. Test the API endpoints:
   - Browse to: `http://122.176.134.102:5000/api/health` (if you have a health endpoint)
   - Or any other GET endpoint like `/api/Divisions`

---

## Common Issues

### Issue: "Login failed for user 'IIS APPPOOL\...'"
**Solution**: Run `sql/fix_iis_permissions.sql` in SSMS

### Issue: "Cannot open database"
**Solution**: 
- Check database name matches connection string
- Verify database exists in SSMS
- Run `CREATE DATABASE` if needed

### Issue: Configuration changes not taking effect
**Solution**:
- Delete files in `bin/Release/net8.0/publish/`
- Republish from Visual Studio
- Restart IIS App Pool

### Issue: Still getting old database name "RajFabDBNew"
**Solution**:
- There might be an IIS configuration override
- Check IIS Manager → Your App → Configuration Editor
- Look for connection string overrides under:
  - `connectionStrings`
  - `system.webServer/aspNetCore` → `environmentVariables`

---

## Quick Fix Commands

```sql
-- Check if database exists
SELECT name FROM sys.databases WHERE name IN ('RajFabDB', 'RajFabDBNew');

-- Check current user permissions
USE [RajFabDB]
SELECT 
    dp.name, 
    dp.type_desc,
    r.name AS role_name
FROM sys.database_principals dp
LEFT JOIN sys.database_role_members drm ON dp.principal_id = drm.member_principal_id
LEFT JOIN sys.database_principals r ON drm.role_principal_id = r.principal_id
WHERE dp.name LIKE '%rajfabbackend%' OR dp.name LIKE '%IIS APPPOOL%';
```

```powershell
# PowerShell: Restart IIS App Pool
Restart-WebAppPool -Name "rajfabbackend"

# PowerShell: Check IIS App Pool status
Get-WebAppPoolState -Name "rajfabbackend"
```

---

## Database Naming Convention

**Recommendation**: Use `RajFabDB` consistently across:
- SQL Server database name
- All `appsettings*.json` files
- IIS configuration
- Documentation

If you need to rename the database:

```sql
-- Rename database in SQL Server
ALTER DATABASE [RajFabDBNew] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
ALTER DATABASE [RajFabDBNew] MODIFY NAME = [RajFabDB];
ALTER DATABASE [RajFabDB] SET MULTI_USER;
```
