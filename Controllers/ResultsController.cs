using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Result;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public ResultsController(RaceDayDbContext db) { _db = db; }
    private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    private string CurrentRole => User.FindFirst(ClaimTypes.Role)!.Value;

    /// <summary>Create result (Organiser owner only)</summary>
    [HttpPost("enrolments/{enrolmentId}/results")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Create(int enrolmentId, [FromBody] CreateResultDto dto)
    {
        var e = await _db.Enrolments.Include(x => x.Category).ThenInclude(c => c.Event)
            .FirstOrDefaultAsync(x => x.EnrolmentId == enrolmentId);
        if (e == null) return NotFound();
        if (e.Category.Event.OrganiserId != CurrentUserId) return Forbid();
        if (await _db.Results.AnyAsync(r => r.EnrolmentId == enrolmentId))
            return Conflict(new { message = "Result already exists" });

        var r = new Result
        {
            EnrolmentId = enrolmentId,
            FinishTime = dto.FinishTime,
            OverallPosition = dto.OverallPosition,
            CategoryPosition = dto.CategoryPosition,
            ChipTime = dto.ChipTime,
            GunTime = dto.GunTime,
            Status = dto.Status
        };
        _db.Results.Add(r);
        await _db.SaveChangesAsync();
        return StatusCode(201, new { r.ResultId, r.Status });
    }

    /// <summary>Get result for an enrolment</summary>
    [HttpGet("enrolments/{enrolmentId}/results")]
    public async Task<IActionResult> GetByEnrolment(int enrolmentId)
    {
        var r = await _db.Results
            .Include(x => x.Enrolment).ThenInclude(e => e.Participant)
            .Include(x => x.Enrolment).ThenInclude(e => e.Category).ThenInclude(c => c.Event)
            .FirstOrDefaultAsync(x => x.EnrolmentId == enrolmentId);
        if (r == null) return NotFound();
        if (CurrentRole == "Participant" && r.Enrolment.ParticipantId != CurrentUserId) return Forbid();
        if (CurrentRole == "Organiser" && r.Enrolment.Category.Event.OrganiserId != CurrentUserId) return Forbid();
        return Ok(Map(r));
    }

    /// <summary>Update result (Organiser owner only)</summary>
    [HttpPut("results/{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateResultDto dto)
    {
        var r = await _db.Results.Include(x => x.Enrolment).ThenInclude(e => e.Category).ThenInclude(c => c.Event)
            .FirstOrDefaultAsync(x => x.ResultId == id);
        if (r == null) return NotFound();
        if (r.Enrolment.Category.Event.OrganiserId != CurrentUserId) return Forbid();
        r.FinishTime = dto.FinishTime; r.OverallPosition = dto.OverallPosition;
        r.CategoryPosition = dto.CategoryPosition; r.ChipTime = dto.ChipTime;
        r.GunTime = dto.GunTime; r.Status = dto.Status;
        r.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { r.ResultId, r.Status });
    }

    /// <summary>Delete result (Organiser owner only)</summary>
    [HttpDelete("results/{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await _db.Results.Include(x => x.Enrolment).ThenInclude(e => e.Category).ThenInclude(c => c.Event)
            .FirstOrDefaultAsync(x => x.ResultId == id);
        if (r == null) return NotFound();
        if (r.Enrolment.Category.Event.OrganiserId != CurrentUserId) return Forbid();
        _db.Results.Remove(r);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Get event leaderboard (public)</summary>
    [HttpGet("events/{eventId}/results")]
    [AllowAnonymous]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventId == eventId)) return NotFound();
        var list = await _db.Results
            .Include(r => r.Enrolment).ThenInclude(e => e.Participant)
            .Include(r => r.Enrolment).ThenInclude(e => e.Category).ThenInclude(c => c.Event)
            .Where(r => r.Enrolment.Category.EventId == eventId && r.Status == "Finished")
            .OrderBy(r => r.OverallPosition).ToListAsync();
        return Ok(list.Select(Map));
    }

    private ResultResponseDto Map(Result r) => new()
    {
        ResultId = r.ResultId,
        EnrolmentId = r.EnrolmentId,
        ParticipantName = r.Enrolment.Participant.FullName,
        EventName = r.Enrolment.Category.Event.Name,
        CategoryName = r.Enrolment.Category.Name,
        FinishTime = r.FinishTime,
        OverallPosition = r.OverallPosition,
        CategoryPosition = r.CategoryPosition,
        ChipTime = r.ChipTime,
        GunTime = r.GunTime,
        Status = r.Status
    };
}