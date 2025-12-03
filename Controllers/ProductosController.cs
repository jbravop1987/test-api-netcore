using Microsoft.AspNetCore.Mvc;
using test_api.DTOs;
using test_api.Services;
using test_api.Core.Responses;

namespace test_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly IProductoService _productoService;

        public ProductosController(IProductoService productoService)
        {
            _productoService = productoService;
        }

        /// <summary>
        /// Obtiene todos los productos con información de categoría
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductoDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var productos = await _productoService.GetAllProductosAsync();
            return Ok(ApiResponse<IEnumerable<ProductoDto>>.SuccessResponse(
                productos,
                "Productos obtenidos exitosamente"
            ));
        }

        /// <summary>
        /// Obtiene un producto por ID con información de categoría
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProductoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var producto = await _productoService.GetProductoByIdAsync(id);
            if (producto == null)
            {
                return NotFound(ApiResponse.ErrorResponse($"Producto con ID {id} no encontrado"));
            }

            return Ok(ApiResponse<ProductoDto>.SuccessResponse(
                producto,
                "Producto obtenido exitosamente"
            ));
        }

        /// <summary>
        /// Crea un nuevo producto (requiere CategoriaId válido)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ProductoDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateProductoDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse.ErrorResponse("Datos inválidos", errors));
            }

            var producto = await _productoService.CreateProductoAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = producto.Id }, 
                ApiResponse<ProductoDto>.SuccessResponse(producto, "Producto creado exitosamente"));
        }

        /// <summary>
        /// Actualiza un producto existente
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProductoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductoDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(ApiResponse.ErrorResponse("El ID en la URL no coincide con el ID del cuerpo"));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse.ErrorResponse("Datos inválidos", errors));
            }

            var producto = await _productoService.UpdateProductoAsync(dto);
            return Ok(ApiResponse<ProductoDto>.SuccessResponse(
                producto,
                "Producto actualizado exitosamente"
            ));
        }

        /// <summary>
        /// Elimina un producto
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            await _productoService.DeleteProductoAsync(id);
            return Ok(ApiResponse.SuccessResponse("Producto eliminado exitosamente"));
        }
    }
}
