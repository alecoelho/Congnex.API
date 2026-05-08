using Congnex.Domain.Enums;

namespace Congnex.Application.Interfaces;

public interface INotificationService
{
    Task<string> RegisterDeviceAsync(string token, DevicePlatform platform, Guid userId);
    Task DeregisterDeviceAsync(string hubRegistrationId);
    Task SendToUserAsync(Guid userId, string title, string body, Dictionary<string, string>? data = null);
}
