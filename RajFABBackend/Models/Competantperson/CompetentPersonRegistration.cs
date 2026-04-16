using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace RajFabAPI.Models.CompetentPerson
{
    

   public class CompetentPersonRegistration
{
    public Guid Id { get; set; }

    public string ApplicationId { get; set; } = null!;
    public string? CompetentRegistrationNo { get; set; }

    public string RegistrationType { get; set; } = null!; // Individual / Institution
    public string Type { get; set; } = "new";              // New / Amend / Renewal

    public string Status { get; set; } = "Pending";

    public decimal Version { get; set; } = 1.0m;

    public bool IsActive { get; set; } = true;

    public int RenewalYears { get; set; } = 1;

    public DateTime? ValidUpto { get; set; }

    public decimal Amount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navigation

    public CompetantEstablishmentDetail? Establishment { get; set; }

    public CompetantOccupierDetail? Occupier { get; set; }

    public ICollection<CompetantPersonDetail>? Persons { get; set; }
}

}
