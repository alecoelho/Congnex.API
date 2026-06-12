using Congnex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Application.Admin.Commands;

public record MultipleChoiceRowDto(
    string QuestionText,
    string Option1,
    string Option2,
    string Option3,
    string Option4,
    string CorrectAnswer,
    string Difficulty,
    string? ImageUrl,
    string? Instruction,
    string? Label);

public record ImportMultipleChoiceQuestionsCommand(Guid LessonId, List<MultipleChoiceRowDto> Rows)
    : IRequest<ImportQuestionsResult>;

public sealed class ImportMultipleChoiceQuestionsHandler(ICongnexDbContext db)
    : IRequestHandler<ImportMultipleChoiceQuestionsCommand, ImportQuestionsResult>
{
    public async Task<ImportQuestionsResult> Handle(
        ImportMultipleChoiceQuestionsCommand cmd, CancellationToken ct)
    {
        var lesson = await db.Lessons.FindAsync([cmd.LessonId], ct)
            ?? throw new KeyNotFoundException("Lesson not found.");

        var currentOrder = await db.Questions
            .Where(q => q.LessonId == cmd.LessonId)
            .CountAsync(ct);

        var errors = new List<string>();
        int imported = 0;

        foreach (var row in cmd.Rows)
        {
            if (string.IsNullOrWhiteSpace(row.QuestionText))
            {
                errors.Add($"Linha {imported + 1}: questionText vazio, ignorado.");
                continue;
            }

            var options = new[] { row.Option1, row.Option2, row.Option3, row.Option4 };

            if (!options.Any(o => string.Equals(o, row.CorrectAnswer, StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add($"Linha {imported + 1}: correctAnswer '{row.CorrectAnswer}' não encontrado nas opções.");
                continue;
            }

            var type = string.IsNullOrWhiteSpace(row.ImageUrl) ? "multiple_choice" : "image_choice";

            var question = new Question
            {
                LessonId     = cmd.LessonId,
                Type         = type,
                QuestionText = row.QuestionText,
                CorrectAnswer = row.CorrectAnswer,
                Difficulty   = NormalizeDifficulty(row.Difficulty),
                ImageUrl     = row.ImageUrl,
                Instruction  = row.Instruction,
                Label        = row.Label,
                OrderIndex   = ++currentOrder
            };
            db.Questions.Add(question);
            await db.SaveChangesAsync(ct); // salva para obter o Id

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
            "facil" or "fácil" or "easy"   => "easy",
            "medio" or "médio" or "medium"  => "medium",
            "dificil" or "difícil" or "hard" => "hard",
            _ => "easy"
        };
}
