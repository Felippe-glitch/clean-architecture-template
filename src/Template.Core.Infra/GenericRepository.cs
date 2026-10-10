using System.Linq.Expressions;
using System.Reflection;

using Microsoft.EntityFrameworkCore;

using Template.Core.CrossCutting.Pagination;
using Template.Core.Domain;

namespace Template.Core.Infra;

public abstract class GenericRepository<T>(TemplateDbContext Context) : IGenericRepository<T> where T : class
{
    protected readonly TemplateDbContext _context = Context;

    public async Task<T> GetAsync(int id, CancellationToken cancellationToken = default)
        => await _context.Set<T>().FindAsync([id], cancellationToken);

    public Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _context.Set<T>().Update(entity);
        return Task.FromResult(entity);
    }

    public async Task<PaginatedResult<T>> ListAsync(IQueryable<T> query, int page, int pageSize, string sortBy = "id", SortDirection sortDirection = SortDirection.Ascending)
    {
        return await ListAsync<T>(query, page, pageSize, sortBy, sortDirection);
    }

    protected static async Task<PaginatedResult<TResult>> ListAsync<TResult>(IQueryable<TResult> query, int page, int pageSize, string sortBy = "id", SortDirection sortDirection = SortDirection.Ascending) where TResult : class
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        int totalItems = await query.CountAsync();

        query = ApplySorting(query, sortBy, sortDirection);

        IList<TResult> data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResult<TResult>(data, page, pageSize, totalItems);
    }

    public async Task<T> InsertAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<T>().AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _context.Set<T>().Remove(entity);
        return Task.CompletedTask;
    }

    private static IQueryable<TResult> ApplySorting<TResult>(IQueryable<TResult> query, string sortBy, SortDirection sortDirection)
    {
        PropertyInfo? property =
            typeof(TResult).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
            ?? typeof(TResult).GetProperty("Id", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (property is null)
            return query;

        ParameterExpression parameter = Expression.Parameter(typeof(TResult), "x");
        MemberExpression access = Expression.Property(parameter, property);
        LambdaExpression selector = Expression.Lambda(access, parameter);

        string method = sortDirection == SortDirection.Ascending ? "OrderBy" : "OrderByDescending";

        MethodCallExpression call = Expression.Call(
            typeof(Queryable),
            method,
            new[] { typeof(TResult), property.PropertyType },
            query.Expression,
            Expression.Quote(selector));

        return query.Provider.CreateQuery<TResult>(call);
    }
}
