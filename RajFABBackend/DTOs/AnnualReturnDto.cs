using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace RajFabAPI.DTOs
{
    /// <summary>
    /// Annual Return Data Transfer Object for API responses
    /// </summary>
    public class AnnualReturnDto
    {
        /// <summary>
        /// Unique identifier for the annual return
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Factory Registration Number
        /// </summary>
        [Required]
        [StringLength(255)]
        public string FactoryRegistrationNumber { get; set; }

        /// <summary>
        /// Active status of the annual return
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// JSON form data containing annual return details
        /// </summary>
        public JsonElement FormData { get; set; }

        /// <summary>
        /// Version of the annual return (decimal 3,1 format)
        /// </summary>
        public decimal Version { get; set; }

        /// <summary>
        /// Timestamp when the record was created (UTC)
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when the record was last updated (UTC)
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Request model for creating a new annual return
    /// </summary>
    public class CreateAnnualReturnRequest
    {
        /// <summary>
        /// Factory Registration Number (required)
        /// </summary>
        [Required]
        [StringLength(255)]
        public string FactoryRegistrationNumber { get; set; }

        /// <summary>
        /// Active status of the annual return
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// JSON form data containing annual return details (required)
        /// </summary>
        [Required]
        public JsonElement FormData { get; set; }
    }

    /// <summary>
    /// Request model for updating an existing annual return
    /// </summary>
    public class UpdateAnnualReturnRequest
    {
        /// <summary>
        /// Active status of the annual return (optional)
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// JSON form data containing annual return details (optional)
        /// </summary>
        public JsonElement? FormData { get; set; }

        /// <summary>
        /// Version of the annual return (optional)
        /// </summary>
        public decimal? Version { get; set; }
    }
}
