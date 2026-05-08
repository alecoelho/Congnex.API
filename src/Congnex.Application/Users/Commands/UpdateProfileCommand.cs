using Congnex.Application.Interfaces;
using MediatR;

namespace Congnex.Application.Users.Commands;

public record UpdateProfileCommand(
    Guid    UserId,
    string? FirstName,
    string? LastName,
    int?    DailyMinutes,
    string? Motivations) : IRequest;

public sealed class UpdateProfileCommandHandler(ICongnexDbContext db)
    : IRequestHandler<UpdateProfileCommand>
{
    public async Task Handle(UpdateProfileCommand req, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([req.UserId], ct)
            ?? throw new KeyNotFoundException("User not found.");

        if (req.FirstName is not null)    user.FirstName    = req.FirstName;
        if (req.LastName is not null)     user.LastName     = req.LastName;
        if (req.DailyMinutes.HasValue)    user.DailyMinutes = req.DailyMinutes.Value;
        if (req.Motivations is not null)  user.Motivations  = req.Motivations;

        await db.SaveChangesAsync(ct);
    }
}
