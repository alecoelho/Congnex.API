using Congnex.Domain.Common;

namespace Congnex.Domain.Entities;

public class FlashcardReview : Entity
{
    public Guid UserId { get; set; }
    public Guid LessonId { get; set; }
    public string Word { get; set; } = string.Empty;
    public bool Remembered { get; set; }
    public int ReviewCount { get; set; } = 1;
    public int CorrectCount { get; set; }
    public DateTime LastReviewedAt { get; set; } = DateTime.UtcNow;
    public DateTime NextReviewAt { get; set; }
    public int IntervalDays { get; set; } = 1;

    public User User { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
}
