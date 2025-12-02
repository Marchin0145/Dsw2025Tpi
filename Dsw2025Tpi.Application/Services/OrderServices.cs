using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Dsw2025Tpi.Application.Services
{
    public class OrderServices
    {

        private readonly IRepository _repository;

        public OrderServices(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<OrderModel.ResponseOrder> AddOrder(OrderModel.RequestOrder request,string userName)
        {
            if (string.IsNullOrWhiteSpace(request.shippingAddress) ||
                string.IsNullOrWhiteSpace(request.billingAddress) ||
                !request.orderItems.Any())
            {

                throw new ArgumentException("valores incompletos");
            }

            var customer = await _repository.First<Customer>(u => u.UserName == userName);

            if (customer is null)
            {
                throw new NotFoundEntityException($"Usuario no encontrado");
            }

            var items = new List<OrderItem>();
            var itemsResponse = new List<OrderItemModel.ResponseOrderItem>();
            var orden = new Order(customer.Id, request.shippingAddress, request.billingAddress, request.notes);

            foreach (var item in request.orderItems)
            {
                var producto = await _repository.GetById<Product>(item.productId);
                if (producto is null || item.quantity < 0 || producto.StockQuantity < item.quantity || !(producto.IsActive))
                {
                    throw new ArgumentException("valores incompletos o erroneos en productos");
                }
                items.Add(new OrderItem(orden.Id, item.productId, producto, item.quantity, producto.CurrentUnitPrice));
                itemsResponse.Add(new OrderItemModel.ResponseOrderItem(item.productId, item.quantity, producto.Description, producto.CurrentUnitPrice, item.quantity * producto.CurrentUnitPrice));
                producto.StockQuantity -= item.quantity;
                //await _repository.Update(producto);

            }


            orden.setOrderItems(items);


            await _repository.Add(orden);
            return new OrderModel.ResponseOrder(orden.Id, orden.CustomerId, orden.Date, orden.ShippingAddress, orden.BillingAddress, orden.Notes, orden.Date, orden.TotalAmount, orden.Status, itemsResponse);

        }


        public async void ValidationProducts(List<OrderItemModel.RequestOrderItem> lista) {
            foreach (var item in lista)
            {
                var producto = await _repository.GetById<Product>(item.productId);
                if (producto is null || item.quantity < 0 || producto.StockQuantity < item.quantity || !(producto.IsActive))
                {
                    throw new ArgumentException("valores incompletos o erroneos en productos");
                }
            }
        }

        public async Task<Order?> GetOrderById(Guid id) {
           var orden = await _repository.GetById<Order>(id, "OrderItems");

            if (orden is null) throw new NotFoundEntityException("no se encontro ninguna orden con ese id");

            foreach (var o in orden.OrderItems)
            {
                o.Subtotal = o.UnitPrice * o.Quantity;
                var producto = await _repository.GetById<Product>(o.ProductId);
                o.NameProduct = producto != null ? producto.Name : "Desconocido";
            }
            orden.TotalAmount=orden.OrderItems.Sum(o => o.Subtotal);

            
                var usuario = await _repository.GetById<Customer>(orden.CustomerId);
                orden.NameCustomer = usuario != null ? usuario.Name : "Desconocido";
           

            return orden;
        }

        public async Task<List<Order>?> GetFilteredOrders(OrderStatus? status, Guid? customerId, int? page , int? limit, string? nameCustomer, string? userName = null)
        {
            try
            {
                // 1. Filtrar por usuario actual si corresponde
                Guid? customerActualId = null;
                if (!string.IsNullOrWhiteSpace(userName))
                {
                    var customerActual = await _repository.First<Customer>(c => c.UserName == userName);
                    if (customerActual == null)
                        return new List<Order>(); // No hay órdenes para ese usuario
                    customerActualId = customerActual.Id;
                }

                // 2. Filtrar por nombre de cliente si corresponde
                List<Guid>? idsClientes = null;
                if (!string.IsNullOrWhiteSpace(nameCustomer))
                {
                    var clientesFiltrados = await _repository.GetFiltered<Customer>(
                        c => c.Name.Contains(nameCustomer)
                    );
                    idsClientes = clientesFiltrados?.Select(c => c.Id).ToList();
                }

                // 3. Construir el filtro principal
                var orders = (await _repository.GetFiltered<Order>(
                    o => (status == null || o.Status == status.Value) &&
                         (customerId == null || o.CustomerId == customerId.Value) &&
                         (customerActualId == null || o.CustomerId == customerActualId.Value) &&
                         (idsClientes == null || idsClientes.Contains(o.CustomerId)),
                    "OrderItems"
                )).ToList();

                if (orders == null || !orders.Any())
                    throw new NoContentException("No se encontraron órdenes");

                // 4. Obtener todos los clientes involucrados en las órdenes (para evitar múltiples consultas)
                var customerIds = orders.Select(o => o.CustomerId).Distinct().ToList();
                var customers = await _repository.GetFiltered<Customer>(c => customerIds.Contains(c.Id));
                var customerDict = customers?.ToDictionary(c => c.Id, c => c.Name) ?? new Dictionary<Guid, string>();

                // 5. Procesar órdenes y asignar nombre de cliente
                foreach (var order in orders)
                {
                    order.OrderItems.ForEach(item => item.Subtotal = item.Quantity * item.UnitPrice);
                    order.TotalAmount = order.OrderItems.Sum(item => item.Subtotal);
                    order.NameCustomer = customerDict.TryGetValue(order.CustomerId, out var name) ? name : "Desconocido";
                }

                // 6. Paginación segura
                if (limit != null && page != null && limit > 0 && page > 0)
                    orders = orders.Skip(((int)page - 1) * (int)limit).Take((int)limit).ToList();

                return orders;
            }
            catch (InternalServerErrorException)
            {
                throw new InternalServerErrorException("El servidor falló inesperadamente");
            }
        }


    }
}
