using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Payment;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public PaymentsController(RaceDayDbContext db) { _db = db; }
    private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    private string CurrentRole => User.FindFirst(ClaimTypes.Role)!.Value;

    /// <summary>Create payment for an enrolment (Participant owner only)</summary>
    [HttpPost("enrolments/{enrolmentId}/payments")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Create(int enrolmentId, [FromBody] CreatePaymentDto dto)
    {
        var e = await _db.Enrolments.FindAsync(enrolmentId);
        if (e == null) return NotFound();
        if (e.ParticipantId != CurrentUserId) return Forbid();
        var p = new Payment
        {
            EnrolmentId = enrolmentId,
            Amount = dto.Amount,
            Method = dto.Method,
            Status = "Completed",
            TransactionId = "TXN-" + Guid.NewGuid().ToString("N")[..8].ToUpper()
        };
        _db.Payments.Add(p);
        await _db.SaveChangesAsync();
        return StatusCode(201, new PaymentResponseDto
        {
            PaymentId = p.PaymentId,
            EnrolmentId = p.EnrolmentId,
            Amount = p.Amount,
            PaymentDate = p.PaymentDate,
            Method = p.Method,
            Status = p.Status,
            TransactionId = p.TransactionId
        });
    }

    /// <summary>List payments for an enrolment</summary>
    [HttpGet("enrolments/{enrolmentId}/payments")]
    public async Task<IActionResult> GetForEnrolment(int enrolmentId)
    {
        var e = await _db.Enrolments.Include(x => x.Category).ThenInclude(c => c.Event)
            .FirstOrDefaultAsync(x => x.EnrolmentId == enrolmentId);
        if (e == null) return NotFound();
        if (CurrentRole == "Participant" && e.ParticipantId != CurrentUserId) return Forbid();
        if (CurrentRole == "Organiser" && e.Category.Event.OrganiserId != CurrentUserId) return Forbid();
        var list = await _db.Payments.Where(p => p.EnrolmentId == enrolmentId)
            .Select(p => new PaymentResponseDto
            {
                PaymentId = p.PaymentId,
                EnrolmentId = p.EnrolmentId,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                Method = p.Method,
                Status = p.Status,
                TransactionId = p.TransactionId
            }).ToListAsync();
        return Ok(list);
    }
}