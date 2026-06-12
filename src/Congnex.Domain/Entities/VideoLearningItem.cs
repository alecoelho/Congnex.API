using Congnex.Domain.Common;

namespace Congnex.Domain.Entities;

public class VideoLearningItem : Entity
{
    public Guid VideoId { get; set; }
    public string ItemType { get; set; } = string.Empty; // object, word, phrase, expression, grammar
    public string TextEn { get; set; } = string.Empty;
    public string? TextPt { get; set; }
    public string? Category { get; set; }
    public string Difficulty { get; set; } = "easy"; // easy, medium, hard
    public double? TimestampStart { get; set; }
    public double? TimestampEnd { get; set; }

    public LessonVideo Video { get; set; } = null!;
    public ICollection<Question> Questions { get; set; } = [];
}
