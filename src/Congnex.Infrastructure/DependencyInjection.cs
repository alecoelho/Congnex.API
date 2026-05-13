using Congnex.Application.Interfaces;
using Congnex.Application.Settings;
using Congnex.Domain.Services;
using Congnex.Infrastructure.Persistence;
using Congnex.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        services.AddScoped<ICongnexDbContext>(sp =>
            sp.GetRequiredService<CongnexDbContext>());

        // HTTP
        services.AddHttpClient();

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

        return services;
    }
}
