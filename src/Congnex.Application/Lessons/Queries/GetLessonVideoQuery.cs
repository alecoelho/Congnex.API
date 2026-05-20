using Congnex.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Application.Lessons.Queries;

public record GetLessonVideoQuery(Guid LessonId) : IRequest<LessonVideoDto?>;

public record LessonVideoDto(string YoutubeVideoId, string YoutubeUrl, string Title);

public sealed class GetLessonVideoQueryHandler(ICongnexDbContext db)
    : IRequestHandler<GetLessonVideoQuery, LessonVideoDto?>
{
    public async Task<LessonVideoDto?> Handle(GetLessonVideoQuery req, CancellationToken ct)
    {
        var video = await db.LessonVideos
            .Where(v => v.LessonId == req.LessonId)
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => new LessonVideoDto(v.YoutubeVideoId, v.YoutubeUrl, v.Title))
            .FirstOrDefaultAsync(ct);

        return video;
    }
}
