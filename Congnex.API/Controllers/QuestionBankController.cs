using System.Text.Json;
using Congnex.API.Filters;
using Congnex.Application.Common;
using Congnex.Application.Interfaces;
using Congnex.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Congnex.API.Controllers;

[ApiController]
[Route("api/admin/question-bank")]
[AdminApiKey]
public class QuestionBankController(
    IXylaService xylaService,
    ICongnexDbContext db,
    IServiceScopeFactory scopeFactory,
    IWebHostEnvironment env,
    ILogger<QuestionBankController> logger) : ControllerBase
{
    private static readonly string[] Levels  = ["A1", "A2", "B1", "B2", "C1", "C2"];
    private static readonly string[] Tipos   = ["funcoes_comunicativas", "vocabulario", "gramatica", "habilidades_receptivas", "completar_frase"];
    private static readonly string[] Domains = ["rotina_diaria", "trabalho", "viagem", "saude", "negocios", "tecnologia", "compras", "educacao", "familia_relacionamentos", "alimentacao", "cultura_entretenimento", "meio_ambiente"];

    // ── GET /status ───────────────────────────────────────────────────────────
    [HttpGet("status")]
    public async Task<IActionResult> Status(CancellationToken ct)
    {
        var counts = await db.QuestionBankItems
            .GroupBy(q => new { q.CefrLevel, q.QuestionType, q.Domain })
            .Select(g => new { g.Key.CefrLevel, g.Key.QuestionType, g.Key.Domain, Count = g.Count() })
            .ToListAsync(ct);

        int total   = counts.Sum(c => c.Count);
        int covered = counts.Count;
        int matrix  = Levels.Length * Tipos.Length * Domains.Length; // 360

        return Ok(ApiResponse<object>.Ok(new { total, covered, matrix, missing = matrix - covered, items = counts }));
    }

    // ── POST /generate ────────────────────────────────────────────────────────
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateRequest req, CancellationToken ct)
    {
        if (!Levels.Contains(req.CefrLevel) || !Tipos.Contains(req.QuestionType) || !Domains.Contains(req.Domain))
            return BadRequest(ApiResponse.Fail("Invalid cefrLevel, questionType, or domain."));

        var allDomains = await xylaService.GenerateQuestionBankForTipoAsync(req.CefrLevel, req.QuestionType, ct);
        if (!allDomains.TryGetValue(req.Domain, out var questions) || questions.Count == 0)
            return StatusCode(500, ApiResponse.Fail("AI returned no questions for the requested domain."));

        int saved = await SaveToDatabaseAsync(db, req.CefrLevel, req.QuestionType, req.Domain, questions, ct);
        await WriteSeedFileAsync(req.CefrLevel, req.QuestionType, req.Domain, questions);

        return Ok(ApiResponse<object>.Ok(new { saved, total = questions.Count }));
    }

    // ── POST /generate-missing ────────────────────────────────────────────────
    /// <summary>Background job: generates all matrix items that have zero questions in the DB.</summary>
    [HttpPost("generate-missing")]
    public IActionResult GenerateMissing()
    {
        _ = Task.Run(() => RunGenerationAsync(Levels));
        return Accepted(ApiResponse<object>.Ok(new { message = "Generating all missing matrix items in background. Check /status to track progress." }));
    }

    // ── POST /generate-level/{level} ─────────────────────────────────────────
    /// <summary>Background job: generates missing matrix items for one CEFR level.</summary>
    [HttpPost("generate-level/{level}")]
    public IActionResult GenerateLevel(string level)
    {
        var normalized = level.ToUpperInvariant();
        if (!Levels.Contains(normalized))
            return BadRequest(ApiResponse.Fail("Invalid CEFR level."));

        _ = Task.Run(() => RunGenerationAsync([normalized]));
        return Accepted(ApiResponse<object>.Ok(new { message = $"Generating missing matrix items for level {normalized} in background." }));
    }

    // ── POST /seed-from-files ─────────────────────────────────────────────────
    [HttpPost("seed-from-files")]
    public async Task<IActionResult> SeedFromFiles(CancellationToken ct)
    {
        var seedDir = Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", "seed", "questions"));
        if (!Directory.Exists(seedDir))
            return NotFound(ApiResponse.Fail($"Seed directory not found: {seedDir}"));

        var files = Directory.GetFiles(seedDir, "*.json");
        int total = 0, skipped = 0;

        foreach (var file in files)
        {
            try
            {
                var json  = await System.IO.File.ReadAllTextAsync(file, ct);
                var items = JsonSerializer.Deserialize<List<SeedQuestionRow>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (items is null) continue;

                foreach (var row in items)
                {
                    if (string.IsNullOrWhiteSpace(row.QuestionText) || string.IsNullOrWhiteSpace(row.CorrectAnswer)) continue;

                    var exists = await db.QuestionBankItems.AnyAsync(
                        q => q.QuestionText == row.QuestionText && q.CefrLevel == row.CefrLevel, ct);
                    if (exists) { skipped++; continue; }

                    var dbType = row.Type ?? (row.QuestionType == "completar_frase"        ? "complete_sentence"
                                           : row.QuestionType == "habilidades_receptivas"  ? "translation"
                                           : "multiple_choice");

                    var bankQ = new QuestionBank
                    {
                        CefrLevel     = row.CefrLevel    ?? "",
                        QuestionType  = row.QuestionType ?? "",
                        Domain        = row.TopicDomain  ?? "",
                        Type          = dbType,
                        QuestionText  = row.QuestionText,
                        CorrectAnswer = row.CorrectAnswer,
                        Difficulty    = row.Difficulty   ?? "easy"
                    };

                    if (row.Options is { Count: > 0 })
                    {
                        int idx = 0;
                        foreach (var opt in row.Options)
                            bankQ.Options.Add(new QuestionBankOption
                            {
                                OptionText = opt,
                                IsCorrect  = string.Equals(opt, row.CorrectAnswer, StringComparison.OrdinalIgnoreCase),
                                OrderIndex = idx++
                            });
                    }

                    db.QuestionBankItems.Add(bankQ);
                    total++;
                }

                await db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to seed from file {File}", file);
            }
        }

        return Ok(ApiResponse<object>.Ok(new { imported = total, skipped }));
    }

    // ── DELETE /reset ─────────────────────────────────────────────────────────
    [HttpDelete("reset")]
    public async Task<IActionResult> Reset(CancellationToken ct)
    {
        var count = await db.QuestionBankItems.CountAsync(ct);
        db.QuestionBankItems.RemoveRange(db.QuestionBankItems);
        await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(new { deleted = count }));
    }

    // ── Background generation ─────────────────────────────────────────────────

    private async Task RunGenerationAsync(string[] levels)
    {
        foreach (var level in levels)
        {
            foreach (var tipo in Tipos)
            {
                try
                {
                    // Check if ALL 12 domains already exist for this level×tipo
                    await using var checkScope = scopeFactory.CreateAsyncScope();
                    var checkDb = checkScope.ServiceProvider.GetRequiredService<ICongnexDbContext>();

                    var existingDomains = await checkDb.QuestionBankItems
                        .Where(q => q.CefrLevel == level && q.QuestionType == tipo)
                        .Select(q => q.Domain)
                        .Distinct()
                        .ToListAsync();

                    var missingDomains = Domains.Except(existingDomains).ToArray();
                    if (missingDomains.Length == 0)
                    {
                        logger.LogDebug("[BankGen] Skipping {Level}/{Tipo} — all domains exist", level, tipo);
                        continue;
                    }

                    // 1 AI call generates all 12 domains at once
                    var allDomains = await xylaService.GenerateQuestionBankForTipoAsync(level, tipo);

                    foreach (var domain in missingDomains)
                    {
                        if (!allDomains.TryGetValue(domain, out var questions) || questions.Count == 0)
                        {
                            logger.LogWarning("[BankGen] No questions returned for {Level}/{Tipo}/{Domain}", level, tipo, domain);
                            continue;
                        }

                        await using var scope = scopeFactory.CreateAsyncScope();
                        var scopedDb = scope.ServiceProvider.GetRequiredService<ICongnexDbContext>();

                        await SaveToDatabaseAsync(scopedDb, level, tipo, domain, questions, CancellationToken.None);
                        await WriteSeedFileAsync(level, tipo, domain, questions);
                        logger.LogInformation("[BankGen] ✓ {Level}/{Tipo}/{Domain} — {Count} questions", level, tipo, domain, questions.Count);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[BankGen] Failed {Level}/{Tipo}", level, tipo);
                }
            }
        }
        logger.LogInformation("[BankGen] Generation complete for levels: {Levels}", string.Join(", ", levels));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static async Task<int> SaveToDatabaseAsync(
        ICongnexDbContext targetDb,
        string level, string tipo, string domain,
        List<BankQuestionDto> questions, CancellationToken ct)
    {
        int saved = 0;
        foreach (var q in questions)
        {
            var exists = await targetDb.QuestionBankItems.AnyAsync(
                b => b.QuestionText == q.QuestionText && b.CefrLevel == level, ct);
            if (exists) continue;

            var bank = new QuestionBank
            {
                CefrLevel     = level,
                QuestionType  = tipo,
                Domain        = domain,
                Type          = q.Type,
                QuestionText  = q.QuestionText,
                CorrectAnswer = q.CorrectAnswer,
                Difficulty    = q.Difficulty
            };

            int idx = 0;
            foreach (var opt in q.Options)
                bank.Options.Add(new QuestionBankOption
                {
                    OptionText = opt.OptionText,
                    IsCorrect  = opt.IsCorrect,
                    OrderIndex = idx++
                });

            targetDb.QuestionBankItems.Add(bank);
            saved++;
        }

        await targetDb.SaveChangesAsync(ct);
        return saved;
    }

    private async Task WriteSeedFileAsync(
        string level, string tipo, string domain, List<BankQuestionDto> questions)
    {
        try
        {
            var dbType = questions.FirstOrDefault()?.Type ?? "multiple_choice";
            await Congnex.Infrastructure.Persistence.QuestionBankSeeder.AppendToSeedFileAsync(
                env.ContentRootPath, level, tipo, domain, dbType,
                questions.Select(q => (q.QuestionText, q.CorrectAnswer, q.Difficulty,
                    q.Options.Select(o => o.OptionText).ToList())));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to write seed file for {Level}/{Tipo}/{Domain}", level, tipo, domain);
        }
    }
}

public record GenerateRequest(string CefrLevel, string QuestionType, string Domain);

internal class SeedQuestionRow
{
    public string?       CefrLevel     { get; set; }
    public string?       QuestionType  { get; set; }
    public string?       TopicDomain   { get; set; }
    public string?       Type          { get; set; }
    public string?       QuestionText  { get; set; }
    public string?       CorrectAnswer { get; set; }
    public List<string>? Options       { get; set; }
    public string?       Difficulty    { get; set; }
}
