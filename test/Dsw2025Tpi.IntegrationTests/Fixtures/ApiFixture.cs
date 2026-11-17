using System.Net.Http.Headers;
using Dsw2025Tpi.Api;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data.Contexts;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Dsw2025Tpi.IntegrationTests.Fixtures;

// Create one fixture per test to get fresh application and Identity databases.
public sealed class ApiFixture : IDisposable
{
    private const string AdminEmail = "admin@tp4.example";

    private readonly SqliteConnection _applicationDatabase = CreateDatabase();
    private readonly SqliteConnection _identityDatabase = CreateDatabase();
    private readonly ApiFactory _factory;
    private HttpClient? _client;

    public ApiFixture()
    {
        _factory = new ApiFactory(_applicationDatabase, _identityDatabase);
    }

    // Requests are anonymous unless their own Authorization header is set.
    public HttpClient Client => _client ??= _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false,
        HandleCookies = false
    });

    public async Task SeedAsync(params EntityBase[] entities)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
        context.AddRange(entities);
        await context.SaveChangesAsync();
    }

    public AuthenticationHeaderValue AdminAuthorization() => CreateAuthorization(AdminEmail, "Admin");

    public AuthenticationHeaderValue CustomerAuthorization(Customer customer) =>
        CreateAuthorization(customer.UserName, "Customer");

    private AuthenticationHeaderValue CreateAuthorization(string userName, string role)
    {
        var tokens = _factory.Services.GetRequiredService<JwtTokenService>();
        return new AuthenticationHeaderValue("Bearer", tokens.GenerateToken(userName, role));
    }

    public void Dispose()
    {
        try
        {
            _factory.Dispose();
        }
        finally
        {
            _identityDatabase.Dispose();
            _applicationDatabase.Dispose();
        }
    }

    private static SqliteConnection CreateDatabase() => new(new SqliteConnectionStringBuilder
    {
        DataSource = Guid.NewGuid().ToString("N"),
        Mode = SqliteOpenMode.Memory,
        Cache = SqliteCacheMode.Shared,
        Pooling = false,
        ForeignKeys = true
    }.ToString());

    private sealed class ApiFactory(SqliteConnection applicationDatabase, SqliteConnection identityDatabase)
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions>();
                ReplaceContext<Dsw2025TpiContext>(services, applicationDatabase.ConnectionString);
                ReplaceContext<AuthenticateContext>(services, identityDatabase.ConnectionString);
                services.AddHostedService<DatabaseInitializer>();
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            // The entry point reads JWT and admin settings before building the application.
            builder.ConfigureHostConfiguration(config => config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    [HostDefaults.EnvironmentKey] = "Testing",
                    ["AdminUser:Email"] = AdminEmail,
                    ["AdminUser:Password"] = "TP4-fixture!Admin42",
                    ["Jwt:Key"] = "TP4-fixture-signing-key-for-integration-tests-only",
                    ["Jwt:Issuer"] = "tp4-integration",
                    ["Jwt:Audience"] = "tp4-integration",
                    ["Jwt:ExpireInMinutes"] = "10",
                    ["ConnectionStrings:Dsw2025Tpi"] = ""
                }));

            // Keep each shared in-memory database alive across request scopes.
            try
            {
                applicationDatabase.Open();
                identityDatabase.Open();
                return base.CreateHost(builder);
            }
            catch
            {
                identityDatabase.Close();
                applicationDatabase.Close();
                throw;
            }
        }

        private static void ReplaceContext<TContext>(IServiceCollection services, string connectionString)
            where TContext : DbContext
        {
            services.RemoveAll<TContext>();
            services.RemoveAll<DbContextOptions<TContext>>();
            services.AddDbContext<TContext>(options => options.UseSqlite(connectionString));
        }
    }

    private sealed class DatabaseInitializer(IServiceScopeFactory scopes) : IHostedService
    {
        // The application seeds Identity after ApplicationStarted.
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await using var scope = scopes.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>()
                .Database.EnsureCreatedAsync(cancellationToken);
            await scope.ServiceProvider.GetRequiredService<AuthenticateContext>()
                .Database.EnsureCreatedAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
