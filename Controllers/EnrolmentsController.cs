using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Enrolment;
using RaceDay.API.DTOs.Result;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public EnrolmentsController(RaceDayDbContext db) { _db = db; }
    private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    private string CurrentRole => User.FindFirst(ClaimTypes.Role)!.Value;

    /// <summary>Enrol in a category (Participant only)</summary>
    [HttpPost("enrolments")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Create([FromBody] CreateEnrolmentDto dto)
    {
        var cat = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == dto.CategoryId);
        if (cat == null) return NotFound(new { message = "Category not found" });
        if (cat.Event.Status != "Open") return BadRequest(new { message = "Event not open for enrolment" });
        if (await _db.Enrolments.AnyAsync(e => e.ParticipantId == CurrentUserId && e.CategoryId == dto.CategoryId))
            return BadRequest(new { message = "Already enrolled in this category" });
        var count = await _db.Enrolments.CountAsync(e => e.CategoryId == dto.CategoryId && e.Status != "Cancelled");
        if (count >= cat.MaxParticipants) return BadRequest(new { message = "Category is full" });

        var enr = new Enrolment { ParticipantId = CurrentUserId, CategoryId = dto.CategoryId, Status = "Registered" };
        _db.Enrolments.Add(enr);
        await _db.SaveChangesAsync();
        return StatusCode(201, new { enr.EnrolmentId, enr.Status });
    }

    /// <summary>Get a single enrolment</summary>
    [HttpGet("enrolments/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var e = await _db.Enrolments
            .Include(x => x.Participant).Include(x => x.Category).ThenInclude(c => c.Event)
            .Include(x => x.Result).FirstOrDefaultAsync(x => x.EnrolmentId == id);
        if (e == null) return NotFound();
        if (CurrentRole == "Participant" && e.ParticipantId != CurrentUserId) return Forbid();
        if (CurrentRole == "Organiser" && e.Category.Event.OrganiserId != CurrentUserId) return Forbid();
        return Ok(Map(e));
    }

    /// <summary>Cancel enrolment (Participant owner only)</summary>
    [HttpDelete("enrolments/{id}")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await _db.Enrolments.FindAsync(id);
        if (e == null) return NotFound();
        if (e.ParticipantId != CurrentUserId) return Forbid();
        e.Status = "Cancelled";
        e.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Get all enrolments for an event (Organiser owner only)</summary>
    [HttpGet("events/{eventId}/enrolments")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        var ev = await _db.Events.FindAsync(eventId);
        if (ev == null) return NotFound();
        if (ev.OrganiserId != CurrentUserId) return Forbid();
        var list = await _db.Enrolments
            .Include(e => e.Participant).Include(e => e.Category).Include(e => e.Result)
            .Where(e => e.Category.EventId == eventId).ToListAsync();
        return Ok(list.Select(Map));
    }

    private EnrolmentResponseDto Map(Enrolment e) => new()
    {
        EnrolmentId = e.EnrolmentId,
        ParticipantId = e.ParticipantId,
        ParticipantName = e.Participant.FullName,
        CategoryId = e.CategoryId,
        CategoryName = e.Category.Name,
        EventId = e.Category.EventId,
        EventName = e.Category.Event?.Name ?? "",
        RegistrationDate = e.RegistrationDate,
        Status = e.Status,
        BibNumber = e.BibNumber,
        Result = e.Result == null ? null : new ResultResponseDto
        {
            ResultId = e.Result.ResultId,
            EnrolmentId = e.Result.EnrolmentId,
            FinishTime = e.Result.FinishTime,
            OverallPosition = e.Result.OverallPosition,
            CategoryPosition = e.Result.CategoryPosition,
            ChipTime = e.Result.ChipTime,
            GunTime = e.Result.GunTime,
            Status = e.Result.Status
        }
    };
}