namespace Congnex.Application.Interfaces;

public record GoogleUserInfo(string Sub, string Email, string FirstName, string LastName);

public interface IGoogleAuthService
{
    Task<GoogleUserInfo?> VerifyIdTokenAsync(string idToken);
    Task<string?> ExchangeCodeForIdTokenAsync(string code, string redirectUri);
    Task<GoogleUserInfo?> GetUserInfoFromAccessTokenAsync(string accessToken);
}
