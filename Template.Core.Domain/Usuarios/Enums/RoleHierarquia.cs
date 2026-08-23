namespace Template.Core.Domain.Usuarios.Enums;

/// <summary>
/// Hierarquia de gestao de contas. Cada papel so pode criar/gerenciar usuarios
/// cujos papeis estejam estritamente "abaixo" dele:
/// ADMIN (unico, do seed) gerencia apenas DEVELOPER; DEVELOPER gerencia
/// SECRETARIA, PASTOR e MIDIA; os demais nao gerenciam usuarios.
/// </summary>
public static class RoleHierarquia
{
    private static readonly IReadOnlyDictionary<UsuarioRoleEnum, HashSet<UsuarioRoleEnum>> Gerenciaveis =
        new Dictionary<UsuarioRoleEnum, HashSet<UsuarioRoleEnum>>
        {
            [UsuarioRoleEnum.ADMIN] = [UsuarioRoleEnum.DEVELOPER],
            [UsuarioRoleEnum.DEVELOPER] = [UsuarioRoleEnum.SECRETARIA, UsuarioRoleEnum.PASTOR, UsuarioRoleEnum.MIDIA],
        };

    /// <summary>Indica se <paramref name="ator"/> pode criar/gerenciar uma conta com o papel <paramref name="alvo"/>.</summary>
    public static bool PodeGerenciar(UsuarioRoleEnum ator, UsuarioRoleEnum alvo)
        => Gerenciaveis.TryGetValue(ator, out HashSet<UsuarioRoleEnum>? permitidos) && permitidos.Contains(alvo);
}
