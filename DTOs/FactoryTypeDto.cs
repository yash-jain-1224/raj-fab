namespace RajFabAPI.DTOs
{
    public class FactoryTypeNewDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateFactoryTypeNewRequest
    {
        public string Name { get; set; } = string.Empty;
    }
}