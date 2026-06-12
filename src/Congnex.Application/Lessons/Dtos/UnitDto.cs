namespace Congnex.Application.Lessons.Dtos;

public record UnitDto(
    Guid         Id,
    string       Title,
    string       Description,
    int          OrderIndex,
    List<LessonDto> Lessons);

public record LessonDto(
    Guid      Id,
    string    Title,
    int       OrderIndex,
    int       XpReward,
    string    Status,        // "locked" | "current" | "completed"
    int       Score,
    DateTime? CompletedAt);
