using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;
using System.Text;

namespace RajFabAPI.Services
{
    public class DynamicTableService : IDynamicTableService
    {
        private readonly ApplicationDbContext _context;

        public DynamicTableService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateTableForModuleAsync(FormModule module, List<FormField> fields)
        {
            try
            {
                var tableName = GetTableName(module.Name);
                
                if (await TableExistsForModuleAsync(module.Name))
                {
                    return await UpdateTableForModuleAsync(module, fields);
                }

                var sql = GenerateCreateTableSql(tableName, fields);
                await _context.Database.ExecuteSqlRawAsync(sql);
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateTableForModuleAsync(FormModule module, List<FormField> fields)
        {
            try
            {
                var tableName = GetTableName(module.Name);
                
                // Get existing columns
                var existingColumns = await GetExistingColumnsAsync(tableName);
                
                // Add new columns
                foreach (var field in fields)
                {
                    var columnName = GetColumnName(field.Name);
                    if (!existingColumns.Contains(columnName.ToLower()))
                    {
                        var sql = $"ALTER TABLE [{tableName}] ADD [{columnName}] {GetSqlDataType(field.Type)} NULL";
                        await _context.Database.ExecuteSqlRawAsync(sql);
                    }
                }
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteTableForModuleAsync(FormModule module)
        {
            try
            {
                var tableName = GetTableName(module.Name);
                var sql = $"DROP TABLE IF EXISTS [{tableName}]";
                await _context.Database.ExecuteSqlRawAsync(sql);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> TableExistsForModuleAsync(string moduleName)
        {
            try
            {
                var tableName = GetTableName(moduleName);
                var sql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = {0}";
                var result = await _context.Database.SqlQueryRaw<int>(sql, tableName).FirstOrDefaultAsync();
                return result > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<List<string>> GetExistingColumnsAsync(string tableName)
        {
            try
            {
                var sql = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = {0}";
                var columns = await _context.Database.SqlQueryRaw<string>(sql, tableName).ToListAsync();
                return columns.Select(c => c.ToLower()).ToList();
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        private string GenerateCreateTableSql(string tableName, List<FormField> fields)
        {
            var sql = new StringBuilder();
            sql.AppendLine($"CREATE TABLE [{tableName}] (");
            sql.AppendLine("    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),");
            sql.AppendLine("    [SubmissionId] UNIQUEIDENTIFIER NULL,");
            sql.AppendLine("    [UserId] NVARCHAR(450) NULL,");
            sql.AppendLine("    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),");
            sql.AppendLine("    [UpdatedAt] DATETIME2 DEFAULT GETUTCDATE(),");

            foreach (var field in fields.OrderBy(f => f.Order))
            {
                var columnName = GetColumnName(field.Name);
                var dataType = GetSqlDataType(field.Type);
                var nullable = field.Required ? "NOT NULL" : "NULL";
                
                sql.AppendLine($"    [{columnName}] {dataType} {nullable},");
            }

            // Remove the last comma and add closing parenthesis
            sql.Length -= 3; // Remove ",\r\n"
            sql.AppendLine();
            sql.AppendLine(");");

            // Add indexes
            sql.AppendLine($"CREATE INDEX IX_{tableName}_SubmissionId ON [{tableName}] ([SubmissionId]);");
            sql.AppendLine($"CREATE INDEX IX_{tableName}_UserId ON [{tableName}] ([UserId]);");
            sql.AppendLine($"CREATE INDEX IX_{tableName}_CreatedAt ON [{tableName}] ([CreatedAt]);");

            return sql.ToString();
        }

        private string GetTableName(string moduleName)
        {
            // Convert module name to a valid table name
            var tableName = moduleName.Replace(" ", "_")
                                   .Replace("-", "_")
                                   .Replace(".", "_");
            
            // Remove invalid characters and ensure it starts with a letter
            tableName = new string(tableName.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
            
            if (!char.IsLetter(tableName[0]))
            {
                tableName = "Module_" + tableName;
            }

            return $"Dynamic_{tableName}";
        }

        private string GetColumnName(string fieldName)
        {
            // Convert field name to a valid column name
            var columnName = fieldName.Replace(" ", "_")
                                    .Replace("-", "_")
                                    .Replace(".", "_");
            
            // Remove invalid characters
            columnName = new string(columnName.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
            
            if (string.IsNullOrEmpty(columnName) || !char.IsLetter(columnName[0]))
            {
                columnName = "Field_" + columnName;
            }

            return columnName;
        }

        private string GetSqlDataType(string fieldType)
        {
            return fieldType.ToLower() switch
            {
                "text" => "NVARCHAR(500)",
                "email" => "NVARCHAR(320)",
                "tel" => "NVARCHAR(20)",
                "number" => "DECIMAL(18,2)",
                "date" => "DATE",
                "select" => "NVARCHAR(200)",
                "radio" => "NVARCHAR(200)",
                "checkbox" => "BIT",
                "textarea" => "NVARCHAR(MAX)",
                "file" => "NVARCHAR(500)", // Store file path/URL
                _ => "NVARCHAR(500)"
            };
        }
    }
}