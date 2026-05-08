using Congnex.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Application.Lessons.Queries;

public record QuestionDto(
    Guid          Id,
    string        Type,
    string        Prompt,
    List<string>  CorrectAnswers,
    string?       Options,
    string?       MediaUrl,
    int           OrderIndex);

public record GetLessonQuestionsQuery(Guid LessonId) : IRequest<List<QuestionDto>>;

public sealed class GetLessonQuestionsQueryHandler(ICongnexDbContext db)
    : IRequestHandler<GetLessonQuestionsQuery, List<QuestionDto>>
{
    public async Task<List<QuestionDto>> Handle(GetLessonQuestionsQuery req, CancellationToken ct)
    {
        var exists = await db.Lessons.AnyAsync(l => l.Id == req.LessonId, ct);
        if (!exists) throw new KeyNotFoundException("Lesson not found.");

        return await db.Questions
            .Where(q => q.LessonId == req.LessonId)
            .OrderBy(q => q.OrderIndex)
            .Select(q => new QuestionDto(
                q.Id,
                q.Type.ToString(),
                q.Prompt,
                q.CorrectAnswers,
                q.Options,
                q.MediaUrl,
                q.OrderIndex))
            .ToListAsync(ct);
    }
}
