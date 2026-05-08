using Congnex.Domain.Common;
using Congnex.Domain.Enums;

namespace Congnex.Domain.Entities;

public class UserProgress : Entity
{
    public Guid UserId { get; set; }
    public Guid LessonId { get; set; }
    public LessonStatus Status { get; set; } = LessonStatus.Locked;
    public int Score { get; set; }
    public DateTime? CompletedAt { get; set; }

    public User User { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
}
