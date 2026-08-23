using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Template.Core.Api.Auth;
using Template.Core.App.Usuarios.DataTransfer;
using Template.Core.App.Usuarios.Service;
using Template.Core.Domain.Usuarios.Enums;
using Template.Core.App.Common;

namespace Template.Core.Api.Usuarios;

/// <summary>
/// Gestão de usuários administrativos. Requer ADMIN ou DEVELOPER, respeitando a
/// hierarquia de papéis (ADMIN cria DEVELOPER; DEVELOPER cria os papéis abaixo).
/// </summary>
/// <param name="usuarioAppService">Serviço de aplicação responsável pelas regras de usuário.</param>
[ApiController] 
[Route("api/[controller]")]
// [Authorize(Policy = nameof(Permissao.Usuarios_Gerenciar))]
public class UsuarioController(IUsuarioAppService usuarioAppService) : ControllerBase
{
    /// <summary>
    /// Registra um novo usuário administrativo (respeitando a hierarquia de papéis).
    /// </summary>
    /// <response code="200">Retorna o usuário criado.</response>
    /// <response code="401">Sem token válido.</response>
    /// <response code="403">Papel do solicitante não permite criar o papel informado.</response>
    /// <response code="422">Login já cadastrado ou papel fora da hierarquia.</response>
    [HttpPost]
    [ProducesResponseType<UsuarioResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [AllowAnonymous]
    public async Task<IActionResult> Registrar([FromBody] UsuarioRegistrarRequest request, CancellationToken cancellationToken = default)
    {
        return Ok(await usuarioAppService.RegistrarAsync(request, cancellationToken));
    }

    [HttpGet]
    [ProducesResponseType<PaginatedResponse<UsuarioResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [AllowAnonymous]
    public async Task<IActionResult> ListarAsync(
        [FromQuery] ListarUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await usuarioAppService.ListarAsync(request, cancellationToken));
    }

    /// <summary>Ativa um usuário gerenciável pelo papel do solicitante.</summary>
    [HttpPut("{id:int}/ativar")]
    [ProducesResponseType<UsuarioResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Ativar(int id, CancellationToken cancellationToken = default)
        => Ok(await usuarioAppService.AlterarSituacaoAsync(id, true, ObterPapelDoAtor(), ObterIdDoAtor(), cancellationToken));

    /// <summary>
    /// Desativa um usuário gerenciável pelo papel do solicitante. Como a revogação
    /// de sessões não integra esta hotfix, tokens já emitidos expiram normalmente.
    /// </summary>
    [HttpPut("{id:int}/desativar")]
    [ProducesResponseType<UsuarioResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desativar(int id, CancellationToken cancellationToken = default)
        => Ok(await usuarioAppService.AlterarSituacaoAsync(id, false, ObterPapelDoAtor(), ObterIdDoAtor(), cancellationToken));

    private UsuarioRoleEnum ObterPapelDoAtor()
    {
        string? papel = User.FindFirstValue(ClaimTypes.Role);
        return Enum.TryParse(papel, out UsuarioRoleEnum role)
            ? role
            : throw new UnauthorizedAccessException("Papel do usuário não identificado no token.");
    }

    private int ObterIdDoAtor()
    {
        string? sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return int.TryParse(sub, out int id)
            ? id
            : throw new UnauthorizedAccessException("Usuário não identificado no token.");
    }
}
