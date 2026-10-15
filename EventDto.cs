using System.ComponentModel.DataAnnotations;
namespace RaceDay.API.DTOs.Event;

public class CreateEventDto
{
    [Required, MaxLength(200)] public string Name { get; set; } = "";
    public string? Description { get; set; }
    [Required] public DateTime StartDate { get; set; }
    [Required] public DateTime EndDate { get; set; }
    [Required, MaxLength(200)] public string Location { get; set; } = "";
    [Required] public decimal DistanceKm { get; set; }
    [Required] public string EventType { get; set; } = "Run";
    public string Status { get; set; } = "Draft";
}

public class EventResponseDto
{
    public int EventId { get; set; }
    public int OrganiserId { get; set; }
    public string OrganiserName { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Location { get; set; } = "";
    public decimal DistanceKm { get; set; }
    public string EventType { get; set; } = "";
    public string Status { get; set; } = "";
    public List<CategoryResponseDto> Categories { get; set; } = new();
}

public class CategoryResponseDto
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = "";
    public string CategoryType { get; set; } = "";
    public string? AgeGroup { get; set; }
    public decimal? DistanceKm { get; set; }
    public int MaxParticipants { get; set; }
    public decimal FeeAmount { get; set; }
}