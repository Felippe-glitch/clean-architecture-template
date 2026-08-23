using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Template.Core.Domain.Usuarios.Enums;

namespace Template.Core.IoC.Config.Auth;

public class PermissaoHandler : AuthorizationHandler<PermissaoRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissaoRequirement requirement)
    {
        var roleClaim = context.User.FindFirstValue(ClaimTypes.Role) ?? context.User.FindFirstValue("role");
        if (roleClaim is null || !Enum.TryParse(roleClaim, out UsuarioRoleEnum role))
        {
            return Task.CompletedTask;
        }

        if (PermissoesPorRole.TemPermissao(role, requirement.Permissao))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
