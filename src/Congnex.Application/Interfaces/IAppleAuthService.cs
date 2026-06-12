namespace Congnex.Application.Interfaces;

public record AppleUserInfo(string Sub, string? Email);

public interface IAppleAuthService
{
    Task<AppleUserInfo?> VerifyIdTokenAsync(string idToken);
}
