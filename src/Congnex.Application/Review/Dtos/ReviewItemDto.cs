namespace Congnex.Application.Review.Dtos;

public record ReviewItemDto(
    Guid          Id,
    string        Source,
    string        Prompt,
    string        Options,       // JSON array of choices
    List<string>  CorrectAnswers,
    DateTime      DueDate,
    int           Reps,
    string        State);

public record ReviewResultDto(
    Guid      ReviewItemId,
    DateTime  NextDueDate,
    float     Stability,
    float     Difficulty,
    string    State,
    int       Reps,
    int       Lapses);
