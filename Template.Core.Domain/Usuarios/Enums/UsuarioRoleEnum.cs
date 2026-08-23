using System.ComponentModel;

namespace Template.Core.Domain.Usuarios.Enums;

public enum UsuarioRoleEnum
{
    [Description("Administrador")]
    ADMIN = 0,
    [Description("Desenvolvedor")]
    DEVELOPER = 1,
    [Description("Secretaria")]
    SECRETARIA = 2,
    [Description("Pastor")]
    PASTOR = 3,
    [Description("Midia")]
    MIDIA = 4,
}
