using Congnex.Application.Settings;
using Congnex.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((ctx, cfg) =>
    {
        cfg.AddEnvironmentVariables();
        if (ctx.HostingEnvironment.IsDevelopment())
            cfg.AddJsonFile("local.settings.json", optional: true, reloadOnChange: false);
    })
    .ConfigureServices((ctx, services) =>
    {
        services.AddInfrastructure(ctx.Configuration);
    })
    .Build();

host.Run();
