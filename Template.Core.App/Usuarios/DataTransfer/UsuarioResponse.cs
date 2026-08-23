using Template.Core.App.Common;
using Template.Core.Domain.Usuarios.Enums;

namespace Template.Core.App.Usuarios.DataTransfer;

public record UsuarioResponse
{
    public int Id { get; init; }
    public string Login { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public UsuarioRoleEnum Role { get; init; }
    public bool Ativo { get; init; }
}

public record ListarUsuarioResponse : PaginatedResponse<UsuarioResponse>
{
}