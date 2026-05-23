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

        // Semantic Kernel — singleton Kernel with the "xyla" chat completion service
        services.AddSingleton(sp =>
        {
            var azureOpts = sp.GetRequiredService<IOptions<AzureSettings>>().Value.AIFoundry;
            return Kernel.CreateBuilder()
                .AddAzureOpenAIChatCompletion(
                    deploymentName: azureOpts.DeploymentName,
                    endpoint:       azureOpts.Endpoint,
                    apiKey:         azureOpts.ApiKey,
                    serviceId:      "xyla")
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
        services.AddScoped<IVideoSearchProvider, BraveVideoSearchProvider>();

        return services;
    }
}
