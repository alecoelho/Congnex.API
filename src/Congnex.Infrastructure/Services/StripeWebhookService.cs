using Congnex.Application.Interfaces;
using Congnex.Application.Settings;
using Congnex.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace Congnex.Infrastructure.Services;

public sealed class StripeWebhookService(
    ICongnexDbContext db,
    IOptions<StripeSettings> stripeOpts) : IStripeWebhookService
{
    private readonly StripeSettings _cfg = stripeOpts.Value;

    public async Task<bool> HandleAsync(string json, string? signature, CancellationToken ct = default)
    {
        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, signature, _cfg.WebhookSecret);
        }
        catch (StripeException)
        {
            return false;
        }

        switch (stripeEvent.Type)
        {
            case EventTypes.CheckoutSessionCompleted:
                await HandleCheckoutCompleted(stripeEvent.Data.Object as Session, ct);
                break;

            case EventTypes.CustomerSubscriptionUpdated:
                await HandleSubscriptionUpdated(stripeEvent.Data.Object as Subscription, ct);
                break;

            case EventTypes.CustomerSubscriptionDeleted:
                await HandleSubscriptionDeleted(stripeEvent.Data.Object as Subscription, ct);
                break;

            case EventTypes.InvoicePaymentFailed:
                await HandlePaymentFailed(stripeEvent.Data.Object as Invoice, ct);
                break;

            case EventTypes.InvoicePaymentSucceeded:
                await HandlePaymentSucceeded(stripeEvent.Data.Object as Invoice, ct);
                break;
        }

        return true;
    }

    private async Task HandleCheckoutCompleted(Session? session, CancellationToken ct)
    {
        if (session is null) return;
        if (!Guid.TryParse(session.Metadata.GetValueOrDefault("userId"), out var userId)) return;

        var user = await db.Users.FindAsync([userId], ct);
        if (user is null) return;

        var stripeSub = await new SubscriptionService().GetAsync(session.SubscriptionId, cancellationToken: ct);

        var sub = new Domain.Entities.Subscription
        {
            UserId               = userId,
            StripeCustomerId     = session.CustomerId,
            StripeSubscriptionId = stripeSub.Id,
            StripePriceId        = _cfg.SuperPlanPriceId,
            Status               = Domain.Entities.Subscription.ParseStatus(stripeSub.Status),
            CurrentPeriodStart   = stripeSub.CurrentPeriodStart,
            CurrentPeriodEnd     = stripeSub.CurrentPeriodEnd
        };

        db.Subscriptions.Add(sub);
        user.Plan = UserPlan.Super;
        await db.SaveChangesAsync(ct);
    }

    private async Task HandleSubscriptionUpdated(Subscription? stripeSub, CancellationToken ct)
    {
        if (stripeSub is null) return;

        var sub = await db.Subscriptions
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSub.Id, ct);
        if (sub is null) return;

        sub.ApplyStripeUpdate(
            stripeStatus:      stripeSub.Status,
            cancelAtPeriodEnd: stripeSub.CancelAtPeriodEnd,
            cancelAt:          stripeSub.CancelAt,
            canceledAt:        stripeSub.CanceledAt,
            periodStart:       stripeSub.CurrentPeriodStart,
            periodEnd:         stripeSub.CurrentPeriodEnd,
            priceId:           stripeSub.Items.Data.FirstOrDefault()?.Price.Id);

        var user = await db.Users.FindAsync([sub.UserId], ct);
        if (user is not null)
            user.Plan = sub.HasActiveAccess() ? UserPlan.Super : UserPlan.Free;

        await db.SaveChangesAsync(ct);
    }

    private async Task HandleSubscriptionDeleted(Subscription? stripeSub, CancellationToken ct)
    {
        if (stripeSub is null) return;

        var sub = await db.Subscriptions
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSub.Id, ct);
        if (sub is null) return;

        sub.Status    = SubscriptionStatus.Canceled;
        sub.UpdatedAt = DateTime.UtcNow;

        var user = await db.Users.FindAsync([sub.UserId], ct);
        if (user is not null) user.Plan = UserPlan.Free;

        await db.SaveChangesAsync(ct);
    }

    private async Task HandlePaymentFailed(Invoice? invoice, CancellationToken ct)
    {
        if (invoice?.SubscriptionId is null) return;

        var sub = await db.Subscriptions
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == invoice.SubscriptionId, ct);
        if (sub is null) return;

        sub.Status    = SubscriptionStatus.PastDue;
        sub.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private async Task HandlePaymentSucceeded(Invoice? invoice, CancellationToken ct)
    {
        if (invoice?.SubscriptionId is null) return;

        var stripeSub = await new SubscriptionService().GetAsync(invoice.SubscriptionId, cancellationToken: ct);
        var sub = await db.Subscriptions
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == invoice.SubscriptionId, ct);
        if (sub is null) return;

        sub.Status             = SubscriptionStatus.Active;
        sub.CurrentPeriodStart = stripeSub.CurrentPeriodStart;
        sub.CurrentPeriodEnd   = stripeSub.CurrentPeriodEnd;
        sub.UpdatedAt          = DateTime.UtcNow;

        var user = await db.Users.FindAsync([sub.UserId], ct);
        if (user is not null) user.Plan = UserPlan.Super;

        await db.SaveChangesAsync(ct);
    }
}
