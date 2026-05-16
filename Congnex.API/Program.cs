using System.Text;
using System.Threading.RateLimiting;
using Congnex.Application.Settings;
using Congnex.Infrastructure;
using Congnex.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ────────────────────────────────────────────────────────────────
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .Enrich.FromLogContext()
       .WriteTo.Console());

// ── Infrastructure (DB, services, settings) ────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ── MediatR ────────────────────────────────────────────────────────────────
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(
        typeof(Congnex.Application.Common.ApiResponse).Assembly));

// ── JWT Authentication ─────────────────────────────────────────────────────
var jwtCfg = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtCfg.SecretKey)),
            ValidateIssuer   = true,
            ValidIssuer      = jwtCfg.Issuer,
            ValidateAudience = true,
            ValidAudience    = jwtCfg.Audience,
            ValidateLifetime = true,
            ClockSkew        = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ── Controllers ────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Swagger ────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Congnex API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type   = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter your JWT token"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        }] = []
    });
});

// ── Rate limiting ──────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(o =>
{
    // 10 Xyla messages per minute per user
    o.AddPolicy("xyla", context =>
    {
        var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                  ?? context.Connection.RemoteIpAddress?.ToString()
                  ?? "anon";

        return RateLimitPartition.GetFixedWindowLimiter(userId, _ => new FixedWindowRateLimiterOptions
        {
            Window            = TimeSpan.FromMinutes(1),
            PermitLimit       = 10,
            QueueLimit        = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    });

    o.RejectionStatusCode = 429;
});

// ── Health checks ──────────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

// ── CORS (dev only) ────────────────────────────────────────────────────────
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// ══════════════════════════════════════════════════════════════════════════
var app = builder.Build();

// Auto-migrate + seed on startup (dev convenience — use explicit migration in prod)
if (app.Environment.IsDevelopment())
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CongnexDbContext>();
        await db.Database.MigrateAsync();
        await Congnex.Infrastructure.Persistence.DbSeeder.SeedAsync(db);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Could not apply migrations — is the database running?");
    }
}

app.UseSerilogRequestLogging();
app.UseRateLimiter();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Congnex API v1"));
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
