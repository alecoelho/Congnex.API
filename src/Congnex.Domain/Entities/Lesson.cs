using Congnex.Domain.Common;

namespace Congnex.Domain.Entities;

public class Lesson : Entity
{
    public Guid UnitId { get; set; }
    public int OrderIndex { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int XpReward { get; set; } = 10;

    public Unit Unit { get; set; } = null!;
    public ICollection<Question> Questions { get; set; } = [];
    public ICollection<UserProgress> UserProgress { get; set; } = [];
    public ICollection<LessonVideo> Videos { get; set; } = [];
    public ICollection<UserQuestionAnswer> UserQuestionAnswers { get; set; } = [];
}
