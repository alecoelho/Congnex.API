using Congnex.API.Filters;
using Congnex.Application.Admin.Commands;
using Congnex.Application.Common;
using Congnex.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Congnex.API.Controllers;

[ApiController]
[Route("api/admin")]
[AdminApiKey]
public class AdminController(IMediator mediator) : ControllerBase
{
    // ── POST /api/admin/videos/import ─────────────────────────────────────────
    /// <summary>
    /// Importa um vídeo + transcrição via planilha Excel.
    /// Colunas: title | description | unitTitle | videoUrl | xpReward | transcript
    /// Retorna o lessonId para uso nos endpoints de questões.
    /// </summary>
    [HttpPost("videos/import")]
    public async Task<IActionResult> ImportVideo(
        IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse.Fail("Arquivo não enviado."));

        try
        {
            ImportVideoLessonRequest request;
            await using (var stream = file.OpenReadStream())
                request = ExcelImportService.ReadVideoLesson(stream);

            var result = await mediator.Send(new ImportVideoLessonCommand(request), ct);
            return Ok(ApiResponse<object>.Ok(new
            {
                lessonId = result.LessonId,
                title    = result.Title,
                message  = "Vídeo e transcrição importados com sucesso."
            }));
        }
        catch (InvalidDataException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    // ── POST /api/admin/lessons/{lessonId}/questions/multiple-choice ──────────
    /// <summary>
    /// Colunas: questionText | option1 | option2 | option3 | option4 | correctAnswer | difficulty | imageUrl* | instruction* | label*
    /// (* = opcional). Se imageUrl preenchido, type = image_choice.
    /// </summary>
    [HttpPost("lessons/{lessonId:guid}/questions/multiple-choice")]
    public async Task<IActionResult> ImportMultipleChoice(
        Guid lessonId, IFormFile file, CancellationToken ct)
        => await ImportQuestions(file, stream =>
        {
            var rows = ExcelImportService.ReadMultipleChoice(stream);
            return mediator.Send(new ImportMultipleChoiceQuestionsCommand(lessonId, rows), ct);
        });

    // ── POST /api/admin/lessons/{lessonId}/questions/translation ──────────────
    /// <summary>
    /// Colunas: questionText | correctAnswer | difficulty | instruction* | label*
    /// </summary>
    [HttpPost("lessons/{lessonId:guid}/questions/translation")]
    public async Task<IActionResult> ImportTranslation(
        Guid lessonId, IFormFile file, CancellationToken ct)
        => await ImportQuestions(file, stream =>
        {
            var rows = ExcelImportService.ReadTranslation(stream);
            return mediator.Send(new ImportTranslationQuestionsCommand(lessonId, rows), ct);
        });

    // ── POST /api/admin/lessons/{lessonId}/questions/complete-sentence ─────────
    /// <summary>
    /// Colunas: questionText | correctAnswer | difficulty | instruction* | label*
    /// questionText deve conter ___ no lugar do espaço a preencher.
    /// </summary>
    [HttpPost("lessons/{lessonId:guid}/questions/complete-sentence")]
    public async Task<IActionResult> ImportCompleteSentence(
        Guid lessonId, IFormFile file, CancellationToken ct)
        => await ImportQuestions(file, stream =>
        {
            var rows = ExcelImportService.ReadCompleteSentence(stream);
            return mediator.Send(new ImportCompleteSentenceQuestionsCommand(lessonId, rows), ct);
        });

    // ── POST /api/admin/lessons/{lessonId}/questions/match-pairs ──────────────
    /// <summary>
    /// Colunas: groupLabel | leftText | rightText | difficulty
    /// Linhas com mesmo groupLabel formam um único Question com múltiplos pares.
    /// </summary>
    [HttpPost("lessons/{lessonId:guid}/questions/match-pairs")]
    public async Task<IActionResult> ImportMatchPairs(
        Guid lessonId, IFormFile file, CancellationToken ct)
        => await ImportQuestions(file, stream =>
        {
            var rows = ExcelImportService.ReadMatchPairs(stream);
            return mediator.Send(new ImportMatchPairsQuestionsCommand(lessonId, rows), ct);
        });

    // ── POST /api/admin/lessons/{lessonId}/questions/listening ────────────────
    /// <summary>
    /// Colunas: questionText | audioText | option1 | option2 | option3 | option4 | correctAnswer | difficulty | instruction* | label*
    /// </summary>
    [HttpPost("lessons/{lessonId:guid}/questions/listening")]
    public async Task<IActionResult> ImportListening(
        Guid lessonId, IFormFile file, CancellationToken ct)
        => await ImportQuestions(file, stream =>
        {
            var rows = ExcelImportService.ReadListening(stream);
            return mediator.Send(new ImportListeningQuestionsCommand(lessonId, rows), ct);
        });

    // ── POST /api/admin/lessons/{lessonId}/questions/pronunciation ─────────────
    /// <summary>
    /// Colunas: questionText | correctAnswer | difficulty | instruction* | label*
    /// </summary>
    [HttpPost("lessons/{lessonId:guid}/questions/pronunciation")]
    public async Task<IActionResult> ImportPronunciation(
        Guid lessonId, IFormFile file, CancellationToken ct)
        => await ImportQuestions(file, stream =>
        {
            var rows = ExcelImportService.ReadPronunciation(stream);
            return mediator.Send(new ImportPronunciationQuestionsCommand(lessonId, rows), ct);
        });

    // ── Helper ────────────────────────────────────────────────────────────────

    private async Task<IActionResult> ImportQuestions(
        IFormFile? file,
        Func<Stream, Task<ImportQuestionsResult>> process)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse.Fail("Arquivo não enviado."));

        try
        {
            ImportQuestionsResult result;
            await using (var stream = file.OpenReadStream())
                result = await process(stream);

            return Ok(ApiResponse<object>.Ok(new
            {
                imported = result.Imported,
                errors   = result.Errors
            }));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
        catch (InvalidDataException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }
}
