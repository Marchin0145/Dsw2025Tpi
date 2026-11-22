using System.Net;
using System.Net.Http.Json;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Testing.Builders;
using Xunit;

namespace Dsw2025Tpi.IntegrationTests;

public class OrderCreationIntegrationTests
{
    [Fact]
    public async Task CreateOrder_WhenUnauthenticated_ReturnsUnauthorized()
    {
        using var fixture = new Fixtures.ApiFixture();
        var product = new ProductBuilder().Build();

        var response = await fixture.Client.PostAsJsonAsync(
            "api/orders",
            CreateRequest(product, quantity: 3));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_WhenAuthenticatedWithValidData_ReturnsCreatedAndPersistsOrderAndStockInSqlite()
    {
        using var fixture = new Fixtures.ApiFixture();
        var customer = new CustomerBuilder().Build();
        var product = new ProductBuilder { StockQuantity = 10 }.Build();
        await fixture.SeedAsync(customer, product);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/orders")
        {
            Content = JsonContent.Create(CreateRequest(product, quantity: 3))
        };
        httpRequest.Headers.Authorization = fixture.CustomerAuthorization(customer);

        var response = await fixture.Client.SendAsync(httpRequest);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var savedProduct = await fixture.FindProductByIdAsync(product.Id);
        Assert.NotNull(savedProduct);
        Assert.Equal(7, savedProduct.StockQuantity);

        var savedOrder = await fixture.FindOrderByCustomerIdAsync(customer.Id);
        Assert.NotNull(savedOrder);
        Assert.Single(savedOrder.orderItems!);
        Assert.Equal(product.Id, savedOrder.orderItems[0].productId);
        Assert.Equal(3, savedOrder.orderItems[0].quantity);
        Assert.Equal(300m, savedOrder.totalAmount);
        Assert.Equal(1, await fixture.CountOrdersAsync());
    }

    [Fact]
    public async Task CreateOrder_WhenStockIsInsufficient_ReturnsBadRequestWithErrorMessage()
    {
        using var fixture = new Fixtures.ApiFixture();
        var customer = new CustomerBuilder().Build();
        var product = new ProductBuilder { StockQuantity = 2 }.Build();
        await fixture.SeedAsync(customer, product);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/orders")
        {
            Content = JsonContent.Create(CreateRequest(product, quantity: 5))
        };
        httpRequest.Headers.Authorization = fixture.CustomerAuthorization(customer);

        var response = await fixture.Client.SendAsync(httpRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Stock insuficiente", responseBody);
        Assert.Contains(product.Name, responseBody);

        var unchangedProduct = await fixture.FindProductByIdAsync(product.Id);
        Assert.NotNull(unchangedProduct);
        Assert.Equal(2, unchangedProduct.StockQuantity);
        Assert.Equal(0, await fixture.CountOrdersAsync());
    }

    private static OrderModel.RequestOrder CreateRequest(Product product, int quantity) =>
        new(
            "Av. Independencia 1800",
            "Av. Independencia 1800",
            "Pedido de integración",
            [new OrderItemModel.RequestOrderItem(product.Id, product.Name, quantity, product.CurrentUnitPrice)]);
}
