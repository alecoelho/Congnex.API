using System.Security.Claims;
using Congnex.Application.Common;
using Congnex.Application.Lessons.Commands;
using Congnex.Application.Lessons.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Congnex.API.Controllers;

[ApiController]
[Route("api/flashcards")]
[Authorize]
public class FlashcardsController(IMediator mediator) : ControllerBase
{
    public record ReviewFlashcardRequest(Guid LessonId, string Word, bool Remembered);

    // ── POST /api/flashcards/review ─────────────────────────────────────────
    [HttpPost("review")]
    public async Task<IActionResult> Review(ReviewFlashcardRequest req, CancellationToken ct)
    {
        await mediator.Send(
            new ReviewFlashcardCommand(GetUserId(), req.LessonId, req.Word, req.Remembered), ct);
        return Ok(ApiResponse.Ok());
    }

    // ── GET /api/flashcards/due ─────────────────────────────────────────────
    [HttpGet("due")]
    public async Task<IActionResult> GetDue([FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var items = await mediator.Send(new GetDueFlashcardsQuery(GetUserId(), limit), ct);
        return Ok(ApiResponse<object>.Ok(items));
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());
}
