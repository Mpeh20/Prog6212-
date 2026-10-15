using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RaceDay.API.Models;

public class Enrolment
{
    [Key] public int EnrolmentId { get; set; }
    [Required] public int ParticipantId { get; set; }
    [Required] public int CategoryId { get; set; }
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
    [MaxLength(20)] public string Status { get; set; } = "Registered";
    public int? BibNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [ForeignKey(nameof(ParticipantId))] public User Participant { get; set; } = null!;
    [ForeignKey(nameof(CategoryId))] public Category Category { get; set; } = null!;
    public Result? Result { get; set; }
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}