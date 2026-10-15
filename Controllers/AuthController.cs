using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Auth;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly IPasswordService _password;
    private readonly ITokenService _token;
    public AuthController(RaceDayDbContext db, IPasswordService password, ITokenService token)
    { _db = db; _password = password; _token = token; }

    /// <summary>Register a new user (Organiser or Participant)</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (dto.Role != "Organiser" && dto.Role != "Participant")
            return BadRequest(new { message = "Role must be Organiser or Participant" });
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return Conflict(new { message = "Email already registered" });

        var user = new User
        {
            Email = dto.Email,
            PasswordHash = _password.Hash(dto.Password),
            FullName = dto.FullName,
            Role = dto.Role
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return StatusCode(201, new { user.UserId, user.Email, user.FullName, user.Role });
    }

    /// <summary>Login and receive JWT token</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !_password.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid credentials" });
        var token = _token.CreateToken(user);
        return Ok(new AuthResponseDto
        {
            Token = token,
            User = new UserDto { UserId = user.UserId, Email = user.Email, FullName = user.FullName, Role = user.Role }
        });
    }
}