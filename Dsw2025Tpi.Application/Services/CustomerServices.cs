using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;

namespace Dsw2025Tpi.Application.Services
{
    public class CustomerServices
    {
        private readonly IRepository _repository;

        public CustomerServices(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<Customer?> AddCustomer(CustomerModel.RequestCustomer request) 
        {
            if (request.UserId is null) {
                throw new ArgumentException("El UserId no puede ser nulo");
            }

            var customer = new Customer(request.UserId,request.Name,request.Email,request.PhoneNumber);

            await _repository.Add(customer);

            return customer;
        }
            


             


    }
}
