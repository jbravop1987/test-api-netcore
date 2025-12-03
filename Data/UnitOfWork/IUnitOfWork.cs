using test_api.Data.Repositories;
using test_api.Models;

namespace test_api.Data.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Producto> Productos { get; }
        IRepository<Categoria> Categorias { get; }
        
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
