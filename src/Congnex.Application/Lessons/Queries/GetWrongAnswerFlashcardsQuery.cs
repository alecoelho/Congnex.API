using Congnex.Application.Common;
using Congnex.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Application.Lessons.Queries;

public record WrongAnswerFlashcardDto(
    Guid   QuestionId,
    string QuestionText,
    string? UserAnswer,
    string CorrectAnswer,
    string Explanation
);

public record GetWrongAnswerFlashcardsQuery(Guid LessonId, Guid UserId)
    : IRequest<List<WrongAnswerFlashcardDto>>;

public sealed class GetWrongAnswerFlashcardsQueryHandler(
    ICongnexDbContext db,
    IXylaService xylaService)
    : IRequestHandler<GetWrongAnswerFlashcardsQuery, List<WrongAnswerFlashcardDto>>
{
    public async Task<List<WrongAnswerFlashcardDto>> Handle(
        GetWrongAnswerFlashcardsQuery req, CancellationToken ct)
    {
        // Fetch wrong answers with question details
        var wrongAnswers = await db.UserQuestionAnswers
            .Where(a => a.LessonId == req.LessonId
                     && a.UserId   == req.UserId
                     && !a.IsCorrect)
            .Join(db.Questions,
                  a => a.QuestionId,
                  q => q.Id,
                  (a, q) => new
                  {
                      a.QuestionId,
                      q.QuestionText,
                      q.CorrectAnswer,
                      a.TextAnswer,
                      a.SelectedOptionId,
                  })
            .ToListAsync(ct);

        if (wrongAnswers.Count == 0)
            return [];

        // Resolve option text for multiple-choice answers
        var optionIds = wrongAnswers
            .Where(w => w.SelectedOptionId.HasValue)
            .Select(w => w.SelectedOptionId!.Value)
            .Distinct()
            .ToList();

        var optionTexts = optionIds.Count > 0
            ? await db.QuestionOptions
                .Where(o => optionIds.Contains(o.Id))
                .ToDictionaryAsync(o => o.Id, o => o.OptionText, ct)
            : new Dictionary<Guid, string>();

        // Generate AI explanations concurrently
        var tasks = wrongAnswers.Select(async w =>
        {
            string? userAnswer = w.TextAnswer;
            if (string.IsNullOrEmpty(userAnswer) && w.SelectedOptionId.HasValue)
                optionTexts.TryGetValue(w.SelectedOptionId.Value, out userAnswer);

            var explanation = await xylaService.GenerateAnswerExplanationAsync(
                w.QuestionText,
                w.CorrectAnswer ?? "",
                userAnswer,
                ct);

            return new WrongAnswerFlashcardDto(
                QuestionId:   w.QuestionId,
                QuestionText: w.QuestionText,
                UserAnswer:   userAnswer,
                CorrectAnswer: w.CorrectAnswer ?? "",
                Explanation:  explanation);
        });

        return [.. await Task.WhenAll(tasks)];
    }
}
