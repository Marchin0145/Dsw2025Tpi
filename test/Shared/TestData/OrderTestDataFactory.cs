using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Testing.Builders;

namespace Dsw2025Tpi.Testing.TestData;

internal static class OrderTestDataFactory
{
    public static (Customer Customer, OrderBuilder Order) Valid()
    {
        var mouse = new ProductBuilder().Build();
        var keyboard = new ProductBuilder
        {
            Sku = "TP4-002",
            InternalCode = "INT-002",
            Name = "Teclado Mecánico",
            UnitPrice = 250m
        }.Build();

        return (new CustomerBuilder().Build(), new OrderBuilder(
            [new OrderItemBuilder(mouse, 2), new OrderItemBuilder(keyboard, 3)]));
    }

    public static (Customer Customer, OrderBuilder Order) InsufficientStock()
    {
        var product = new ProductBuilder { StockQuantity = 2 }.Build();
        return (new CustomerBuilder().Build(), new OrderBuilder([new OrderItemBuilder(product, 5)]));
    }

    public static (string UserName, OrderBuilder Order) MissingCustomer()
    {
        var (_, order) = Valid();
        return ("cliente-inexistente@tp4.example", order);
    }
}
