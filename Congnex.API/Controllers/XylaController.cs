using System.Security.Claims;
using System.Text.Json;
using Congnex.API.Controllers.Xyla;
using Congnex.Application.Common;
using Congnex.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Congnex.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class XylaController : ControllerBase
{
    private readonly IXylaService _xyla;
    private readonly ILogger<XylaController> _logger;

    public XylaController(IXylaService xyla, ILogger<XylaController> logger)
    {
        _xyla   = xyla;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new Xyla interview session. Call once when AIInterviewScreen mounts.
    /// Returns the sessionId that must be passed to every subsequent /message call.
    /// </summary>
    [HttpPost("start")]
    public async Task<ActionResult<XylaStartResponse>> Start(CancellationToken ct)
    {
        var userId = ParseUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var sessionId = await _xyla.StartSessionAsync(userId, ct);
        return Ok(ApiResponse<XylaStartResponse>.Ok(new XylaStartResponse { SessionId = sessionId }));
    }

    /// <summary>
    /// Sends the student's message and streams Xyla's response as Server-Sent Events.
    /// Event format: data: {"content":"..."} or data: {"event_type":"plan_ready","sessionId":"..."}
    /// Stream ends with: data: [DONE]
    /// </summary>
    [HttpPost("message")]
    [EnableRateLimiting("xyla")]
    public async Task Message([FromBody] XylaMessageRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            Response.StatusCode = 400;
            await Response.WriteAsJsonAsync(new { message = "Invalid request." });
            return;
        }

        var userId = ParseUserId();
        if (userId == Guid.Empty)
        {
            Response.StatusCode = 401;
            return;
        }

        Response.ContentType = "text/event-stream";
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        try
        {
            await foreach (var token in _xyla.StreamMessageAsync(
                request.SessionId, userId, request.Message, ct))
            {
                if (token.StartsWith("[PLAN_READY:"))
                {
                    try
                    {
                        var videosJson = token["[PLAN_READY:".Length..];
                        using var videosDoc = JsonDocument.Parse(videosJson);
                        var evt = JsonSerializer.Serialize(new
                        {
                            event_type = "plan_ready",
                            sessionId  = request.SessionId,
                            videos     = videosDoc.RootElement
                        });
                        await Response.WriteAsync($"data: {evt}\n\n", CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to parse plan_ready token for user {UserId}", userId);
                    }
                }
                else
                {
                    var data = JsonSerializer.Serialize(new { content = token });
                    await Response.WriteAsync($"data: {data}\n\n", CancellationToken.None);
                }

                await Response.Body.FlushAsync(CancellationToken.None);
            }

            await Response.WriteAsync("data: [DONE]\n\n", CancellationToken.None);
            await Response.Body.FlushAsync(CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            // Client disconnected — no action needed
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Xyla streaming error for user {UserId}", userId);

            if (!Response.HasStarted)
            {
                Response.StatusCode = 503;
                await Response.WriteAsJsonAsync(new { message = "Xyla está indisponível no momento. Tente novamente em instantes." });
            }
            else
            {
                try
                {
                    var err = JsonSerializer.Serialize(new { content = "\n\n*Desculpe, tive um problema. Pode tentar novamente?*" });
                    await Response.WriteAsync($"data: {err}\n\n", CancellationToken.None);
                    await Response.WriteAsync("data: [DONE]\n\n", CancellationToken.None);
                    await Response.Body.FlushAsync(CancellationToken.None);
                }
                catch { /* response may be closed */ }
            }
        }
    }

    private Guid ParseUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
    }
}
