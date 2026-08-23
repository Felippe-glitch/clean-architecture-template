using System.Linq.Expressions;
using System.Reflection;
using NHibernate;
using NHibernate.Linq;
using Template.Core.Domain;
using Template.Core.Domain.Abstractions;

namespace Template.Core.Infra;

public abstract class GenericRepository<T>(ISession Session) : IGenericRepository<T> where T : class
{
    public readonly ISession _session = Session;

    public async Task<T> RecuperarAsync(int id, CancellationToken cancellationToken = default)
    {
        T entity = await _session.GetAsync<T>(id);
        return entity;
    }

    public async Task<T> AtualizarAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _session.UpdateAsync(entity);
        return entity;
    }

    public async Task<PaginatedResult<T>> ListarAsync(IQueryable<T> query, int pagina, int quantidade, string cpOrd = "id", TipoOrdenacao tpOrd = TipoOrdenacao.Ascendente)
    {
        return await ListarAsync<T>(query, pagina, quantidade, cpOrd, tpOrd);
    }

    protected static async Task<PaginatedResult<TResult>> ListarAsync<TResult>(IQueryable<TResult> query, int pagina, int quantidade, string cpOrd = "id", TipoOrdenacao tpOrd = TipoOrdenacao.Ascendente) where TResult : class
    {
        if (pagina < 1) pagina = 1;
        if (quantidade < 1) quantidade = 10;

        int totalItems = await query.CountAsync();

        query = AplicarOrdenacao(query, cpOrd, tpOrd);

        IList<TResult> data = await query
            .Skip((pagina - 1) * quantidade)
            .Take(quantidade)
            .ToListAsync();

        return new PaginatedResult<TResult>(data, pagina, quantidade, totalItems);
    }

    public async Task<T> InserirAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _session.SaveAsync(entity);
        return entity;
    }

    public async Task DeletarAsync(T tentity, CancellationToken cancellationToken = default)
    {
        await _session.DeleteAsync(tentity);
    }

    private static IQueryable<TResult> AplicarOrdenacao<TResult>(IQueryable<TResult> query, string cpOrd, TipoOrdenacao tpOrd)
    {
        PropertyInfo? propriedade =
            typeof(TResult).GetProperty(cpOrd, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
            ?? typeof(TResult).GetProperty("Id", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (propriedade is null)
            return query;

        ParameterExpression parametro = Expression.Parameter(typeof(TResult), "x");
        MemberExpression acesso = Expression.Property(parametro, propriedade);
        LambdaExpression seletor = Expression.Lambda(acesso, parametro);

        string metodo = tpOrd == TipoOrdenacao.Ascendente ? "OrderBy" : "OrderByDescending";

        MethodCallExpression chamada = Expression.Call(
            typeof(Queryable),
            metodo,
            new[] { typeof(TResult), propriedade.PropertyType },
            query.Expression,
            Expression.Quote(seletor));

        return query.Provider.CreateQuery<TResult>(chamada);
    }
}
