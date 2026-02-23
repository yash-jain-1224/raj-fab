using RajFabAPI.DTOs;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace RajFabAPI.Services.Interface
{
    public interface IApplicationHistoryService
    {
        Task<AreaResponseDto?> GetByIdAsync(Guid id);
    }
}
