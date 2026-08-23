using Template.Core.Domain.Abstractions;
using Template.Core.Domain.Usuarios.Entity;
using Template.Core.Domain.Usuarios.Repository;
using Template.Core.Domain.Usuarios.Repository.Filters;
using NHibernate;
using NHibernate.Linq;

namespace Template.Core.Infra.Usuarios.Repository;

public class UsuarioRepository(ISession Session) : GenericRepository<Usuario>(Session), IUsuarioRepository
{
    public async Task<Usuario?> RecuperarPorLoginAsync(string login, CancellationToken cancellationToken = default)
    {
        string normalizado = Usuario.NormalizarLogin(login);
        return await _session.Query<Usuario>()
            .SingleOrDefaultAsync(u => u.Login == normalizado, cancellationToken);
    }

    public async Task<bool> ExisteAlgumAsync(CancellationToken cancellationToken = default)
        => await _session.Query<Usuario>().AnyAsync(cancellationToken);

    public Task<List<Usuario>> Filtrar(ListarUsuarioFilter filtros, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
