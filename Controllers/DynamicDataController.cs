using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DynamicDataController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DynamicDataController> _logger;
        
        // Security constants
        private const int MAX_TABLE_NAME_LENGTH = 128;
        private const int MAX_COLUMN_NAME_LENGTH = 128;
        private const int MAX_STRING_VALUE_LENGTH = 4000;
        private const int MAX_COLUMNS_PER_INSERT = 100;
        private static readonly Regex SafeIdentifierRegex = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$", RegexOptions.Compiled);

        public DynamicDataController(ApplicationDbContext context, ILogger<DynamicDataController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get data from dynamic table for a specific module
        /// </summary>
        [HttpGet("module/{moduleId}")]
        public async Task<ActionResult<IEnumerable<Dictionary<string, object>>>> GetModuleData(Guid moduleId)
        {
            try
            {
                var module = await _context.Modules.FindAsync(moduleId);
                if (module == null)
                    return NotFound($"Module with ID {moduleId} not found.");

                var tableName = GetTableName(module.Name);
                
                if (!await TableExistsAsync(tableName))
                    return Ok(new List<Dictionary<string, object>>());

                var sql = $"SELECT * FROM [{tableName}] ORDER BY [CreatedAt] DESC";
                var data = await ExecuteQueryAsync(sql);
                
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving data: {ex.Message}");
            }
        }

        /// <summary>
        /// Get single record from dynamic table
        /// </summary>
        [HttpGet("module/{moduleId}/record/{recordId}")]
        public async Task<ActionResult<Dictionary<string, object>>> GetModuleRecord(Guid moduleId, Guid recordId)
        {
            try
            {
                var module = await _context.Modules.FindAsync(moduleId);
                if (module == null)
                    return NotFound($"Module with ID {moduleId} not found.");

                var tableName = GetTableName(module.Name);
                
                if (!await TableExistsAsync(tableName))
                    return NotFound("Table not found.");

                var sql = $"SELECT * FROM [{tableName}] WHERE [Id] = {{0}}";
                var data = await ExecuteQueryAsync(sql, recordId);
                
                if (!data.Any())
                    return NotFound("Record not found.");
                
                return Ok(data.First());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving record: {ex.Message}");
            }
        }

        /// <summary>
        /// Insert data into dynamic table
        /// </summary>
        [HttpPost("module/{moduleId}")]
        public async Task<ActionResult<Dictionary<string, object>>> InsertModuleData(Guid moduleId, [FromBody] Dictionary<string, object> data)
        {
            try
            {
                // Validate input data
                var validationResult = ValidateInputData(data);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Invalid input data for module {ModuleId}: {Errors}", moduleId, string.Join(", ", validationResult.Errors));
                    return BadRequest(new { errors = validationResult.Errors });
                }

                var module = await _context.Modules.FindAsync(moduleId);
                if (module == null)
                {
                    _logger.LogWarning("Module not found: {ModuleId}", moduleId);
                    return NotFound($"Module with ID {moduleId} not found.");
                }

                var tableName = GetTableName(module.Name);
                
                if (!ValidateTableName(tableName))
                {
                    _logger.LogError("Invalid table name generated from module: {ModuleName}", module.Name);
                    return BadRequest("Invalid module configuration.");
                }
                
                if (!await TableExistsAsync(tableName))
                    return BadRequest("Dynamic table not found. Please create a form for this module first.");

                var recordId = Guid.NewGuid();
                var columns = new List<string> { "Id", "CreatedAt", "UpdatedAt" };
                var values = new List<object> { recordId, DateTime.Now, DateTime.Now };

                // Add user-provided data with validation
                foreach (var kvp in data)
                {
                    var columnName = GetColumnName(kvp.Key);
                    
                    if (!ValidateColumnName(columnName))
                    {
                        _logger.LogWarning("Invalid column name: {ColumnName}", kvp.Key);
                        continue;
                    }
                    
                    if (await ColumnExistsAsync(tableName, columnName))
                    {
                        var sanitizedValue = SanitizeValue(kvp.Value);
                        columns.Add($"[{columnName}]");
                        values.Add(sanitizedValue ?? DBNull.Value);
                    }
                }

                var columnList = string.Join(", ", columns.Select(c => c.StartsWith("[") ? c : $"[{c}]"));
                var parameterList = string.Join(", ", Enumerable.Range(0, values.Count).Select(i => $"{{{i}}}"));
                
                var sql = $"INSERT INTO [{tableName}] ({columnList}) VALUES ({parameterList})";
                await _context.Database.ExecuteSqlRawAsync(sql, values.ToArray());

                // Return the created record
                var selectSql = $"SELECT * FROM [{tableName}] WHERE [Id] = {{0}}";
                var result = await ExecuteQueryAsync(selectSql, recordId);
                
                return CreatedAtAction(nameof(GetModuleRecord), 
                    new { moduleId, recordId }, 
                    result.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inserting data: {ex.Message}");
            }
        }

        /// <summary>
        /// Update data in dynamic table
        /// </summary>
        [HttpPost("module/{moduleId}/record/{recordId}/update")]
        public async Task<ActionResult<Dictionary<string, object>>> UpdateModuleData(
            Guid moduleId, 
            Guid recordId, 
            [FromBody] Dictionary<string, object> data)
        {
            try
            {
                // Validate input data
                var validationResult = ValidateInputData(data);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Invalid input data for update on module {ModuleId}, record {RecordId}: {Errors}", 
                        moduleId, recordId, string.Join(", ", validationResult.Errors));
                    return BadRequest(new { errors = validationResult.Errors });
                }

                var module = await _context.Modules.FindAsync(moduleId);
                if (module == null)
                {
                    _logger.LogWarning("Module not found for update: {ModuleId}", moduleId);
                    return NotFound($"Module with ID {moduleId} not found.");
                }

                var tableName = GetTableName(module.Name);
                
                if (!ValidateTableName(tableName))
                {
                    _logger.LogError("Invalid table name generated from module: {ModuleName}", module.Name);
                    return BadRequest("Invalid module configuration.");
                }
                
                if (!await TableExistsAsync(tableName))
                    return BadRequest("Dynamic table not found.");

                var updatePairs = new List<string> { "[UpdatedAt] = {0}" };
                var values = new List<object> { DateTime.Now };

                // Add user-provided data with validation
                foreach (var kvp in data)
                {
                    var columnName = GetColumnName(kvp.Key);
                    
                    if (!ValidateColumnName(columnName))
                    {
                        _logger.LogWarning("Invalid column name in update: {ColumnName}", kvp.Key);
                        continue;
                    }
                    
                    if (await ColumnExistsAsync(tableName, columnName))
                    {
                        var sanitizedValue = SanitizeValue(kvp.Value);
                        updatePairs.Add($"[{columnName}] = {{{values.Count}}}");
                        values.Add(sanitizedValue ?? DBNull.Value);
                    }
                }

                values.Add(recordId); // For WHERE clause
                var updateClause = string.Join(", ", updatePairs);
                
                var sql = $"UPDATE [{tableName}] SET {updateClause} WHERE [Id] = {{{values.Count - 1}}}";
                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(sql, values.ToArray());

                if (rowsAffected == 0)
                    return NotFound("Record not found.");

                // Return the updated record
                var selectSql = $"SELECT * FROM [{tableName}] WHERE [Id] = {{0}}";
                var result = await ExecuteQueryAsync(selectSql, recordId);
                
                return Ok(result.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating data: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete record from dynamic table
        /// </summary>
        [HttpPost("module/{moduleId}/record/{recordId}/delete")]
        public async Task<IActionResult> DeleteModuleData(Guid moduleId, Guid recordId)
        {
            try
            {
                var module = await _context.Modules.FindAsync(moduleId);
                if (module == null)
                    return NotFound($"Module with ID {moduleId} not found.");

                var tableName = GetTableName(module.Name);
                
                if (!await TableExistsAsync(tableName))
                    return BadRequest("Dynamic table not found.");

                var sql = $"DELETE FROM [{tableName}] WHERE [Id] = {{0}}";
                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(sql, recordId);

                if (rowsAffected == 0)
                    return NotFound("Record not found.");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting record: {ex.Message}");
            }
        }

        private async Task<List<Dictionary<string, object?>>> ExecuteQueryAsync(string sql, params object[] parameters)
        {
            var result = new List<Dictionary<string, object?>>();
            
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = $"@p{i}";
                parameter.Value = parameters[i] ?? DBNull.Value;
                command.Parameters.Add(parameter);
                command.CommandText = command.CommandText.Replace($"{{{i}}}", $"@p{i}");
            }

            await _context.Database.OpenConnectionAsync();
            
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                result.Add(row);
            }
            
            return result;
        }

        private async Task<bool> TableExistsAsync(string tableName)
        {
            var sql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = {0}";
            var result = await _context.Database.SqlQueryRaw<int>(sql, tableName).FirstOrDefaultAsync();
            return result > 0;
        }

        private async Task<bool> ColumnExistsAsync(string tableName, string columnName)
        {
            var sql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = {0} AND COLUMN_NAME = {1}";
            var result = await _context.Database.SqlQueryRaw<int>(sql, tableName, columnName).FirstOrDefaultAsync();
            return result > 0;
        }

        private string GetTableName(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
                throw new ArgumentException("Module name cannot be empty", nameof(moduleName));
            
            // Sanitize by allowing only alphanumeric and underscores
            var tableName = moduleName.Replace(" ", "_")
                                   .Replace("-", "_")
                                   .Replace(".", "_");
            
            tableName = new string(tableName.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
            
            if (string.IsNullOrEmpty(tableName) || !char.IsLetter(tableName[0]))
            {
                tableName = "Module_" + tableName;
            }

            tableName = $"Dynamic_{tableName}";
            
            // Enforce length limit
            if (tableName.Length > MAX_TABLE_NAME_LENGTH)
            {
                tableName = tableName.Substring(0, MAX_TABLE_NAME_LENGTH);
            }

            return tableName;
        }

        private string GetColumnName(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentException("Field name cannot be empty", nameof(fieldName));
            
            // Sanitize by allowing only alphanumeric and underscores
            var columnName = fieldName.Replace(" ", "_")
                                    .Replace("-", "_")
                                    .Replace(".", "_");
            
            columnName = new string(columnName.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
            
            if (string.IsNullOrEmpty(columnName) || !char.IsLetter(columnName[0]))
            {
                columnName = "Field_" + columnName;
            }

            // Enforce length limit
            if (columnName.Length > MAX_COLUMN_NAME_LENGTH)
            {
                columnName = columnName.Substring(0, MAX_COLUMN_NAME_LENGTH);
            }

            return columnName;
        }

        /// <summary>
        /// Validates table name against strict security rules
        /// </summary>
        private bool ValidateTableName(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return false;
            
            if (tableName.Length > MAX_TABLE_NAME_LENGTH)
                return false;
            
            // Must start with Dynamic_ prefix for safety
            if (!tableName.StartsWith("Dynamic_"))
                return false;
            
            // Only allow alphanumeric and underscores
            return SafeIdentifierRegex.IsMatch(tableName);
        }

        /// <summary>
        /// Validates column name against strict security rules
        /// </summary>
        private bool ValidateColumnName(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                return false;
            
            if (columnName.Length > MAX_COLUMN_NAME_LENGTH)
                return false;
            
            // Only allow alphanumeric and underscores, must start with letter or underscore
            return SafeIdentifierRegex.IsMatch(columnName);
        }

        /// <summary>
        /// Validates input data dictionary
        /// </summary>
        private ValidationResult ValidateInputData(Dictionary<string, object> data)
        {
            var errors = new List<string>();

            if (data == null)
            {
                errors.Add("Data cannot be null");
                return new ValidationResult { IsValid = false, Errors = errors };
            }

            if (data.Count > MAX_COLUMNS_PER_INSERT)
            {
                errors.Add($"Too many fields. Maximum allowed is {MAX_COLUMNS_PER_INSERT}");
            }

            foreach (var kvp in data)
            {
                // Validate key
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    errors.Add("Field name cannot be empty");
                    continue;
                }

                if (kvp.Key.Length > MAX_COLUMN_NAME_LENGTH)
                {
                    errors.Add($"Field name '{kvp.Key}' exceeds maximum length of {MAX_COLUMN_NAME_LENGTH}");
                }

                // Validate value
                if (kvp.Value != null)
                {
                    var valueType = kvp.Value.GetType();
                    
                    if (kvp.Value is string strValue)
                    {
                        if (strValue.Length > MAX_STRING_VALUE_LENGTH)
                        {
                            errors.Add($"Value for field '{kvp.Key}' exceeds maximum length of {MAX_STRING_VALUE_LENGTH}");
                        }
                    }
                    else if (kvp.Value is JsonElement jsonElement)
                    {
                        var jsonString = jsonElement.ToString();
                        if (jsonString.Length > MAX_STRING_VALUE_LENGTH)
                        {
                            errors.Add($"JSON value for field '{kvp.Key}' exceeds maximum length");
                        }
                    }
                }
            }

            return new ValidationResult 
            { 
                IsValid = errors.Count == 0, 
                Errors = errors 
            };
        }

        /// <summary>
        /// Sanitizes a value before inserting into database
        /// </summary>
        private object? SanitizeValue(object? value)
        {
            if (value == null)
                return null;

            // Handle string values
            if (value is string strValue)
            {
                // Trim whitespace
                strValue = strValue.Trim();
                
                // Enforce length limit
                if (strValue.Length > MAX_STRING_VALUE_LENGTH)
                {
                    strValue = strValue.Substring(0, MAX_STRING_VALUE_LENGTH);
                }
                
                return strValue;
            }

            // Handle JsonElement
            if (value is JsonElement jsonElement)
            {
                return jsonElement.ValueKind switch
                {
                    JsonValueKind.String => SanitizeValue(jsonElement.GetString()),
                    JsonValueKind.Number => jsonElement.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    _ => jsonElement.ToString()
                };
            }

            // Return other types as-is (numbers, booleans, dates, etc.)
            return value;
        }

        private class ValidationResult
        {
            public bool IsValid { get; set; }
            public List<string> Errors { get; set; } = new();
        }
    }
}