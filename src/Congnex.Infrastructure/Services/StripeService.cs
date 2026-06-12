using Congnex.Application.Interfaces;
using Congnex.Application.Settings;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace Congnex.Infrastructure.Services;

public sealed class StripeService : IStripeService
{
    private readonly StripeSettings _cfg;

    public StripeService(IOptions<StripeSettings> opts)
    {
        _cfg = opts.Value;
        StripeConfiguration.ApiKey = _cfg.SecretKey;
    }

    public async Task<string> CreateCustomerAsync(string email, string name)
    {
        var svc     = new CustomerService();
        var options = new CustomerCreateOptions { Email = email, Name = name };
        var customer = await svc.CreateAsync(options);
        return customer.Id;
    }

    public async Task<string> CreateCheckoutSessionAsync(
        string stripeCustomerId,
        string userId,
        string successUrl,
        string cancelUrl)
    {
        var svc = new SessionService();
        var options = new SessionCreateOptions
        {
            Customer   = stripeCustomerId,
            Mode       = "subscription",
            LineItems  =
            [
                new SessionLineItemOptions
                {
                    Price    = _cfg.SuperPlanPriceId,
                    Quantity = 1
                }
            ],
            SuccessUrl    = successUrl,
            CancelUrl     = cancelUrl,
            Metadata      = new Dictionary<string, string> { ["userId"] = userId },
            SubscriptionData = new SessionSubscriptionDataOptions
            {
                Metadata = new Dictionary<string, string> { ["userId"] = userId }
            }
        };
        var session = await svc.CreateAsync(options);
        return session.Url;
    }

    public async Task<string> CreateCustomerPortalSessionAsync(string stripeCustomerId)
    {
        var svc = new Stripe.BillingPortal.SessionService();
        var options = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer  = stripeCustomerId,
            ReturnUrl = _cfg.CustomerPortalReturnUrl
        };
        var session = await svc.CreateAsync(options);
        return session.Url;
    }

    /// <summary>Cancel at period end: user keeps access until current period expires.</summary>
    public async Task CancelAtPeriodEndAsync(string stripeSubscriptionId)
    {
        var svc     = new SubscriptionService();
        var options = new SubscriptionUpdateOptions { CancelAtPeriodEnd = true };
        await svc.UpdateAsync(stripeSubscriptionId, options);
    }

    /// <summary>Undo a pending cancellation — subscription renews normally.</summary>
    public async Task ReactivateAsync(string stripeSubscriptionId)
    {
        var svc     = new SubscriptionService();
        var options = new SubscriptionUpdateOptions { CancelAtPeriodEnd = false };
        await svc.UpdateAsync(stripeSubscriptionId, options);
    }
}
