namespace Congnex.Application.Interfaces;

public interface IXylaService
{
    Task<Guid> StartSessionAsync(Guid userId, CancellationToken ct = default);

    IAsyncEnumerable<string> StreamMessageAsync(
        Guid sessionId,
        Guid userId,
        string message,
        CancellationToken ct = default);
}
