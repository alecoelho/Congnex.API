using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Congnex.API.Filters;

/// <summary>Marca uma action para NÃO exigir a admin key (ex: cadastro de vídeos).</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class SkipAdminKeyAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AdminApiKeyAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Actions marcadas com [SkipAdminKey] não exigem a chave.
        if (context.ActionDescriptor.EndpointMetadata.OfType<SkipAdminKeyAttribute>().Any())
        {
            await next();
            return;
        }

        var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var expectedKey = config["Admin:ApiKey"];

        if (string.IsNullOrWhiteSpace(expectedKey))
        {
            context.Result = new ObjectResult(new { error = "Admin key not configured." })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue("X-Admin-Key", out var providedKey)
            || providedKey != expectedKey)
        {
            context.Result = new UnauthorizedObjectResult(new { error = "Invalid or missing admin key." });
            return;
        }

        await next();
    }
}
