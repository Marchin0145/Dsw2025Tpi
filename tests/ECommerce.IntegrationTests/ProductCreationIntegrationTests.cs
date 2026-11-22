using System.Net;
using System.Net.Http.Json;
using ECommerce.Testing.Builders;
using ECommerce.Testing.TestData;
using Xunit;

namespace ECommerce.IntegrationTests;

public class ProductCreationIntegrationTests
{
    [Fact]
    public async Task AddProduct_WhenUnauthenticatedOrNotAdmin_ReturnsUnauthorizedOrForbidden()
    {
        using var factory = new CustomWebApplicationFactory();
        var request = ProductTestDataFactory.ValidRequest();

        var anonymousResponse = await factory.CreateClient().PostAsJsonAsync("api/products", request);

        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);

        using var customerRequest = new HttpRequestMessage(HttpMethod.Post, "api/products")
        {
            Content = JsonContent.Create(request)
        };
        customerRequest.Headers.Authorization = factory.CustomerAuthorization();

        var customerResponse = await factory.CreateClient().SendAsync(customerRequest);

        Assert.Equal(HttpStatusCode.Forbidden, customerResponse.StatusCode);
    }

    [Fact]
    public async Task AddProduct_WhenAuthenticatedAsAdmin_PersistsInSqliteAndReturnsCreated()
    {
        using var factory = new CustomWebApplicationFactory();
        var request = ProductTestDataFactory.ValidRequest();
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/products")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Authorization = factory.AdminAuthorization();

        var response = await factory.CreateClient().SendAsync(httpRequest);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var persistedProduct = await factory.FindProductBySkuAsync(request.sku);
        Assert.NotNull(persistedProduct);
        Assert.Equal(request.internalCode, persistedProduct.InternalCode);
        Assert.Equal(request.name, persistedProduct.Name);
        Assert.Equal(request.description, persistedProduct.Description);
        Assert.Equal(request.currentUnitPrice, persistedProduct.CurrentUnitPrice);
        Assert.Equal(request.stockQuantity, persistedProduct.StockQuantity);
    }

    [Fact]
    public async Task AddProduct_WhenSkuAlreadyExistsInDatabase_ReturnsConflictStatusCode()
    {
        using var factory = new CustomWebApplicationFactory();
        var existingProduct = new ProductBuilder().Build();
        await factory.SeedAsync(existingProduct);

        var duplicateRequest = ProductTestDataFactory.ValidRequest() with
        {
            internalCode = "TP4-DUPLICATE"
        };
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/products")
        {
            Content = JsonContent.Create(duplicateRequest)
        };
        httpRequest.Headers.Authorization = factory.AdminAuthorization();

        var response = await factory.CreateClient().SendAsync(httpRequest);

        Assert.True(
            response.StatusCode is HttpStatusCode.Conflict or HttpStatusCode.BadRequest,
            $"Se esperaba 409 o 400, pero la API devolvió {(int)response.StatusCode}.");
        Assert.Equal(1, await factory.CountProductsAsync());
    }
}