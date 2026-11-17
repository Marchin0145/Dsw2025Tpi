using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Testing.TestData;
using Dsw2025Tpi.UnitTests.Mocks;
using Moq;
using Xunit;

namespace Dsw2025Tpi.UnitTests;

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
        Assert.Equal(request.internalCode, product.InternalCode);
        Assert.Equal(request.name, product.Name);
        Assert.Equal(request.description, product.Description);
        Assert.Equal(request.currentUnitPrice, product.CurrentUnitPrice);
        Assert.Equal(request.stockQuantity, product.StockQuantity);
        Assert.True(product.IsActive);
        repository.Verify(repo => repo.Add(It.IsAny<Product>()), Times.Once);
        repository.Verify(repo => repo.Add(It.Is<Product>(added => ReferenceEquals(added, product))), Times.Once);
    }
}
