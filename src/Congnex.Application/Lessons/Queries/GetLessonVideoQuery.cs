using System.Text.RegularExpressions;
using Congnex.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Application.Lessons.Queries;

public record LessonVideoDto(Guid Id, string YoutubeVideoId, string YoutubeUrl, string? Title);

public record GetLessonVideoQuery(Guid LessonId) : IRequest<LessonVideoDto?>;

public sealed class GetLessonVideoQueryHandler(ICongnexDbContext db)
    : IRequestHandler<GetLessonVideoQuery, LessonVideoDto?>
{
    public async Task<LessonVideoDto?> Handle(GetLessonVideoQuery req, CancellationToken ct)
    {
        // Primary: lesson_videos row saved during generation
        var video = await db.LessonVideos
            .Where(v => v.LessonId == req.LessonId)
            .OrderBy(v => v.Id)
            .Select(v => new LessonVideoDto(v.Id, v.YoutubeVideoId, v.YoutubeUrl, v.Title))
            .FirstOrDefaultAsync(ct);

        if (video is not null) return video;

        // Fallback for users whose interview ran before lesson_videos were being saved:
        // retrieve the video URL stored in user_interview_answers and extract the YouTube ID.
        var lesson = await db.Lessons
            .Where(l => l.Id == req.LessonId)
            .Select(l => new { l.UserId })
            .FirstOrDefaultAsync(ct);

        if (lesson is null) return null;

        var answer = await db.UserInterviewAnswers
            .Where(a => a.UserId == lesson.UserId && a.VideoUrl != "")
            .OrderByDescending(a => a.Id)
            .Select(a => new { a.VideoUrl, a.VideoCategory })
            .FirstOrDefaultAsync(ct);

        if (answer?.VideoUrl is null or "") return null;

        var youtubeId = ExtractYouTubeId(answer.VideoUrl);
        if (youtubeId is null) return null;

        return new LessonVideoDto(Guid.Empty, youtubeId, answer.VideoUrl, answer.VideoCategory);
    }

    private static string? ExtractYouTubeId(string url)
    {
        var match = Regex.Match(url, @"[?&]v=([A-Za-z0-9_-]{11})");
        if (match.Success) return match.Groups[1].Value;
        match = Regex.Match(url, @"youtu\.be/([A-Za-z0-9_-]{11})");
        return match.Success ? match.Groups[1].Value : null;
    }
}
