using System.Collections.Generic;
using System.Data.Common;
using Dsw2025Tpi.Api;
using Dsw2025Tpi.Data.Contexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ECommerce.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private DbConnection? _dbConnection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("AdminUser__Email", "admin@empresa.com");
        Environment.SetEnvironmentVariable("AdminUser__Password", "Admin123!");
        Environment.SetEnvironmentVariable("Jwt__key", "3f8b2e1c-4d5a-4e6f-9a7b-2c1d8e5f6a9c-!@#QwErTyUiOp1234567890");
        Environment.SetEnvironmentVariable("Jwt__issuer", "Dsw2025Tpi.Api");
        Environment.SetEnvironmentVariable("Jwt__audience", "Dsw2025Tpi.ApiUsers");

        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:Dsw2025Tpi"] = "Data Source=:memory:;Mode=Memory;Cache=Shared",
                ["AdminUser__Email"] = "admin@empresa.com",
                ["AdminUser__Password"] = "Admin123!",
                ["Jwt:key"] = "3f8b2e1c-4d5a-4e6f-9a7b-2c1d8e5f6a9c-!@#QwErTyUiOp1234567890",
                ["Jwt:issuer"] = "Dsw2025Tpi.Api",
                ["Jwt:audience"] = "Dsw2025Tpi.ApiUsers"
            };

            configBuilder.AddInMemoryCollection(settings);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<Dsw2025TpiContext>));
            services.RemoveAll(typeof(DbContextOptions<AuthenticateContext>));
            services.RemoveAll(typeof(Dsw2025TpiContext));
            services.RemoveAll(typeof(AuthenticateContext));

            _dbConnection = new SqliteConnection("Data Source=:memory:;Mode=Memory;Cache=Shared");
            _dbConnection.Open();

            services.AddSingleton(_dbConnection);
            services.AddDbContext<Dsw2025TpiContext>(options => options.UseSqlite(_dbConnection));
            services.AddDbContext<AuthenticateContext>(options => options.UseSqlite(_dbConnection));

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var appDbContext = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
            var authDbContext = scope.ServiceProvider.GetRequiredService<AuthenticateContext>();

            appDbContext.Database.EnsureCreated();
            authDbContext.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _dbConnection?.Dispose();
        }

        base.Dispose(disposing);
    }
}

[CollectionDefinition("IntegrationTests", DisableParallelization = true)]
public class IntegrationTestsCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}
