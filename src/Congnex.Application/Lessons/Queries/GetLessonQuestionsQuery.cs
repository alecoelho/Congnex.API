using Congnex.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Application.Lessons.Queries;

public record QuestionOptionDto(Guid Id, string Text, string? ImageUrl, string? AudioUrl, bool IsCorrect, int OrderIndex);
public record QuestionPairDto(Guid Id, string LeftText, string RightText, string? LeftAudioUrl, string? RightAudioUrl, int OrderIndex);

public record QuestionDto(
    Guid            Id,
    string          Type,
    string?         Label,
    string          QuestionText,
    string?         Prompt,
    string?         Instruction,
    string?         CorrectAnswer,
    string?         AudioText,
    string?         ImageUrl,
    string          Difficulty,
    int             OrderIndex,
    List<QuestionOptionDto> Options,
    List<QuestionPairDto> Pairs,
    bool            IsAnswered,
    bool?           UserIsCorrect,
    string?         UserTextAnswer);

public record GetLessonQuestionsQuery(Guid LessonId, Guid? UserId = null) : IRequest<List<QuestionDto>>;

public sealed class GetLessonQuestionsQueryHandler(ICongnexDbContext db)
    : IRequestHandler<GetLessonQuestionsQuery, List<QuestionDto>>
{
    public async Task<List<QuestionDto>> Handle(GetLessonQuestionsQuery req, CancellationToken ct)
    {
        var exists = await db.Lessons.AnyAsync(l => l.Id == req.LessonId, ct);
        if (!exists) throw new KeyNotFoundException("Lesson not found.");

        var questions = await db.Questions
            .Where(q => q.LessonId == req.LessonId)
            .OrderBy(q => q.OrderIndex)
            .Include(q => q.Options.OrderBy(o => o.OrderIndex))
            .Include(q => q.Pairs.OrderBy(p => p.OrderIndex))
            .ToListAsync(ct);

        // Fetch existing user answers for this lesson
        Dictionary<Guid, (bool IsCorrect, string? TextAnswer)> answeredMap = [];
        if (req.UserId.HasValue)
        {
            answeredMap = await db.UserQuestionAnswers
                .Where(a => a.LessonId == req.LessonId && a.UserId == req.UserId.Value)
                .ToDictionaryAsync(
                    a => a.QuestionId,
                    a => (a.IsCorrect, a.TextAnswer),
                    ct);
        }

        return questions.Select(q =>
        {
            var isAnswered    = answeredMap.TryGetValue(q.Id, out var ans);
            bool? userCorrect = isAnswered ? ans.IsCorrect : null;
            return new QuestionDto(
                Id:             q.Id,
                Type:           q.Type,
                Label:          q.Label,
                QuestionText:   q.QuestionText,
                Prompt:         q.Prompt,
                Instruction:    q.Instruction,
                CorrectAnswer:  q.CorrectAnswer,
                AudioText:      q.AudioText,
                ImageUrl:       q.ImageUrl,
                Difficulty:     q.Difficulty,
                OrderIndex:     q.OrderIndex,
                Options:        q.Options.Select(o => new QuestionOptionDto(o.Id, o.OptionText, o.OptionImageUrl, o.OptionAudioUrl, o.IsCorrect, o.OrderIndex)).ToList(),
                Pairs:          q.Pairs.Select(p => new QuestionPairDto(p.Id, p.LeftText, p.RightText, p.LeftAudioUrl, p.RightAudioUrl, p.OrderIndex)).ToList(),
                IsAnswered:     isAnswered,
                UserIsCorrect:  userCorrect,
                UserTextAnswer: isAnswered ? ans.TextAnswer : null);
        }).ToList();
    }
}
