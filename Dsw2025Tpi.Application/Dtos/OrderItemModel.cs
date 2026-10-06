using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;

namespace Dsw2025Tpi.Application.Dtos
{
    public record OrderItemModel
    {
        public record RequestOrderItem(
              Guid productId,
              string name,
              int quantity,
              decimal currentUnitPrice);
        public record ResponseOrderItem(
              Guid productId,
          int quantity,
          string description,
          decimal currentUnitPrice,
          decimal subtotal

    );
    }

             
        
    
    
      
}
