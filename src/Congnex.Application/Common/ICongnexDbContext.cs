using Congnex.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Application.Common;

public interface ICongnexDbContext
{
    DbSet<User> Users { get; }
    DbSet<Unit> Units { get; }
    DbSet<Lesson> Lessons { get; }
    DbSet<Question> Questions { get; }
    DbSet<LessonVideo> LessonVideos { get; }
    DbSet<VideoLearningItem> VideoLearningItems { get; }
    DbSet<QuestionOption> QuestionOptions { get; }
    DbSet<QuestionPair> QuestionPairs { get; }
    DbSet<UserProgress> UserProgress { get; }
    DbSet<UserQuestionAnswer> UserQuestionAnswers { get; }
    DbSet<ReviewItem> ReviewItems { get; }
    DbSet<StudyPlan> StudyPlans { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<NotificationPreference> NotificationPreferences { get; }
    DbSet<DeviceToken> DeviceTokens { get; }
    DbSet<FlashcardReview> FlashcardReviews { get; }
    DbSet<UserInterviewAnswer> UserInterviewAnswers { get; }
    DbSet<XylaConversation> XylaConversations { get; }
    DbSet<XylaMessage> XylaMessages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
