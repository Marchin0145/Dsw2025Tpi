using System.Collections.Generic;
using System.Data.Common;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Dsw2025Tpi.Api;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data.Contexts;
using Dsw2025Tpi.Domain.Entities;
using ECommerce.Testing.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ECommerce.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SqliteConnection _applicationDatabase = CreateDatabase();
    private readonly SqliteConnection _identityDatabase = CreateDatabase();

    private const string AdminEmail = "admin@tp4.example";
    private static readonly string AdminPassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    private static readonly string JwtSigningKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions>();
            ReplaceContext<Dsw2025TpiContext>(services, _applicationDatabase.ConnectionString);
            ReplaceContext<AuthenticateContext>(services, _identityDatabase.ConnectionString);
            services.AddHostedService<DatabaseInitializer>();
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config => config.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                [HostDefaults.EnvironmentKey] = "Testing",
                ["AdminUser:Email"] = AdminEmail,
                ["AdminUser:Password"] = AdminPassword,
                ["Jwt:Key"] = JwtSigningKey,
                ["Jwt:Issuer"] = "tp4-integration",
                ["Jwt:Audience"] = "tp4-integration",
                ["Jwt:ExpireInMinutes"] = "10",
                ["ConnectionStrings:Dsw2025Tpi"] = ""
            }));

        try
        {
            _applicationDatabase.Open();
            _identityDatabase.Open();
            return base.CreateHost(builder);
        }
        catch
        {
            _identityDatabase.Close();
            _applicationDatabase.Close();
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

    private sealed class DatabaseInitializer(IServiceScopeFactory scopes) : IHostedService
    {
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

    public async Task InitializeAsync() => await Task.CompletedTask;

    public new async Task DisposeAsync()
    {
        try
        {
            base.Dispose();
        }
        finally
        {
            _identityDatabase.Dispose();
            _applicationDatabase.Dispose();
        }
    }

    public new HttpClient CreateClient() => base.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false,
        HandleCookies = false
    });

    public async Task SeedAsync(params EntityBase[] entities)
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
        context.AddRange(entities);
        await context.SaveChangesAsync();
    }

    public AuthenticationHeaderValue AdminAuthorization() => CreateAuthorization(AdminEmail, "Admin");

    public AuthenticationHeaderValue CustomerAuthorization(Customer customer) =>
        CreateAuthorization(customer.UserName, "Customer");

    public AuthenticationHeaderValue CustomerAuthorization() =>
        CustomerAuthorization(new CustomerBuilder().Build());

    public async Task<Product?> FindProductBySkuAsync(string sku)
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
        return await context.Set<Product>().SingleOrDefaultAsync(product => product.Sku == sku);
    }

    public async Task<Product?> FindProductByIdAsync(Guid id)
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
        return await context.Set<Product>().SingleOrDefaultAsync(product => product.Id == id);
    }

    public async Task<int> CountProductsAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
        return await context.Set<Product>().CountAsync();
    }

    public async Task<OrderModel.ResponseOrder?> FindOrderByCustomerIdAsync(Guid customerId)
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
        var order = await context.Set<Order>()
            .Include(savedOrder => savedOrder.OrderItems)
            .SingleOrDefaultAsync(savedOrder => savedOrder.CustomerId == customerId);

        return order is null
            ? null
            : new OrderModel.ResponseOrder(
                order.Id,
                order.CustomerId,
                order.Date,
                order.ShippingAddress,
                order.BillingAddress,
                order.Notes,
                order.Date,
                order.OrderItems!.Sum(item => item.Quantity * item.UnitPrice),
                order.Status,
                order.OrderItems.Select(item => new OrderItemModel.ResponseOrderItem(
                    item.ProductId,
                    item.Quantity,
                    string.Empty,
                    item.UnitPrice,
                    item.Quantity * item.UnitPrice)).ToList());
    }

    public async Task<int> CountOrdersAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
        return await context.Set<Order>().CountAsync();
    }

    private AuthenticationHeaderValue CreateAuthorization(string userName, string role)
    {
        var tokens = Services.GetRequiredService<JwtTokenService>();
        return new AuthenticationHeaderValue("Bearer", tokens.GenerateToken(userName, role));
    }

    private static SqliteConnection CreateDatabase() => new(new SqliteConnectionStringBuilder
    {
        DataSource = Guid.NewGuid().ToString("N"),
        Mode = SqliteOpenMode.Memory,
        Cache = SqliteCacheMode.Shared,
        Pooling = false,
        ForeignKeys = true
    }.ToString());
}