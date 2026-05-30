using Congnex.Application.Interfaces;
using Congnex.Application.Settings;
using Congnex.Domain.Services;
using Congnex.Infrastructure.Persistence;
using Congnex.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace Congnex.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        // Settings
        services.Configure<JwtSettings>(config.GetSection("Jwt"));
        services.Configure<AzureSettings>(config.GetSection("Azure"));
        services.Configure<StripeSettings>(config.GetSection("Stripe"));
        services.Configure<GoogleSettings>(config.GetSection("Google"));
        services.Configure<AppleSettings>(config.GetSection("Apple"));

        // Database
        var connStr = config.GetConnectionString("MySQL")!;
        services.AddDbContext<CongnexDbContext>(opts =>
            opts.UseMySql(
                connStr,
                new MySqlServerVersion(new Version(8, 0, 36)),
                b => b.MigrationsAssembly(typeof(CongnexDbContext).Assembly.FullName)));

        services.AddDbContextFactory<CongnexDbContext>(opts =>
            opts.UseMySql(
                connStr,
                new MySqlServerVersion(new Version(8, 0, 36)),
                b => b.MigrationsAssembly(typeof(CongnexDbContext).Assembly.FullName)),
            ServiceLifetime.Scoped);

        services.AddScoped<ICongnexDbContext>(sp =>
            sp.GetRequiredService<CongnexDbContext>());

        // Memory cache (used by XylaService for in-memory session history)
        services.AddMemoryCache();

        // HTTP
        services.AddHttpClient();

        // Brave Search HTTP client (used by XylaService for YouTube video discovery)
        var braveApiKey = config["Azure:BraveSearch:ApiKey"] ?? string.Empty;
        services.AddHttpClient("brave", client =>
        {
            client.BaseAddress = new Uri("https://api.search.brave.com/");
            client.DefaultRequestHeaders.Add("X-Subscription-Token", braveApiKey);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        // YouTube HTTP client (used by YouTubeTranscriptService for transcript scraping)
        services.AddHttpClient("youtube", client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                "(KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        // Semantic Kernel — singleton Kernel with "xyla" (gpt-4.1) and "xyla-mini" (gpt-4.1-mini)
        // xyla      → interview agent (conversational, needs full model)
        // xyla-mini → question generation (structured/deterministic, mini is sufficient)
        services.AddSingleton(sp =>
        {
            var azureOpts = sp.GetRequiredService<IOptions<AzureSettings>>().Value.AIFoundry;
            return Kernel.CreateBuilder()
                .AddAzureOpenAIChatCompletion(
                    deploymentName: azureOpts.DeploymentName,
                    endpoint:       azureOpts.Endpoint,
                    apiKey:         azureOpts.ApiKey,
                    serviceId:      "xyla")
                .AddAzureOpenAIChatCompletion(
                    deploymentName: azureOpts.MiniDeploymentName,
                    endpoint:       azureOpts.Endpoint,
                    apiKey:         azureOpts.ApiKey,
                    serviceId:      "xyla-mini")
                .Build();
        });

        // Domain services
        services.AddSingleton<FsrsService>();

        // Infrastructure services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddScoped<IAppleAuthService, AppleAuthService>();
        services.AddScoped<IStripeService, StripeService>();
        services.AddScoped<IStripeWebhookService, StripeWebhookService>();
        services.AddScoped<IBlobStorageService, BlobStorageService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<INotificationService, NotificationService>();

        // Xyla AI interview service
        services.AddScoped<IXylaService, XylaService>();
        services.AddScoped<IYouTubeTranscriptService, YouTubeTranscriptService>();
        services.AddSingleton<TranscriptSegmentService>();

        return services;
    }
}
