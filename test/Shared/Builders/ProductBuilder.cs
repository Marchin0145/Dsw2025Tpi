using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Entities;

namespace Dsw2025Tpi.Testing.Builders;

internal sealed record ProductBuilder
{
    public string Sku { get; init; } = "TP4-001";
    public string InternalCode { get; init; } = "INT-001";
    public string Name { get; init; } = "Mouse Gamer";
    public string Description { get; init; } = "Producto de prueba";
    public decimal UnitPrice { get; init; } = 100m;
    public int StockQuantity { get; init; } = 10;
    public bool IsActive { get; init; } = true;

    public Product Build() =>
        new(Sku, InternalCode, Name, Description, UnitPrice, StockQuantity) { IsActive = IsActive };

    public ProductModel.RequestProduct BuildRequest() =>
        new(Sku, InternalCode, Name, Description, UnitPrice, StockQuantity);
}
