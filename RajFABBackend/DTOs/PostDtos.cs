using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    public class CreatePostDto
    {
        [Required] public string Name { get; set; } = string.Empty;
        public int SeniorityOrder { get; set; } = 0;
    }

    public class UpdatePostDto
    {
        [Required] public string Name { get; set; } = string.Empty;
        public int SeniorityOrder { get; set; } = 0;
    }

    public class PostResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SeniorityOrder { get; set; }
    }
}
