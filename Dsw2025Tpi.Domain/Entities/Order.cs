using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities
{
    public class Order:EntityBase
    {

        public Guid CustomerId { get; set; }

        public string NameCustomer { get; set; }

        // public Customer Customer { get; set; }

        public DateTime Date { get; } = DateTime.UtcNow;

        public string ShippingAddress { get; set; }
        public string BillingAddress { get; set; }
        public string Notes { get; set; }

        public decimal TotalAmount { get;set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public  List<OrderItem>? OrderItems { get; set; }

        public Order(Guid customerId, string shippingAddress, string billingAddress, string notes)
        {
            this.CustomerId = customerId;
            this.Date = DateTime.UtcNow;
            this.ShippingAddress = shippingAddress;
            this.BillingAddress = billingAddress;
            this.Notes = notes;
            this.Status = OrderStatus.Pending;
        }

        public void setOrderItems(List<OrderItem> list) {
            OrderItems = list;

            TotalAmount = list.Sum(p=>p.Subtotal);

        
        }

        public Order() {
        
        }

    }
}
