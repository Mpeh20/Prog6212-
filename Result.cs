using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RaceDay.API.Models;

public class Result
{
    [Key] public int ResultId { get; set; }
    [Required] public int EnrolmentId { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? OverallPosition { get; set; }
    public int? CategoryPosition { get; set; }
    public TimeSpan? ChipTime { get; set; }
    public TimeSpan? GunTime { get; set; }
    [MaxLength(20)] public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [ForeignKey(nameof(EnrolmentId))] public Enrolment Enrolment { get; set; } = null!;
}