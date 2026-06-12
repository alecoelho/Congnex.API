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
    public record AnswerRequest(Guid QuestionId, Guid? SelectedOptionId, string? TextAnswer, bool IsCorrect, int TimeSpentSeconds = 0);
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
            var questions = await mediator.Send(new GetLessonQuestionsQuery(lessonId, GetUserId()), ct);
            return Ok(ApiResponse<object>.Ok(questions));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
    }

    // ── POST /api/lessons/{lessonId}/answer ────────────────────────────────
    public record SaveAnswerRequest(
        Guid    QuestionId,
        Guid?   SelectedOptionId,
        string? TextAnswer,
        bool    IsCorrect);

    [HttpPost("{lessonId:guid}/answer")]
    public async Task<IActionResult> SaveAnswer(Guid lessonId, SaveAnswerRequest req, CancellationToken ct)
    {
        await mediator.Send(new SaveQuestionAnswerCommand(
            GetUserId(), lessonId, req.QuestionId, req.SelectedOptionId, req.TextAnswer, req.IsCorrect), ct);
        return Ok(ApiResponse.Ok());
    }

    // ── GET /api/lessons/{lessonId}/wrong-answers ──────────────────────────
    [HttpGet("{lessonId:guid}/wrong-answers")]
    public async Task<IActionResult> GetWrongAnswers(Guid lessonId, CancellationToken ct)
    {
        var flashcards = await mediator.Send(
            new GetWrongAnswerFlashcardsQuery(lessonId, GetUserId()), ct);
        return Ok(ApiResponse<object>.Ok(flashcards));
    }

    // ── POST /api/lessons/{lessonId}/complete ───────────────────────────────
    [HttpPost("{lessonId:guid}/complete")]
    public async Task<IActionResult> CompleteLesson(
        Guid lessonId, CompleteLessonRequest req, CancellationToken ct)
    {
        try
        {
            var answers = req.Answers
                .Select(a => new AnswerDto(a.QuestionId, a.SelectedOptionId, a.TextAnswer, a.IsCorrect, a.TimeSpentSeconds))
                .ToList();

            var result = await mediator.Send(
                new CompleteLessonCommand(GetUserId(), lessonId, req.Score, answers), ct);

            return Ok(ApiResponse<object>.Ok(result));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
    }

    // ── GET /api/lessons/{lessonId}/video ──────────────────────────────────
    [HttpGet("{lessonId:guid}/video")]
    public async Task<IActionResult> GetVideo(Guid lessonId, CancellationToken ct)
    {
        var video = await mediator.Send(new GetLessonVideoQuery(lessonId), ct);
        if (video is null) return NotFound(ApiResponse.Fail("Video not found for this lesson."));
        return Ok(ApiResponse<object>.Ok(video));
    }

    // ── GET /api/lessons/{lessonId}/flashcards ──────────────────────────────
    [HttpGet("{lessonId:guid}/flashcards")]
    public async Task<IActionResult> GetFlashcards(Guid lessonId, CancellationToken ct)
    {
        try
        {
            var flashcards = await mediator.Send(new GetLessonFlashcardsQuery(lessonId), ct);
            return Ok(ApiResponse<object>.Ok(flashcards));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());
}
