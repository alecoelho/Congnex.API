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

        // When answers were already saved incrementally, req.Answers may be empty.
        // In that case, read existing answers from DB to compute stats.
        int correctCount;
        int totalCount;
        List<Guid> correctIds;

        if (req.Answers.Count > 0)
        {
            // Save any answers not yet in DB (idempotent: skip existing)
            var existingIds = await db.UserQuestionAnswers
                .Where(a => a.LessonId == req.LessonId && a.UserId == req.UserId)
                .Select(a => a.QuestionId)
                .ToListAsync(ct);

            foreach (var a in req.Answers.Where(a => !existingIds.Contains(a.QuestionId)))
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

            correctCount = req.Answers.Count(a => a.IsCorrect);
            totalCount   = req.Answers.Count;
            correctIds   = req.Answers.Where(a => a.IsCorrect).Select(a => a.QuestionId).ToList();
        }
        else
        {
            // Answers already saved incrementally — read from DB
            var dbAnswers = await db.UserQuestionAnswers
                .Where(a => a.LessonId == req.LessonId && a.UserId == req.UserId)
                .Select(a => new { a.QuestionId, a.IsCorrect })
                .ToListAsync(ct);

            correctCount = dbAnswers.Count(a => a.IsCorrect);
            totalCount   = dbAnswers.Count;
            correctIds   = dbAnswers.Where(a => a.IsCorrect).Select(a => a.QuestionId).ToList();
        }

        progress.Status         = LessonStatus.Completed;
        progress.Score          = totalCount > 0 ? (int)Math.Round((double)correctCount / totalCount * 100) : req.Score;
        progress.CorrectAnswers = correctCount;
        progress.TotalQuestions = totalCount;
        progress.XpEarned       = lesson.XpReward;
        progress.CompletedAt    = DateTime.UtcNow;

        // Create/reset ReviewItems for correct answers
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

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("Duplicate", StringComparison.OrdinalIgnoreCase) == true
                                        || ex.InnerException?.Message.Contains("1062") == true)
        {
            // Concurrent completion call — ignore duplicate key violations
        }

        return new CompleteLessonResult(xpEarned, user.Xp, user.Streak);
    }
}
