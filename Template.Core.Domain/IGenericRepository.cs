using Template.Core.Domain.Abstractions;

namespace Template.Core.Domain;

public interface IGenericRepository<T> where T : class
{
    Task<T> RecuperarAsync(int id, CancellationToken cancellationToken = default);
    Task<T> InserirAsync(T entity, CancellationToken cancellationToken = default);
    Task<T> AtualizarAsync(T entity, CancellationToken cancellationToken = default);
    Task<PaginatedResult<T>> ListarAsync(IQueryable<T> query, int pagina, int quantidade, string cpOrd = "id", TipoOrdenacao tpOrd = TipoOrdenacao.Ascendente);
    Task DeletarAsync(T tentity, CancellationToken cancellationToken = default);
}