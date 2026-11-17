using System.Net;
using System.Net.Http.Json;
using Dsw2025Tpi.Testing.Builders;
using Dsw2025Tpi.Testing.TestData;
using Xunit;

namespace Dsw2025Tpi.IntegrationTests;

public class ProductCreationIntegrationTests
{
    [Fact]
    public async Task AddProduct_WhenUnauthenticatedOrNotAdmin_ReturnsUnauthorizedOrForbidden()
    {
        using var fixture = new Fixtures.ApiFixture();
        var request = ProductTestDataFactory.ValidRequest();

        var anonymousResponse = await fixture.Client.PostAsJsonAsync("api/products", request);

        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);

        using var customerRequest = new HttpRequestMessage(HttpMethod.Post, "api/products")
        {
            Content = JsonContent.Create(request)
        };
        customerRequest.Headers.Authorization = fixture.CustomerAuthorization();

        var customerResponse = await fixture.Client.SendAsync(customerRequest);

        Assert.Equal(HttpStatusCode.Forbidden, customerResponse.StatusCode);
    }

    [Fact]
    public async Task AddProduct_WhenAuthenticatedAsAdmin_PersistsInSqliteAndReturnsCreated()
    {
        using var fixture = new Fixtures.ApiFixture();
        var request = ProductTestDataFactory.ValidRequest();
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/products")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Authorization = fixture.AdminAuthorization();

        var response = await fixture.Client.SendAsync(httpRequest);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var persistedProduct = await fixture.FindProductBySkuAsync(request.sku);
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
        using var fixture = new Fixtures.ApiFixture();
        var existingProduct = new ProductBuilder().Build();
        await fixture.SeedAsync(existingProduct);

        var duplicateRequest = ProductTestDataFactory.ValidRequest() with
        {
            internalCode = "TP4-DUPLICATE"
        };
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/products")
        {
            Content = JsonContent.Create(duplicateRequest)
        };
        httpRequest.Headers.Authorization = fixture.AdminAuthorization();

        var response = await fixture.Client.SendAsync(httpRequest);

        Assert.True(
            response.StatusCode is HttpStatusCode.Conflict or HttpStatusCode.BadRequest,
            $"Se esperaba 409 o 400, pero la API devolvió {(int)response.StatusCode}.");
        Assert.Equal(1, await fixture.CountProductsAsync());
    }
}
