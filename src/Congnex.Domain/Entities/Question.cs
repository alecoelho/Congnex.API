using Congnex.Domain.Common;

namespace Congnex.Domain.Entities;

public class Question : Entity
{
    public Guid LessonId { get; set; }
    public Guid? VideoId { get; set; }
    public Guid? LearningItemId { get; set; }
    public string Type { get; set; } = string.Empty; // multiple_choice, image_choice, listening_choice, translation_pt, translation_en, complete_sentence, match_pairs, video_listening, pronunciation
    public string? Label { get; set; }
    public string? Prompt { get; set; }
    public string? Instruction { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? CorrectAnswer { get; set; }
    public string? AudioText { get; set; }
    public string? ImageUrl { get; set; }
    public int OrderIndex { get; set; }
    public string Difficulty { get; set; } = "easy";

    public Lesson Lesson { get; set; } = null!;
    public LessonVideo? Video { get; set; }
    public VideoLearningItem? LearningItem { get; set; }
    public ICollection<QuestionOption> Options { get; set; } = [];
    public ICollection<QuestionPair> Pairs { get; set; } = [];
    public ICollection<UserQuestionAnswer> UserAnswers { get; set; } = [];
    public ICollection<ReviewItem> ReviewItems { get; set; } = [];
}
