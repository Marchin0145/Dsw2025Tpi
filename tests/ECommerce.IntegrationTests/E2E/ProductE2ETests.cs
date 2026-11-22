using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ECommerce.Testing.TestData;
using Xunit;

namespace ECommerce.IntegrationTests.E2E;

[Collection("IntegrationTests")]
public class ProductE2ETests
{
    [Fact]
    public async Task E2E_CreateProductThenListBySku_ReturnsCreatedProduct()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var uniqueSku = $"E2E-{Guid.NewGuid():N}";

        // Arrange
        var request = ProductTestDataFactory.ValidRequest() with { sku = uniqueSku };
        var authHeader = factory.AdminAuthorization();

        // Act - Step 1: Create product via POST
        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "api/products")
        {
            Content = JsonContent.Create(request)
        };
        createRequest.Headers.Authorization = authHeader;

        var createResponse = await client.SendAsync(createRequest);

        // Assert - Step 1: Verify 201 Created
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(createdProduct);
        Assert.Equal(uniqueSku, createdProduct.Sku);

        // Act - Step 2: Search product by SKU via GET
        var searchResponse = await client.GetAsync($"api/products?search={uniqueSku}");

        // Assert - Step 2: Verify search returns the created product
        Assert.Equal(HttpStatusCode.OK, searchResponse.StatusCode);

        using var document = JsonDocument.Parse(await searchResponse.Content.ReadAsStringAsync());
        var productItems = document.RootElement.GetProperty("productItems");
        Assert.Equal(1, productItems.GetArrayLength());
        Assert.Equal(uniqueSku, productItems[0].GetProperty("sku").GetString());
        Assert.Equal(createdProduct.Id, productItems[0].GetProperty("id").GetGuid());
        Assert.Equal(1, document.RootElement.GetProperty("totalItems").GetInt32());
    }

    private sealed record ProductResponse(Guid Id, string Sku);
}