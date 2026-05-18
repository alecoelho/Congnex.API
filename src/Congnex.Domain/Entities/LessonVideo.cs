using Congnex.Domain.Common;

namespace Congnex.Domain.Entities;

public class LessonVideo : Entity
{
    public Guid LessonId { get; set; }
    public string YoutubeVideoId { get; set; } = string.Empty;
    public string YoutubeUrl { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? TranscriptJson { get; set; }
    public string Language { get; set; } = "en";
    public int DurationSeconds { get; set; }

    public Lesson Lesson { get; set; } = null!;
    public ICollection<VideoLearningItem> LearningItems { get; set; } = [];
    public ICollection<Question> Questions { get; set; } = [];
}
