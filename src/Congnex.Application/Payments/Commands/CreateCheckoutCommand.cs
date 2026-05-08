using Congnex.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Application.Payments.Commands;

public record CreateCheckoutCommand(Guid UserId, string SuccessUrl, string CancelUrl) : IRequest<string>;

public sealed class CreateCheckoutCommandHandler(
    ICongnexDbContext db,
    IStripeService stripe) : IRequestHandler<CreateCheckoutCommand, string>
{
    public async Task<string> Handle(CreateCheckoutCommand req, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([req.UserId], ct)
            ?? throw new KeyNotFoundException("User not found.");

        var hasSub = await db.Subscriptions.AnyAsync(
            s => s.UserId == req.UserId && s.Status == Domain.Enums.SubscriptionStatus.Active, ct);
        if (hasSub)
            throw new InvalidOperationException("User already has an active subscription.");

        var customerId = await stripe.CreateCustomerAsync(user.Email, user.FullName);
        return await stripe.CreateCheckoutSessionAsync(
            customerId, req.UserId.ToString(), req.SuccessUrl, req.CancelUrl);
    }
}
