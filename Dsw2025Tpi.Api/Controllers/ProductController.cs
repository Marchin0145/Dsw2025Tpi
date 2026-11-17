using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Dsw2025Tpi.Api.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly ProductServices _service;
        public ProductController(ProductServices servicio) => _service = servicio;

        [ProducesResponseType(typeof(ProductModel.ResponseProduct), StatusCodes.Status201Created)]
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> addProduct([FromBody]ProductModel.RequestProduct objeto)
        {
                var producto = await _service.AddProduct(objeto);
                return CreatedAtAction(nameof(GetProductById), new { id = producto.Id},producto);
        }

        
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> getProducts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery(Name = "search")] string? search = null)
        {
            var products = await _service.GetProducts(false, pageNumber, pageSize, search);
            return Ok(products);
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> getAdminProducts(
            [FromQuery] bool includeInactive = true,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery(Name = "search")] string? search = null)
        {
            var products = await _service.GetProducts(includeInactive, pageNumber, pageSize, search);
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(Guid id)
        { 
            var producto = await _service.GetProductById(id);

            return Ok(producto);
        }

        [HttpPut("{id}")]
        //[Authorize(Roles = "Employee")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody]ProductModel.RequestProduct productoActualizado) 
        {
            var producto= await _service.UpdateProduct(id, productoActualizado);
           return Ok(producto);
        }
        [HttpPatch("{id}")]
        //[Authorize(Roles = "Employee")]
        public async Task<IActionResult> DisableProduct(Guid id)
        {
            await _service.DisableProduct(id);
            return NoContent();
        }



    }
}
