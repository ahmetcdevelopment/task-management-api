using System.Linq.Expressions;

namespace TaskManagement.API.Repositories;

public interface IBaseRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(string id);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> CreateAsync(T entity);
    Task<bool> UpdateAsync(string id, T entity);
    Task<bool> DeleteAsync(string id);
    Task<long> CountAsync(Expression<Func<T, bool>>? predicate = null);
}
