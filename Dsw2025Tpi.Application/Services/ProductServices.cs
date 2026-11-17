using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Dsw2025Tpi.Application.Services
{
    public class ProductServices
    {
        private readonly IRepository _repository;

        public ProductServices(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<ProductListModel> GetProducts(
            bool includeInactive = false,
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "La página debe ser mayor a cero.");
            }

            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "El tamaño de página debe ser mayor a cero.");
            }

            var products = await _repository.GetAll<Product>();
            if (products is null)
            {
                throw new NoContentException("no se encontraron productos");
            }

            IEnumerable<Product> filteredProducts = products;
            if (!includeInactive)
            {
                filteredProducts = filteredProducts.Where(product => product.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                filteredProducts = filteredProducts.Where(product =>
                    product.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    product.Sku!.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    product.InternalCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            var orderedProducts = filteredProducts
                .OrderBy(product => product.Sku, StringComparer.OrdinalIgnoreCase)
                .ThenBy(product => product.Id)
                .ToArray();
            var totalItems = orderedProducts.Length;
            var page = orderedProducts
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToArray();

            return new ProductListModel(page, totalItems, pageNumber, pageSize);
        }


        public async Task<Product?> AddProduct(ProductModel.RequestProduct request) {

            if (request.currentUnitPrice <= 0)
            {
                throw new IncorrectPriceException("Precio menor o igual a cero");
            }

            if (request.stockQuantity <= 0)
            {
                throw new ArgumentException("el stock debe ser mayor a cero");
            }

            if (string.IsNullOrWhiteSpace(request.name) ||
                  string.IsNullOrWhiteSpace(request.sku) ||
                  string.IsNullOrWhiteSpace(request.internalCode)
                    )
            {
                throw new ArgumentException("valores incompletos");
            }
            var existenciaSku = await _repository.First<Product>(P => P.Sku == request.sku);
            if (existenciaSku is not null) throw new DuplicatedEntityException("ya existe un producto con ese sku");

            var existenciaIC = await _repository.First<Product>(P => P.InternalCode == request.internalCode);
            if (existenciaIC is not null) throw new DuplicatedEntityException("ya existe un producto con ese internalCode");


            var producto = new Product(request.sku, request.internalCode, request.name, request.description, request.currentUnitPrice, request.stockQuantity);


                await _repository.Add(producto);
            return producto;

        }

        public async Task<Product?> GetProductById(Guid id) {
           var product= await _repository.GetById<Product>(id);
            if (product is null) throw new NotFoundEntityException("no se encontro el producto");
            return product;
        } 

        public async Task<Product?> UpdateProduct(Guid id,ProductModel.RequestProduct request) {
            if (request.currentUnitPrice <= 0)
            {
                throw new IncorrectPriceException("Precio menor o igual a cero");
            }
            if (request.stockQuantity< 0)
            {
                throw new IncorrectPriceException("el stock no puede ser negativo");
            }


            if (string.IsNullOrWhiteSpace(request.name) ||
               string.IsNullOrWhiteSpace(request.sku) ||
               string.IsNullOrWhiteSpace(request.internalCode) 
                ) throw new ArgumentException("valores incompletos");
           
            
            var producto = await _repository.GetById<Product>(id);
            


            if (producto is null) throw new NotFoundEntityException("no se econtro el producto");

            producto.Sku= request.sku;
            producto.InternalCode = request.internalCode;
            producto.Name = request.name;
            producto.Description = request.description;
            producto.CurrentUnitPrice = request.currentUnitPrice;
            producto.StockQuantity = request.stockQuantity;

            await _repository.Update(producto);

            return producto;
        }

        public async Task<bool> DisableProduct(Guid id)
        {
            var producto = await _repository.GetById<Product>(id);
            if (producto is null) throw new NotFoundEntityException("no se econtro el producto");
            if(producto.IsActive is false) throw new ArgumentException("el producto ya se encuentra deshabilitado");
            producto.IsActive = false;
            await _repository.Update(producto);
            return true;
        }



    }
}
