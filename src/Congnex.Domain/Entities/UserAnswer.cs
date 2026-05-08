using Congnex.Domain.Common;

namespace Congnex.Domain.Entities;

public class UserAnswer : Entity
{
    public Guid UserId { get; set; }
    public Guid QuestionId { get; set; }
    public List<string> GivenAnswers { get; set; } = [];  // JSON array — what the user selected
    public bool IsCorrect { get; set; }
    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Question Question { get; set; } = null!;
}
