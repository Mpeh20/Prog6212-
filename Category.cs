using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RaceDay.API.Models;

public class Category
{
    [Key] public int CategoryId { get; set; }
    [Required] public int EventId { get; set; }
    [Required, MaxLength(100)] public string Name { get; set; } = "";
    [Required, MaxLength(20)] public string CategoryType { get; set; } = "Distance";
    [MaxLength(50)] public string? AgeGroup { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? DistanceKm { get; set; }
    public int MaxParticipants { get; set; } = 100;
    [Column(TypeName = "decimal(10,2)")] public decimal FeeAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [ForeignKey(nameof(EventId))] public Event Event { get; set; } = null!;
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}