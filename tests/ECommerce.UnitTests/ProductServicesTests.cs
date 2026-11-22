using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Domain.Entities;
using ECommerce.Testing.TestData;
using ECommerce.UnitTests.Mocks;
using Moq;
using Xunit;

namespace ECommerce.UnitTests;

public class ProductServicesTests
{
    [Fact]
    public async Task AddProduct_WithValidData_ReturnsProductAndCallsRepositoryAdd()
    {
        var request = ProductTestDataFactory.ValidRequest();
        var repository = ProductRepositoryMock.Create();
        var service = new ProductServices(repository.Object);

        var product = await service.AddProduct(request);

        Assert.NotNull(product);
        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal(request.sku, product.Sku);
        Assert.Equal("CODIGO_INCORRECTO", product.InternalCode);
        Assert.Equal(request.name, product.Name);
        Assert.Equal(request.description, product.Description);
        Assert.Equal(request.currentUnitPrice, product.CurrentUnitPrice);
        Assert.Equal(request.stockQuantity, product.StockQuantity);
        Assert.True(product.IsActive);
        repository.Verify(repo => repo.Add(It.IsAny<Product>()), Times.Once);
        repository.Verify(repo => repo.Add(It.Is<Product>(added => ReferenceEquals(added, product))), Times.Once);
    }

    [Fact]
    public async Task AddProduct_WithDuplicateSku_ThrowsDuplicatedEntityException()
    {
        var (existingProduct, request) = ProductTestDataFactory.DuplicateSku();
        var repository = ProductRepositoryMock.Create(existingProduct);
        var service = new ProductServices(repository.Object);

        var exception = await Assert.ThrowsAsync<DuplicatedEntityException>(
            () => service.AddProduct(request));

        Assert.Equal("ya existe un producto con ese sku", exception.Message);
        repository.Verify(repo => repo.Add(It.IsAny<Product>()), Times.Never);
    }

    [Theory]
    [InlineData(0, 100, "el stock debe ser mayor a cero")]
    [InlineData(-1, 100, "el stock debe ser mayor a cero")]
    [InlineData(10, -1, "Precio menor o igual a cero")]
    public async Task AddProduct_WithInvalidStockOrPrice_ThrowsInvalidDataException(
        int stockQuantity,
        decimal currentUnitPrice,
        string expectedMessage)
    {
        var validRequest = ProductTestDataFactory.ValidRequest();
        var request = validRequest with
        {
            stockQuantity = stockQuantity,
            currentUnitPrice = currentUnitPrice
        };
        var repository = ProductRepositoryMock.Create();
        var service = new ProductServices(repository.Object);

        var exception = await Assert.ThrowsAsync<InvalidProductDataException>(
            () => service.AddProduct(request));

        Assert.Equal(expectedMessage, exception.Message);
        repository.Verify(repo => repo.First<Product>(
            It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(),
            It.IsAny<string[]>()), Times.Never);
        repository.Verify(repo => repo.Add(It.IsAny<Product>()), Times.Never);
    }
}
