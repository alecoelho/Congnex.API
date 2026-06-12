using Congnex.Domain.Common;
using Congnex.Domain.Enums;

namespace Congnex.Domain.Entities;

public class DeviceToken : Entity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DevicePlatform Platform { get; set; }
    public string? HubRegistrationId { get; set; }

    public User User { get; set; } = null!;
}
