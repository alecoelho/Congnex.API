using Congnex.Application.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Congnex.Functions;

public class ReviewNotificationFunction(
    ICongnexDbContext db,
    INotificationService notifications,
    ILogger<ReviewNotificationFunction> log)
{
    // Runs every hour — sends push notifications to users with due review items
    // Respects each user's notification preference window (StartHour–EndHour UTC)
    [Function(nameof(ReviewNotificationFunction))]
    public async Task Run([TimerTrigger("0 0 * * * *")] TimerInfo timer, CancellationToken ct)
    {
        var now     = DateTime.UtcNow;
        var nowHour = now.Hour;

        // Load all enabled notification prefs with at least one due review item
        var eligible = await db.NotificationPreferences
            .Where(p => p.IsEnabled && p.StartHour <= nowHour && nowHour < p.EndHour)
            .Select(p => p.UserId)
            .ToListAsync(ct);

        if (eligible.Count == 0) return;

        // Filter to users who actually have items due now
        var usersWithDue = await db.ReviewItems
            .Where(r => eligible.Contains(r.UserId) && r.DueDate <= now)
            .Select(r => r.UserId)
            .Distinct()
            .ToListAsync(ct);

        var sent = 0;
        foreach (var userId in usersWithDue)
        {
            try
            {
                await notifications.SendToUserAsync(
                    userId,
                    title: "Time to review!",
                    body: "You have cards due. Keep your streak going!",
                    data: new Dictionary<string, string> { ["screen"] = "Review" });
                sent++;
            }
            catch (Exception ex)
            {
                log.LogWarning(ex, "Failed to send review notification to user {UserId}.", userId);
            }
        }

        log.LogInformation("Sent review notifications to {Sent}/{Total} eligible users.", sent, usersWithDue.Count);
    }
}
