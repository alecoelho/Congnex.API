using Congnex.Application.Common;
using Congnex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Congnex.Application.Lessons.Commands;

public record SaveQuestionAnswerCommand(
    Guid  UserId,
    Guid  LessonId,
    Guid  QuestionId,
    Guid? SelectedOptionId,
    string? TextAnswer,
    bool  IsCorrect
) : IRequest;

public sealed class SaveQuestionAnswerCommandHandler(
    ICongnexDbContext db,
    ILogger<SaveQuestionAnswerCommandHandler> logger)
    : IRequestHandler<SaveQuestionAnswerCommand>
{
    public async Task Handle(SaveQuestionAnswerCommand req, CancellationToken ct)
    {
        var exists = await db.UserQuestionAnswers
            .AnyAsync(a => a.UserId == req.UserId && a.QuestionId == req.QuestionId, ct);

        if (exists) return;

        db.UserQuestionAnswers.Add(new UserQuestionAnswer
        {
            UserId           = req.UserId,
            LessonId         = req.LessonId,
            QuestionId       = req.QuestionId,
            SelectedOptionId = req.SelectedOptionId,
            TextAnswer       = req.TextAnswer,
            IsCorrect        = req.IsCorrect,
            AnsweredAt       = DateTime.UtcNow
        });

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("Duplicate", StringComparison.OrdinalIgnoreCase) == true
                                        || ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true
                                        || ex.InnerException?.Message.Contains("1062") == true)
        {
            // Concurrent insert — answer already saved by another request; safe to ignore.
            logger.LogDebug("Duplicate answer ignored for user {UserId} question {QuestionId}", req.UserId, req.QuestionId);
        }
    }
}
