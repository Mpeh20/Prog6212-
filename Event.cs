using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RaceDay.API.Models;

public class Event
{
    [Key] public int EventId { get; set; }
    [Required] public int OrganiserId { get; set; }
    [Required, MaxLength(200)] public string Name { get; set; } = "";
    public string? Description { get; set; }
    [Required] public DateTime StartDate { get; set; }
    [Required] public DateTime EndDate { get; set; }
    [Required, MaxLength(200)] public string Location { get; set; } = "";
    [Required, Column(TypeName = "decimal(5,2)")] public decimal DistanceKm { get; set; }
    [Required, MaxLength(20)] public string EventType { get; set; } = "Run";
    [MaxLength(20)] public string Status { get; set; } = "Draft";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [ForeignKey(nameof(OrganiserId))] public User Organiser { get; set; } = null!;
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}