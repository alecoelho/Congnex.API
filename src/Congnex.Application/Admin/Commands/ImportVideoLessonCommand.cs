using Congnex.Application.Interfaces;
using Congnex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainUnit = Congnex.Domain.Entities.Unit;

namespace Congnex.Application.Admin.Commands;

// ── DTOs ────────────────────────────────────────────────────────────────────

public record TranscriptLineDto(string StartTime, string Text);

public record ImportVideoLessonRequest(
    string Title,
    string? Description,
    string UnitTitle,
    string VideoUrl,
    int XpReward,
    string Level,
    string? Domain,
    List<TranscriptLineDto> Transcript);

public record ImportVideoLessonResult(Guid LessonId, string Title, int QuestionsMatched);

// ── Command ──────────────────────────────────────────────────────────────────

public record ImportVideoLessonCommand(ImportVideoLessonRequest Request)
    : IRequest<ImportVideoLessonResult>;

public sealed class ImportVideoLessonCommandHandler(
    ICongnexDbContext db,
    IQuestionMatchingService matchingService)
    : IRequestHandler<ImportVideoLessonCommand, ImportVideoLessonResult>
{
    public async Task<ImportVideoLessonResult> Handle(
        ImportVideoLessonCommand cmd, CancellationToken ct)
    {
        var req = cmd.Request;

        // ── 1. Unit: reutiliza existente ou cria novo ─────────────────────
        var unit = await db.Units
            .FirstOrDefaultAsync(u => u.Title == req.UnitTitle, ct);

        if (unit is null)
        {
            var maxOrder = await db.Units.AnyAsync(ct)
                ? await db.Units.MaxAsync(u => u.OrderIndex, ct)
                : 0;

            unit = new DomainUnit
            {
                Title       = req.UnitTitle,
                Description = req.UnitTitle,
                OrderIndex  = maxOrder + 1,
                LanguageCode = "en"
            };
            db.Units.Add(unit);
            await db.SaveChangesAsync(ct);
        }

        // ── 2. Lesson ─────────────────────────────────────────────────────
        var lessonOrder = await db.Lessons
            .Where(l => l.UnitId == unit.Id)
            .CountAsync(ct) + 1;

        var lesson = new Lesson
        {
            UnitId      = unit.Id,
            Title       = req.Title,
            Description = req.Description,
            OrderIndex  = lessonOrder,
            XpReward    = req.XpReward > 0 ? req.XpReward : 10,
            Level       = NormalizeLevel(req.Level)
        };
        db.Lessons.Add(lesson);
        await db.SaveChangesAsync(ct);

        // ── 3. LessonVideo ────────────────────────────────────────────────
        var youtubeId = ExtractYouTubeId(req.VideoUrl);

        var video = new LessonVideo
        {
            LessonId       = lesson.Id,
            YoutubeVideoId = youtubeId,
            YoutubeUrl     = req.VideoUrl,
            Title          = req.Title,
            Language       = "en"
        };
        db.LessonVideos.Add(video);
        await db.SaveChangesAsync(ct);

        // ── 4. VideoLearningItems (transcrição) ───────────────────────────
        for (int i = 0; i < req.Transcript.Count; i++)
        {
            var line    = req.Transcript[i];
            var start   = ParseTimestamp(line.StartTime);
            var end     = i + 1 < req.Transcript.Count
                            ? ParseTimestamp(req.Transcript[i + 1].StartTime)
                            : start + 7.0;

            db.VideoLearningItems.Add(new VideoLearningItem
            {
                VideoId        = video.Id,
                TextEn         = line.Text,
                ItemType       = "phrase",
                Difficulty     = "easy",
                TimestampStart = start,
                TimestampEnd   = end
            });
        }

        await db.SaveChangesAsync(ct);

        // ── 5. Seleciona questões do banco relacionadas ao vídeo e copia ──
        int matched = 0;
        var normalizedLevel = NormalizeLevel(req.Level);
        if (normalizedLevel is not null)
        {
            var fullTranscript = string.Join(" ", req.Transcript.Select(t => t.Text));
            matched = await matchingService.MatchAndCopyAsync(
                lesson.Id, normalizedLevel, NormalizeDomain(req.Domain), fullTranscript, limit: 60, ct);
        }

        return new ImportVideoLessonResult(lesson.Id, lesson.Title, matched);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string? NormalizeLevel(string? level) =>
        level?.Trim().ToUpperInvariant() switch
        {
            "A1" => "A1", "A2" => "A2",
            "B1" => "B1", "B2" => "B2",
            "C1" => "C1", "C2" => "C2",
            _ => null
        };

    private static readonly HashSet<string> ValidDomains = new(StringComparer.OrdinalIgnoreCase)
    {
        "rotina_diaria", "trabalho", "viagem", "saude", "negocios", "tecnologia",
        "compras", "educacao", "familia_relacionamentos", "alimentacao",
        "cultura_entretenimento", "meio_ambiente"
    };

    // Domínio inválido/vazio → null (busca usa só nível + transcrição)
    private static string? NormalizeDomain(string? domain)
    {
        if (string.IsNullOrWhiteSpace(domain)) return null;
        var d = domain.Trim().ToLowerInvariant().Replace(' ', '_');
        return ValidDomains.Contains(d) ? d : null;
    }

    private static string ExtractYouTubeId(string url)
    {
        // https://www.youtube.com/watch?v=XXXXXXXXXXX
        // https://youtu.be/XXXXXXXXXXX
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            if (uri.Host.Contains("youtu.be"))
                return uri.AbsolutePath.TrimStart('/');

            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var v = query["v"];
            if (!string.IsNullOrEmpty(v)) return v;
        }
        return url; // fallback: retorna a URL inteira
    }

    /// <summary>
    /// Converte "1:05" ou "0:07" ou "1:23:45" em segundos (double).
    /// Ignora prefixos numéricos como "1. 0:07 texto" — extrai só o timestamp.
    /// </summary>
    private static double ParseTimestamp(string raw)
    {
        // Remove prefixo numérico: "2. 0:07 texto" → "0:07 texto"
        var text = System.Text.RegularExpressions.Regex.Replace(raw.Trim(), @"^\d+\.\s*", "");

        // Pega o primeiro token que parece timestamp (x:xx ou x:xx:xx)
        var match = System.Text.RegularExpressions.Regex.Match(text, @"(\d+):(\d+)(?::(\d+))?");
        if (!match.Success) return 0;

        var parts = text.Split(':');
        if (parts.Length == 2 &&
            int.TryParse(parts[0], out var min) &&
            int.TryParse(parts[1].Split(' ')[0], out var sec))
            return min * 60 + sec;

        if (parts.Length == 3 &&
            int.TryParse(parts[0], out var h) &&
            int.TryParse(parts[1], out var m) &&
            int.TryParse(parts[2].Split(' ')[0], out var s))
            return h * 3600 + m * 60 + s;

        return 0;
    }
}
