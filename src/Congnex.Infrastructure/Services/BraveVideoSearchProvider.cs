using System.Text.Json;
using Congnex.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Congnex.Infrastructure.Services;

public class BraveVideoSearchProvider : IVideoSearchProvider
{
    private readonly HttpClient _brave;
    private readonly ILogger<BraveVideoSearchProvider> _logger;

    public BraveVideoSearchProvider(IHttpClientFactory httpFactory, ILogger<BraveVideoSearchProvider> logger)
    {
        _brave = httpFactory.CreateClient("brave");
        _logger = logger;
    }

    public async Task<List<VideoCandidate>> SearchAndRankAsync(VideoSearchContext context, CancellationToken ct = default)
    {
        var candidates = new List<VideoCandidate>();

        // Generate multiple search queries for better coverage
        var queries = GenerateQueries(context);

        foreach (var query in queries.Take(3)) // Max 3 queries to avoid rate limiting
        {
            try
            {
                var encoded = Uri.EscapeDataString(query);
                var response = await _brave.GetAsync($"res/v1/web/search?q={encoded}&count=10", ct);

                if (!response.IsSuccessStatusCode) continue;

                using var doc = await JsonDocument.ParseAsync(
                    await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);

                if (!doc.RootElement.TryGetProperty("web", out var web) ||
                    !web.TryGetProperty("results", out var results))
                    continue;

                foreach (var r in results.EnumerateArray())
                {
                    var url = r.TryGetProperty("url", out var u) ? u.GetString() : null;
                    var title = r.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
                    var snippet = r.TryGetProperty("description", out var d) ? d.GetString() : null;

                    if (url is null) continue;
                    if (!url.Contains("youtube.com/shorts") && !url.Contains("youtube.com/watch")) continue;

                    // Avoid duplicates
                    if (candidates.Any(c => c.Url == url)) continue;

                    var videoId = ExtractVideoId(url);
                    if (videoId is null) continue;

                    var score = CalculateScore(url, title, snippet, context);
                    var matchedStructures = FindMatchedStructures(title, snippet, context.TargetStructures);
                    var confidence = score >= 60 ? "high" : score >= 35 ? "medium" : "low";

                    candidates.Add(new VideoCandidate(
                        VideoId: videoId,
                        Url: url,
                        Title: title,
                        Snippet: snippet,
                        MatchScore: score,
                        Confidence: confidence,
                        Source: "brave_search",
                        TranscriptAvailable: false,
                        MatchedStructures: matchedStructures
                    ));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Brave search failed for query in video ranking");
            }
        }

        // Filter out negative scores (likely Portuguese content) and sort by score
        return candidates
            .Where(c => c.MatchScore > 0)
            .OrderByDescending(c => c.MatchScore)
            .ToList();
    }

    private static List<string> GenerateQueries(VideoSearchContext context)
    {
        var queries = new List<string>();

        // Use English terms for the profession/interest
        var interestEn = context.Interest?.ToLowerInvariant() switch
        {
            "mecânica" or "mecânico" => "mechanic auto repair",
            "programação" or "programador" => "software developer programming",
            "enfermagem" or "enfermeiro" or "enfermeira" => "nurse healthcare",
            "direito" or "advogado" => "lawyer legal",
            "cozinha" or "cozinheiro" or "chef" => "chef cooking kitchen",
            "motorista" => "driver driving",
            "vendas" or "vendedor" => "sales customer service",
            "construção" or "pedreiro" => "construction worker",
            "limpeza" => "cleaning housekeeping",
            "restaurante" or "garçom" => "restaurant waiter",
            _ => context.Interest ?? ""
        };

        // Extract key English words from target structures
        var structureKeywords = string.Join(" ", context.TargetStructures
            .Take(2)
            .SelectMany(s => s.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Where(w => w.Length > 3 && !IsPortugueseWord(w))
            .Distinct()
            .Take(4));

        // Query 1: YouTube videos + English profession + structure keywords
        queries.Add($"site:youtube.com/watch {interestEn} English conversation {structureKeywords}");

        // Query 2: English for beginners + profession
        queries.Add($"site:youtube.com/watch English for beginners {interestEn}");

        // Query 3: ESL/English learning + profession context
        queries.Add($"site:youtube.com/watch learn English {interestEn} vocabulary");

        return queries;
    }

    private static bool IsPortugueseWord(string word)
    {
        var ptWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "como", "para", "você", "que", "não", "uma", "com", "por", "mais",
            "seu", "sua", "isso", "este", "esta", "aqui", "muito", "também"
        };
        return ptWords.Contains(word);
    }

    private static int CalculateScore(string url, string title, string? snippet, VideoSearchContext context)
    {
        var score = 0;
        var titleLower = title.ToLowerInvariant();
        var snippetLower = snippet?.ToLowerInvariant() ?? "";
        var combined = titleLower + " " + snippetLower;

        // ── PENALTY: Portuguese content (-50) ──
        var ptIndicators = new[] { "português", "em português", "aula de inglês", "tradução",
            "como falar", "inglês para brasileiros", "aprenda inglês", "dicas de inglês",
            "como dizer", "como se diz", "significado", "pronúncia em" };
        if (ptIndicators.Any(pt => combined.Contains(pt)))
            score -= 50;

        // ── BONUS: English content (+30) ──
        var enIndicators = new[] { "english", "conversation", "learn english", "beginner english",
            "esl", "english for", "speak english", "english vocabulary", "english lesson" };
        if (enIndicators.Any(en => combined.Contains(en)))
            score += 30;

        // +30 if title/snippet contains words from target_structures
        foreach (var structure in context.TargetStructures)
        {
            var words = structure.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 3).ToList();
            var matchCount = words.Count(w => combined.Contains(w));
            if (matchCount >= 2)
            {
                score += 30;
                break;
            }
        }

        // +25 if contains profession/interest (in English)
        if (context.Interest is not null)
        {
            var interestLower = context.Interest.ToLowerInvariant();
            if (combined.Contains(interestLower))
                score += 25;
            // Also check English equivalents
            var enTerms = new[] { "mechanic", "repair", "engine", "nurse", "doctor", "lawyer",
                "developer", "programmer", "driver", "chef", "cook", "waiter", "construction" };
            if (enTerms.Any(t => combined.Contains(t)))
                score += 15;
        }

        // +20 if URL is a regular YouTube video (embeddable)
        if (url.Contains("youtube.com/watch"))
            score += 20;

        // +10 if URL is a Short (less likely to allow embed)
        if (url.Contains("youtube.com/shorts"))
            score += 10;

        // +15 bonus for known ESL channels that allow embed
        var trustedChannels = new[] { "english with", "speak english", "english addict",
            "rachel's english", "english coach", "engvid", "english lessons" };
        if (trustedChannels.Any(ch => combined.Contains(ch)))
            score += 15;

        // +10 if matches beginner/A1/A2 keywords
        if (combined.Contains("beginner") || combined.Contains("basic") || combined.Contains("easy") ||
            combined.Contains("a1") || combined.Contains("a2") || combined.Contains("simple"))
            score += 10;

        // -30 if seems like a long lesson/course
        if (combined.Contains("full course") || combined.Contains("complete lesson") ||
            combined.Contains("1 hour") || combined.Contains("2 hours"))
            score -= 30;

        // -20 if no clear relation and not a Short
        if (score <= 0 && !url.Contains("youtube.com/shorts"))
            score -= 20;

        return score; // Can be negative (will be filtered out)
    }

    private static List<string> FindMatchedStructures(string title, string? snippet, List<string> structures)
    {
        var combined = (title + " " + (snippet ?? "")).ToLowerInvariant();
        return structures
            .Where(s =>
            {
                var words = s.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => w.Length > 3).ToList();
                return words.Count > 0 && words.Count(w => combined.Contains(w)) >= 2;
            })
            .ToList();
    }

    private static string? ExtractVideoId(string url)
    {
        // youtube.com/shorts/VIDEO_ID
        if (url.Contains("youtube.com/shorts/"))
        {
            var parts = url.Split("youtube.com/shorts/");
            if (parts.Length > 1)
            {
                var id = parts[1].Split('?')[0].Split('/')[0];
                return string.IsNullOrEmpty(id) ? null : id;
            }
        }

        // youtube.com/watch?v=VIDEO_ID
        if (url.Contains("youtube.com/watch"))
        {
            var uri = Uri.TryCreate(url, UriKind.Absolute, out var u) ? u : null;
            if (uri is null) return null;
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            return query["v"];
        }

        return null;
    }
}
