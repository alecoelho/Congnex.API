namespace Congnex.Application.Interfaces;

public interface IStripeService
{
    Task<string> CreateCheckoutSessionAsync(string stripeCustomerId, string userId, string successUrl, string cancelUrl);
    Task<string> CreateCustomerPortalSessionAsync(string stripeCustomerId);
    Task<string> CreateCustomerAsync(string email, string name);

    /// <summary>Cancel at period end — user keeps access until CurrentPeriodEnd.</summary>
    Task CancelAtPeriodEndAsync(string stripeSubscriptionId);

    /// <summary>Reverse a cancel-at-period-end before it takes effect.</summary>
    Task ReactivateAsync(string stripeSubscriptionId);
}
