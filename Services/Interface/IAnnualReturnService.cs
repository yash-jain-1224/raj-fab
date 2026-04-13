using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    /// <summary>
    /// Service interface for Annual Return operations
    /// </summary>
    public interface IAnnualReturnService
    {
        /// <summary>
        /// Retrieves all annual returns from the database
        /// </summary>
        /// <returns>List of all annual returns ordered by creation date</returns>
        Task<ApiResponseDto<List<AnnualReturnDto>>> GetAllAnnualReturnsAsync();

        /// <summary>
        /// Retrieves all annual returns for a specific factory
        /// </summary>
        /// <param name="factoryRegistrationNumber">The factory registration number to filter by</param>
        /// <returns>List of annual returns for the specified factory</returns>
        Task<ApiResponseDto<List<AnnualReturnDto>>> GetAnnualReturnsByFactoryRegistrationNumberAsync(string factoryRegistrationNumber);

        /// <summary>
        /// Retrieves a specific annual return by its ID
        /// </summary>
        /// <param name="id">The annual return ID</param>
        /// <returns>The annual return if found</returns>
        Task<ApiResponseDto<AnnualReturnDto>> GetAnnualReturnByIdAsync(string id);

        /// <summary>
        /// Creates a new annual return
        /// </summary>
        /// <param name="request">The creation request containing factory registration number and form data</param>
        /// <returns>The created annual return</returns>
        Task<ApiResponseDto<AnnualReturnDto>> CreateAnnualReturnAsync(CreateAnnualReturnRequest request);

        /// <summary>
        /// Updates an existing annual return
        /// </summary>
        /// <param name="id">The annual return ID to update</param>
        /// <param name="request">The update request with fields to update</param>
        /// <returns>The updated annual return</returns>
        Task<ApiResponseDto<AnnualReturnDto>> UpdateAnnualReturnAsync(string id, UpdateAnnualReturnRequest request);

        /// <summary>
        /// Deletes an annual return
        /// </summary>
        /// <param name="id">The annual return ID to delete</param>
        /// <returns>Success status of the deletion</returns>
        Task<ApiResponseDto<bool>> DeleteAnnualReturnAsync(string id);

        /// <summary>
        /// Generates a Form-25 Annual Return PDF for the given annual return ID
        /// </summary>
        /// <param name="id">The annual return ID</param>
        /// <returns>File path of the generated PDF</returns>
        Task<string> GenerateAnnualReturnPdfAsync(string id);
    }
}
