using Congnex.Application.Interfaces;
using Congnex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Application.Lessons.Commands;

public record ReviewFlashcardCommand(
    Guid   UserId,
    Guid   LessonId,
    string Word,
    bool   Remembered) : IRequest;

public sealed class ReviewFlashcardCommandHandler(ICongnexDbContext db)
    : IRequestHandler<ReviewFlashcardCommand>
{
    public async Task Handle(ReviewFlashcardCommand req, CancellationToken ct)
    {
        var review = await db.FlashcardReviews
            .FirstOrDefaultAsync(r => r.UserId == req.UserId && r.Word == req.Word, ct);

        if (review is null)
        {
            review = new FlashcardReview
            {
                UserId   = req.UserId,
                LessonId = req.LessonId,
                Word     = req.Word,
            };
            db.FlashcardReviews.Add(review);
        }

        review.Remembered     = req.Remembered;
        review.ReviewCount++;
        review.LastReviewedAt = DateTime.UtcNow;

        if (req.Remembered)
        {
            review.CorrectCount++;
            review.IntervalDays *= 2;
            if (review.IntervalDays < 1) review.IntervalDays = 1;
        }
        else
        {
            review.IntervalDays = 1;
        }

        review.NextReviewAt = DateTime.UtcNow.AddDays(review.IntervalDays);

        await db.SaveChangesAsync(ct);
    }
}
