using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Congnex.Application.Interfaces;
using Congnex.Application.Settings;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace Congnex.Infrastructure.Services;

public sealed class GoogleAuthService(
    IOptions<GoogleSettings> opts,
    IHttpClientFactory http) : IGoogleAuthService
{
    private readonly GoogleSettings _cfg = opts.Value;

    public async Task<GoogleUserInfo?> VerifyIdTokenAsync(string idToken)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings { Audience = [_cfg.ClientId] });

            return new GoogleUserInfo(
                Sub:       payload.Subject,
                Email:     payload.Email,
                FirstName: payload.GivenName ?? payload.Email?.Split('@')[0] ?? "",
                LastName:  payload.FamilyName ?? "");
        }
        catch (InvalidJwtException)
        {
            return null;
        }
    }

    public async Task<string?> ExchangeCodeForIdTokenAsync(string code, string redirectUri)
    {
        var client = http.CreateClient();
        var resp = await client.PostAsync(
            "https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"]          = code,
                ["client_id"]     = _cfg.ClientId,
                ["client_secret"] = _cfg.ClientSecret,
                ["redirect_uri"]  = redirectUri,
                ["grant_type"]    = "authorization_code",
            }));

        if (!resp.IsSuccessStatusCode) return null;

        var body = await resp.Content.ReadFromJsonAsync<GoogleTokenResponse>();
        return body?.IdToken;
    }

    public async Task<GoogleUserInfo?> GetUserInfoFromAccessTokenAsync(string accessToken)
    {
        var client = http.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var resp = await client.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");
        if (!resp.IsSuccessStatusCode) return null;

        var info = await resp.Content.ReadFromJsonAsync<GoogleUserInfoResponse>();
        if (info?.Sub is null || info.Email is null) return null;

        return new GoogleUserInfo(
            Sub:       info.Sub,
            Email:     info.Email,
            FirstName: info.GivenName ?? info.Email.Split('@')[0],
            LastName:  info.FamilyName ?? "");
    }

    private sealed class GoogleTokenResponse
    {
        [JsonPropertyName("id_token")]
        public string? IdToken { get; init; }
    }

    private sealed class GoogleUserInfoResponse
    {
        [JsonPropertyName("sub")]        public string? Sub        { get; init; }
        [JsonPropertyName("email")]      public string? Email      { get; init; }
        [JsonPropertyName("given_name")] public string? GivenName  { get; init; }
        [JsonPropertyName("family_name")]public string? FamilyName { get; init; }
    }
}
