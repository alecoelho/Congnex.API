using Congnex.Application.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Congnex.Functions;

public class LifeRegenerationFunction(ICongnexDbContext db, ILogger<LifeRegenerationFunction> log)
{
    private const int MaxLives = 3;

    // Runs every 30 minutes — free-tier users regenerate 1 life per 30 min up to max 3
    [Function(nameof(LifeRegenerationFunction))]
    public async Task Run([TimerTrigger("0 */30 * * * *")] TimerInfo timer, CancellationToken ct)
    {
        var users = await db.Users
            .Where(u => u.Lives < MaxLives)
            .ToListAsync(ct);

        foreach (var user in users)
            user.Lives = Math.Min(user.Lives + 1, MaxLives);

        if (users.Count > 0)
        {
            await db.SaveChangesAsync(ct);
            log.LogInformation("Lives regenerated for {Count} users.", users.Count);
        }
    }
}
