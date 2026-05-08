namespace Congnex.Application.Auth.Dtos;

public record AuthResult(
    string AccessToken,
    string RefreshToken,
    int    ExpiresIn,
    Guid   UserId,
    string FirstName,
    string LastName,
    string Email,
    string Plan);
