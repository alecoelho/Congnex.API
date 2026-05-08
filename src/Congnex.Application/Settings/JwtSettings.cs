namespace Congnex.Application.Settings;

public sealed class JwtSettings
{
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public int AccessTokenExpiryMinutes { get; init; } = 15;
    public int RefreshTokenExpiryDays { get; init; } = 30;
}
