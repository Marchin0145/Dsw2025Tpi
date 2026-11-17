using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Entities;

namespace Dsw2025Tpi.Testing.Builders;

internal sealed record OrderBuilder(IReadOnlyList<OrderItemBuilder> Items)
{
    public string ShippingAddress { get; init; } = "Av. Independencia 1800";
    public string BillingAddress { get; init; } = "Av. Independencia 1800";
    public string Notes { get; init; } = "Datos de prueba";

    public Order Build(Customer customer)
    {
        var order = new Order(customer.Id, ShippingAddress, BillingAddress, Notes);
        order.setOrderItems(Items.Select(item => item.Build(order.Id)).ToList());
        return order;
    }

    public OrderModel.RequestOrder BuildRequest() =>
        new(ShippingAddress, BillingAddress, Notes, Items.Select(item => item.BuildRequest()).ToList());
}
