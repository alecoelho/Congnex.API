using Congnex.Application.Interfaces;
using Congnex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Application.Payments.Commands;

public record CancelSubscriptionResult(bool CancelAtPeriodEnd, DateTime? CancelAt);

public record CancelSubscriptionCommand(Guid UserId) : IRequest<CancelSubscriptionResult>;

public sealed class CancelSubscriptionCommandHandler(
    ICongnexDbContext db,
    IStripeService stripe) : IRequestHandler<CancelSubscriptionCommand, CancelSubscriptionResult>
{
    public async Task<CancelSubscriptionResult> Handle(CancelSubscriptionCommand req, CancellationToken ct)
    {
        var sub = await db.Subscriptions.FirstOrDefaultAsync(
            s => s.UserId == req.UserId && s.Status == SubscriptionStatus.Active, ct)
            ?? throw new KeyNotFoundException("No active subscription found.");

        if (sub.CancelAtPeriodEnd)
            throw new InvalidOperationException("Subscription is already pending cancellation.");

        if (sub.StripeSubscriptionId is null)
            throw new InvalidOperationException("No Stripe subscription ID.");

        await stripe.CancelAtPeriodEndAsync(sub.StripeSubscriptionId);

        sub.CancelAtPeriodEnd = true;
        sub.CancelAt          = sub.CurrentPeriodEnd;
        sub.CanceledAt        = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return new CancelSubscriptionResult(true, sub.CancelAt);
    }
}
