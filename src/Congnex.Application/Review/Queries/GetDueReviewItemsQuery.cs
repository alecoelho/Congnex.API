using Congnex.Application.Common;
using Congnex.Application.Review.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Application.Review.Queries;

public record GetDueReviewItemsQuery(Guid UserId, int Limit = 20) : IRequest<List<ReviewItemDto>>;

public sealed class GetDueReviewItemsQueryHandler(ICongnexDbContext db)
    : IRequestHandler<GetDueReviewItemsQuery, List<ReviewItemDto>>
{
    public async Task<List<ReviewItemDto>> Handle(GetDueReviewItemsQuery req, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var items = await db.ReviewItems
            .Where(r => r.UserId == req.UserId && r.DueDate <= now)
            .OrderBy(r => r.DueDate)
            .Take(req.Limit)
            .Include(r => r.Question)
            .ToListAsync(ct);

        return items.Select(r =>
        {
            var prompt = r.Question?.Prompt ?? "";
            var questionText = r.Question?.QuestionText ?? "";
            var correctAnswer = r.Question?.CorrectAnswer ?? "";

            return new ReviewItemDto(
                Id:             r.Id,
                Source:         r.Source,
                Prompt:         prompt,
                Options:        "[]",
                CorrectAnswers: string.IsNullOrEmpty(correctAnswer) ? [] : [correctAnswer],
                DueDate:        r.DueDate,
                Reps:           r.Reps,
                State:          r.State.ToString());
        }).ToList();
    }
}
