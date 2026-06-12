namespace Congnex.Application.Users.Dtos;

public record ProfileDto(
    Guid   UserId,
    string FirstName,
    string LastName,
    string Email,
    string Plan,
    int    Xp,
    int    Streak,
    int    Lives,
    int    Energy,
    int    MaxEnergy,
    string Level,
    int    DailyMinutes,
    string Motivations);
