using Congnex.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Application.Auth.Commands;

public record LogoutCommand(Guid UserId) : IRequest;

public sealed class LogoutCommandHandler(ICongnexDbContext db) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand req, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == req.UserId, ct);
        if (user is null) return;

        user.RefreshTokenHash      = null;
        user.RefreshTokenExpiresAt = null;
        await db.SaveChangesAsync(ct);
    }
}
