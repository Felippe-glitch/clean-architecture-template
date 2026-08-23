using Template.Core.Domain.Abstractions;
using Template.Core.Domain.Usuarios.Entity;
using Template.Core.Domain.Usuarios.Repository.Filters;

namespace Template.Core.Domain.Usuarios.Repository;

public interface IUsuarioRepository : IGenericRepository<Usuario>
{
    Task<Usuario> RecuperarPorLoginAsync(string login, CancellationToken cancellationToken = default);
    Task<bool> ExisteAlgumAsync(CancellationToken cancellationToken = default);
    Task<List<Usuario>> Filtrar(ListarUsuarioFilter filtros, CancellationToken cancellationToken);
}
