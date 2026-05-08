using Congnex.Application.Auth.Dtos;
using Congnex.Application.Interfaces;
using Congnex.Application.Settings;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Congnex.Application.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<AuthResult>;

public sealed class LoginCommandHandler(
    ICongnexDbContext db,
    IJwtTokenService jwt,
    IPasswordHasher hasher,
    IOptions<JwtSettings> jwtOpts) : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly JwtSettings _jwt = jwtOpts.Value;

    public async Task<AuthResult> Handle(LoginCommand req, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email, ct);

        if (user is null || user.PasswordHash is null || !hasher.Verify(req.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var refreshToken = jwt.GenerateRefreshToken();
        user.RefreshTokenHash     = hasher.Hash(refreshToken);
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays);
        await db.SaveChangesAsync(ct);

        return new AuthResult(
            AccessToken:  jwt.GenerateAccessToken(user),
            RefreshToken: refreshToken,
            ExpiresIn:    _jwt.AccessTokenExpiryMinutes * 60,
            UserId:       user.Id,
            FirstName:    user.FirstName,
            LastName:     user.LastName,
            Email:        user.Email,
            Plan:         user.Plan.ToString());
    }
}
