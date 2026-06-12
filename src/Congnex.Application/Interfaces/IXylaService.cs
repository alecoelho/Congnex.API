namespace Congnex.Application.Interfaces;

public interface IXylaService
{
    Task<Guid> StartSessionAsync(Guid userId, CancellationToken ct = default);

    IAsyncEnumerable<string> StreamMessageAsync(
        Guid sessionId,
        Guid userId,
        string message,
        CancellationToken ct = default);

    Task<string> GenerateAnswerExplanationAsync(
        string questionText,
        string correctAnswer,
        string? wrongAnswer,
        CancellationToken ct = default);

    /// <summary>
    /// Called after a lesson is completed. Generates the next lesson in the same unit,
    /// or the first lesson of the next unit if the completed lesson was the last one.
    /// Fire-and-forget internally; returns immediately.
    /// </summary>
    Task GenerateNextLessonAsync(Guid userId, Guid completedLessonId, CancellationToken ct = default);
}
