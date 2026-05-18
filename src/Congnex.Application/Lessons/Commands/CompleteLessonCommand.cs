using Congnex.Application.Common;
using Congnex.Domain.Entities;
using Congnex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Application.Lessons.Commands;

public record AnswerDto(Guid QuestionId, Guid? SelectedOptionId, string? TextAnswer, bool IsCorrect, int TimeSpentSeconds = 0);

public record CompleteLessonResult(int XpEarned, int TotalXp, int NewStreak);

public record CompleteLessonCommand(
    Guid         UserId,
    Guid         LessonId,
    int          Score,
    List<AnswerDto> Answers) : IRequest<CompleteLessonResult>;

public sealed class CompleteLessonCommandHandler(ICongnexDbContext db)
    : IRequestHandler<CompleteLessonCommand, CompleteLessonResult>
{
    public async Task<CompleteLessonResult> Handle(CompleteLessonCommand req, CancellationToken ct)
    {
        var lesson = await db.Lessons.FindAsync([req.LessonId], ct)
            ?? throw new KeyNotFoundException("Lesson not found.");

        var user = await db.Users.FindAsync([req.UserId], ct)
            ?? throw new KeyNotFoundException("User not found.");

        // Upsert progress
        var progress = await db.UserProgress.FirstOrDefaultAsync(
            p => p.UserId == req.UserId && p.LessonId == req.LessonId, ct);

        if (progress is null)
        {
            progress = new UserProgress
            {
                UserId   = req.UserId,
                LessonId = req.LessonId,
            };
            db.UserProgress.Add(progress);
        }

        progress.Status         = LessonStatus.Completed;
        progress.Score          = req.Score;
        progress.CorrectAnswers = req.Answers.Count(a => a.IsCorrect);
        progress.TotalQuestions  = req.Answers.Count;
        progress.XpEarned       = lesson.XpReward;
        progress.CompletedAt    = DateTime.UtcNow;

        // Persist answers
        foreach (var a in req.Answers)
        {
            db.UserQuestionAnswers.Add(new UserQuestionAnswer
            {
                UserId           = req.UserId,
                LessonId         = req.LessonId,
                QuestionId       = a.QuestionId,
                SelectedOptionId = a.SelectedOptionId,
                TextAnswer       = a.TextAnswer,
                IsCorrect        = a.IsCorrect,
                TimeSpentSeconds = a.TimeSpentSeconds,
                AnsweredAt       = DateTime.UtcNow
            });
        }

        // Create/reset ReviewItems for correct answers
        var correctIds = req.Answers.Where(a => a.IsCorrect).Select(a => a.QuestionId).ToList();
        foreach (var qId in correctIds)
        {
            var existing = await db.ReviewItems.FirstOrDefaultAsync(
                r => r.UserId == req.UserId && r.QuestionId == qId, ct);
            if (existing is null)
            {
                db.ReviewItems.Add(new ReviewItem
                {
                    UserId     = req.UserId,
                    QuestionId = qId,
                    Source     = "lesson",
                    DueDate    = DateTime.UtcNow.AddDays(1)
                });
            }
        }

        // Award XP + update streak
        int xpEarned = lesson.XpReward;
        user.Xp += xpEarned;

        var today     = DateTime.UtcNow.Date;
        var lastLesson = user.LastLessonAt?.Date;
        if (lastLesson == null || lastLesson < today.AddDays(-1))
            user.Streak = 1;                           // streak broken — reset
        else if (lastLesson < today)
            user.Streak++;                             // new day — increment

        user.LastLessonAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return new CompleteLessonResult(xpEarned, user.Xp, user.Streak);
    }
}
