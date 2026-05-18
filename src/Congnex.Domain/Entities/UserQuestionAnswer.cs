using Congnex.Domain.Common;

namespace Congnex.Domain.Entities;

public class UserQuestionAnswer : Entity
{
    public Guid UserId { get; set; }
    public Guid LessonId { get; set; }
    public Guid QuestionId { get; set; }
    public Guid? SelectedOptionId { get; set; }
    public string? TextAnswer { get; set; }
    public string? AudioUrl { get; set; }
    public bool IsCorrect { get; set; }
    public int TimeSpentSeconds { get; set; }
    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
    public Question Question { get; set; } = null!;
    public QuestionOption? SelectedOption { get; set; }
}
