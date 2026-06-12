using Congnex.Application.Interfaces;
using Congnex.Application.Settings;
using Congnex.Domain.Entities;
using Congnex.Domain.Enums;
using Microsoft.Azure.NotificationHubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Congnex.Infrastructure.Services;

public sealed class NotificationService(
    ICongnexDbContext db,
    IOptions<AzureSettings> opts) : INotificationService
{
    private readonly NotificationHubsSettings _cfg = opts.Value.NotificationHubs;

    private NotificationHubClient Hub() =>
        NotificationHubClient.CreateClientFromConnectionString(_cfg.ConnectionString, _cfg.HubName);

    public async Task<string> RegisterDeviceAsync(string token, DevicePlatform platform, Guid userId)
    {
        var hub = Hub();

        var tags = new[] { $"userId:{userId}" };

        RegistrationDescription registration = platform switch
        {
            DevicePlatform.iOS     => new AppleRegistrationDescription(token, tags),
            DevicePlatform.Android => new FcmV1RegistrationDescription(token, tags),
            _                      => throw new ArgumentOutOfRangeException(nameof(platform))
        };

        var result = await hub.CreateOrUpdateRegistrationAsync(registration);

        // Persist device token
        var existing = await db.DeviceTokens.FirstOrDefaultAsync(t => t.Token == token);
        if (existing is null)
        {
            db.DeviceTokens.Add(new DeviceToken
            {
                UserId             = userId,
                Token              = token,
                Platform           = platform,
                HubRegistrationId  = result.RegistrationId
            });
        }
        else
        {
            existing.HubRegistrationId = result.RegistrationId;
            existing.UserId            = userId;
        }

        await db.SaveChangesAsync();
        return result.RegistrationId;
    }

    public async Task DeregisterDeviceAsync(string hubRegistrationId)
    {
        var hub = Hub();
        await hub.DeleteRegistrationAsync(hubRegistrationId);

        var token = await db.DeviceTokens
            .FirstOrDefaultAsync(t => t.HubRegistrationId == hubRegistrationId);
        if (token is not null)
        {
            db.DeviceTokens.Remove(token);
            await db.SaveChangesAsync();
        }
    }

    public async Task SendToUserAsync(Guid userId, string title, string body,
        Dictionary<string, string>? data = null)
    {
        var hub  = Hub();
        var tag  = $"userId:{userId}";
        var props = data ?? [];

        // iOS (APNs)
        var apnsPayload = System.Text.Json.JsonSerializer.Serialize(new
        {
            aps = new { alert = new { title, body }, sound = "default" },
            data = props
        });

        // Android (FCM v1)
        var fcmPayload = System.Text.Json.JsonSerializer.Serialize(new
        {
            message = new
            {
                notification = new { title, body },
                data         = props
            }
        });

        await Task.WhenAll(
            hub.SendAppleNativeNotificationAsync(apnsPayload, tag),
            hub.SendFcmV1NativeNotificationAsync(fcmPayload, tag));
    }
}
