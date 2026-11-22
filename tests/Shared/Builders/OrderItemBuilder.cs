using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Entities;

namespace ECommerce.Testing.Builders;

internal sealed record OrderItemBuilder(Product Product, int Quantity = 1)
{
    public OrderItem Build(Guid orderId) =>
        new(orderId, Product.Id, Product, Quantity, Product.CurrentUnitPrice)
        {
            Subtotal = Quantity * Product.CurrentUnitPrice
        };

    public OrderItemModel.RequestOrderItem BuildRequest() =>
        new(Product.Id, Product.Name, Quantity, Product.CurrentUnitPrice);
}
