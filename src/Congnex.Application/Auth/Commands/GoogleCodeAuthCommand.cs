using Congnex.Application.Auth.Dtos;
using Congnex.Application.Interfaces;
using Congnex.Application.Settings;
using Congnex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Congnex.Application.Auth.Commands;

public record GoogleCodeAuthCommand(string Code, string RedirectUri) : IRequest<AuthResult>;

public sealed class GoogleCodeAuthCommandHandler(
    ICongnexDbContext db,
    IJwtTokenService jwt,
    IPasswordHasher hasher,
    IGoogleAuthService google,
    IEmailService email,
    IOptions<JwtSettings> jwtOpts) : IRequestHandler<GoogleCodeAuthCommand, AuthResult>
{
    private readonly JwtSettings _jwt = jwtOpts.Value;

    public async Task<AuthResult> Handle(GoogleCodeAuthCommand req, CancellationToken ct)
    {
        var idToken = await google.ExchangeCodeForIdTokenAsync(req.Code, req.RedirectUri)
            ?? throw new UnauthorizedAccessException("Failed to exchange Google authorization code.");

        var info = await google.VerifyIdTokenAsync(idToken)
            ?? throw new UnauthorizedAccessException("Invalid Google ID token.");

        var user = await db.Users.FirstOrDefaultAsync(
            u => u.GoogleSub == info.Sub || u.Email == info.Email, ct);

        bool isNew = user is null;
        if (isNew)
        {
            user = new User
            {
                FirstName = info.FirstName,
                LastName  = info.LastName,
                Email     = info.Email,
                GoogleSub = info.Sub
            };
            db.Users.Add(user);
        }
        else
        {
            user!.GoogleSub = info.Sub;
        }

        var refreshToken = jwt.GenerateRefreshToken();
        user.RefreshTokenHash      = hasher.Hash(refreshToken);
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays);
        await db.SaveChangesAsync(ct);

        if (isNew)
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
