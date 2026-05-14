using Congnex.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Congnex.Application.Lessons.Queries;

public record FlashcardDto(
    string Word,
    string Translation,
    string? Emoji);

public record GetLessonFlashcardsQuery(Guid LessonId) : IRequest<List<FlashcardDto>>;

public sealed class GetLessonFlashcardsQueryHandler(ICongnexDbContext db)
    : IRequestHandler<GetLessonFlashcardsQuery, List<FlashcardDto>>
{
    public async Task<List<FlashcardDto>> Handle(GetLessonFlashcardsQuery req, CancellationToken ct)
    {
        var exists = await db.Lessons.AnyAsync(l => l.Id == req.LessonId, ct);
        if (!exists) throw new KeyNotFoundException("Lesson not found.");

        var questions = await db.Questions
            .Where(q => q.LessonId == req.LessonId)
            .OrderBy(q => q.OrderIndex)
            .ToListAsync(ct);

        var flashcards = new List<FlashcardDto>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var q in questions)
        {
            if (q.Options is null) continue;

            try
            {
                using var doc = JsonDocument.Parse(q.Options);
                var root = doc.RootElement;

                switch (q.Type.ToString())
                {
                    case "imageWordChoice":
                        ExtractImageWordChoice(root, flashcards, seen);
                        break;
                    case "multipleChoice":
                        ExtractMultipleChoice(root, flashcards, seen);
                        break;
                    case "translate":
                        ExtractTranslate(root, flashcards, seen);
                        break;
                    case "listeningWordSelection":
                        ExtractListening(root, flashcards, seen);
                        break;
                    case "listenAndTap":
                        ExtractListening(root, flashcards, seen);
                        break;
                }
            }
            catch
            {
                // Skip malformed JSON
            }
        }

        return flashcards;
    }

    private static void ExtractImageWordChoice(JsonElement root, List<FlashcardDto> cards, HashSet<string> seen)
    {
        if (!root.TryGetProperty("targetWord", out var word)) return;
        var w = word.GetString() ?? "";
        if (string.IsNullOrEmpty(w) || !seen.Add(w)) return;

        string? emoji = null;
        if (root.TryGetProperty("options", out var options))
        {
            foreach (var opt in options.EnumerateArray())
            {
                if (opt.TryGetProperty("label", out var label) &&
                    label.GetString() == w &&
                    opt.TryGetProperty("emoji", out var em))
                {
                    emoji = em.GetString();
                    break;
                }
            }
        }

        cards.Add(new FlashcardDto(w, w, emoji));
    }

    private static void ExtractMultipleChoice(JsonElement root, List<FlashcardDto> cards, HashSet<string> seen)
    {
        if (!root.TryGetProperty("audioText", out var audio)) return;
        if (!root.TryGetProperty("correctAnswer", out var answer)) return;

        var w = audio.GetString() ?? "";
        var t = answer.GetString() ?? "";
        if (string.IsNullOrEmpty(w) || !seen.Add(w)) return;

        cards.Add(new FlashcardDto(w, t, null));
    }

    private static void ExtractTranslate(JsonElement root, List<FlashcardDto> cards, HashSet<string> seen)
    {
        if (!root.TryGetProperty("audioText", out var audio)) return;
        if (!root.TryGetProperty("correctAnswer", out var answer)) return;

        var w = audio.GetString() ?? "";
        var t = answer.GetString() ?? "";
        if (string.IsNullOrEmpty(w) || !seen.Add(w)) return;

        cards.Add(new FlashcardDto(w, t, null));
    }

    private static void ExtractListening(JsonElement root, List<FlashcardDto> cards, HashSet<string> seen)
    {
        string? w = null;
        if (root.TryGetProperty("audioWord", out var aw))
            w = aw.GetString();
        else if (root.TryGetProperty("audioText", out var at))
            w = at.GetString();

        if (string.IsNullOrEmpty(w) || !seen.Add(w)) return;

        var t = w; // Same word (listening exercises)
        if (root.TryGetProperty("correctAnswer", out var ca))
            t = ca.GetString() ?? w;

        cards.Add(new FlashcardDto(w, t, null));
    }
}
