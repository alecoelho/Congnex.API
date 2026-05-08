using Congnex.Domain.Common;

namespace Congnex.Domain.Entities;

public class AiQuestion : Entity
{
    public Guid UserId { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string Options { get; set; } = "[]";   // JSON: string[4]
    public int CorrectIndex { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = "en";
    public bool IsActive { get; set; } = true;

    public User User { get; set; } = null!;
    public ICollection<ReviewItem> ReviewItems { get; set; } = [];
}
