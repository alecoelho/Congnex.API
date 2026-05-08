using Congnex.Application.Interfaces;
using Congnex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Application.Payments.Commands;

public record ReactivateSubscriptionCommand(Guid UserId) : IRequest;

public sealed class ReactivateSubscriptionCommandHandler(
    ICongnexDbContext db,
    IStripeService stripe) : IRequestHandler<ReactivateSubscriptionCommand>
{
    public async Task Handle(ReactivateSubscriptionCommand req, CancellationToken ct)
    {
        var sub = await db.Subscriptions.FirstOrDefaultAsync(
            s => s.UserId == req.UserId && s.Status == SubscriptionStatus.Active, ct)
            ?? throw new KeyNotFoundException("No active subscription found.");

        if (!sub.CancelAtPeriodEnd)
            throw new InvalidOperationException("Subscription is not pending cancellation.");

        if (sub.StripeSubscriptionId is null)
            throw new InvalidOperationException("No Stripe subscription ID.");

        await stripe.ReactivateAsync(sub.StripeSubscriptionId);

        sub.CancelAtPeriodEnd = false;
        sub.CancelAt          = null;
        sub.CanceledAt        = null;
        await db.SaveChangesAsync(ct);
    }
}
