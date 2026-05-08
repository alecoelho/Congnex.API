using System.Security.Claims;
using Congnex.Application.Common;
using Congnex.Application.Lessons.Commands;
using Congnex.Application.Lessons.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Congnex.API.Controllers;

[ApiController]
[Route("api/lessons")]
[Authorize]
public class LessonsController(IMediator mediator) : ControllerBase
{
    public record AnswerRequest(Guid QuestionId, List<string> GivenAnswers, bool IsCorrect);
    public record CompleteLessonRequest(int Score, List<AnswerRequest> Answers);

    // ── GET /api/lessons/units ──────────────────────────────────────────────
    [HttpGet("units")]
    public async Task<IActionResult> GetUnits(CancellationToken ct)
    {
        var units = await mediator.Send(new GetUnitsQuery(GetUserId()), ct);
        return Ok(ApiResponse<object>.Ok(units));
    }

    // ── GET /api/lessons/{lessonId}/questions ───────────────────────────────
    [HttpGet("{lessonId:guid}/questions")]
    public async Task<IActionResult> GetQuestions(Guid lessonId, CancellationToken ct)
    {
        try
        {
            var questions = await mediator.Send(new GetLessonQuestionsQuery(lessonId), ct);
            return Ok(ApiResponse<object>.Ok(questions));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
    }

    // ── POST /api/lessons/{lessonId}/complete ───────────────────────────────
    [HttpPost("{lessonId:guid}/complete")]
    public async Task<IActionResult> CompleteLesson(
        Guid lessonId, CompleteLessonRequest req, CancellationToken ct)
    {
        try
        {
            var answers = req.Answers
                .Select(a => new AnswerDto(a.QuestionId, a.GivenAnswers, a.IsCorrect))
                .ToList();

            var result = await mediator.Send(
                new CompleteLessonCommand(GetUserId(), lessonId, req.Score, answers), ct);

            return Ok(ApiResponse<object>.Ok(result));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());
}
