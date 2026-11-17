using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dsw2025Tpi.Testing.Builders;
using Xunit;

namespace Dsw2025Tpi.IntegrationTests;

public class ProductListingIntegrationTests
{
    [Fact]
    public async Task GetProducts_PublicEndpoint_ReturnsOkWithPaginatedJsonStructure()
    {
        using var fixture = new Fixtures.ApiFixture();

        var response = await fixture.Client.GetAsync("api/products");

        Assert.True(response.StatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent);
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return;
        }

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(document.RootElement.TryGetProperty("productItems", out var products));
        Assert.Equal(JsonValueKind.Array, products.ValueKind);
        Assert.True(document.RootElement.TryGetProperty("totalItems", out var totalItems));
        Assert.Equal(JsonValueKind.Number, totalItems.ValueKind);
    }

    [Fact]
    public async Task GetProducts_WithQueryStringFilters_ReturnsFilteredResultsFromSqlite()
    {
        using var fixture = new Fixtures.ApiFixture();
        var matchingProduct = new ProductBuilder
        {
            Sku = "CAT-001",
            InternalCode = "INT-001",
            Name = "Mouse Gamer"
        }.Build();
        var otherProducts = new[]
        {
            new ProductBuilder
            {
                Sku = "CAT-002",
                InternalCode = "INT-002",
                Name = "Teclado Mecánico"
            }.Build(),
            new ProductBuilder
            {
                Sku = "CAT-003",
                InternalCode = "INT-003",
                Name = "Monitor"
            }.Build()
        };
        await fixture.SeedAsync(matchingProduct, otherProducts[0], otherProducts[1]);

        var response = await fixture.Client.GetAsync(
            "api/products?search=mouse&pageNumber=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var products = document.RootElement.GetProperty("productItems");
        Assert.Equal(1, products.GetArrayLength());
        Assert.Equal(
            matchingProduct.Id,
            products[0].GetProperty("id").GetGuid());
        Assert.Equal(1, document.RootElement.GetProperty("totalItems").GetInt32());
    }

    [Fact]
    public async Task GetAuthProducts_AdminEndpoint_WhenCalledByAdmin_IncludesInactiveProducts()
    {
        using var fixture = new Fixtures.ApiFixture();
        var inactiveProduct = new ProductBuilder
        {
            Sku = "CAT-INACTIVE",
            InternalCode = "INT-INACTIVE",
            Name = "Producto inactivo",
            IsActive = false
        }.Build();
        await fixture.SeedAsync(inactiveProduct);
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/products/admin");
        request.Headers.Authorization = fixture.AdminAuthorization();

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ProductListingResponse>();
        Assert.NotNull(result);
        var returnedProduct = Assert.Single(result.ProductItems);
        Assert.Equal(inactiveProduct.Id, returnedProduct.Id);
        Assert.False(returnedProduct.IsActive);
    }

    private sealed record ProductListingResponse(
        IReadOnlyList<ProductResponse> ProductItems,
        int TotalItems);

    private sealed record ProductResponse(Guid Id, bool IsActive);
}
