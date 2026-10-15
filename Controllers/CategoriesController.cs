using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Category;
using RaceDay.API.DTOs.Event;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public CategoriesController(RaceDayDbContext db) { _db = db; }
    private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    /// <summary>List categories for an event</summary>
    [HttpGet("events/{eventId}/categories")]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventId == eventId)) return NotFound();
        var cats = await _db.Categories.Where(c => c.EventId == eventId)
            .Select(c => new CategoryResponseDto
            {
                CategoryId = c.CategoryId,
                EventId = c.EventId,
                Name = c.Name,
                CategoryType = c.CategoryType,
                AgeGroup = c.AgeGroup,
                DistanceKm = c.DistanceKm,
                MaxParticipants = c.MaxParticipants,
                FeeAmount = c.FeeAmount
            }).ToListAsync();
        return Ok(cats);
    }

    /// <summary>Create category (Organiser owner only)</summary>
    [HttpPost("events/{eventId}/categories")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Create(int eventId, [FromBody] CreateCategoryDto dto)
    {
        var ev = await _db.Events.FindAsync(eventId);
        if (ev == null) return NotFound();
        if (ev.OrganiserId != CurrentUserId) return Forbid();
        var cat = new Category
        {
            EventId = eventId,
            Name = dto.Name,
            CategoryType = dto.CategoryType,
            AgeGroup = dto.AgeGroup,
            DistanceKm = dto.DistanceKm,
            MaxParticipants = dto.MaxParticipants,
            FeeAmount = dto.FeeAmount
        };
        _db.Categories.Add(cat);
        await _db.SaveChangesAsync();
        return StatusCode(201, new { cat.CategoryId, cat.Name });
    }

    /// <summary>Update category (Organiser owner only)</summary>
    [HttpPut("categories/{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateCategoryDto dto)
    {
        var cat = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == id);
        if (cat == null) return NotFound();
        if (cat.Event.OrganiserId != CurrentUserId) return Forbid();
        cat.Name = dto.Name; cat.CategoryType = dto.CategoryType;
        cat.AgeGroup = dto.AgeGroup; cat.DistanceKm = dto.DistanceKm;
        cat.MaxParticipants = dto.MaxParticipants; cat.FeeAmount = dto.FeeAmount;
        await _db.SaveChangesAsync();
        return Ok(new { cat.CategoryId, cat.Name });
    }

    /// <summary>Delete category (Organiser owner only)</summary>
    [HttpDelete("categories/{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var cat = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == id);
        if (cat == null) return NotFound();
        if (cat.Event.OrganiserId != CurrentUserId) return Forbid();
        _db.Categories.Remove(cat);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}