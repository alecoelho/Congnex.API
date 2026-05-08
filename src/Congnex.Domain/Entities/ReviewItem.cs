using Congnex.Domain.Common;
using Congnex.Domain.Enums;

namespace Congnex.Domain.Entities;

public class ReviewItem : Entity
{
    public Guid UserId { get; set; }
    public Guid? QuestionId { get; set; }          // null when source = AiGenerated
    public Guid? AiQuestionId { get; set; }        // null when source = Lesson

    // "lesson" | "ai_generated"
    public string Source { get; set; } = "lesson";

    // FSRS state
    public float Stability { get; set; }
    public float Difficulty { get; set; } = 5f;
    public DateTime DueDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastReviewAt { get; set; }
    public int Reps { get; set; }
    public int Lapses { get; set; }
    public FsrsCardState State { get; set; } = FsrsCardState.New;

    public User User { get; set; } = null!;
    public Question? Question { get; set; }
    public AiQuestion? AiQuestion { get; set; }
}
