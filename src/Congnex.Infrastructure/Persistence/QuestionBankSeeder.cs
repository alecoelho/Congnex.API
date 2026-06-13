using System.Text.Json;
using Congnex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Congnex.Infrastructure.Persistence;

public static class QuestionBankSeeder
{
    public const string SeedFileName = "question-bank.json";

    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Seeds the question_bank table from seed/question-bank.json if the table is empty.
    /// </summary>
    public static async Task SeedAsync(CongnexDbContext db, string contentRootPath, ILogger logger)
    {
        var alreadyHas = await db.QuestionBankItems.AnyAsync();
        if (alreadyHas)
        {
            logger.LogInformation("[QuestionBankSeeder] Table already has data — skipping seed.");
            return;
        }

        var seedFile = GetSeedFilePath(contentRootPath);
        if (!File.Exists(seedFile))
        {
            logger.LogWarning("[QuestionBankSeeder] Seed file not found at {File} — skipping.", seedFile);
            return;
        }

        logger.LogInformation("[QuestionBankSeeder] Seeding question bank from {File}...", seedFile);

        var rows = await ReadSeedFileAsync(seedFile);
        if (rows.Count == 0)
        {
            logger.LogWarning("[QuestionBankSeeder] Seed file is empty — skipping.");
            return;
        }

        int total = 0;
        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.QuestionText) ||
                string.IsNullOrWhiteSpace(row.CorrectAnswer)) continue;

            var dbType = row.Type ?? (row.QuestionType == "completar_frase"       ? "complete_sentence"
                                   : row.QuestionType == "habilidades_receptivas" ? "translation"
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

            // Flush every 200 to avoid giant change-tracker batches
            if (total % 200 == 0)
                await db.SaveChangesAsync();
        }

        await db.SaveChangesAsync();
        logger.LogInformation("[QuestionBankSeeder] Seeded {Count} questions.", total);
    }

    /// <summary>Returns the path to the consolidated seed file.</summary>
    public static string GetSeedFilePath(string contentRootPath) =>
        Path.GetFullPath(Path.Combine(contentRootPath, "..", "seed", "questions", SeedFileName));

    /// <summary>Reads the consolidated seed file. Returns empty list if file doesn't exist. Attempts recovery from partial JSON.</summary>
    public static async Task<List<SeedRow>> ReadSeedFileAsync(string filePath)
    {
        if (!File.Exists(filePath)) return [];

        var json = await File.ReadAllTextAsync(filePath);

        try
        {
            return JsonSerializer.Deserialize<List<SeedRow>>(json, JsonOpts) ?? [];
        }
        catch (JsonException ex)
        {
            // If parsing fails, try to extract valid JSON objects from the content
            var rows = new List<SeedRow>();
            var matches = System.Text.RegularExpressions.Regex.Matches(json, @"\{[^{}]*(?:\{[^{}]*\}[^{}]*)*\}");

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                try
                {
                    var obj = JsonSerializer.Deserialize<SeedRow>(match.Value, JsonOpts);
                    if (obj?.QuestionText != null && obj.CorrectAnswer != null)
                        rows.Add(obj);
                }
                catch { /* Skip invalid entries */ }
            }

            return rows.Count > 0 ? rows : [];
        }
    }

    /// <summary>Appends new questions to the consolidated seed file (creates it if absent).</summary>
    public static async Task AppendToSeedFileAsync(
        string contentRootPath,
        string level, string tipo, string domain, string type,
        IEnumerable<(string QuestionText, string CorrectAnswer, string Difficulty, List<string> Options)> questions)
    {
        var filePath = GetSeedFilePath(contentRootPath);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        var existing = await ReadSeedFileAsync(filePath);

        foreach (var (questionText, correctAnswer, difficulty, options) in questions)
            existing.Add(new SeedRow
            {
                CefrLevel     = level,
                QuestionType  = tipo,
                TopicDomain   = domain,
                Type          = type,
                QuestionText  = questionText,
                CorrectAnswer = correctAnswer,
                Difficulty    = difficulty,
                Options       = options
            });

        var json = JsonSerializer.Serialize(existing,
            new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, json);
    }

    public class SeedRow
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
}
