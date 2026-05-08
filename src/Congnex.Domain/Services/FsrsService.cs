using Congnex.Domain.Enums;

namespace Congnex.Domain.Services;

/// <summary>FSRS-5 spaced repetition algorithm (pure domain logic, no I/O).</summary>
public sealed class FsrsService
{
    // FSRS-5 default weights
    private static readonly float[] W =
    [
        0.4072f, 1.1829f, 3.1262f, 15.4722f,
        7.2102f, 0.5316f, 1.0651f, 0.0589f,
        1.5330f, 0.1544f, 1.0070f, 1.9395f,
        0.1100f, 0.2900f, 2.2700f, 0.1200f, 2.9898f
    ];

    private const float DECAY     = -0.5f;
    private const float FACTOR    = 0.9f;          // (0.9)^(1/DECAY) - 1
    private const float REQUEST_R = 0.9f;          // target retrievability

    public record ScheduleResult(
        float     Stability,
        float     Difficulty,
        int       Reps,
        int       Lapses,
        FsrsCardState State,
        DateTime  DueDate);

    public ScheduleResult Schedule(
        float         stability,
        float         difficulty,
        int           reps,
        int           lapses,
        FsrsCardState state,
        FsrsRating    rating,
        DateTime      now)
    {
        float newStability;
        float newDifficulty;
        int   newReps    = reps + 1;
        int   newLapses  = lapses;
        FsrsCardState newState;

        if (state == FsrsCardState.New)
        {
            (newStability, newDifficulty) = InitNew(rating);
            newState = rating == FsrsRating.Again ? FsrsCardState.Learning : FsrsCardState.Review;
        }
        else
        {
            float elapsed  = Math.Max(0, (float)(now - now).TotalDays); // reset when called fresh
            float retrievability = Retrievability(stability, 0);        // same day review

            if (state == FsrsCardState.Review)
            {
                if (rating == FsrsRating.Again)
                {
                    newStability  = ForgettingStability(difficulty, stability, retrievability);
                    newDifficulty = NextDifficulty(difficulty, rating);
                    newLapses++;
                    newState = FsrsCardState.Relearning;
                }
                else
                {
                    newStability  = RecallStability(difficulty, stability, retrievability, rating);
                    newDifficulty = NextDifficulty(difficulty, rating);
                    newState = FsrsCardState.Review;
                }
            }
            else // Learning / Relearning
            {
                newStability  = rating == FsrsRating.Again
                    ? W[11]
                    : stability + W[11] / 2f;
                newDifficulty = difficulty;
                newState      = FsrsCardState.Review;
            }
        }

        newStability  = Math.Max(0.001f, newStability);
        newDifficulty = Math.Clamp(newDifficulty, 1f, 10f);

        int intervalDays = NextInterval(newStability);
        DateTime dueDate = now.AddDays(intervalDays);

        return new ScheduleResult(newStability, newDifficulty, newReps, newLapses, newState, dueDate);
    }

    private (float stability, float difficulty) InitNew(FsrsRating rating)
    {
        int r  = (int)rating - 1;         // 0–3
        float s = W[r];
        float d = W[4] - MathF.Exp(W[5] * (r - 1)) + 1;
        return (s, Math.Clamp(d, 1f, 10f));
    }

    private float RecallStability(float d, float s, float r, FsrsRating rating)
    {
        float hardPenalty = rating == FsrsRating.Hard ? W[15] : 1f;
        float easyBonus   = rating == FsrsRating.Easy ? W[16] : 1f;
        return s * (MathF.Exp(W[8]) * (11 - d) * MathF.Pow(s, -W[9]) *
                   (MathF.Exp((1 - r) * W[10]) - 1) * hardPenalty * easyBonus);
    }

    private float ForgettingStability(float d, float s, float r) =>
        W[11] * MathF.Pow(d, -W[12]) * (MathF.Pow(s + 1, W[13]) - 1) * MathF.Exp((1 - r) * W[14]);

    private float NextDifficulty(float d, FsrsRating rating)
    {
        float delta = -W[6] * ((int)rating - 3);
        float mean  = W[4] - MathF.Exp(W[5] * (d - 3) * 0.5f);
        return Math.Clamp(d + delta * (10 - d) / 9f + (mean - d) * 0.1f, 1f, 10f);
    }

    private static float Retrievability(float stability, float elapsedDays) =>
        MathF.Pow(1 + FACTOR * elapsedDays / stability, DECAY);

    private static int NextInterval(float stability)
    {
        float interval = stability / FACTOR * (MathF.Pow(REQUEST_R, 1f / DECAY) - 1);
        return Math.Max(1, (int)MathF.Round(interval));
    }
}
