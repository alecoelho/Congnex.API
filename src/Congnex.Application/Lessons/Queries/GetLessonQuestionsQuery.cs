using Congnex.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Application.Lessons.Queries;

public record QuestionDto(
    Guid          Id,
    string        Type,
    string        Label,
    string        Question,
    object        Data,
    int           OrderIndex);

public record GetLessonQuestionsQuery(Guid LessonId) : IRequest<List<QuestionDto>>;

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
            .ToListAsync(ct);

        return questions.Select(q =>
        {
            // Options stores the full JSON "data" object for the frontend
            // MediaUrl stores the "label" (e.g. "PALAVRA NOVA", "TRADUÇÃO")
            var data = q.Options is not null
                ? System.Text.Json.JsonSerializer.Deserialize<object>(q.Options)!
                : new { };

            return new QuestionDto(
                Id:         q.Id,
                Type:       q.Type.ToString(),
                Label:      q.MediaUrl ?? "QUESTÃO",
                Question:   q.Prompt,
                Data:       data,
                OrderIndex: q.OrderIndex);
        }).ToList();
    }
}
