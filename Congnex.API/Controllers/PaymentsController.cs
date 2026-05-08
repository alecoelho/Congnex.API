using System.Security.Claims;
using Congnex.Application.Common;
using Congnex.Application.Interfaces;
using Congnex.Application.Payments.Commands;
using Congnex.Application.Payments.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Congnex.API.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController(IMediator mediator, IStripeWebhookService webhookService) : ControllerBase
{
    // ── POST /api/payments/checkout ─────────────────────────────────────────
    [Authorize]
    [HttpPost("checkout")]
    public async Task<IActionResult> CreateCheckout(CancellationToken ct)
    {
        try
        {
            var url = await mediator.Send(new CreateCheckoutCommand(
                UserId:     GetUserId(),
                SuccessUrl: $"{Request.Scheme}://{Request.Host}/payments/success",
                CancelUrl:  $"{Request.Scheme}://{Request.Host}/payments/cancel"), ct);

            return Ok(ApiResponse<object>.Ok(new { checkoutUrl = url }));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse.Fail(ex.Message));
        }
    }

    // ── POST /api/payments/cancel ───────────────────────────────────────────
    [Authorize]
    [HttpPost("cancel")]
    public async Task<IActionResult> Cancel(CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new CancelSubscriptionCommand(GetUserId()), ct);
            return Ok(ApiResponse<object>.Ok(new
            {
                cancelAtPeriodEnd = result.CancelAtPeriodEnd,
                cancelAt          = result.CancelAt,
                message           = "Subscription will cancel at period end. Access continues until then."
            }));
        }
        catch (KeyNotFoundException ex)    { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return Conflict(ApiResponse.Fail(ex.Message)); }
    }

    // ── POST /api/payments/reactivate ───────────────────────────────────────
    [Authorize]
    [HttpPost("reactivate")]
    public async Task<IActionResult> Reactivate(CancellationToken ct)
    {
        try
        {
            await mediator.Send(new ReactivateSubscriptionCommand(GetUserId()), ct);
            return Ok(ApiResponse<object>.Ok(new
            {
                cancelAtPeriodEnd = false,
                message           = "Subscription reactivated. It will renew automatically."
            }));
        }
        catch (KeyNotFoundException ex)    { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return Conflict(ApiResponse.Fail(ex.Message)); }
    }

    // ── GET /api/payments/subscription ─────────────────────────────────────
    [Authorize]
    [HttpGet("subscription")]
    public async Task<IActionResult> GetSubscription(CancellationToken ct)
    {
        var dto = await mediator.Send(new GetSubscriptionQuery(GetUserId()), ct);
        return Ok(ApiResponse<object>.Ok(new
        {
            plan              = dto.Plan,
            status            = dto.Status,
            cancelAtPeriodEnd = dto.CancelAtPeriodEnd,
            cancelAt          = dto.CancelAt,
            currentPeriodEnd  = dto.CurrentPeriodEnd,
            renewsAt          = dto.RenewsAt
        }));
    }

    // ── POST /api/payments/portal ───────────────────────────────────────────
    [Authorize]
    [HttpPost("portal")]
    public async Task<IActionResult> CreatePortalSession(CancellationToken ct)
    {
        try
        {
            var url = await mediator.Send(new CreatePortalSessionCommand(GetUserId()), ct);
            return Ok(ApiResponse<object>.Ok(new { portalUrl = url }));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
    }

    // ── POST /api/payments/webhook ──────────────────────────────────────────
    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook(CancellationToken ct)
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync(ct);
        var sig  = Request.Headers["Stripe-Signature"].FirstOrDefault();

        var handled = await webhookService.HandleAsync(json, sig, ct);
        return handled ? Ok() : BadRequest("Invalid Stripe signature.");
    }

    // ── Helpers ─────────────────────────────────────────────────────────────
    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());
}
