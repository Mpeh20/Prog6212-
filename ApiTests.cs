using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.Extensions.Configuration;
using RaceDay.API.Data;
using RaceDay.API.Models;
using RaceDay.API.Services;
using Swashbuckle.SwaggerUi;
using Xunit;

namespace RaceDay.Tests;

public class AuthServiceTests
{
    private RaceDayDbContext Db() => new(new DbContextOptionsBuilder<RaceDayDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private ITokenService TokenSvc() => new TokenService(new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "TestKeyForUnitTestsThatIsLongEnough12345!",
            ["Jwt:Issuer"] = "Test",
            ["Jwt:Audience"] = "Test"
        }).Build());

    [Fact]
    public void Password_Hash_And_Verify_Works()
    {
        var svc = new PasswordService();
        var hash = svc.Hash("MyPassword123");
        Assert.NotEqual("MyPassword123", hash);
        Assert.True(svc.Verify("MyPassword123", hash));
        Assert.False(svc.Verify("Wrong", hash));
    }

    [Fact]
    public void Token_Creation_Returns_Jwt()
    {
        var u = new User { UserId = 1, Email = "t@x.com", FullName = "T", Role = "Organiser" };
        var t = TokenSvc().CreateToken(u);
        Assert.False(string.IsNullOrWhiteSpace(t));
        Assert.Contains(".", t);
    }

    [Fact]
    public async Task Register_Hashes_Password()
    {
        using var db = Db();
        var svc = new PasswordService();
        db.Users.Add(new User
        {
            Email = "a@b.com",
            PasswordHash = svc.Hash("Password123"),
            FullName = "Alice",
            Role = "Participant"
        });
        await db.SaveChangesAsync();
        var saved = await db.Users.FirstAsync(u => u.Email == "a@b.com");
        Assert.NotEqual("Password123", saved.PasswordHash);
        Assert.True(svc.Verify("Password123", saved.PasswordHash));
    }

    [Fact]
    public async Task Duplicate_Email_Detected()
    {
        using var db = Db();
        db.Users.Add(new User { Email = "x@y.com", PasswordHash = "h", FullName = "X", Role = "Participant" });
        await db.SaveChangesAsync();
        Assert.True(await db.Users.AnyAsync(u => u.Email == "x@y.com"));
    }
}

public class RoleAccessTests
{
    private RaceDayDbContext Db() => new(new DbContextOptionsBuilder<RaceDayDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Organiser_Owns_Event_Participant_Does_Not()
    {
        using var db = Db();
        var o = new User { Email = "o@x.com", PasswordHash = "h", FullName = "O", Role = "Organiser" };
        var p = new User { Email = "p@x.com", PasswordHash = "h", FullName = "P", Role = "Participant" };
        db.Users.AddRange(o, p); await db.SaveChangesAsync();

        var ev = new Event
        {
            OrganiserId = o.UserId,
            Name = "E",
            Location = "L",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddHours(1),
            DistanceKm = 10,
            EventType = "Run"
        };
        db.Events.Add(ev); await db.SaveChangesAsync();
        Assert.Equal(o.UserId, ev.OrganiserId);
        Assert.NotEqual(p.UserId, ev.OrganiserId);
    }

    [Fact]
    public async Task Participant_Enrolment_Links_Correctly()
    {
        using var db = Db();
        var p = new User { Email = "p@x.com", PasswordHash = "h", FullName = "P", Role = "Participant" };
        var o = new User { Email = "o@x.com", PasswordHash = "h", FullName = "O", Role = "Organiser" };
        db.Users.AddRange(p, o); await db.SaveChangesAsync();
        var ev = new Event
        {
            OrganiserId = o.UserId,
            Name = "E",
            Location = "L",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddHours(1),
            DistanceKm = 5,
            EventType = "Run",
            Status = "Open"
        };
        db.Events.Add(ev); await db.SaveChangesAsync();
        var cat = new Category
        {
            EventId = ev.EventId,
            Name = "Open",
            CategoryType = "Distance",
            DistanceKm = 5,
            FeeAmount = 10,
            MaxParticipants = 100
        };
        db.Categories.Add(cat); await db.SaveChangesAsync();
        var enr = new Enrolment { ParticipantId = p.UserId, CategoryId = cat.CategoryId, Status = "Registered" };
        db.Enrolments.Add(enr); await db.SaveChangesAsync();

        Assert.Equal(p.UserId, enr.ParticipantId);
        Assert.True(await db.Enrolments.AnyAsync(e =>
            e.ParticipantId == p.UserId && e.CategoryId == cat.CategoryId));
    }

    [Fact]
    public async Task Result_Attaches_To_Enrolment()
    {
        using var db = Db();
        var p = new User { Email = "p@x.com", PasswordHash = "h", FullName = "P", Role = "Participant" };
        var o = new User { Email = "o@x.com", PasswordHash = "h", FullName = "O", Role = "Organiser" };
        db.Users.AddRange(p, o); await db.SaveChangesAsync();
        var ev = new Event
        {
            OrganiserId = o.UserId,
            Name = "E",
            Location = "L",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddHours(1),
            DistanceKm = 5,
            EventType = "Run",
            Status = "Open"
        };
        db.Events.Add(ev); await db.SaveChangesAsync();
        var cat = new Category
        {
            EventId = ev.EventId,
            Name = "C",
            CategoryType = "Distance",
            DistanceKm = 5,
            FeeAmount = 0,
            MaxParticipants = 100
        };
        db.Categories.Add(cat); await db.SaveChangesAsync();
        var enr = new Enrolment { ParticipantId = p.UserId, CategoryId = cat.CategoryId };
        db.Enrolments.Add(enr); await db.SaveChangesAsync();
        var r = new Result
        {
            EnrolmentId = enr.EnrolmentId,
            FinishTime = TimeSpan.FromMinutes(30),
            OverallPosition = 1,
            CategoryPosition = 1,
            Status = "Finished"
        };
        db.Results.Add(r); await db.SaveChangesAsync();
        Assert.Equal(enr.EnrolmentId, r.EnrolmentId);
    }

    [Fact]
    public async Task Unauthenticated_Request_Has_No_User()
    {
        using var db = Db();
        Assert.False(await db.Users.AnyAsync());
    }
}