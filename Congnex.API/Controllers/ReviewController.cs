using System.Security.Claims;
using Congnex.Application.Common;
using Congnex.Application.Review.Commands;
using Congnex.Application.Review.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Congnex.API.Controllers;

[ApiController]
[Route("api/review")]
[Authorize]
public class ReviewController(IMediator mediator) : ControllerBase
{
    public record SubmitReviewRequest(Guid ReviewItemId, int Rating);

    // ── GET /api/review/due ─────────────────────────────────────────────────
    [HttpGet("due")]
    public async Task<IActionResult> GetDue([FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var items = await mediator.Send(new GetDueReviewItemsQuery(GetUserId(), limit), ct);
        return Ok(ApiResponse<object>.Ok(items));
    }

    // ── POST /api/review/submit ─────────────────────────────────────────────
    [HttpPost("submit")]
    public async Task<IActionResult> Submit(SubmitReviewRequest req, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(
                new SubmitReviewCommand(GetUserId(), req.ReviewItemId, req.Rating), ct);
            return Ok(ApiResponse<object>.Ok(result));
        }
        catch (KeyNotFoundException ex)  { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (ArgumentException ex)     { return BadRequest(ApiResponse.Fail(ex.Message)); }
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());
}
