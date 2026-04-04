
namespace RajFabAPI.Models.FactoryModels
{
    public class EstablishmentEntityMapping
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string EstablishmentRegistrationId { get; internal set; }
        public DateTime CreatedAt { get; internal set; }
        public DateTime UpdatedAt { get; internal set; }
        public Guid EntityId { get; internal set; }
        public string EntityType { get; internal set; }
    }
}
