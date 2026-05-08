using Congnex.Application.Interfaces;
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
            .Include(r => r.AiQuestion)
            .ToListAsync(ct);

        return items.Select(r =>
        {
            var prompt  = r.Source == "lesson"
                ? r.Question?.Prompt ?? ""
                : r.AiQuestion?.Prompt ?? "";

            var options = r.Source == "lesson"
                ? r.Question?.Options ?? "[]"
                : r.AiQuestion?.Options ?? "[]";

            var correctAnswers = r.Source == "lesson"
                ? r.Question?.CorrectAnswers ?? []
                : [r.AiQuestion?.CorrectIndex.ToString() ?? "0"];

            return new ReviewItemDto(
                Id:             r.Id,
                Source:         r.Source,
                Prompt:         prompt,
                Options:        options,
                CorrectAnswers: correctAnswers,
                DueDate:        r.DueDate,
                Reps:           r.Reps,
                State:          r.State.ToString());
        }).ToList();
    }
}
