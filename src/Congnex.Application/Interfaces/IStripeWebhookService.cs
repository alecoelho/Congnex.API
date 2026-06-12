namespace Congnex.Application.Interfaces;

public interface IStripeWebhookService
{
    /// <summary>Validates the Stripe-Signature and processes the event. Returns false if signature is invalid.</summary>
    Task<bool> HandleAsync(string json, string? signature, CancellationToken ct = default);
}
