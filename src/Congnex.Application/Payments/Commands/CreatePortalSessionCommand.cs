using Congnex.Application.Interfaces;
using Congnex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Application.Payments.Commands;

public record CreatePortalSessionCommand(Guid UserId) : IRequest<string>;

public sealed class CreatePortalSessionCommandHandler(
    ICongnexDbContext db,
    IStripeService stripe) : IRequestHandler<CreatePortalSessionCommand, string>
{
    public async Task<string> Handle(CreatePortalSessionCommand req, CancellationToken ct)
    {
        var sub = await db.Subscriptions.FirstOrDefaultAsync(
            s => s.UserId == req.UserId && s.Status == SubscriptionStatus.Active, ct)
            ?? throw new KeyNotFoundException("No active subscription found.");

        return await stripe.CreateCustomerPortalSessionAsync(sub.StripeCustomerId);
    }
}
