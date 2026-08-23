using System.ComponentModel;

namespace Template.Core.Domain.Usuarios.Enums;

/// <summary>
/// Permissões granulares para acesso aos recursos do sistema (Policy-based Authorization).
/// </summary>
public enum Permissao
{
    // Pessoas
    [Description("Listar Pessoas")] Pessoas_Listar,
    [Description("Visualizar Pessoas")] Pessoas_Visualizar,
    [Description("Criar Pessoas")] Pessoas_Criar,
    [Description("Editar Pessoas")] Pessoas_Editar,
    [Description("Excluir Pessoas")] Pessoas_Excluir,

    // Departamentos
    [Description("Listar Departamentos")] Departamentos_Listar,
    [Description("Visualizar Departamentos")] Departamentos_Visualizar,
    [Description("Criar Departamentos")] Departamentos_Criar,
    [Description("Editar Departamentos")] Departamentos_Editar,
    [Description("Excluir Departamentos")] Departamentos_Excluir,

    // Eventos
    [Description("Listar Eventos")] Eventos_Listar,
    [Description("Visualizar Eventos")] Eventos_Visualizar,
    [Description("Criar Eventos")] Eventos_Criar,
    [Description("Editar Eventos")] Eventos_Editar,
    [Description("Excluir Eventos")] Eventos_Excluir,

    // Notícias
    [Description("Listar Notícias")] Noticias_Listar,
    [Description("Visualizar Notícias")] Noticias_Visualizar,
    [Description("Criar Notícias")] Noticias_Criar,
    [Description("Editar Notícias")] Noticias_Editar,
    [Description("Excluir Notícias")] Noticias_Excluir,

    // Usuários
    [Description("Gestão de Usuários")] Usuarios_Gerenciar,
}
