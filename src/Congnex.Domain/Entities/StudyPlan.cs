using Congnex.Domain.Common;

namespace Congnex.Domain.Entities;

public class StudyPlan : Entity
{
    public Guid UserId { get; set; }
    public string AssessedLevel { get; set; } = string.Empty;   // beginner | intermediate | advanced
    public string Content { get; set; } = "{}";                  // JSON payload from AI
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
