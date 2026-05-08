using Congnex.Domain.Common;
using Congnex.Domain.Enums;

namespace Congnex.Domain.Entities;

public class Subscription : Entity
{
    public Guid UserId { get; set; }

    // Stripe identifiers
    public string StripeCustomerId { get; set; } = string.Empty;
    public string? StripeSubscriptionId { get; set; }
    public string? StripePriceId { get; set; }

    // Status mirrors Stripe exactly
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    // Cancellation — user cancelled but access continues until period end
    public bool CancelAtPeriodEnd { get; set; }
    public DateTime? CancelAt { get; set; }       // when access actually ends
    public DateTime? CanceledAt { get; set; }     // when user triggered cancellation

    // Billing period
    public DateTime? CurrentPeriodStart { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }

    // Navigation
    public User User { get; set; } = null!;

    // ── Domain helpers ──────────────────────────────────────────────────────

    /// <summary>Returns true while the user has active Super access.
    /// A canceled-but-not-yet-expired subscription keeps Status = Active,
    /// so this correctly returns true until the period ends.</summary>
    public bool HasActiveAccess() =>
        Status == SubscriptionStatus.Active || Status == SubscriptionStatus.Trialing;

    /// <summary>Apply a Stripe subscription.updated event payload.</summary>
    public void ApplyStripeUpdate(
        string stripeStatus,
        bool cancelAtPeriodEnd,
        DateTime? cancelAt,
        DateTime? canceledAt,
        DateTime? periodStart,
        DateTime? periodEnd,
        string? priceId)
    {
        Status = ParseStatus(stripeStatus);
        CancelAtPeriodEnd = cancelAtPeriodEnd;
        CancelAt = cancelAt;
        CanceledAt = canceledAt;
        CurrentPeriodStart = periodStart;
        CurrentPeriodEnd = periodEnd;
        if (priceId is not null) StripePriceId = priceId;
        UpdatedAt = DateTime.UtcNow;
    }

    public static SubscriptionStatus ParseStatus(string raw) => raw switch
    {
        "trialing"   => SubscriptionStatus.Trialing,
        "active"     => SubscriptionStatus.Active,
        "past_due"   => SubscriptionStatus.PastDue,
        "canceled"   => SubscriptionStatus.Canceled,
        "unpaid"     => SubscriptionStatus.Unpaid,
        "incomplete" => SubscriptionStatus.Incomplete,
        _            => SubscriptionStatus.Incomplete
    };
}
