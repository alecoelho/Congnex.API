using System.Security.Claims;
using Congnex.Application.Common;
using Congnex.Application.Notifications.Commands;
using Congnex.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Congnex.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(IMediator mediator) : ControllerBase
{
    public record RegisterDeviceRequest(string Token, string Platform);

    // ── POST /api/notifications/device ─────────────────────────────────────
    [HttpPost("device")]
    public async Task<IActionResult> RegisterDevice(RegisterDeviceRequest req, CancellationToken ct)
    {
        if (!Enum.TryParse<DevicePlatform>(req.Platform, ignoreCase: true, out var platform))
            return BadRequest(ApiResponse.Fail("Invalid platform. Use 'iOS' or 'Android'."));

        try
        {
            var registrationId = await mediator.Send(
                new RegisterDeviceCommand(GetUserId(), req.Token, platform), ct);
            return Ok(ApiResponse<object>.Ok(new { registrationId }));
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    // ── DELETE /api/notifications/device/{registrationId} ──────────────────
    [HttpDelete("device/{registrationId}")]
    public async Task<IActionResult> DeregisterDevice(string registrationId, CancellationToken ct)
    {
        await mediator.Send(new DeregisterDeviceCommand(registrationId), ct);
        return Ok(ApiResponse.Ok());
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());
}
