using Template.Core.App.Usuarios.DataTransfer;
using Template.Core.Domain.Usuarios.Entity;
using Template.Core.Domain.Usuarios.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace Template.Core.App.Usuarios.Service;

public interface IUsuarioAppService
{
    Task<UsuarioResponse> RegistrarAsync(UsuarioRegistrarRequest request, CancellationToken cancellationToken = default);
    Task<UsuarioResponse> AlterarSituacaoAsync(int id, bool ativo, UsuarioRoleEnum atorRole, int atorId, CancellationToken cancellationToken = default);
    Task<ListarUsuarioResponse> ListarAsync(ListarUsuarioRequest request, CancellationToken cancellationToken);
    
}
