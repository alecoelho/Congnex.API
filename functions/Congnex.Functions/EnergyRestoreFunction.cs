using Congnex.Application.Interfaces;
using Congnex.Domain.Enums;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Congnex.Functions;

public class EnergyRestoreFunction(ICongnexDbContext db, ILogger<EnergyRestoreFunction> log)
{
    private const int MaxEnergy = 100;

    // Runs daily at 06:00 UTC — restores energy to full for all users each morning
    [Function(nameof(EnergyRestoreFunction))]
    public async Task Run([TimerTrigger("0 0 6 * * *")] TimerInfo timer, CancellationToken ct)
    {
        var users = await db.Users
            .Where(u => u.Energy < MaxEnergy)
            .ToListAsync(ct);

        foreach (var user in users)
            user.Energy = MaxEnergy;

        if (users.Count > 0)
        {
            await db.SaveChangesAsync(ct);
            log.LogInformation("Energy restored for {Count} users.", users.Count);
        }
    }
}
