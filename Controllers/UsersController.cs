using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Auth;
using RaceDay.API.DTOs.Enrolment;
using RaceDay.API.DTOs.Result;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public UsersController(RaceDayDbContext db) { _db = db; }
    private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    /// <summary>Get current user's profile</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var u = await _db.Users.FindAsync(CurrentUserId);
        if (u == null) return NotFound();
        return Ok(new UserDto { UserId = u.UserId, Email = u.Email, FullName = u.FullName, Role = u.Role });
    }

    /// <summary>Update current user's profile</summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileDto dto)
    {
        var u = await _db.Users.FindAsync(CurrentUserId);
        if (u == null) return NotFound();
        if (!string.IsNullOrEmpty(dto.FullName)) u.FullName = dto.FullName;
        if (!string.IsNullOrEmpty(dto.Email))
        {
            if (await _db.Users.AnyAsync(x => x.Email == dto.Email && x.UserId != CurrentUserId))
                return Conflict(new { message = "Email already in use" });
            u.Email = dto.Email;
        }
        u.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new UserDto { UserId = u.UserId, Email = u.Email, FullName = u.FullName, Role = u.Role });
    }

    /// <summary>Get user by ID</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var u = await _db.Users.FindAsync(id);
        if (u == null) return NotFound();
        return Ok(new UserDto { UserId = u.UserId, Email = u.Email, FullName = u.FullName, Role = u.Role });
    }

    /// <summary>Get current user's enrolments</summary>
    [HttpGet("me/enrolments")]
    public async Task<IActionResult> GetMyEnrolments()
    {
        var list = await _db.Enrolments
            .Include(e => e.Category).ThenInclude(c => c.Event)
            .Include(e => e.Participant).Include(e => e.Result)
            .Where(e => e.ParticipantId == CurrentUserId)
            .ToListAsync();

        return Ok(list.Select(e => new EnrolmentResponseDto
        {
            EnrolmentId = e.EnrolmentId,
            ParticipantId = e.ParticipantId,
            ParticipantName = e.Participant.FullName,
            CategoryId = e.CategoryId,
            CategoryName = e.Category.Name,
            EventId = e.Category.EventId,
            EventName = e.Category.Event.Name,
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
        }));
    }
}
public class UpdateProfileDto { public string? FullName { get; set; } public string? Email { get; set; } }