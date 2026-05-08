using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Congnex.Application.Interfaces;
using Congnex.Application.Settings;
using Congnex.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Congnex.Infrastructure.Services;

public sealed class JwtTokenService(IOptions<JwtSettings> opts) : IJwtTokenService
{
    private readonly JwtSettings _cfg = opts.Value;

    public string GenerateAccessToken(User user)
    {
        var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg.SecretKey));
        var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_cfg.AccessTokenExpiryMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,        user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email,      user.Email),
            new Claim(JwtRegisteredClaimNames.GivenName,  user.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new Claim("plan",                             user.Plan.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti,        Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer:   _cfg.Issuer,
            audience: _cfg.Audience,
            claims:   claims,
            expires:  expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    public Guid? ValidateAccessToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg.SecretKey));

        try
        {
            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey         = key,
                ValidateIssuer           = true,
                ValidIssuer              = _cfg.Issuer,
                ValidateAudience         = true,
                ValidAudience            = _cfg.Audience,
                ValidateLifetime         = true,
                ClockSkew                = TimeSpan.Zero
            }, out var validated);

            var jwt = (JwtSecurityToken)validated;
            return Guid.TryParse(jwt.Subject, out var id) ? id : null;
        }
        catch
        {
            return null;
        }
    }
}
