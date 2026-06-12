namespace Congnex.Application.Settings;

public sealed class StripeSettings
{
    public string SecretKey { get; init; } = string.Empty;
    public string WebhookSecret { get; init; } = string.Empty;
    public string PublishableKey { get; init; } = string.Empty;
    public string SuperPlanPriceId { get; init; } = string.Empty;
    public string CustomerPortalReturnUrl { get; init; } = string.Empty;
}
