using Congnex.Application.Auth.Dtos;
using Congnex.Application.Interfaces;
using Congnex.Application.Settings;
using Congnex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Congnex.Application.Auth.Commands;

public record AppleAuthCommand(string IdToken, string? FullName) : IRequest<AuthResult>;

public sealed class AppleAuthCommandHandler(
    ICongnexDbContext db,
    IJwtTokenService jwt,
    IPasswordHasher hasher,
    IAppleAuthService apple,
    IEmailService email,
    IOptions<JwtSettings> jwtOpts) : IRequestHandler<AppleAuthCommand, AuthResult>
{
    private readonly JwtSettings _jwt = jwtOpts.Value;

    public async Task<AuthResult> Handle(AppleAuthCommand req, CancellationToken ct)
    {
        var info = await apple.VerifyIdTokenAsync(req.IdToken)
            ?? throw new UnauthorizedAccessException("Invalid Apple ID token.");

        var user = await db.Users.FirstOrDefaultAsync(
            u => u.AppleSub == info.Sub || (info.Email != null && u.Email == info.Email), ct);

        bool isNew = user is null;
        if (isNew)
        {
            // Apple only sends name on first sign-in; split FullName on first space
            var nameParts = (req.FullName ?? info.Email?.Split('@')[0] ?? "User").Split(' ', 2);
            user = new User
            {
                FirstName = nameParts[0],
                LastName  = nameParts.Length > 1 ? nameParts[1] : "",
                Email     = info.Email ?? string.Empty,
                AppleSub  = info.Sub
            };
            db.Users.Add(user);
        }
        else
        {
            user!.AppleSub = info.Sub;
        }

        var refreshToken = jwt.GenerateRefreshToken();
        user.RefreshTokenHash      = hasher.Hash(refreshToken);
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays);
        await db.SaveChangesAsync(ct);

        if (isNew && !string.IsNullOrEmpty(user.Email))
            _ = email.SendWelcomeAsync(user.Email, user.FullName);

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
