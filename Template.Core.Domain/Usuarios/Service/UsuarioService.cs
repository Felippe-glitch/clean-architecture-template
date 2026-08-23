using Template.Core.Domain.Abstractions.Exceptions;
using Template.Core.Domain.Usuarios.Command;
using Template.Core.Domain.Usuarios.Entity;
using Template.Core.Domain.Usuarios.Enums;
using Template.Core.Domain.Usuarios.Repository;

namespace Template.Core.Domain.Usuarios.Service;

public class UsuarioService(IUsuarioRepository usuarioRepository) : IUsuarioService
{
    public async Task<Usuario> Registrar(UsuarioRegistrarCommand command, CancellationToken cancellationToken = default)
    {

        string login = Usuario.NormalizarLogin(command.Login);

        Usuario existente = await usuarioRepository.RecuperarPorLoginAsync(login, cancellationToken);
        if (existente is not null)
            throw new RegraDeNegocioVioladaException("Já existe um usuário com este login.");

        Usuario usuario = new(command.Login, command.SenhaHash, command.Email, command.Role);
        await usuarioRepository.InserirAsync(usuario, cancellationToken);

        return usuario;
    }

    public async Task<Usuario?> RecuperarPorLogin(string login, CancellationToken cancellationToken = default)
        => await usuarioRepository.RecuperarPorLoginAsync(Usuario.NormalizarLogin(login), cancellationToken);

    public async Task<Usuario> Validar(int id, CancellationToken cancellationToken = default)
        => await usuarioRepository.RecuperarAsync(id, cancellationToken)
           ?? throw new EntidadeNaoEncontradaException(nameof(Usuario));

    public async Task<Usuario> AlterarSituacao(
        int id,
        bool ativo,
        UsuarioRoleEnum atorRole,
        int atorId,
        CancellationToken cancellationToken = default)
    {
        if (id == atorId)
            throw new RegraDeNegocioVioladaException("Não é possível alterar a situação do próprio usuário.");

        Usuario usuario = await Validar(id, cancellationToken);
        if (!RoleHierarquia.PodeGerenciar(atorRole, usuario.Role))
            throw new RegraDeNegocioVioladaException("Seu perfil não pode gerenciar este usuário.");

        if (ativo)
            usuario.Ativar();
        else
            usuario.Desativar();

        await usuarioRepository.AtualizarAsync(usuario, cancellationToken);
        return usuario;
    }

    public async Task<bool> ExisteAlgum(CancellationToken cancellationToken = default)
        => await usuarioRepository.ExisteAlgumAsync(cancellationToken);
}
