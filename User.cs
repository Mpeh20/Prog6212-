using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
namespace RaceDay.API.Models;

public class User
{
    [Key] public int UserId { get; set; }
    [Required, MaxLength(255), EmailAddress] public string Email { get; set; } = "";
    [Required] public string PasswordHash { get; set; } = "";
    [Required, MaxLength(100)] public string FullName { get; set; } = "";
    [Required, MaxLength(20)] public string Role { get; set; } = "Participant";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Event> OrganisedEvents { get; set; } = new List<Event>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}