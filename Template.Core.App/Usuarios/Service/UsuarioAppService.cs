using Mapster;
using Microsoft.Extensions.Logging;

using Template.Core.App.Common;
using Template.Core.App.Usuarios.DataTransfer;
using Template.Core.Domain.Usuarios.Command;
using Template.Core.Domain.Usuarios.Entity;
using Template.Core.Domain.Usuarios.Enums;
using Template.Core.Domain.Usuarios.Service;
using Template.Core.Domain.Usuarios.Repository;
using System.Threading.Tasks;
using System.Threading;
using Template.Core.Domain.Usuarios.Repository.Filters;
using System.Collections.Generic;

namespace Template.Core.App.Usuarios.Service;

public class UsuarioAppService(
    IUsuarioService usuarioService,
    IPasswordHasher passwordHasher,
    IUsuarioRepository usuarioRepository,
    ILogger<UsuarioAppService> logger,
    IUnitOfWork unitOfWork) : IUsuarioAppService
{
    public async Task<UsuarioResponse> RegistrarAsync(UsuarioRegistrarRequest request, CancellationToken cancellationToken)
    {
        try
        {
            UsuarioRegistrarCommand command = new()
            {
                Login = request.Login,
                SenhaHash = passwordHasher.Hash(request.Senha),
                Email = request.Email,
                // O [Required] do DTO já barrou o null no model binding.
                Role = request.Role!.Value,
            };

            unitOfWork.BeginTransaction();
            Usuario criado = await usuarioService.Registrar(command, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            return criado.Adapt<UsuarioResponse>();
        }
        catch
        {
            logger.LogError("Erro ao registrar usuário com login {Login}", request.Login);
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<UsuarioResponse> AlterarSituacaoAsync(
        int id,
        bool ativo,
        UsuarioRoleEnum atorRole,
        int atorId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            unitOfWork.BeginTransaction();
            Usuario usuario = await usuarioService.AlterarSituacao(id, ativo, atorRole, atorId, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            return usuario.Adapt<UsuarioResponse>();
        }
        catch
        {
            logger.LogError("Erro ao alterar a situação do usuário com id {Id}", id);
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public Task<ListarUsuarioResponse> ListarAsync(ListarUsuarioRequest request, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }

    // public async Task<ListarUsuarioResponse> ListarAsync(
    //     ListarUsuarioRequest request,
    //     CancellationToken cancellationToken)
    // {
    //     ListarUsuarioFilter filtros = new()
    //     {
    //         Ativo = request.Ativo
    //     };

    //     var resultado = await usuarioRepository.Filtrar(
    //         filtros,
    //         cancellationToken);

    //     return new ListarUsuarioResponse
    //     {
    //         Total = resultado.Total,
    //         Data = resultado.Itens.Adapt<List<UsuarioResponse>>()
    //     };
    // }

}
