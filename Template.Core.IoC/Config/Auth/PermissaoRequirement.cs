using Microsoft.AspNetCore.Authorization;
using Template.Core.Domain.Usuarios.Enums;

namespace Template.Core.IoC.Config.Auth;

public class PermissaoRequirement(Permissao permissao) : IAuthorizationRequirement
{
    public Permissao Permissao { get; } = permissao;
}
