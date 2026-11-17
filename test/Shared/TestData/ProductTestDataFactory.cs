using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Testing.Builders;

namespace Dsw2025Tpi.Testing.TestData;

internal static class ProductTestDataFactory
{
    public static ProductModel.RequestProduct ValidRequest() => new ProductBuilder().BuildRequest();

    public static (Product Existing, ProductModel.RequestProduct Request) DuplicateSku()
    {
        var product = new ProductBuilder();
        return (product.Build(), (product with { InternalCode = "INT-002" }).BuildRequest());
    }

    public static ProductModel.RequestProduct ZeroStock() =>
        new ProductBuilder { StockQuantity = 0 }.BuildRequest();

    public static ProductModel.RequestProduct NegativeStock() =>
        new ProductBuilder { StockQuantity = -1 }.BuildRequest();

    public static ProductModel.RequestProduct NegativePrice() =>
        new ProductBuilder { UnitPrice = -1m }.BuildRequest();

    public static IReadOnlyList<Product> SearchCatalog() =>
    [
        CatalogProduct(1, "Mouse Gamer").Build(),
        CatalogProduct(2, "Teclado Mecánico").Build(),
        (CatalogProduct(3, "Monitor") with { IsActive = false }).Build()
    ];

    public static (IReadOnlyList<Product> Products, int PageNumber, int PageSize) Pagination()
    {
        var products = Enumerable.Range(1, 15)
            .Select(index => CatalogProduct(index, $"Producto {index:D2}").Build())
            .ToArray();

        return (products, 2, 5);
    }

    private static ProductBuilder CatalogProduct(int index, string name) =>
        new() { Sku = $"TP4-{index:D3}", InternalCode = $"INT-{index:D3}", Name = name };
}
