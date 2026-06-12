using Congnex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Application.Admin.Commands;

// questionText deve conter ___ no lugar do espaço a preencher
public record CompleteSentenceRowDto(
    string QuestionText,
    string CorrectAnswer,
    string Difficulty,
    string? WrongOption1,
    string? WrongOption2,
    string? WrongOption3,
    string? Instruction,
    string? Label);

public record ImportCompleteSentenceQuestionsCommand(Guid LessonId, List<CompleteSentenceRowDto> Rows)
    : IRequest<ImportQuestionsResult>;

public sealed class ImportCompleteSentenceQuestionsHandler(ICongnexDbContext db)
    : IRequestHandler<ImportCompleteSentenceQuestionsCommand, ImportQuestionsResult>
{
    public async Task<ImportQuestionsResult> Handle(
        ImportCompleteSentenceQuestionsCommand cmd, CancellationToken ct)
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

            if (!row.QuestionText.Contains("___"))
                errors.Add($"Aviso linha {imported + 1}: questionText não contém '___'. Verifique o formato.");

            var question = new Question
            {
                LessonId      = cmd.LessonId,
                Type          = "complete_sentence",
                QuestionText  = row.QuestionText,
                CorrectAnswer = row.CorrectAnswer,
                Difficulty    = NormalizeDifficulty(row.Difficulty),
                Instruction   = row.Instruction,
                Label         = row.Label,
                OrderIndex    = ++currentOrder
            };

            // Adiciona opções (correta + erradas) se fornecidas
            var wrongOptions = new[] { row.WrongOption1, row.WrongOption2, row.WrongOption3 }
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .ToList();

            if (wrongOptions.Any())
            {
                int optIdx = 0;
                question.Options.Add(new QuestionOption { OptionText = row.CorrectAnswer, IsCorrect = true,  OrderIndex = optIdx++ });
                foreach (var wrong in wrongOptions)
                    question.Options.Add(new QuestionOption { OptionText = wrong!, IsCorrect = false, OrderIndex = optIdx++ });
            }

            db.Questions.Add(question);
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
