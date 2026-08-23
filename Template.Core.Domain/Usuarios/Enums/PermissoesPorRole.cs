namespace Template.Core.Domain.Usuarios.Enums;

/// <summary>
/// Mapeamento de permissões granulares atribuídas a cada papel (Role) do sistema.
/// </summary>
public static class PermissoesPorRole
{
    private static readonly IReadOnlyDictionary<UsuarioRoleEnum, HashSet<Permissao>> Mapeamento =
        new Dictionary<UsuarioRoleEnum, HashSet<Permissao>>
        {
            [UsuarioRoleEnum.ADMIN] = ObterTodas(),
            [UsuarioRoleEnum.DEVELOPER] = ObterTodas(),

            [UsuarioRoleEnum.SECRETARIA] =
            [
                Permissao.Pessoas_Listar, Permissao.Pessoas_Visualizar, Permissao.Pessoas_Criar, Permissao.Pessoas_Editar,
                Permissao.Departamentos_Listar, Permissao.Departamentos_Visualizar, Permissao.Departamentos_Criar, Permissao.Departamentos_Editar,
                Permissao.Eventos_Listar, Permissao.Eventos_Visualizar, Permissao.Eventos_Criar, Permissao.Eventos_Editar,
                Permissao.Noticias_Listar, Permissao.Noticias_Visualizar, Permissao.Noticias_Criar, Permissao.Noticias_Editar,
            ],

            [UsuarioRoleEnum.PASTOR] =
            [
                Permissao.Pessoas_Listar, Permissao.Pessoas_Visualizar, Permissao.Pessoas_Criar, Permissao.Pessoas_Editar,
                Permissao.Departamentos_Listar, Permissao.Departamentos_Visualizar, // Sem criar/editar/excluir departamentos
                Permissao.Eventos_Listar, Permissao.Eventos_Visualizar, Permissao.Eventos_Criar, Permissao.Eventos_Editar,
                Permissao.Noticias_Listar, Permissao.Noticias_Visualizar, Permissao.Noticias_Criar, Permissao.Noticias_Editar,
            ],

            [UsuarioRoleEnum.MIDIA] =
            [
                // Leitura de departamentos e pre-requisito de criar/editar evento: o formulario do
                // Admin popula o select de departamento por GET /api/Departamento. Sem isto o campo
                // vem vazio (o servico do portal engole o 403), nao ha escrita liberada aqui.
                Permissao.Departamentos_Listar, Permissao.Departamentos_Visualizar,
                Permissao.Eventos_Listar, Permissao.Eventos_Visualizar, Permissao.Eventos_Criar, Permissao.Eventos_Editar,
                Permissao.Noticias_Listar, Permissao.Noticias_Visualizar, Permissao.Noticias_Criar, Permissao.Noticias_Editar,
            ],
        };

    /// <remarks>
    /// Devolve <see cref="IReadOnlySet{T}"/> de proposito: o conjunto retornado e a propria matriz de
    /// autorizacao do processo, e expo-la como <c>HashSet</c> deixaria qualquer chamador muta-la.
    /// </remarks>
    public static IReadOnlySet<Permissao> ObterPermissoes(UsuarioRoleEnum role)
        => Mapeamento.TryGetValue(role, out var permissoes) ? permissoes : [];

    public static bool TemPermissao(UsuarioRoleEnum role, Permissao permissao)
        => ObterPermissoes(role).Contains(permissao);

    private static HashSet<Permissao> ObterTodas()
        => new(Enum.GetValues<Permissao>());
}
