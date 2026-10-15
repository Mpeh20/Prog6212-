using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Event;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public EventsController(RaceDayDbContext db) { _db = db; }
    private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    /// <summary>List all events</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        var q = _db.Events.Include(e => e.Organiser).Include(e => e.Categories).AsQueryable();
        if (!string.IsNullOrEmpty(status)) q = q.Where(e => e.Status == status);
        var events = await q.Select(e => new EventResponseDto
        {
            EventId = e.EventId,
            OrganiserId = e.OrganiserId,
            OrganiserName = e.Organiser.FullName,
            Name = e.Name,
            Description = e.Description,
            StartDate = e.StartDate,
            EndDate = e.EndDate,
            Location = e.Location,
            DistanceKm = e.DistanceKm,
            EventType = e.EventType,
            Status = e.Status,
            Categories = e.Categories.Select(c => new CategoryResponseDto
            {
                CategoryId = c.CategoryId,
                EventId = c.EventId,
                Name = c.Name,
                CategoryType = c.CategoryType,
                AgeGroup = c.AgeGroup,
                DistanceKm = c.DistanceKm,
                MaxParticipants = c.MaxParticipants,
                FeeAmount = c.FeeAmount
            }).ToList()
        }).ToListAsync();
        return Ok(events);
    }

    /// <summary>Get event by ID</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var e = await _db.Events.Include(x => x.Organiser).Include(x => x.Categories)
            .FirstOrDefaultAsync(x => x.EventId == id);
        if (e == null) return NotFound();
        return Ok(new EventResponseDto
        {
            EventId = e.EventId,
            OrganiserId = e.OrganiserId,
            OrganiserName = e.Organiser.FullName,
            Name = e.Name,
            Description = e.Description,
            StartDate = e.StartDate,
            EndDate = e.EndDate,
            Location = e.Location,
            DistanceKm = e.DistanceKm,
            EventType = e.EventType,
            Status = e.Status,
            Categories = e.Categories.Select(c => new CategoryResponseDto
            {
                CategoryId = c.CategoryId,
                EventId = c.EventId,
                Name = c.Name,
                CategoryType = c.CategoryType,
                AgeGroup = c.AgeGroup,
                DistanceKm = c.DistanceKm,
                MaxParticipants = c.MaxParticipants,
                FeeAmount = c.FeeAmount
            }).ToList()
        });
    }

    /// <summary>Create event (Organiser only)</summary>
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
    {
        var ev = new Event
        {
            OrganiserId = CurrentUserId,
            Name = dto.Name,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Location = dto.Location,
            DistanceKm = dto.DistanceKm,
            EventType = dto.EventType,
            Status = dto.Status
        };
        _db.Events.Add(ev);
        await _db.SaveChangesAsync();
        return StatusCode(201, new { ev.EventId, ev.Name, ev.Status });
    }

    /// <summary>Update event (Organiser owner only)</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateEventDto dto)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound();
        if (ev.OrganiserId != CurrentUserId) return Forbid();
        ev.Name = dto.Name; ev.Description = dto.Description;
        ev.StartDate = dto.StartDate; ev.EndDate = dto.EndDate;
        ev.Location = dto.Location; ev.DistanceKm = dto.DistanceKm;
        ev.EventType = dto.EventType; ev.Status = dto.Status;
        ev.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { ev.EventId, ev.Name, ev.Status });
    }

    /// <summary>Delete event (Organiser owner only)</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound();
        if (ev.OrganiserId != CurrentUserId) return Forbid();
        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}