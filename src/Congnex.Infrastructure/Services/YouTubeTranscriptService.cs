using System.Text;
using System.Text.RegularExpressions;
using Congnex.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Congnex.Infrastructure.Services;

public class YouTubeTranscriptService : IYouTubeTranscriptService
{
    private readonly HttpClient _http;
    private readonly ILogger<YouTubeTranscriptService> _logger;

    public YouTubeTranscriptService(IHttpClientFactory factory, ILogger<YouTubeTranscriptService> logger)
    {
        _http   = factory.CreateClient("youtube");
        _logger = logger;
    }

    public async Task<string?> GetTranscriptAsync(string videoUrl, CancellationToken ct = default)
    {
        try
        {
            var videoId = ExtractVideoId(videoUrl);
            if (videoId is null) return null;

            // Try the timedtext API first (fast path, works for many videos)
            var transcript = await TryTimedTextApiAsync(videoId, ct);
            if (transcript is { Length: > 0 }) return transcript;

            // Fallback: scrape caption track URL from the video page
            return await ScrapeTranscriptAsync(videoId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not fetch transcript for {VideoUrl}", videoUrl);
            return null;
        }
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private async Task<string?> TryTimedTextApiAsync(string videoId, CancellationToken ct)
    {
        try
        {
            var url = $"https://www.youtube.com/api/timedtext?v={videoId}&lang=en";
            var response = await _http.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode) return null;

            var xml = await response.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(xml) || !xml.Contains("<text")) return null;

            return ParseCaptionXml(xml);
        }
        catch
        {
            return null;
        }
    }

    private async Task<string?> ScrapeTranscriptAsync(string videoId, CancellationToken ct)
    {
        try
        {
            var pageHtml = await _http.GetStringAsync(
                $"https://www.youtube.com/watch?v={videoId}", ct);

            // Extract captionTracks[0].baseUrl from ytInitialPlayerResponse
            var match = Regex.Match(pageHtml,
                @"""captionTracks""\s*:\s*\[.*?""baseUrl""\s*:\s*""([^""]+)""",
                RegexOptions.Singleline);

            if (!match.Success) return null;

            var captionUrl = match.Groups[1].Value
                .Replace(@"&", "&")
                .Replace(@"\\u0026", "&");

            var xml = await _http.GetStringAsync(captionUrl, ct);
            return ParseCaptionXml(xml);
        }
        catch
        {
            return null;
        }
    }

    private static string? ExtractVideoId(string url)
    {
        if (url.Contains("youtube.com/watch"))
        {
            var m = Regex.Match(url, @"[?&]v=([^&]+)");
            return m.Success ? m.Groups[1].Value : null;
        }
        if (url.Contains("youtu.be/"))
        {
            var parts = url.Split("youtu.be/");
            return parts.Length > 1 ? parts[1].Split('?', '#')[0] : null;
        }
        if (Regex.IsMatch(url, @"^[A-Za-z0-9_-]{11}$"))
            return url;
        return null;
    }

    private static string ParseCaptionXml(string xml)
    {
        var sb = new StringBuilder();
        foreach (Match m in Regex.Matches(xml, @"<text[^>]*>([^<]+)</text>"))
        {
            var text = DecodeHtmlEntities(m.Groups[1].Value).Trim();
            if (text.Length > 0) sb.Append(text).Append(' ');
        }
        return sb.ToString().Trim();
    }

    private static string DecodeHtmlEntities(string text) =>
        text.Replace("&amp;", "&")
            .Replace("&lt;",  "<")
            .Replace("&gt;",  ">")
            .Replace("&quot;", "\"")
            .Replace("&#39;", "'")
            .Replace("&apos;", "'");
}
