using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Testing.TestData;
using Dsw2025Tpi.UnitTests.Mocks;
using Xunit;

namespace Dsw2025Tpi.UnitTests;

public class ProductListingServicesTests
{
    [Fact]
    public async Task GetProducts_WhenIncludeInactiveIsFalse_ReturnsOnlyActiveProducts()
    {
        var products = ProductTestDataFactory.SearchCatalog();
        var repository = ProductRepositoryMock.Create(products.ToArray());
        var service = new ProductServices(repository.Object);

        var result = await service.GetProducts(includeInactive: false);

        Assert.All(result.ProductItems, product => Assert.True(product.IsActive));
        Assert.Equal(2, result.TotalItems);
    }

    [Fact]
    public async Task GetProducts_WithPaginationFilter_AppliesSkipAndTakeCorrectly()
    {
        var (products, pageNumber, pageSize) = ProductTestDataFactory.Pagination();
        var repository = ProductRepositoryMock.Create(products.ToArray());
        var service = new ProductServices(repository.Object);

        var result = await service.GetProducts(
            includeInactive: true,
            pageNumber: pageNumber,
            pageSize: pageSize);

        Assert.Equal(15, result.TotalItems);
        Assert.Equal(5, result.ProductItems.Count);
        Assert.Equal(
            products.OrderBy(product => product.Sku, StringComparer.OrdinalIgnoreCase)
                .Skip(5)
                .Take(5)
                .Select(product => product.Id),
            result.ProductItems.Select(product => product.Id));
    }

    [Theory]
    [InlineData("mOuSe", "Mouse Gamer")]
    [InlineData("tp4-001", "Mouse Gamer")]
    [InlineData("int-002", "Teclado Mecánico")]
    public async Task GetProducts_WithSearchTerm_FiltersByNameSkuOrInternalCode(
        string searchTerm,
        string expectedName)
    {
        var products = ProductTestDataFactory.SearchCatalog();
        var repository = ProductRepositoryMock.Create(products.ToArray());
        var service = new ProductServices(repository.Object);

        var result = await service.GetProducts(searchTerm: searchTerm);

        var product = Assert.Single(result.ProductItems);
        Assert.Equal(expectedName, product.Name);
    }
}
