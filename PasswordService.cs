namespace RaceDay.API.Services;
public interface IPasswordService { string Hash(string p); bool Verify(string p, string h); }
public class PasswordService : IPasswordService
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}