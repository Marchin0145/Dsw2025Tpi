using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Testing.TestData;
using Dsw2025Tpi.UnitTests.Mocks;
using Moq;
using Xunit;

namespace Dsw2025Tpi.UnitTests;

public class OrderServicesTests
{
    [Fact]
    public async Task CreateOrder_WithValidItemsAndStock_CalculatesTotalAndDecrementsProductStock()
    {
        var (customer, orderBuilder) = OrderTestDataFactory.Valid();
        var products = orderBuilder.Items.Select(item => item.Product).ToArray();
        var repository = OrderRepositoryMock.Create(customer, products);
        var service = new OrderServices(repository.Object);

        var result = await service.AddOrder(orderBuilder.BuildRequest(), customer.UserName);

        Assert.Equal(products.Sum(product =>
            product.CurrentUnitPrice * orderBuilder.Items.Single(item => item.Product == product).Quantity),
            result.totalAmount);
        Assert.Equal(8, products[0].StockQuantity);
        Assert.Equal(7, products[1].StockQuantity);
        Assert.Equal(2, result.orderItems!.Count);
        repository.Verify(repo => repo.Update(It.IsAny<Product>()), Times.Exactly(2));
        repository.Verify(repo => repo.Add(It.Is<Order>(order =>
            order.TotalAmount == result.totalAmount)), Times.Once);
    }

    [Fact]
    public async Task CreateOrder_WhenProductStockIsInsufficient_ThrowsInsufficientStockException()
    {
        var (customer, orderBuilder) = OrderTestDataFactory.InsufficientStock();
        var product = orderBuilder.Items.Single().Product;
        var initialStock = product.StockQuantity;
        var repository = OrderRepositoryMock.Create(customer, [product]);
        var service = new OrderServices(repository.Object);

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.AddOrder(orderBuilder.BuildRequest(), customer.UserName));

        Assert.Equal("valores incompletos o erroneos en productos", exception.Message);
        Assert.Equal(initialStock, product.StockQuantity);
        repository.Verify(repo => repo.Update(It.IsAny<Product>()), Times.Never);
        repository.Verify(repo => repo.Add(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrder_WhenCustomerDoesNotExist_ThrowsCustomerNotFoundException()
    {
        var (userName, orderBuilder) = OrderTestDataFactory.MissingCustomer();
        var products = orderBuilder.Items.Select(item => item.Product).ToArray();
        var repository = OrderRepositoryMock.Create(null, products);
        var service = new OrderServices(repository.Object);

        var exception = await Assert.ThrowsAsync<NotFoundEntityException>(
            () => service.AddOrder(orderBuilder.BuildRequest(), userName));

        Assert.Equal("Usuario no encontrado", exception.Message);
        Assert.All(products, product => Assert.Equal(10, product.StockQuantity));
        repository.Verify(repo => repo.GetById<Product>(It.IsAny<Guid>(), It.IsAny<string[]>()), Times.Never);
        repository.Verify(repo => repo.Update(It.IsAny<Product>()), Times.Never);
        repository.Verify(repo => repo.Add(It.IsAny<Order>()), Times.Never);
    }
}
