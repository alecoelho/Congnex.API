using Congnex.Application.Auth.Dtos;
using Congnex.Application.Interfaces;
using Congnex.Application.Settings;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Congnex.Application.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResult>;

public sealed class RefreshTokenCommandHandler(
    ICongnexDbContext db,
    IJwtTokenService jwt,
    IPasswordHasher hasher,
    IOptions<JwtSettings> jwtOpts) : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    private readonly JwtSettings _jwt = jwtOpts.Value;

    public async Task<AuthResult> Handle(RefreshTokenCommand req, CancellationToken ct)
    {
        // Load all users that have an active refresh token and check BCrypt in-memory
        // (refresh tokens are rare so this is fine; alternatively store a lookup index)
        var candidates = await db.Users
            .Where(u => u.RefreshTokenHash != null && u.RefreshTokenExpiresAt > DateTime.UtcNow)
            .ToListAsync(ct);

        var user = candidates.FirstOrDefault(u => hasher.Verify(req.RefreshToken, u.RefreshTokenHash!));
        if (user is null)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var newRefresh = jwt.GenerateRefreshToken();
        user.RefreshTokenHash      = hasher.Hash(newRefresh);
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays);
        await db.SaveChangesAsync(ct);

        return new AuthResult(
            AccessToken:  jwt.GenerateAccessToken(user),
            RefreshToken: newRefresh,
            ExpiresIn:    _jwt.AccessTokenExpiryMinutes * 60,
            UserId:       user.Id,
            FirstName:    user.FirstName,
            LastName:     user.LastName,
            Email:        user.Email,
            Plan:         user.Plan.ToString());
    }
}
