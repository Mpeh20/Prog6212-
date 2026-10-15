using System.ComponentModel.DataAnnotations;
namespace RaceDay.API.DTOs.Payment;
public class CreatePaymentDto { [Required] public decimal Amount { get; set; } [Required] public string Method { get; set; } = "Credit Card"; }
public class PaymentResponseDto
{
    public int PaymentId { get; set; }
    public int EnrolmentId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Method { get; set; } = "";
    public string Status { get; set; } = "";
    public string? TransactionId { get; set; }
}