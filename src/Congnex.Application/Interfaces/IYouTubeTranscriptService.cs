namespace Congnex.Application.Interfaces;

public interface IYouTubeTranscriptService
{
    Task<string?> GetTranscriptAsync(string videoUrl, CancellationToken ct = default);
}
