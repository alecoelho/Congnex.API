using Congnex.Application.Interfaces;
using Congnex.Application.Payments.Dtos;
using Congnex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Application.Payments.Queries;

public record GetSubscriptionQuery(Guid UserId) : IRequest<SubscriptionDto>;

public sealed class GetSubscriptionQueryHandler(ICongnexDbContext db)
    : IRequestHandler<GetSubscriptionQuery, SubscriptionDto>
{
    public async Task<SubscriptionDto> Handle(GetSubscriptionQuery req, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([req.UserId], ct);
        var sub  = await db.Subscriptions
            .Where(s => s.UserId == req.UserId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(ct);

        return new SubscriptionDto(
            Plan:             user?.Plan.ToString() ?? "Free",
            Status:           sub?.Status.ToString(),
            CancelAtPeriodEnd: sub?.CancelAtPeriodEnd ?? false,
            CancelAt:         sub?.CancelAt,
            CurrentPeriodEnd: sub?.CurrentPeriodEnd,
            RenewsAt:         sub is { CancelAtPeriodEnd: false, Status: SubscriptionStatus.Active }
                                  ? sub.CurrentPeriodEnd : null);
    }
}
