using Congnex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Application.Admin.Commands;

// Várias linhas com mesmo GroupLabel formam um único Question com múltiplos pares
public record MatchPairsRowDto(
    string GroupLabel,
    string LeftText,
    string RightText,
    string Difficulty);

public record ImportMatchPairsQuestionsCommand(Guid LessonId, List<MatchPairsRowDto> Rows)
    : IRequest<ImportQuestionsResult>;

public sealed class ImportMatchPairsQuestionsHandler(ICongnexDbContext db)
    : IRequestHandler<ImportMatchPairsQuestionsCommand, ImportQuestionsResult>
{
    public async Task<ImportQuestionsResult> Handle(
        ImportMatchPairsQuestionsCommand cmd, CancellationToken ct)
    {
        _ = await db.Lessons.FindAsync([cmd.LessonId], ct)
            ?? throw new KeyNotFoundException("Lesson not found.");

        var currentOrder = await db.Questions
            .Where(q => q.LessonId == cmd.LessonId)
            .CountAsync(ct);

        var errors = new List<string>();
        int imported = 0;

        // Agrupa linhas pelo GroupLabel
        var groups = cmd.Rows
            .Where(r => !string.IsNullOrWhiteSpace(r.GroupLabel)
                     && !string.IsNullOrWhiteSpace(r.LeftText)
                     && !string.IsNullOrWhiteSpace(r.RightText))
            .GroupBy(r => r.GroupLabel.Trim());

        foreach (var group in groups)
        {
            var firstRow    = group.First();
            var difficulty  = NormalizeDifficulty(firstRow.Difficulty);

            var question = new Question
            {
                LessonId     = cmd.LessonId,
                Type         = "match_pairs",
                QuestionText = group.Key, // GroupLabel vira o texto da questão
                Difficulty   = difficulty,
                OrderIndex   = ++currentOrder
            };
            db.Questions.Add(question);
            await db.SaveChangesAsync(ct); // precisa do Id para os pares

            int pairIndex = 1;
            foreach (var row in group)
            {
                db.QuestionPairs.Add(new QuestionPair
                {
                    QuestionId = question.Id,
                    LeftText   = row.LeftText.Trim(),
                    RightText  = row.RightText.Trim(),
                    OrderIndex = pairIndex++
                });
            }

            await db.SaveChangesAsync(ct);
            imported++;
        }

        // Linhas ignoradas
        var ignored = cmd.Rows.Count(r =>
            string.IsNullOrWhiteSpace(r.GroupLabel) ||
            string.IsNullOrWhiteSpace(r.LeftText)   ||
            string.IsNullOrWhiteSpace(r.RightText));

        if (ignored > 0)
            errors.Add($"{ignored} linha(s) ignorada(s) por dados incompletos.");

        return new ImportQuestionsResult(imported, errors);
    }

    private static string NormalizeDifficulty(string? d) =>
        d?.ToLowerInvariant() switch
        {
            "facil" or "fácil" or "easy"    => "easy",
            "medio" or "médio" or "medium"  => "medium",
            "dificil" or "difícil" or "hard" => "hard",
            _ => "easy"
        };
}
