using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Congnex.Application.Interfaces;
using Congnex.Domain.Entities;
using Congnex.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace Congnex.Infrastructure.Services;

public class XylaService : IXylaService
{
    private readonly Kernel _kernel;
    private readonly IDbContextFactory<CongnexDbContext> _dbFactory;
    private readonly ILogger<XylaService> _logger;
    private readonly IMemoryCache _cache;
    private readonly IVideoSearchProvider _videoSearch;

    private const string PlanStart       = "<xyla_plan>";
    private const string PlanEnd         = "</xyla_plan>";
    private const string PlanReadyPrefix = "[PLAN_READY:";

    private const int MaxHistoryMessages = 20; // Keep last 20 messages (10 exchanges)

    private static readonly TimeSpan SessionTtl = TimeSpan.FromMinutes(30);

    private static readonly string AgentInstructions = LoadPrompt("xyla-system.md");

    // ── Session context (cached per session) ───────────────────────────────────

    private record SessionContext(ChatHistory History, string? FirstName, int? Age, string? Motivations, int DailyMinutes);

    public XylaService(
        Kernel kernel,
        IDbContextFactory<CongnexDbContext> dbFactory,
        ILogger<XylaService> logger,
        IMemoryCache cache,
        IVideoSearchProvider videoSearch)
    {
        _kernel      = kernel;
        _dbFactory   = dbFactory;
        _logger      = logger;
        _cache       = cache;
        _videoSearch  = videoSearch;
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    public async Task<Guid> StartSessionAsync(Guid userId, CancellationToken ct = default)
    {
        var sessionId = Guid.NewGuid();

        // Fetch user data once at session start
        string? firstName = null;
        int? age = null;
        string? motivations = null;
        int dailyMinutes = 10;

        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            var user = await db.Users
                .Where(u => u.Id == userId)
                .Select(u => new { u.FirstName, u.Motivations, u.DailyMinutes, u.DateOfBirth })
                .FirstOrDefaultAsync(ct);

            if (user is not null)
            {
                firstName = user.FirstName;
                motivations = user.Motivations;
                dailyMinutes = user.DailyMinutes;
                age = user.DateOfBirth.HasValue
                    ? (int)((DateTime.UtcNow - user.DateOfBirth.Value).TotalDays / 365.25)
                    : null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not fetch user data at session start for {UserId}", userId);
        }

        var context = new SessionContext(new ChatHistory(), firstName, age, motivations, dailyMinutes);
        _cache.Set(SessionKey(sessionId), context, SessionTtl);

        // Persist conversation start
        try
        {
            await using var db2 = await _dbFactory.CreateDbContextAsync(ct);
            db2.XylaConversations.Add(new XylaConversation
            {
                UserId = userId,
                SessionId = sessionId,
            });
            await db2.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not persist conversation start for session {SessionId}", sessionId);
        }

        return sessionId;
    }

    public async IAsyncEnumerable<string> StreamMessageAsync(
        Guid sessionId,
        Guid userId,
        string message,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (!_cache.TryGetValue(SessionKey(sessionId), out SessionContext? session) || session is null)
        {
            yield return "A sessão não foi encontrada ou já foi encerrada.";
            yield break;
        }

        var chatHistory = session.History;
        var safeMessage = PromptSanitizer.Sanitize(message, maxLength: 600);
        chatHistory.AddUserMessage(safeMessage);

        // Persist user message (fire-and-forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(CancellationToken.None);
                var conv = await db.XylaConversations.FirstOrDefaultAsync(c => c.SessionId == sessionId);
                if (conv is not null)
                {
                    db.XylaMessages.Add(new XylaMessage
                    {
                        ConversationId = conv.Id,
                        Role = "user",
                        Content = safeMessage,
                    });
                    await db.SaveChangesAsync();
                }
            }
            catch { /* non-critical */ }
        });

        // Trim history to limit context growth
        TrimHistory(chatHistory);

        // Build personalized instructions from cached session data (no DB query needed)
        var instructions = BuildPersonalizedInstructions(session.FirstName, session.Age, session.Motivations, session.DailyMinutes);

#pragma warning disable SKEXP0001, SKEXP0110, CS0618
        var agent = new ChatCompletionAgent
        {
            Kernel       = _kernel,
            Name         = "XylaAgent",
            Instructions = instructions,
            Arguments    = new KernelArguments(new AzureOpenAIPromptExecutionSettings
            {
                ServiceId   = "xyla",
                MaxTokens   = 1500,
                Temperature = 0.4
            })
        };
#pragma warning restore SKEXP0001, SKEXP0110, CS0618

        var fullResponse = new StringBuilder();
        var channel      = Channel.CreateUnbounded<string>(new UnboundedChannelOptions { SingleWriter = true });

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(60));

        _ = Task.Run(async () =>
        {
            try
            {
                bool insidePlan = false;
                int streamedUpTo = 0;

#pragma warning disable SKEXP0110, CS0618
                await foreach (var chunk in agent.InvokeStreamingAsync(
                    chatHistory, cancellationToken: timeoutCts.Token))
                {
                    var content = chunk.Content ?? string.Empty;
                    if (string.IsNullOrEmpty(content)) continue;

                    fullResponse.Append(content);

                    if (insidePlan) continue; // Don't stream plan content

                    var currentFull = fullResponse.ToString();
                    var planIdx = currentFull.IndexOf(PlanStart, StringComparison.Ordinal);

                    if (planIdx >= 0)
                    {
                        insidePlan = true;
                        // Stream remaining visible text before plan tag
                        if (planIdx > streamedUpTo)
                        {
                            var remaining = currentFull[streamedUpTo..planIdx];
                            await channel.Writer.WriteAsync(remaining, CancellationToken.None);
                            streamedUpTo = planIdx;
                        }
                        continue;
                    }

                    // Safety buffer to avoid streaming partial <xyla_plan> tag
                    var safeEnd = currentFull.Length - PlanStart.Length;
                    if (safeEnd > streamedUpTo)
                    {
                        var toStream = currentFull[streamedUpTo..safeEnd];
                        await channel.Writer.WriteAsync(toStream, CancellationToken.None);
                        streamedUpTo = safeEnd;
                    }
                }
#pragma warning restore SKEXP0110, CS0618

                var raw = fullResponse.ToString();
                var (visible, planJson) = ExtractPlan(raw);

                // Stream any remaining visible text held back by safety buffer
                if (!insidePlan && streamedUpTo < raw.Length)
                {
                    var leftover = raw[streamedUpTo..];
                    if (!string.IsNullOrEmpty(leftover))
                        await channel.Writer.WriteAsync(leftover, CancellationToken.None);
                }

                chatHistory.AddAssistantMessage(visible);
                _cache.Set(SessionKey(sessionId), session, SessionTtl);

                // Persist assistant message
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await using var db = await _dbFactory.CreateDbContextAsync(CancellationToken.None);
                        var conv = await db.XylaConversations.FirstOrDefaultAsync(c => c.SessionId == sessionId);
                        if (conv is not null)
                        {
                            db.XylaMessages.Add(new XylaMessage
                            {
                                ConversationId = conv.Id,
                                Role = "assistant",
                                Content = visible,
                            });

                            if (planJson is not null)
                            {
                                conv.IsCompleted = true;
                                conv.CompletedAt = DateTime.UtcNow;
                                var doc = JsonDocument.Parse(planJson).RootElement;
                                conv.DetectedLevel = doc.TryGetProperty("cefr_level", out var l) ? l.GetString() : null;
                                conv.DetectedGoal = doc.TryGetProperty("student_goal", out var g) ? g.GetString() : null;
                                conv.DetectedInterest = doc.TryGetProperty("student_interest", out var i) ? i.GetString() : null;
                            }

                            await db.SaveChangesAsync();
                        }
                    }
                    catch { /* non-critical */ }
                });

                if (planJson is not null)
                {
                    _cache.Remove(SessionKey(sessionId));

                    var videos = await BuildVideoListAsync(planJson, CancellationToken.None);

                    await SaveAnswersAsync(userId, planJson, videos, CancellationToken.None);

                    // Signal plan completion (without video links — frontend navigates directly)
                    await channel.Writer.WriteAsync(PlanReadyPrefix + "[]", CancellationToken.None);
                }
            }
            catch (OperationCanceledException)
            {
                if (!ct.IsCancellationRequested)
                    _logger.LogWarning("Xyla agent timed out for session {SessionId}", sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Xyla agent failed for session {SessionId}", sessionId);
                await channel.Writer.WriteAsync(
                    "\n\n*Desculpe, tive um problema. Pode tentar novamente?*", CancellationToken.None);
            }
            finally
            {
                channel.Writer.Complete();
            }
        }, CancellationToken.None);

        await foreach (var token in channel.Reader.ReadAllAsync(ct))
            yield return token;
    }

    // ── Context trimming ───────────────────────────────────────────────────────

    private static void TrimHistory(ChatHistory history)
    {
        // Keep system message (if any) + last MaxHistoryMessages
        while (history.Count > MaxHistoryMessages)
        {
            history.RemoveAt(0);
        }
    }

    // ── Persist results ────────────────────────────────────────────────────────

    private async Task SaveAnswersAsync(
        Guid userId,
        string planJson,
        List<VideoItem> videos,
        CancellationToken ct)
    {
        try
        {
            var doc   = JsonDocument.Parse(planJson).RootElement;
            var level = doc.TryGetProperty("cefr_level", out var lvl) ? lvl.GetString() ?? "A1" : "A1";
            var goal  = doc.TryGetProperty("student_goal", out var g) ? g.GetString() : null;
            var interest = doc.TryGetProperty("student_interest", out var si) ? si.GetString() : null;

            await using var db = await _dbFactory.CreateDbContextAsync(ct);

            // Update user's motivations, interest and level from detected data
            var user = await db.Users.FindAsync([userId], ct);
            if (user is not null)
            {
                if (goal is not null) user.Motivations = goal;
                if (interest is not null) user.Interest = interest;
                user.EnglishLevel = level;

                // Save hobbies and main difficulty
                if (doc.TryGetProperty("student_hobbies", out var hb))
                    user.Hobbies = hb.GetString();
                if (doc.TryGetProperty("main_difficulty", out var md))
                    user.MainDifficulty = md.GetString();

                // Parse confidence score
                if (doc.TryGetProperty("confidence_score", out var cs))
                {
                    var conf = cs.GetString();
                    user.LevelConfidence = conf switch
                    {
                        "high" => 0.9f,
                        "medium" => 0.6f,
                        "low" => 0.3f,
                        _ => 0.5f
                    };
                }
            }

            // Save the first video to lesson_videos (linked to lesson 1 of unit 1)
            if (videos.Count > 0)
            {
                var firstVideo = videos[0];
                var firstLesson = await db.Lessons
                    .Include(l => l.Unit)
                    .Where(l => l.Unit.OrderIndex == 1 && l.OrderIndex == 1)
                    .FirstOrDefaultAsync(ct);

                if (firstLesson is not null)
                {
                    // Extract YouTube video ID from URL
                    var videoId = ExtractYoutubeVideoId(firstVideo.Url);

                    // Extract target structures from plan
                    string? targetStructuresJson = null;
                    if (doc.TryGetProperty("target_structures", out var ts) && ts.ValueKind == JsonValueKind.Array)
                    {
                        targetStructuresJson = ts.GetRawText();
                    }

                    db.LessonVideos.Add(new LessonVideo
                    {
                        LessonId           = firstLesson.Id,
                        YoutubeVideoId     = videoId ?? "",
                        YoutubeUrl         = firstVideo.Url,
                        Title              = firstVideo.Label,
                        TargetStructures   = targetStructuresJson,
                        MatchScore         = firstVideo.MatchScore,
                        MatchConfidence    = firstVideo.Confidence,
                        MatchedStructures  = firstVideo.MatchedStructures is not null
                            ? JsonSerializer.Serialize(firstVideo.MatchedStructures)
                            : null,
                        SearchSource       = "brave_search",
                        Language           = "en",
                        DurationSeconds    = 0,
                    });
                }
            }

            // Also save to user_interview_answers for history
            foreach (var v in videos)
            {
                db.UserInterviewAnswers.Add(new UserInterviewAnswer
                {
                    UserId        = userId,
                    EnglishLevel  = level,
                    VideoUrl      = v.Url,
                    VideoCategory = v.Category,
                });
            }

            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Saved interview results for user {UserId} at level {Level}, goal: {Goal}, interest: {Interest}",
                userId, level, goal ?? "unknown", interest ?? "unknown");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save interview answers for user {UserId}", userId);
        }
    }

    private static string? ExtractYoutubeVideoId(string url)
    {
        // youtube.com/shorts/VIDEO_ID
        if (url.Contains("youtube.com/shorts/"))
        {
            var parts = url.Split("youtube.com/shorts/");
            if (parts.Length > 1)
            {
                var id = parts[1].Split('?')[0].Split('/')[0].Split('#')[0];
                if (!string.IsNullOrEmpty(id)) return id;
            }
        }

        // youtube.com/watch?v=VIDEO_ID
        var uri = Uri.TryCreate(url, UriKind.Absolute, out var u) ? u : null;
        if (uri is null) return null;
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        return query["v"];
    }

    // ── Plan extraction ────────────────────────────────────────────────────────

    private static (string visible, string? planJson) ExtractPlan(string raw)
    {
        var si = raw.IndexOf(PlanStart, StringComparison.Ordinal);
        var ei = raw.IndexOf(PlanEnd,   StringComparison.Ordinal);
        if (si < 0 || ei <= si) return (raw, null);

        var planJson = raw[(si + PlanStart.Length)..ei].Trim();
        var visible  = (raw[..si] + raw[(ei + PlanEnd.Length)..]).Trim();
        return (visible, planJson);
    }

    // ── YouTube video list (Brave Search → real watch URLs) ───────────────────

    private static readonly JsonSerializerOptions CamelCaseOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private async Task<List<VideoItem>> BuildVideoListAsync(string planJson, CancellationToken ct)
    {
        try
        {
            var doc = JsonDocument.Parse(planJson).RootElement;
            var queries = doc.GetProperty("video_queries").EnumerateArray().ToList();

            // Extract target structures
            var structures = new List<string>();
            if (doc.TryGetProperty("target_structures", out var ts) && ts.ValueKind == JsonValueKind.Array)
            {
                foreach (var s in ts.EnumerateArray())
                {
                    var text = s.GetString();
                    if (!string.IsNullOrEmpty(text)) structures.Add(text);
                }
            }

            // Extract context
            var interest = doc.TryGetProperty("student_interest", out var si) ? si.GetString() : null;
            var hobbies = doc.TryGetProperty("student_hobbies", out var hb) ? hb.GetString() : null;
            var level = doc.TryGetProperty("cefr_level", out var lvl) ? lvl.GetString() ?? "A1" : "A1";
            var baseQuery = queries.FirstOrDefault().GetProperty("query").GetString() ?? "";

            var context = new VideoSearchContext(structures, interest, hobbies, level, baseQuery);
            var candidates = await _videoSearch.SearchAndRankAsync(context, ct);

            if (candidates.Count == 0)
            {
                // Fallback
                var fallbackUrl = "https://www.youtube.com/results?search_query=" + Uri.EscapeDataString(baseQuery + " shorts");
                return [new VideoItem("Sua Profissão", fallbackUrl, baseQuery)];
            }

            // Take the best candidate
            var best = candidates[0];
            _logger.LogInformation("Selected video: {Title} (score: {Score}, confidence: {Confidence})",
                best.Title, best.MatchScore, best.Confidence);

            return [new VideoItem(
                queries.FirstOrDefault().GetProperty("topic").GetString() ?? "Video",
                best.Url,
                best.Title,
                best.MatchScore,
                best.Confidence,
                best.MatchedStructures
            )];
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "BuildVideoListAsync failed");
            return [];
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static string SessionKey(Guid sessionId) => $"xyla:{sessionId}";

    // ── Personalized instructions builder ──────────────────────────────────────

    private static string BuildPersonalizedInstructions(string? firstName, int? age, string? motivations, int dailyMinutes)
    {
        var studentContext = $"""

            ## STUDENT DATA (use naturally in conversation)
            - Name: {firstName ?? "not provided"}
            - Age: {(age.HasValue ? $"{age} years old" : "not provided")}
            - Motivations: {motivations ?? "not specified"}
            - Daily study time: {dailyMinutes} minutes
            
            Use this information to personalize the conversation. Call the student by name. 
            {(age.HasValue ? "You already know their age, do NOT ask for it again." : "Ask their age during the conversation.")}
            Reference their motivations and available time naturally.
            """;
        return AgentInstructions + studentContext;
    }

    // ── Prompt file loader ─────────────────────────────────────────────────────

    private static string LoadPrompt(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "prompts", fileName);
        if (File.Exists(path))
            return File.ReadAllText(path);

        // Fallback: try relative to content root
        var altPath = Path.Combine(Directory.GetCurrentDirectory(), "prompts", fileName);
        if (File.Exists(altPath))
            return File.ReadAllText(altPath);

        throw new FileNotFoundException($"Prompt file not found: {fileName}. Searched: {path}, {altPath}");
    }

    // ── Value types ────────────────────────────────────────────────────────────

    private record VideoItem(string Category, string Url, string Label, int MatchScore = 0, string Confidence = "low", List<string>? MatchedStructures = null);
}
