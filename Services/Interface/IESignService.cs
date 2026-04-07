using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IESignService
    {
        Task<string> GenerateESignHtmlAsync(string applicationId);
        Task<string> ProcessEsignResponseAsync(string esignData);
        Task<string> ManualESignVerifyAsync(string applicationId);
        Task<string> GenerateCertificateESignHtmlAsync(string certificateId);
    }
}