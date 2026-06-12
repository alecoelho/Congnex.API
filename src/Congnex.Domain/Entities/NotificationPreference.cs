using Congnex.Domain.Common;

namespace Congnex.Domain.Entities;

public class NotificationPreference : Entity
{
    public Guid UserId { get; set; }
    public bool IsEnabled { get; set; } = true;
    public int FrequencyHours { get; set; } = 2;   // 1 | 2 | 3 | 4
    public int StartHour { get; set; } = 8;         // 0–23
    public int EndHour { get; set; } = 20;           // 0–23
    public string ContentTypes { get; set; } = "[]"; // JSON: string[]

    public User User { get; set; } = null!;
}
