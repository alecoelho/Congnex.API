using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using Congnex.Application.Interfaces;
using Congnex.Application.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Congnex.Infrastructure.Services;

public sealed class AppleAuthService(IOptions<AppleSettings> opts) : IAppleAuthService
{
    private readonly AppleSettings _cfg = opts.Value;
    private static readonly HttpClient Http = new();

    public async Task<AppleUserInfo?> VerifyIdTokenAsync(string idToken)
    {
        try
        {
            var keysResponse = await Http.GetFromJsonAsync<ApplePublicKeys>(
                "https://appleid.apple.com/auth/keys");
            if (keysResponse?.Keys is null) return null;

            var handler   = new JwtSecurityTokenHandler();
            var jwks      = new JsonWebKeySet();
            foreach (var k in keysResponse.Keys)
                jwks.Keys.Add(new JsonWebKey(System.Text.Json.JsonSerializer.Serialize(k)));

            handler.ValidateToken(idToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys        = jwks.Keys,
                ValidateIssuer           = true,
                ValidIssuer              = "https://appleid.apple.com",
                ValidateAudience         = true,
                ValidAudience            = _cfg.ClientId,
                ValidateLifetime         = true,
                ClockSkew                = TimeSpan.Zero
            }, out var validated);

            var jwt   = (JwtSecurityToken)validated;
            var sub   = jwt.Subject;
            var email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

            return new AppleUserInfo(sub, email);
        }
        catch
        {
            return null;
        }
    }

    private sealed record ApplePublicKeys(object[] Keys);
}
