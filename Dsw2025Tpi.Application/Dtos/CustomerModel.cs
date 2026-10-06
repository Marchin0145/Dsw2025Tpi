using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos
{
    public record CustomerModel
    {
        public record RequestCustomer(
    string Name,
    string Email,
    string PhoneNumber,
    string? UserId);
        public record ResponseCustomer(
            Guid Id,
            string Name,
            string Email,
            string PhoneNumber,
            string UserId
);

    }
}
