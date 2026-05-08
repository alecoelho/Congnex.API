namespace Congnex.Application.Common;

public sealed record ApiResponse<T>(bool Success, T? Data, string? Error = null)
{
    public static ApiResponse<T> Ok(T data) => new(true, data);
    public static ApiResponse<T> Fail(string error) => new(false, default, error);
}

public sealed record ApiResponse(bool Success, string? Error = null)
{
    public static ApiResponse Ok() => new(true);
    public static ApiResponse Fail(string error) => new(false, error);
}
