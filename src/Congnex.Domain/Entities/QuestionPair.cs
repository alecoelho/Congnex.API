using Congnex.Domain.Common;

namespace Congnex.Domain.Entities;

public class QuestionPair : Entity
{
    public Guid QuestionId { get; set; }
    public string LeftText { get; set; } = string.Empty;
    public string RightText { get; set; } = string.Empty;
    public string? LeftAudioUrl { get; set; }
    public string? RightAudioUrl { get; set; }
    public string? LeftImageUrl { get; set; }
    public string? RightImageUrl { get; set; }
    public int OrderIndex { get; set; }

    public Question Question { get; set; } = null!;
}
