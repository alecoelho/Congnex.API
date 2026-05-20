using Congnex.Domain.Common;

namespace Congnex.Domain.Entities;

public class XylaConversation : Entity
{
    public Guid UserId { get; set; }
    public Guid SessionId { get; set; }
    public string? DetectedLevel { get; set; }
    public string? DetectedGoal { get; set; }
    public string? DetectedInterest { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<XylaMessage> Messages { get; set; } = [];
}
