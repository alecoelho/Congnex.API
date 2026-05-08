using Congnex.Domain.Common;
using Congnex.Domain.Enums;

namespace Congnex.Domain.Entities;

public class User : Entity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string? GoogleSub { get; set; }
    public string? AppleSub { get; set; }

    // Gamification
    public int Xp { get; set; }
    public int Streak { get; set; }
    public int Lives { get; set; } = 3;
    public int Energy { get; set; } = 100;
    public int MaxEnergy { get; set; } = 100;
    public DateTime? LastLessonAt { get; set; }
    public DateTime? LastLifeRegenAt { get; set; }

    // Preferences
    public UserPlan Plan { get; set; } = UserPlan.Free;
    public int DailyMinutes { get; set; } = 10;
    public string Language { get; set; } = "en";
    public string? Motivations { get; set; }    // JSON array

    // Refresh token (hashed)
    public string? RefreshTokenHash { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }

    // Navigation
    public ICollection<UserProgress> Progress { get; set; } = [];
    public ICollection<UserAnswer> Answers { get; set; } = [];
    public ICollection<ReviewItem> ReviewItems { get; set; } = [];
    public ICollection<Subscription> Subscriptions { get; set; } = [];
    public ICollection<DeviceToken> DeviceTokens { get; set; } = [];
    public StudyPlan? StudyPlan { get; set; }
    public NotificationPreference? NotificationPreference { get; set; }
}
