using Template.Core.CrossCutting.Pagination;

namespace Template.Core.Domain;

public interface IGenericRepository<T> where T : class
{
    Task<T> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<T> InsertAsync(T entity, CancellationToken cancellationToken = default);
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task<PaginatedResult<T>> ListAsync(IQueryable<T> query, int page, int pageSize, string sortBy = "id", SortDirection sortDirection = SortDirection.Ascending);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}
