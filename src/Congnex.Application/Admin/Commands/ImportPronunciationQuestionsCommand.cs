using Congnex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Application.Admin.Commands;

public record PronunciationRowDto(
    string QuestionText,
    string CorrectAnswer,
    string Difficulty,
    string? Instruction,
    string? Label);

public record ImportPronunciationQuestionsCommand(Guid LessonId, List<PronunciationRowDto> Rows)
    : IRequest<ImportQuestionsResult>;

public sealed class ImportPronunciationQuestionsHandler(ICongnexDbContext db)
    : IRequestHandler<ImportPronunciationQuestionsCommand, ImportQuestionsResult>
{
    public async Task<ImportQuestionsResult> Handle(
        ImportPronunciationQuestionsCommand cmd, CancellationToken ct)
    {
        _ = await db.Lessons.FindAsync([cmd.LessonId], ct)
            ?? throw new KeyNotFoundException("Lesson not found.");

        var currentOrder = await db.Questions
            .Where(q => q.LessonId == cmd.LessonId)
            .CountAsync(ct);

        var errors = new List<string>();
        int imported = 0;

        foreach (var row in cmd.Rows)
        {
            if (string.IsNullOrWhiteSpace(row.QuestionText) || string.IsNullOrWhiteSpace(row.CorrectAnswer))
            {
                errors.Add($"Linha {imported + 1}: questionText ou correctAnswer vazio, ignorado.");
                continue;
            }

            db.Questions.Add(new Question
            {
                LessonId      = cmd.LessonId,
                Type          = "pronunciation",
                QuestionText  = row.QuestionText,
                CorrectAnswer = row.CorrectAnswer,
                Difficulty    = NormalizeDifficulty(row.Difficulty),
                Instruction   = row.Instruction,
                Label         = row.Label,
                OrderIndex    = ++currentOrder
            });

            imported++;
        }

        await db.SaveChangesAsync(ct);
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
