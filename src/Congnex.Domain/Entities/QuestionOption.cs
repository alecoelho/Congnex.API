using Congnex.Domain.Common;

namespace Congnex.Domain.Entities;

public class QuestionOption : Entity
{
    public Guid QuestionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public string? OptionImageUrl { get; set; }
    public string? OptionAudioUrl { get; set; }
    public bool IsCorrect { get; set; }
    public int OrderIndex { get; set; }

    public Question Question { get; set; } = null!;
}
