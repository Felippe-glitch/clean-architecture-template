using Template.Core.App.Common;
using Template.Core.Domain.Usuarios.Entity;

namespace Template.Core.App.Usuarios.DataTransfer;

public class ListarUsuarioRequest : PaginatedRequest<Usuario>
{
    public bool? Ativo { get; set; }
}
