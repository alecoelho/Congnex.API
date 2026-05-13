using Congnex.Application.Auth.Dtos;
using Congnex.Application.Common;
using Congnex.Application.Interfaces;
using Congnex.Application.Settings;
using Congnex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Congnex.Application.Auth.Commands;

public record RegisterCommand(string FirstName, string LastName, string Email, string Password, string? Motivations = null) : IRequest<AuthResult>;

public sealed class RegisterCommandHandler(
    ICongnexDbContext db,
    IJwtTokenService jwt,
    IPasswordHasher hasher,
    IEmailService email,
    IOptions<JwtSettings> jwtOpts) : IRequestHandler<RegisterCommand, AuthResult>
{
    private readonly JwtSettings _jwt = jwtOpts.Value;

    public async Task<AuthResult> Handle(RegisterCommand req, CancellationToken ct)
    {
        ValidatePassword(req.Password);

        if (await db.Users.AnyAsync(u => u.Email == req.Email, ct))
            throw new InvalidOperationException("Email already in use.");

        var refreshToken = jwt.GenerateRefreshToken();

        var user = new User
        {
            FirstName             = req.FirstName,
            LastName              = req.LastName,
            Email                 = req.Email,
            PasswordHash          = hasher.Hash(req.Password),
            Motivations           = req.Motivations,
            RefreshTokenHash      = hasher.Hash(refreshToken),
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

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

    private static void ValidatePassword(string password)
    {
        var errors = new List<string>();

        if (password.Length < 8)
            errors.Add("at least 8 characters");
        if (!password.Any(char.IsUpper))
            errors.Add("one uppercase letter");
        if (!password.Any(char.IsDigit))
            errors.Add("one number");
        if (!password.Any(c => !char.IsLetterOrDigit(c)))
            errors.Add("one special character");

        if (errors.Count > 0)
            throw new InvalidOperationException(
                $"Password must contain {string.Join(", ", errors)}.");
    }
}
