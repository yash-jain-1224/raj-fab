namespace RajFabAPI.Models
{
    public class Post
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public int SeniorityOrder { get; set; } = 0;
    }
}
