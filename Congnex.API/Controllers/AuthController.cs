using System.Security.Claims;
using Congnex.Application.Auth.Commands;
using Congnex.Application.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Congnex.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    public record RegisterRequest(string FirstName, string LastName, string Email, string Password, DateTime? DateOfBirth = null, string? Motivations = null);
    public record LoginRequest(string Email, string Password);
    public record GoogleAuthRequest(string IdToken);
    public record GoogleCodeAuthRequest(string Code, string RedirectUri);
    public record GoogleAccessTokenRequest(string AccessToken);
    public record AppleAuthRequest(string IdToken, string? FullName);
    public record RefreshRequest(string RefreshToken);

    // ── POST /api/auth/register ─────────────────────────────────────────────
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(
                new RegisterCommand(req.FirstName, req.LastName, req.Email, req.Password, req.DateOfBirth, req.Motivations), ct);
            return Ok(ApiResponse<object>.Ok(result));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse.Fail(ex.Message));
        }
    }

    // ── POST /api/auth/login ────────────────────────────────────────────────
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new LoginCommand(req.Email, req.Password), ct);
            return Ok(ApiResponse<object>.Ok(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse.Fail(ex.Message));
        }
    }

    // ── POST /api/auth/google ───────────────────────────────────────────────
    [HttpPost("google")]
    public async Task<IActionResult> Google(GoogleAuthRequest req, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GoogleAuthCommand(req.IdToken), ct);
            return Ok(ApiResponse<object>.Ok(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse.Fail(ex.Message));
        }
    }

    // ── POST /api/auth/google-code ─────────────────────────────────────────
    [HttpPost("google-code")]
    public async Task<IActionResult> GoogleCode(GoogleCodeAuthRequest req, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(
                new GoogleCodeAuthCommand(req.Code, req.RedirectUri), ct);
            return Ok(ApiResponse<object>.Ok(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse.Fail(ex.Message));
        }
    }

    // ── POST /api/auth/google-token ────────────────────────────────────────
    [HttpPost("google-token")]
    public async Task<IActionResult> GoogleToken(GoogleAccessTokenRequest req, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(
                new GoogleAccessTokenAuthCommand(req.AccessToken), ct);
            return Ok(ApiResponse<object>.Ok(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse.Fail(ex.Message));
        }
    }

    // ── POST /api/auth/apple ────────────────────────────────────────────────
    [HttpPost("apple")]
    public async Task<IActionResult> Apple(AppleAuthRequest req, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new AppleAuthCommand(req.IdToken, req.FullName), ct);
            return Ok(ApiResponse<object>.Ok(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse.Fail(ex.Message));
        }
    }

    // ── POST /api/auth/refresh ──────────────────────────────────────────────
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest req, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new RefreshTokenCommand(req.RefreshToken), ct);
            return Ok(ApiResponse<object>.Ok(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse.Fail(ex.Message));
        }
    }

    // ── POST /api/auth/logout ───────────────────────────────────────────────
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());

        await mediator.Send(new LogoutCommand(userId), ct);
        return Ok(ApiResponse.Ok());
    }
}
