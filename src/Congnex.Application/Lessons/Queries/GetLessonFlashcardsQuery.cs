using Congnex.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

        var flashcards = new List<FlashcardDto>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // First, try to get flashcards from VideoLearningItems linked to this lesson's videos
        var learningItems = await db.VideoLearningItems
            .Where(i => i.Video.LessonId == req.LessonId)
            .ToListAsync(ct);

        foreach (var item in learningItems)
        {
            if (string.IsNullOrEmpty(item.TextEn) || !seen.Add(item.TextEn)) continue;
            flashcards.Add(new FlashcardDto(item.TextEn, item.TextPt ?? item.TextEn, null));
        }

        // Also extract from questions that have a correct answer (word/translation pairs)
        var questions = await db.Questions
            .Where(q => q.LessonId == req.LessonId && q.CorrectAnswer != null)
            .OrderBy(q => q.OrderIndex)
            .ToListAsync(ct);

        foreach (var q in questions)
        {
            var word = q.QuestionText;
            var translation = q.CorrectAnswer ?? "";

            if (string.IsNullOrEmpty(word) || !seen.Add(word)) continue;
            flashcards.Add(new FlashcardDto(word, translation, null));
        }

        return flashcards;
    }
}
