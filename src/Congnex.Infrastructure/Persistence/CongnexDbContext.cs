using Congnex.Application.Common;
using Congnex.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Infrastructure.Persistence;

public class CongnexDbContext(DbContextOptions<CongnexDbContext> options)
    : DbContext(options), ICongnexDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<UserProgress> UserProgress => Set<UserProgress>();
    public DbSet<UserAnswer> UserAnswers => Set<UserAnswer>();
    public DbSet<ReviewItem> ReviewItems => Set<ReviewItem>();
    public DbSet<AiQuestion> AiQuestions => Set<AiQuestion>();
    public DbSet<StudyPlan> StudyPlans => Set<StudyPlan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.ApplyConfigurationsFromAssembly(typeof(CongnexDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Common.Entity>()
                     .Where(e => e.State == EntityState.Modified))
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(ct);
    }
}
