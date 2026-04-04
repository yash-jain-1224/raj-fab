namespace RajFabAPI.Services.Interface
{
    public interface IDynamicPDFGenerationFormService
    {
        void Generate(string jsonData, string filePath);
    }
}
