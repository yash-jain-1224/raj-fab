# Database Reset Guide (Dev Only)

If you hit GUID⇄string cast errors, you likely have a DB schema with `uniqueidentifier` columns while the code expects string IDs. To rebuild the schema from the current EF model:

Warning: This will WIPE data.

## One-liner scripts
- PowerShell (Windows):
  - `powershell -ExecutionPolicy Bypass -File backend/scripts/reset-db.ps1`
- Bash (macOS/Linux):
  - `bash backend/scripts/reset-db.sh`

These will:
1. Delete `backend/Migrations`
2. Drop the database
3. Create a fresh `InitialCreate` migration from current models
4. Apply it to rebuild the schema

## Manual steps
```bash
cd backend
# Ensure EF CLI is available
 dotnet tool update -g dotnet-ef
# Drop database
 dotnet ef database drop -f
# Remove old migrations
 rm -rf Migrations
# Create and apply fresh migration
 dotnet ef migrations add InitialCreate
 dotnet ef database update
```

## SQL alternative (SSMS)
- Edit `backend/sql/reset_database.sql` and set `@DBName`
- Run it to drop & recreate the DB, then run `dotnet ef database update`

## Verify column types
Run in your DB:
```sql
SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN (
  'DocumentTypes','BoilerDocumentTypes','FactoryTypeDocuments','ManufacturingProcessTypes','ProcessDocuments'
)
ORDER BY TABLE_NAME, COLUMN_NAME;
```
Expect `varchar(36)` for Id and DocumentTypeId in the above tables.

## Next
Smoke test:
- GET /api/documenttypes
- GET /api/boilerdocumenttypes/boiler-registration
