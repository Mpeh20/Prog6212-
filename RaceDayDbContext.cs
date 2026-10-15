
using global::RaceDay.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RaceDay.API.Models;

namespace RaceDay.API.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }
    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);
        mb.Entity<User>().HasIndex(u => u.Email).IsUnique();
        mb.Entity<Enrolment>().HasIndex(e => new { e.ParticipantId, e.CategoryId }).IsUnique();
        mb.Entity<Result>().HasIndex(r => r.EnrolmentId).IsUnique();

        mb.Entity<Event>().HasOne(e => e.Organiser).WithMany(u => u.OrganisedEvents)
            .HasForeignKey(e => e.OrganiserId).OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Category>().HasOne(c => c.Event).WithMany(e => e.Categories)
            .HasForeignKey(c => c.EventId).OnDelete(DeleteBehavior.Cascade);

        mb.Entity<Enrolment>().HasOne(e => e.Participant).WithMany(u => u.Enrolments)
            .HasForeignKey(e => e.ParticipantId).OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Enrolment>().HasOne(e => e.Category).WithMany(c => c.Enrolments)
            .HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Cascade);

        mb.Entity<Result>().HasOne(r => r.Enrolment).WithOne(e => e.Result)
            .HasForeignKey<Result>(r => r.EnrolmentId).OnDelete(DeleteBehavior.Cascade);

        mb.Entity<Payment>().HasOne(p => p.Enrolment).WithMany(e => e.Payments)
            .HasForeignKey(p => p.EnrolmentId).OnDelete(DeleteBehavior.Cascade);
    }
}
