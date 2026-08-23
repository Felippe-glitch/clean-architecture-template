using Template.Core.Domain.Usuarios.Command;
using Template.Core.Domain.Usuarios.Entity;
using Template.Core.Domain.Usuarios.Enums;

namespace Template.Core.Domain.Usuarios.Service;

public interface IUsuarioService
{
    Task<Usuario> Registrar(UsuarioRegistrarCommand command, CancellationToken cancellationToken = default);
    Task<Usuario?> RecuperarPorLogin(string login, CancellationToken cancellationToken = default);
    Task<Usuario> Validar(int id, CancellationToken cancellationToken = default);
    Task<Usuario> AlterarSituacao(int id, bool ativo, UsuarioRoleEnum atorRole, int atorId, CancellationToken cancellationToken = default);
    Task<bool> ExisteAlgum(CancellationToken cancellationToken = default);
}
