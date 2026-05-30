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
        // Fetch wrong answers with question details (including cached AI explanation)
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
                      a.AiExplanation,
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

        // Generate AI explanations only for answers that don't have one cached yet
        var needsAi = wrongAnswers.Where(w => string.IsNullOrEmpty(w.AiExplanation)).ToList();

        Dictionary<Guid, string> freshExplanations = [];
        if (needsAi.Count > 0)
        {
            var aiTasks = needsAi.Select(async w =>
            {
                string? userAnswer = w.TextAnswer;
                if (string.IsNullOrEmpty(userAnswer) && w.SelectedOptionId.HasValue)
                    optionTexts.TryGetValue(w.SelectedOptionId.Value, out userAnswer);

                var explanation = await xylaService.GenerateAnswerExplanationAsync(
                    w.QuestionText, w.CorrectAnswer ?? "", userAnswer, ct);
                return (w.QuestionId, explanation);
            });

            var results = await Task.WhenAll(aiTasks);
            freshExplanations = results.ToDictionary(r => r.QuestionId, r => r.explanation);

            // Persist explanations so subsequent calls use the cache
            var answerEntities = await db.UserQuestionAnswers
                .Where(a => a.LessonId == req.LessonId && a.UserId == req.UserId
                         && needsAi.Select(n => n.QuestionId).Contains(a.QuestionId))
                .ToListAsync(ct);

            foreach (var answer in answerEntities)
                if (freshExplanations.TryGetValue(answer.QuestionId, out var exp))
                    answer.AiExplanation = exp;

            await db.SaveChangesAsync(ct);
        }

        return wrongAnswers.Select(w =>
        {
            string? userAnswer = w.TextAnswer;
            if (string.IsNullOrEmpty(userAnswer) && w.SelectedOptionId.HasValue)
                optionTexts.TryGetValue(w.SelectedOptionId.Value, out userAnswer);

            var explanation = w.AiExplanation
                ?? freshExplanations.GetValueOrDefault(w.QuestionId)
                ?? $"A resposta correta é \"{w.CorrectAnswer}\".";

            return new WrongAnswerFlashcardDto(
                QuestionId:    w.QuestionId,
                QuestionText:  w.QuestionText,
                UserAnswer:    userAnswer,
                CorrectAnswer: w.CorrectAnswer ?? "",
                Explanation:   explanation);
        }).ToList();
    }
}
