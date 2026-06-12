using Congnex.Application.Interfaces;
using Congnex.Application.Review.Dtos;
using Congnex.Domain.Enums;
using Congnex.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Application.Review.Commands;

public record SubmitReviewCommand(Guid UserId, Guid ReviewItemId, int Rating) : IRequest<ReviewResultDto>;

public sealed class SubmitReviewCommandHandler(ICongnexDbContext db, FsrsService fsrs)
    : IRequestHandler<SubmitReviewCommand, ReviewResultDto>
{
    public async Task<ReviewResultDto> Handle(SubmitReviewCommand req, CancellationToken ct)
    {
        if (req.Rating is < 1 or > 4)
            throw new ArgumentException("Rating must be 1-4 (Again/Hard/Good/Easy).");

        var item = await db.ReviewItems.FirstOrDefaultAsync(
            r => r.Id == req.ReviewItemId && r.UserId == req.UserId, ct)
            ?? throw new KeyNotFoundException("Review item not found.");

        var rating = (FsrsRating)req.Rating;
        var result = fsrs.Schedule(
            item.Stability,
            item.Difficulty,
            item.Reps,
            item.Lapses,
            item.State,
            rating,
            DateTime.UtcNow);

        item.Stability    = result.Stability;
        item.Difficulty   = result.Difficulty;
        item.Reps         = result.Reps;
        item.Lapses       = result.Lapses;
        item.State        = result.State;
        item.DueDate      = result.DueDate;
        item.LastReviewAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return new ReviewResultDto(
            ReviewItemId: item.Id,
            NextDueDate:  result.DueDate,
            Stability:    result.Stability,
            Difficulty:   result.Difficulty,
            State:        result.State.ToString(),
            Reps:         result.Reps,
            Lapses:       result.Lapses);
    }
}
