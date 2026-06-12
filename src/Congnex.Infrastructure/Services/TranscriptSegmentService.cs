using Microsoft.Extensions.Logging;

namespace Congnex.Infrastructure.Services;

public record VideoSegment(int StartTime, int EndTime, int Duration, double Score);

public class TranscriptSegmentService
{
    private readonly ILogger<TranscriptSegmentService> _logger;
    private const int MinSegmentDuration = 180; // 3 minutes minimum
    private const int MaxSegmentDuration = 600; // 10 minutes maximum
    private const int ContextExpansion = 60;    // 1 minute expansion if too short

    public TranscriptSegmentService(ILogger<TranscriptSegmentService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Finds the best segment in a transcript that matches the target structures.
    /// Returns null if no good segment found or video is short enough to play fully.
    /// </summary>
    public VideoSegment? FindBestSegment(string transcript, List<string> targetStructures, int videoDurationSeconds)
    {
        // If video is 10 minutes or less, play it fully
        if (videoDurationSeconds <= 600)
            return null;

        // Parse transcript into timed chunks (approximate: split by sentences, estimate timing)
        var chunks = CreateChunks(transcript, videoDurationSeconds);
        if (chunks.Count == 0) return null;

        // Score each chunk based on target structure matching
        var scoredChunks = chunks.Select(c => new
        {
            Chunk = c,
            Score = CalculateChunkScore(c.Text, targetStructures)
        }).ToList();

        // Find the best window of chunks that fits within MaxSegmentDuration
        var bestSegment = FindBestWindow(scoredChunks.Select(s => (s.Chunk, s.Score)).ToList(), videoDurationSeconds);

        if (bestSegment is null) return null;

        // Apply context expansion if segment is too short
        var start = bestSegment.StartTime;
        var end = bestSegment.EndTime;
        var duration = end - start;

        if (duration < MinSegmentDuration)
        {
            var expansion = (MinSegmentDuration - duration) / 2;
            start = Math.Max(0, start - Math.Max(expansion, ContextExpansion));
            end = Math.Min(videoDurationSeconds, end + Math.Max(expansion, ContextExpansion));
        }

        // Ensure we don't exceed max
        if (end - start > MaxSegmentDuration)
            end = start + MaxSegmentDuration;

        _logger.LogInformation("[Segment] Best segment: {Start}s - {End}s (duration: {Duration}s, score: {Score})",
            start, end, end - start, bestSegment.Score);

        return new VideoSegment(start, end, end - start, bestSegment.Score);
    }

    private record TranscriptChunk(string Text, int StartSeconds, int EndSeconds);

    private List<TranscriptChunk> CreateChunks(string transcript, int totalDuration)
    {
        // Split transcript into sentences/phrases
        var sentences = transcript.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 5)
            .ToList();

        if (sentences.Count == 0) return [];

        // Estimate timing: distribute evenly across video duration
        var timePerSentence = (double)totalDuration / sentences.Count;
        var chunks = new List<TranscriptChunk>();

        // Create 30-second chunks
        var chunkDuration = 30;
        var sentencesPerChunk = Math.Max(1, (int)(chunkDuration / timePerSentence));

        for (int i = 0; i < sentences.Count; i += sentencesPerChunk)
        {
            var chunkSentences = sentences.Skip(i).Take(sentencesPerChunk).ToList();
            var text = string.Join(". ", chunkSentences);
            var startSec = (int)(i * timePerSentence);
            var endSec = (int)Math.Min((i + sentencesPerChunk) * timePerSentence, totalDuration);
            chunks.Add(new TranscriptChunk(text, startSec, endSec));
        }

        return chunks;
    }

    private static double CalculateChunkScore(string chunkText, List<string> targetStructures)
    {
        var textLower = chunkText.ToLowerInvariant();
        double score = 0;

        foreach (var structure in targetStructures)
        {
            var words = structure.ToLowerInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 3)
                .ToList();

            if (words.Count == 0) continue;

            // Exact phrase match
            if (textLower.Contains(structure.ToLowerInvariant()))
            {
                score += 10;
                continue;
            }

            // Word overlap
            var matchCount = words.Count(w => textLower.Contains(w));
            var matchRatio = (double)matchCount / words.Count;
            score += matchRatio * 5;
        }

        return score;
    }

    private VideoSegment? FindBestWindow(List<(TranscriptChunk Chunk, double Score)> scoredChunks, int totalDuration)
    {
        if (scoredChunks.Count == 0) return null;

        // Sliding window: find the window with highest total score
        var windowSize = Math.Min(20, scoredChunks.Count); // ~10 minutes window (20 chunks × 30s)
        double bestScore = 0;
        int bestStart = 0;
        int bestEnd = 0;

        for (int i = 0; i <= scoredChunks.Count - Math.Min(6, scoredChunks.Count); i++)
        {
            var window = scoredChunks.Skip(i).Take(windowSize).ToList();
            var windowScore = window.Sum(w => w.Score);
            if (windowScore > bestScore)
            {
                bestScore = windowScore;
                bestStart = window.First().Chunk.StartSeconds;
                bestEnd = window.Last().Chunk.EndSeconds;
            }
        }

        if (bestScore <= 0) return null;

        return new VideoSegment(bestStart, bestEnd, bestEnd - bestStart, bestScore);
    }
}
