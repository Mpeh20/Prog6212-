using System.ComponentModel.DataAnnotations;
using RaceDay.API.DTOs.Result;
namespace RaceDay.API.DTOs.Enrolment;
public class CreateEnrolmentDto { [Required] public int CategoryId { get; set; } }
public class EnrolmentResponseDto
{
    public int EnrolmentId { get; set; }
    public int ParticipantId { get; set; }
    public string ParticipantName { get; set; } = "";
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public int EventId { get; set; }
    public string EventName { get; set; } = "";
    public DateTime RegistrationDate { get; set; }
    public string Status { get; set; } = "";
    public int? BibNumber { get; set; }
    public ResultResponseDto? Result { get; set; }
}