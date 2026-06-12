namespace Congnex.Application.Payments.Dtos;

public record SubscriptionDto(
    string Plan,
    string? Status,
    bool CancelAtPeriodEnd,
    DateTime? CancelAt,
    DateTime? CurrentPeriodEnd,
    DateTime? RenewsAt);
