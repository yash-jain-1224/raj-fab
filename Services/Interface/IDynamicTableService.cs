using RajFabAPI.Models;

namespace RajFabAPI.Services.Interface
{
    public interface IDynamicTableService
    {
        Task<bool> CreateTableForModuleAsync(FormModule module, List<FormField> fields);
        Task<bool> UpdateTableForModuleAsync(FormModule module, List<FormField> fields);
        Task<bool> DeleteTableForModuleAsync(FormModule module);
        Task<bool> TableExistsForModuleAsync(string moduleName);
    }
}