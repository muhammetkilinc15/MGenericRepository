using Microsoft.EntityFrameworkCore;

namespace GenericRepository.Services
{
    public interface IUnitOfWork<TContext> : IDisposable where TContext : DbContext
    {
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitAsync(CancellationToken cancellationToken = default);
        Task RollbackAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        int SaveChanges();
    }
}
