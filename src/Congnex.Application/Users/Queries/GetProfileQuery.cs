using Congnex.Application.Interfaces;
using Congnex.Application.Users.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Application.Users.Queries;

public record GetProfileQuery(Guid UserId) : IRequest<ProfileDto>;

public sealed class GetProfileQueryHandler(ICongnexDbContext db)
    : IRequestHandler<GetProfileQuery, ProfileDto>
{
    public async Task<ProfileDto> Handle(GetProfileQuery req, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([req.UserId], ct)
            ?? throw new KeyNotFoundException("User not found.");

        return new ProfileDto(
            UserId:    user.Id,
            FirstName: user.FirstName,
            LastName:  user.LastName,
            Email:     user.Email,
            Plan:         user.Plan.ToString(),
            Xp:           user.Xp,
            Streak:       user.Streak,
            Lives:        user.Lives,
            Energy:       user.Energy,
            MaxEnergy:    user.MaxEnergy,
            Level:        "beginner",
            DailyMinutes: user.DailyMinutes,
            Motivations:  user.Motivations ?? "[]");
    }
}
