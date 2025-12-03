using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test_api.DTOs;
using test_api.Services;
using test_api.Core.Responses;

namespace test_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // ?? Requiere autenticación para todos los endpoints
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaService _categoriaService;

        public CategoriasController(ICategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }

        /// <summary>
        /// Obtiene todas las categorías con sus productos (requiere autenticación)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoriaDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll()
        {
            var categorias = await _categoriaService.GetAllCategoriasAsync();
            return Ok(ApiResponse<IEnumerable<CategoriaDto>>.SuccessResponse(
                categorias,
                "Categorías obtenidas exitosamente"
            ));
        }

        /// <summary>
        /// Obtiene una categoría por ID con todos sus productos (requiere autenticación)
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<CategoriaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById(int id)
        {
            var categoria = await _categoriaService.GetCategoriaByIdAsync(id);
            if (categoria == null)
            {
                return NotFound(ApiResponse.ErrorResponse($"Categoría con ID {id} no encontrada"));
            }

            return Ok(ApiResponse<CategoriaDto>.SuccessResponse(
                categoria,
                "Categoría obtenida exitosamente"
            ));
        }

        /// <summary>
        /// Crea una nueva categoría (requiere autenticación)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CategoriaDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateCategoriaDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse.ErrorResponse("Datos inválidos", errors));
            }

            var categoria = await _categoriaService.CreateCategoriaAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, 
                ApiResponse<CategoriaDto>.SuccessResponse(categoria, "Categoría creada exitosamente"));
        }

        /// <summary>
        /// Actualiza una categoría existente (requiere autenticación)
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<CategoriaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoriaDto dto)
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

            var categoria = await _categoriaService.UpdateCategoriaAsync(dto);
            return Ok(ApiResponse<CategoriaDto>.SuccessResponse(
                categoria,
                "Categoría actualizada exitosamente"
            ));
        }

        /// <summary>
        /// Elimina una categoría (requiere autenticación)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(int id)
        {
            await _categoriaService.DeleteCategoriaAsync(id);
            return Ok(ApiResponse.SuccessResponse("Categoría eliminada exitosamente (productos asociados también fueron eliminados)"));
        }
    }
}
