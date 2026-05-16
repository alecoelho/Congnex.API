using Congnex.Domain.Common;

namespace Congnex.Domain.Entities;

public class UserInterviewAnswer : Entity
{
    public Guid UserId { get; set; }
    public string EnglishLevel { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public string VideoCategory { get; set; } = string.Empty;

    public User User { get; set; } = null!;
}
