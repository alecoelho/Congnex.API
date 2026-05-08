using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Congnex.Infrastructure.Persistence;

// Used only by `dotnet ef` CLI at design time — never registered in DI.
public class CongnexDbContextFactory : IDesignTimeDbContextFactory<CongnexDbContext>
{
    public CongnexDbContext CreateDbContext(string[] args)
    {
        var connStr = Environment.GetEnvironmentVariable("ConnectionStrings__MySQL")
            ?? "Server=127.0.0.1;Port=3306;Database=congnex;User=root;Password=admin;SslMode=None;";

        var opts = new DbContextOptionsBuilder<CongnexDbContext>()
            .UseMySql(connStr, ServerVersion.AutoDetect(connStr))
            .Options;

        return new CongnexDbContext(opts);
    }
}
