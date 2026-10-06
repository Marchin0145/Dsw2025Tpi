using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities
{
    public class Customer:EntityBase
    {
        public string UserName { get; set; } // Relación con User, si es necesario
        public  string Name { get; set; }
        public  string Email { get; set; }
        public  string PhoneNumber { get; set; }



        public Customer(string userName,string name, string email, string phoneNumber) 
        {
            this.UserName=userName;
            this.Name = name;
            this.Email = email;
            this.PhoneNumber = phoneNumber;
        }

        public Customer() {}
    }
}
