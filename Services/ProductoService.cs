using test_api.DTOs;
using test_api.Data.UnitOfWork;
using test_api.Models;

namespace test_api.Services
{
    public interface IProductoService
    {
        Task<ProductoDto?> GetProductoByIdAsync(int id);
        Task<IEnumerable<ProductoDto>> GetAllProductosAsync();
        Task<ProductoDto> CreateProductoAsync(CreateProductoDto dto);
        Task<ProductoDto> UpdateProductoAsync(UpdateProductoDto dto);
        Task DeleteProductoAsync(int id);
    }

    public class ProductoService : IProductoService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductoService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ProductoDto?> GetProductoByIdAsync(int id)
        {
            // ?? Include la categoría relacionada
            var producto = await _unitOfWork.Productos.GetByIdAsync(id, p => p.Categoria);
            return producto == null ? null : MapToDto(producto);
        }

        public async Task<IEnumerable<ProductoDto>> GetAllProductosAsync()
        {
            // ?? Include la categoría para todos los productos
            var productos = await _unitOfWork.Productos.GetAllAsync(p => p.Categoria);
            return productos.Select(MapToDto).ToList();
        }

        public async Task<ProductoDto> CreateProductoAsync(CreateProductoDto dto)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Validar que existe la categoria
                var categoriaExiste = await _unitOfWork.Categorias.ExistsAsync(c => c.Id == dto.CategoriaId);
                if (!categoriaExiste)
                {
                    throw new KeyNotFoundException($"Categoría con ID {dto.CategoriaId} no encontrada");
                }

                // Validar que no exista otro producto con el mismo nombre
                var existe = await _unitOfWork.Productos.ExistsAsync(p => p.Nombre == dto.Nombre);
                if (existe)
                {
                    throw new InvalidOperationException($"Ya existe un producto con el nombre '{dto.Nombre}'");
                }

                var producto = new Producto
                {
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    Precio = dto.Precio,
                    Stock = dto.Stock,
                    CategoriaId = dto.CategoriaId,
                    FechaCreacion = DateTime.UtcNow
                };

                await _unitOfWork.Productos.AddAsync(producto);
                await _unitOfWork.CommitAsync();

                return MapToDto(producto);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<ProductoDto> UpdateProductoAsync(UpdateProductoDto dto)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // ?? Include la categoría cuando obtenemos el producto
                var producto = await _unitOfWork.Productos.GetByIdAsync(dto.Id, p => p.Categoria);
                if (producto == null)
                {
                    throw new KeyNotFoundException($"Producto con ID {dto.Id} no encontrado");
                }

                // Validar que existe la categoria si cambió
                if (producto.CategoriaId != dto.CategoriaId)
                {
                    var categoriaExiste = await _unitOfWork.Categorias.ExistsAsync(c => c.Id == dto.CategoriaId);
                    if (!categoriaExiste)
                    {
                        throw new KeyNotFoundException($"Categoría con ID {dto.CategoriaId} no encontrada");
                    }
                }

                // Validar que no exista otro producto con el mismo nombre
                var existe = await _unitOfWork.Productos.ExistsAsync(p => p.Nombre == dto.Nombre && p.Id != dto.Id);
                if (existe)
                {
                    throw new InvalidOperationException($"Ya existe otro producto con el nombre '{dto.Nombre}'");
                }

                producto.Nombre = dto.Nombre;
                producto.Descripcion = dto.Descripcion;
                producto.Precio = dto.Precio;
                producto.Stock = dto.Stock;
                producto.Activo = dto.Activo;
                producto.CategoriaId = dto.CategoriaId;
                producto.FechaActualizacion = DateTime.UtcNow;

                await _unitOfWork.Productos.UpdateAsync(producto);
                await _unitOfWork.CommitAsync();

                return MapToDto(producto);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteProductoAsync(int id)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var producto = await _unitOfWork.Productos.GetByIdAsync(id);
                if (producto == null)
                {
                    throw new KeyNotFoundException($"Producto con ID {id} no encontrado");
                }

                await _unitOfWork.Productos.DeleteAsync(producto);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        private static ProductoDto MapToDto(Producto producto)
        {
            return new ProductoDto
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                Stock = producto.Stock,
                Activo = producto.Activo,
                FechaCreacion = producto.FechaCreacion,
                FechaActualizacion = producto.FechaActualizacion,
                CategoriaId = producto.CategoriaId,
                // ?? Ahora SÍ tiene datos porque se cargó con Include
                Categoria = producto.Categoria == null ? null : new CategoriaDto
                {
                    Id = producto.Categoria.Id,
                    Nombre = producto.Categoria.Nombre,
                    Descripcion = producto.Categoria.Descripcion,
                    Activa = producto.Categoria.Activa,
                    FechaCreacion = producto.Categoria.FechaCreacion,
                    FechaActualizacion = producto.Categoria.FechaActualizacion,
                    Productos = new List<ProductoDto>()  // Para evitar circular reference
                }
            };
        }
    }
}
