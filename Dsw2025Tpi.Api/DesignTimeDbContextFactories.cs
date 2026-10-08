using Dsw2025Tpi.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dsw2025Tpi.Api;

public class Dsw2025TpiContextFactory : IDesignTimeDbContextFactory<Dsw2025TpiContext>
{
    public Dsw2025TpiContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<Dsw2025TpiContext>()
            .UseSqlServer(DesignTimeConnectionString.Get(), sql =>
            {
                sql.MigrationsAssembly("Dsw2025Tpi.Api");
                sql.EnableRetryOnFailure();
            })
            .Options;

        return new Dsw2025TpiContext(options);
    }
}

public class AuthenticateContextFactory : IDesignTimeDbContextFactory<AuthenticateContext>
{
    public AuthenticateContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AuthenticateContext>()
            .UseSqlServer(DesignTimeConnectionString.Get(), sql =>
            {
                sql.MigrationsAssembly("Dsw2025Tpi.Api");
                sql.EnableRetryOnFailure();
            })
            .Options;

        return new AuthenticateContext(options);
    }
}

internal static class DesignTimeConnectionString
{
    public static string Get() =>
        Environment.GetEnvironmentVariable("ConnectionStrings__Dsw2025Tpi")
        ?? throw new InvalidOperationException(
            "Set ConnectionStrings__Dsw2025Tpi before running Entity Framework commands.");
}
