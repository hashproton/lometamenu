using System.Linq.Expressions;
using Domain.Common;

namespace Application.Repositories.Common;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<T?> GetByQueryAsync(Expression<Func<T, bool>> query, CancellationToken cancellationToken);
    Task<PaginatedResult<T>> GetAllAsync(PaginatedQuery paginatedQuery, CancellationToken cancellationToken);
    Task<int> AddAsync(T entity, CancellationToken cancellationToken);
    Task UpdateAsync(T entity, CancellationToken cancellationToken);
    Task DeleteAsync(T entity, CancellationToken cancellationToken);
}