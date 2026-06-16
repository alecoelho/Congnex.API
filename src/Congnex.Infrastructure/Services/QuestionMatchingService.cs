using System.Text.RegularExpressions;
using Congnex.Application.Interfaces;
using Congnex.Domain.Entities;
using Congnex.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Congnex.Infrastructure.Services;

/// <summary>
/// Seleciona questões do question_bank por nível + domínio, rankeadas pela relevância
/// da transcrição (FULLTEXT), e copia para a tabela `questions` da lição.
/// </summary>
public sealed class QuestionMatchingService : IQuestionMatchingService
{
    private readonly IDbContextFactory<CongnexDbContext> _dbFactory;
    private readonly ILogger<QuestionMatchingService> _logger;

    public QuestionMatchingService(
        IDbContextFactory<CongnexDbContext> dbFactory,
        ILogger<QuestionMatchingService> logger)
    {
        _dbFactory = dbFactory;
        _logger    = logger;
    }

    public async Task<int> MatchAndCopyAsync(
        Guid lessonId, string cefrLevel, string? domain, string transcript,
        int limit = 60, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var level    = cefrLevel.Trim().ToUpperInvariant();
        var keywords = ExtractKeywords(transcript);

        // ── 1. Seleciona os IDs do banco (FULLTEXT quando há keywords) ─────────
        List<Guid> ids = await SelectBankIdsAsync(db, level, domain, keywords, limit, ct);

        // Fallback: se domínio + transcrição não trouxe o suficiente, completa só por nível
        if (ids.Count < limit)
        {
            var extra = await SelectBankIdsAsync(db, level, domain: null, keywords: "", limit - ids.Count, ct, exclude: ids);
            ids.AddRange(extra);
        }

        if (ids.Count == 0)
        {
            _logger.LogWarning("[QuestionMatch] Nenhuma questão encontrada para level={Level} domain={Domain}", level, domain);
            return 0;
        }

        // ── 2. Carrega as questões do banco com opções ────────────────────────
        var bankQuestions = await db.QuestionBankItems
            .Include(q => q.Options.OrderBy(o => o.OrderIndex))
            .Where(q => ids.Contains(q.Id))
            .ToListAsync(ct);

        // Preserva a ordem de relevância dos IDs
        var ordered = ids
            .Select(id => bankQuestions.FirstOrDefault(q => q.Id == id))
            .Where(q => q is not null)
            .Cast<QuestionBank>()
            .ToList();

        // ── 3. Copia para `questions` da lição ────────────────────────────────
        int order = await db.Questions.CountAsync(q => q.LessonId == lessonId, ct);
        int copied = 0;

        foreach (var bq in ordered)
        {
            var question = new Question
            {
                LessonId      = lessonId,
                Type          = MapType(bq.Type),
                QuestionText  = bq.QuestionText,
                CorrectAnswer = bq.CorrectAnswer,
                Difficulty    = bq.Difficulty,
                OrderIndex    = order++
            };

            int optIdx = 0;
            foreach (var opt in bq.Options)
                question.Options.Add(new QuestionOption
                {
                    QuestionId = question.Id,
                    OptionText = opt.OptionText,
                    IsCorrect  = opt.IsCorrect,
                    OrderIndex = optIdx++
                });

            db.Questions.Add(question);
            copied++;
        }

        await db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "[QuestionMatch] lesson={LessonId} level={Level} domain={Domain} keywords='{Kw}' copied={Copied}",
            lessonId, level, domain, keywords.Length > 60 ? keywords[..60] : keywords, copied);

        return copied;
    }

    // ── Seleção de IDs (raw SQL com FULLTEXT) ──────────────────────────────────

    private static async Task<List<Guid>> SelectBankIdsAsync(
        CongnexDbContext db, string level, string? domain, string keywords,
        int limit, CancellationToken ct, List<Guid>? exclude = null)
    {
        if (limit <= 0) return new();

        var sql = new System.Text.StringBuilder();
        var parameters = new List<MySqlConnector.MySqlParameter>
        {
            new("@level", level),
            new("@limit", limit)
        };

        sql.Append("SELECT id FROM question_bank WHERE cefr_level = @level");

        if (!string.IsNullOrWhiteSpace(domain))
        {
            sql.Append(" AND domain = @domain");
            parameters.Add(new("@domain", domain));
        }

        if (exclude is { Count: > 0 })
        {
            var inList = string.Join(",", exclude.Select((_, i) => $"@ex{i}"));
            sql.Append($" AND id NOT IN ({inList})");
            for (int i = 0; i < exclude.Count; i++)
                parameters.Add(new($"@ex{i}", exclude[i].ToString()));
        }

        if (!string.IsNullOrWhiteSpace(keywords))
        {
            // Ranking por relevância da transcrição (NATURAL LANGUAGE MODE)
            sql.Append(" ORDER BY MATCH(question_text, correct_answer) AGAINST(@kw IN NATURAL LANGUAGE MODE) DESC, RAND()");
            parameters.Add(new("@kw", keywords));
        }
        else
        {
            sql.Append(" ORDER BY RAND()");
        }

        sql.Append(" LIMIT @limit");

        var ids = new List<Guid>();

        // Conexão separada (não a do DbContext) — pode ser disposta com segurança.
        await using var conn = new MySqlConnector.MySqlConnection(db.Database.GetConnectionString());
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql.ToString();
        cmd.Parameters.AddRange(parameters.ToArray());

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            ids.Add(reader.GetGuid(0));

        return ids;
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static string MapType(string bankType) => bankType?.ToLowerInvariant() switch
    {
        "listening"        => "listening_choice",
        "multiple_choice"  => "multiple_choice",
        "translation"      => "translation",
        "complete_sentence"=> "complete_sentence",
        "match_pairs"      => "match_pairs",
        "pronunciation"    => "pronunciation",
        _                  => "multiple_choice"
    };

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        // EN
        "the","a","an","and","or","but","is","are","was","were","be","been","to","of","in","on",
        "at","for","with","you","your","i","me","my","we","our","they","them","this","that","it",
        "he","she","his","her","do","does","did","have","has","had","will","would","can","could",
        "so","if","as","by","from","not","no","yes","what","when","where","who","how","about","here","there",
        // PT
        "que","de","da","do","e","o","a","os","as","um","uma","para","com","por","no","na","se","eu","voce"
    };

    /// <summary>
    /// Extrai até ~40 palavras relevantes da transcrição (3+ letras, sem stopwords),
    /// priorizando as mais frequentes, para usar no MATCH AGAINST.
    /// </summary>
    private static string ExtractKeywords(string transcript)
    {
        if (string.IsNullOrWhiteSpace(transcript)) return "";

        var words = Regex.Matches(transcript.ToLowerInvariant(), @"[a-z']{3,}")
            .Select(m => m.Value.Trim('\''))
            .Where(w => w.Length >= 3 && !StopWords.Contains(w))
            .GroupBy(w => w)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Take(40);

        return string.Join(" ", words);
    }
}
