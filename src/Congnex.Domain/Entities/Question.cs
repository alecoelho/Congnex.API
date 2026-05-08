using Congnex.Domain.Common;
using Congnex.Domain.Enums;

namespace Congnex.Domain.Entities;

public class Question : Entity
{
    public Guid LessonId { get; set; }
    public QuestionType Type { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public List<string> CorrectAnswers { get; set; } = [];  // JSON array — one or many correct options
    public string? Options { get; set; }        // JSON: string[] for MC / pairs
    public string? MediaUrl { get; set; }       // relative blob path
    public int OrderIndex { get; set; }

    public Lesson Lesson { get; set; } = null!;
    public ICollection<UserAnswer> Answers { get; set; } = [];
    public ICollection<ReviewItem> ReviewItems { get; set; } = [];
}
