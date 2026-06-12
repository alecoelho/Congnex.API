using System.Security.Claims;
using Congnex.Application.Common;
using Congnex.Application.Users.Commands;
using Congnex.Application.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Congnex.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(IMediator mediator) : ControllerBase
{
    public record UpdateProfileRequest(string? FirstName, string? LastName, int? DailyMinutes, string? Motivations);

    // ── GET /api/users/me ───────────────────────────────────────────────────
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        try
        {
            var dto = await mediator.Send(new GetProfileQuery(GetUserId()), ct);
            return Ok(ApiResponse<object>.Ok(dto));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
    }

    // ── PUT /api/users/me ───────────────────────────────────────────────────
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest req, CancellationToken ct)
    {
        try
        {
            await mediator.Send(
                new UpdateProfileCommand(GetUserId(), req.FirstName, req.LastName, req.DailyMinutes, req.Motivations), ct);
            return Ok(ApiResponse.Ok());
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());
}
