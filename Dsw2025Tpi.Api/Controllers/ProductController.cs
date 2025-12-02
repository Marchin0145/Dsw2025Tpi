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
    [Authorize]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly ProductServices _service;
        public ProductController(ProductServices servicio) => _service = servicio;

        [ProducesResponseType(typeof(ProductModel.ResponseProduct), StatusCodes.Status201Created)]
        [HttpPost]
       // [Authorize(Roles ="Employee")]
        public async Task<IActionResult> addProduct([FromBody]ProductModel.RequestProduct objeto)
        {
                var producto = await _service.AddProduct(objeto);
                return CreatedAtAction(nameof(GetProductById), new { id = producto.Id},producto);
        }


        [HttpGet]
        [AllowAnonymous] //para que entre cualquiera
        //[Authorize(Roles ="tester")]
        public async Task<IActionResult> getProducts([FromQuery] int? page = null, [FromQuery] int? limit = null, [FromQuery] string? search = null, [FromQuery] bool? isActive = null, [FromQuery] bool isMostStock=false) { 
        var productos= await _service.GetProducts(page, limit, search,isActive,isMostStock);
            return Ok(productos);
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
        public async Task<IActionResult> ChangeStateProduct(Guid id)
        {
            await _service.ChangeStateProduct(id);
            return NoContent();
        }



    }
}
