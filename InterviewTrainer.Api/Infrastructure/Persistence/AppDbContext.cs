using Microsoft.EntityFrameworkCore;
using InterviewTrainer.Api.Domain;


namespace InterviewTrainer.Api.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    public DbSet<InterviewSession> interviewSessions => Set<InterviewSession>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InterviewSession> (entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status)
            .HasConversion(
                status => status.Value,
                value => SessionStatus.FromValue(value))
                .HasMaxLength(20)
                .IsRequired();
        });
    }
}
