using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RaceDay.API.Models;

public class Payment
{
    [Key] public int PaymentId { get; set; }
    [Required] public int EnrolmentId { get; set; }
    [Column(TypeName = "decimal(10,2)")] public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    [Required, MaxLength(50)] public string Method { get; set; } = "Credit Card";
    [MaxLength(20)] public string Status { get; set; } = "Pending";
    [MaxLength(100)] public string? TransactionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [ForeignKey(nameof(EnrolmentId))] public Enrolment Enrolment { get; set; } = null!;
}