using Template.Core.Domain.Usuarios.Enums;

namespace Template.Core.Domain.Usuarios.Command;

/// <summary>
/// Comando de registro de usuário. A senha já chega <b>hasheada</b>
/// (a camada de aplicação faz o hash antes de acionar o domínio).
/// </summary>
public record UsuarioRegistrarCommand
{
    public string Login { get; set; }
    public string SenhaHash { get; set; }
    public string Email { get; set; }
    public UsuarioRoleEnum Role { get; set; }
}
