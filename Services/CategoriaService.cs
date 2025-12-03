using test_api.DTOs;
using test_api.Data.UnitOfWork;
using test_api.Models;

namespace test_api.Services
{
    public interface ICategoriaService
    {
        Task<CategoriaDto?> GetCategoriaByIdAsync(int id);
        Task<IEnumerable<CategoriaDto>> GetAllCategoriasAsync();
        Task<CategoriaDto> CreateCategoriaAsync(CreateCategoriaDto dto);
        Task<CategoriaDto> UpdateCategoriaAsync(UpdateCategoriaDto dto);
        Task DeleteCategoriaAsync(int id);
    }

    public class CategoriaService : ICategoriaService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoriaService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CategoriaDto?> GetCategoriaByIdAsync(int id)
        {
            // ?? Include los productos relacionados
            var categoria = await _unitOfWork.Categorias.GetByIdAsync(id, c => c.Productos);
            return categoria == null ? null : MapToDto(categoria);
        }

        public async Task<IEnumerable<CategoriaDto>> GetAllCategoriasAsync()
        {
            // ?? Include los productos para todas las categorías
            var categorias = await _unitOfWork.Categorias.GetAllAsync(c => c.Productos);
            return categorias.Select(MapToDto).ToList();
        }

        public async Task<CategoriaDto> CreateCategoriaAsync(CreateCategoriaDto dto)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Validar que no exista otra categoría con el mismo nombre
                var existe = await _unitOfWork.Categorias.ExistsAsync(c => c.Nombre == dto.Nombre);
                if (existe)
                {
                    throw new InvalidOperationException($"Ya existe una categoría con el nombre '{dto.Nombre}'");
                }

                var categoria = new Categoria
                {
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    FechaCreacion = DateTime.UtcNow
                };

                await _unitOfWork.Categorias.AddAsync(categoria);
                await _unitOfWork.CommitAsync();

                return MapToDto(categoria);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<CategoriaDto> UpdateCategoriaAsync(UpdateCategoriaDto dto)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // ?? Include los productos cuando obtenemos la categoría
                var categoria = await _unitOfWork.Categorias.GetByIdAsync(dto.Id, c => c.Productos);
                if (categoria == null)
                {
                    throw new KeyNotFoundException($"Categoría con ID {dto.Id} no encontrada");
                }

                // Validar que no exista otra categoría con el mismo nombre
                var existe = await _unitOfWork.Categorias.ExistsAsync(c => c.Nombre == dto.Nombre && c.Id != dto.Id);
                if (existe)
                {
                    throw new InvalidOperationException($"Ya existe otra categoría con el nombre '{dto.Nombre}'");
                }

                categoria.Nombre = dto.Nombre;
                categoria.Descripcion = dto.Descripcion;
                categoria.Activa = dto.Activa;
                categoria.FechaActualizacion = DateTime.UtcNow;

                await _unitOfWork.Categorias.UpdateAsync(categoria);
                await _unitOfWork.CommitAsync();

                return MapToDto(categoria);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteCategoriaAsync(int id)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var categoria = await _unitOfWork.Categorias.GetByIdAsync(id);
                if (categoria == null)
                {
                    throw new KeyNotFoundException($"Categoría con ID {id} no encontrada");
                }

                await _unitOfWork.Categorias.DeleteAsync(categoria);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        private static CategoriaDto MapToDto(Categoria categoria)
        {
            return new CategoriaDto
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                Activa = categoria.Activa,
                FechaCreacion = categoria.FechaCreacion,
                FechaActualizacion = categoria.FechaActualizacion,
                // ?? Ahora SÍ tiene datos porque se cargó con Include
                Productos = categoria.Productos
                    .Select(p => new ProductoDto
                    {
                        Id = p.Id,
                        Nombre = p.Nombre,
                        Descripcion = p.Descripcion,
                        Precio = p.Precio,
                        Stock = p.Stock,
                        Activo = p.Activo,
                        FechaCreacion = p.FechaCreacion,
                        FechaActualizacion = p.FechaActualizacion,
                        CategoriaId = p.CategoriaId,
                        Categoria = null  // Para evitar circular reference
                    })
                    .ToList()
            };
        }
    }
}
