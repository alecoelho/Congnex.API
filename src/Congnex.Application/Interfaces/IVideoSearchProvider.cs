namespace Congnex.Application.Interfaces;

public record VideoCandidate(
    string VideoId,
    string Url,
    string Title,
    string? Snippet,
    int MatchScore,
    string Confidence, // "low", "medium", "high"
    string Source, // "brave_search", "youtube_api"
    bool TranscriptAvailable,
    List<string> MatchedStructures
);

public record VideoSearchContext(
    List<string> TargetStructures,
    string? Interest,
    string? Hobbies,
    string CefrLevel,
    string BaseQuery
);

public interface IVideoSearchProvider
{
    Task<List<VideoCandidate>> SearchAndRankAsync(VideoSearchContext context, CancellationToken ct = default);
}
