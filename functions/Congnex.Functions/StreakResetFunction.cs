using Congnex.Application.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Congnex.Functions;

public class StreakResetFunction(ICongnexDbContext db, ILogger<StreakResetFunction> log)
{
    // Runs daily at 00:05 UTC — after midnight, before any activity windows open
    [Function(nameof(StreakResetFunction))]
    public async Task Run([TimerTrigger("0 5 0 * * *")] TimerInfo timer, CancellationToken ct)
    {
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);

        // Users whose last lesson was before yesterday have broken their streak
        var users = await db.Users
            .Where(u => u.Streak > 0 && u.LastLessonAt < yesterday)
            .ToListAsync(ct);

        foreach (var user in users)
            user.Streak = 0;

        if (users.Count > 0)
        {
            await db.SaveChangesAsync(ct);
            log.LogInformation("Streak reset for {Count} users.", users.Count);
        }
    }
}
