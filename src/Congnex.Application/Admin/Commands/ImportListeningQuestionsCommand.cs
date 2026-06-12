using Congnex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Application.Admin.Commands;

public record ListeningRowDto(
    string QuestionText,
    string AudioText,
    string Option1,
    string Option2,
    string Option3,
    string Option4,
    string CorrectAnswer,
    string Difficulty,
    string? Instruction,
    string? Label);

public record ImportListeningQuestionsCommand(Guid LessonId, List<ListeningRowDto> Rows)
    : IRequest<ImportQuestionsResult>;

public sealed class ImportListeningQuestionsHandler(ICongnexDbContext db)
    : IRequestHandler<ImportListeningQuestionsCommand, ImportQuestionsResult>
{
    public async Task<ImportQuestionsResult> Handle(
        ImportListeningQuestionsCommand cmd, CancellationToken ct)
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
            if (string.IsNullOrWhiteSpace(row.QuestionText) || string.IsNullOrWhiteSpace(row.AudioText))
            {
                errors.Add($"Linha {imported + 1}: questionText ou audioText vazio, ignorado.");
                continue;
            }

            var options = new[] { row.Option1, row.Option2, row.Option3, row.Option4 };

            if (!options.Any(o => string.Equals(o, row.CorrectAnswer, StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add($"Linha {imported + 1}: correctAnswer '{row.CorrectAnswer}' não encontrado nas opções.");
                continue;
            }

            var question = new Question
            {
                LessonId      = cmd.LessonId,
                Type          = "listening_choice",
                QuestionText  = row.QuestionText,
                AudioText     = row.AudioText,
                CorrectAnswer = row.CorrectAnswer,
                Difficulty    = NormalizeDifficulty(row.Difficulty),
                Instruction   = row.Instruction,
                Label         = row.Label,
                OrderIndex    = ++currentOrder
            };
            db.Questions.Add(question);
            await db.SaveChangesAsync(ct);

            int optIndex = 1;
            foreach (var opt in options)
            {
                if (string.IsNullOrWhiteSpace(opt)) continue;
                db.QuestionOptions.Add(new QuestionOption
                {
                    QuestionId = question.Id,
                    OptionText = opt,
                    IsCorrect  = string.Equals(opt, row.CorrectAnswer, StringComparison.OrdinalIgnoreCase),
                    OrderIndex = optIndex++
                });
            }

            await db.SaveChangesAsync(ct);
            imported++;
        }

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
