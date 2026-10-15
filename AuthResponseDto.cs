namespace RaceDay.API.DTOs.Auth;
public class AuthResponseDto { public string Token { get; set; } = ""; public UserDto User { get; set; } = new(); }
public class UserDto { public int UserId { get; set; } public string Email { get; set; } = ""; public string FullName { get; set; } = ""; public string Role { get; set; } = ""; }