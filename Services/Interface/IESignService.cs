using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IESignService
    {
        Task<string> StartEsignAsync(IFormFile pdfFile);
        Task<ESignResult> CompleteEsignAsync(string esignResponseBase64);
        Task<string> GenerateEsignToken();
        Task<SignedXmlData> GenerateESignedXmlAsync(IFormFile pdfFile, string token);
        string GenerateEspRedirectHtml(string xml);
    }
}