using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities
{
    public class OrderItem:EntityBase
    {
        public Guid OrderId { get; set; }
       
     //  public Order Order { get; set; }

        public Guid ProductId { get; set; }
       //public Product Product { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public decimal Subtotal { get; set; }

        public OrderItem(Guid orderId,Guid productId, Product product, int quantity, decimal unitPrice)
        {
            OrderId=orderId ;
            // _order = order;
            ProductId= productId;
           // _product = product;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }

        public OrderItem() { }
   


}
}
