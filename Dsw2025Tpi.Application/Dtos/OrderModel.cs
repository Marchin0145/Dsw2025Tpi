using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Domain.Entities;

namespace Dsw2025Tpi.Application.Dtos
{
    public record OrderModel
    {
        public record RequestOrder(
        string shippingAddress,
        string billingAddress,
        string notes,
        List<OrderItemModel.RequestOrderItem> orderItems);
          
       /* public record RequestOrder
        {
            public string UserId { get; set; }
            public string ShippingAddress { get; init; }
            public string BillingAddress { get; init; }
            public string Notes { get; init; }
            public List<OrderItemModel.RequestOrderItem> OrderItems { get; init; }
        }
       */
        public record ResponseOrder(
             Guid id,
             Guid CustomerId,
             DateTime date,
             string shippingAddress,
             string billingAddress,
             string notes,
             DateTime dateOrder,
             decimal totalAmount,
             OrderStatus status,
             List<OrderItemModel.ResponseOrderItem>? orderItems
            );

    }

}
