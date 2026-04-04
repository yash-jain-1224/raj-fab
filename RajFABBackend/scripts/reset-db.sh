#!/usr/bin/env bash
# Reset EF Core database and migrations (DEV ONLY)
# Usage: bash backend/scripts/reset-db.sh
# - Requires .NET SDK and dotnet-ef
# - This deletes the database and the Migrations folder, then recreates a fresh schema

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKEND_DIR="$(dirname "$SCRIPT_DIR")"
cd "$BACKEND_DIR"

echo "Backend directory: $(pwd)"

if [ -d "Migrations" ]; then
  echo "Removing existing Migrations folder..."
  rm -rf Migrations
fi

echo "Ensuring dotnet-ef is installed/updated..."
dotnet tool update -g dotnet-ef || true

echo "Dropping database..."
dotnet ef database drop -f

echo "Creating fresh initial migration..."
dotnet ef migrations add InitialCreate

echo "Applying migration to database..."
dotnet ef database update

echo "Done. Verify column types in SQL Server and test your APIs."
