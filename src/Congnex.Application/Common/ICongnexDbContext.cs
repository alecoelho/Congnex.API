using Congnex.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Application.Common;

public interface ICongnexDbContext
{
    DbSet<User> Users { get; }
    DbSet<Unit> Units { get; }
    DbSet<Lesson> Lessons { get; }
    DbSet<Question> Questions { get; }
    DbSet<UserProgress> UserProgress { get; }
    DbSet<UserAnswer> UserAnswers { get; }
    DbSet<ReviewItem> ReviewItems { get; }
    DbSet<AiQuestion> AiQuestions { get; }
    DbSet<StudyPlan> StudyPlans { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<NotificationPreference> NotificationPreferences { get; }
    DbSet<DeviceToken> DeviceTokens { get; }
    DbSet<FlashcardReview> FlashcardReviews { get; }
    DbSet<UserInterviewAnswer> UserInterviewAnswers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
