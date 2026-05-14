using Congnex.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Application.Lessons.Queries;

public record DueFlashcardDto(
    string   Word,
    string   Translation,
    string?  Emoji,
    Guid     LessonId,
    int      ReviewCount,
    int      CorrectCount,
    DateTime LastReviewedAt);

public record GetDueFlashcardsQuery(Guid UserId, int Limit = 20) : IRequest<List<DueFlashcardDto>>;

public sealed class GetDueFlashcardsQueryHandler(ICongnexDbContext db)
    : IRequestHandler<GetDueFlashcardsQuery, List<DueFlashcardDto>>
{
    public async Task<List<DueFlashcardDto>> Handle(GetDueFlashcardsQuery req, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var dueReviews = await db.FlashcardReviews
            .Where(r => r.UserId == req.UserId && r.NextReviewAt <= now)
            .OrderBy(r => r.NextReviewAt)
            .Take(req.Limit)
            .ToListAsync(ct);

        // For now, translation = word (we store only the English word)
        // In the future, we could join with questions to get the translation
        return dueReviews.Select(r => new DueFlashcardDto(
            Word:           r.Word,
            Translation:    r.Word, // Placeholder — frontend can map from lesson data
            Emoji:          null,
            LessonId:       r.LessonId,
            ReviewCount:    r.ReviewCount,
            CorrectCount:   r.CorrectCount,
            LastReviewedAt: r.LastReviewedAt
        )).ToList();
    }
}
