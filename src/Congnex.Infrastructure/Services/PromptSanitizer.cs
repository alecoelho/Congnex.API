namespace Congnex.Infrastructure.Services;

/// <summary>
/// Strips control characters and truncates user-controlled strings before they are embedded in AI prompts.
/// Prevents prompt injection attacks where a student might attempt to override Xyla's instructions.
/// </summary>
public static class PromptSanitizer
{
    public static string Sanitize(string? value, int maxLength = 500)
    {
        if (value is null) return "[não informado]";
        var sanitized = string.Concat(value.Where(c => c >= 32)).Trim();
        if (sanitized.Length == 0) return "[não informado]";
        return sanitized[..Math.Min(sanitized.Length, maxLength)];
    }
}
