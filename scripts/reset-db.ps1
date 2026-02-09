# Reset EF Core database and migrations (DEV ONLY)
# Usage: Run in PowerShell
# - Requires .NET SDK and dotnet-ef
# - This deletes the database and the Migrations folder, then recreates a fresh schema

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# Resolve backend directory (this script is in backend/scripts)
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$BackendDir = Split-Path -Parent $ScriptDir
Push-Location $BackendDir

Write-Host "Backend directory:" (Get-Location)

if (Test-Path './Migrations') {
  Write-Host 'Removing existing Migrations folder...'
  Remove-Item -Recurse -Force './Migrations'
}

Write-Host 'Ensuring dotnet-ef is installed/updated...'
dotnet tool update -g dotnet-ef

Write-Host 'Dropping database...'
dotnet ef database drop -f

Write-Host 'Creating fresh initial migration...'
dotnet ef migrations add InitialCreate

Write-Host 'Applying migration to database...'
dotnet ef database update

Write-Host 'Done. Verify column types in SQL Server and test your APIs.'

Pop-Location
