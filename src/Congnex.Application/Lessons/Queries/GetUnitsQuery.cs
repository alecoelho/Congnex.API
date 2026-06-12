using Congnex.Application.Interfaces;
using Congnex.Application.Lessons.Dtos;
using Congnex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Application.Lessons.Queries;

public record GetUnitsQuery(Guid UserId) : IRequest<List<UnitDto>>;

public sealed class GetUnitsQueryHandler(ICongnexDbContext db)
    : IRequestHandler<GetUnitsQuery, List<UnitDto>>
{
    public async Task<List<UnitDto>> Handle(GetUnitsQuery req, CancellationToken ct)
    {
        var units = await db.Units
            .OrderBy(u => u.OrderIndex)
            .Include(u => u.Lessons.OrderBy(l => l.OrderIndex))
            .ToListAsync(ct);

        // Para cada unidade: prefere lições do usuário; senão usa globais (userId = null)
        foreach (var unit in units)
        {
            var userLessons    = unit.Lessons.Where(l => l.UserId == req.UserId).ToList();
            var genericLessons = unit.Lessons.Where(l => l.UserId == null).ToList();

            unit.Lessons = (userLessons.Count > 0 ? userLessons : genericLessons)
                .OrderBy(l => l.OrderIndex).ToList();
        }

        // Todas as unidades são retornadas — unidades sem lições ficam com lista vazia (locked)
        var progressMap = await db.UserProgress
            .Where(p => p.UserId == req.UserId)
            .ToDictionaryAsync(p => p.LessonId, ct);

        // Determina qual lição é "current" (primeira não concluída)
        bool foundCurrent = false;

        return units.Select(unit => new UnitDto(
            Id:          unit.Id,
            Title:       unit.Title,
            Description: unit.Description,
            OrderIndex:  unit.OrderIndex,
            Lessons: unit.Lessons.Select(lesson =>
            {
                if (progressMap.TryGetValue(lesson.Id, out var progress) &&
                    progress.Status == LessonStatus.Completed)
                {
                    return new LessonDto(lesson.Id, lesson.Title, lesson.OrderIndex,
                        lesson.XpReward, "completed", progress.Score, progress.CompletedAt);
                }

                if (!foundCurrent)
                {
                    foundCurrent = true;
                    return new LessonDto(lesson.Id, lesson.Title, lesson.OrderIndex,
                        lesson.XpReward, "current", 0, null);
                }

                return new LessonDto(lesson.Id, lesson.Title, lesson.OrderIndex,
                    lesson.XpReward, "locked", 0, null);
            }).ToList()
        )).ToList();
    }
}
