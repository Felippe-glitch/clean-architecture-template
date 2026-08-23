using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using Template.Core.Api.Config;
using Template.Core.App.Auth.DataTransfer;
using Template.Core.App.Auth.Service;

namespace Template.Core.Api.Auth;

/// <summary>
/// Autenticação: emissão de token JWT para acesso administrativo.
/// </summary>
/// <param name="authAppService">Serviço de aplicação responsável pela autenticação.</param>
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting(RateLimitConfig.AuthPolicyName)]
public class AuthController(IAuthAppService authAppService) : ControllerBase
{
    /// <summary>
    /// Autentica um usuário e devolve um token JWT.
    /// </summary>
    /// <remarks>
    /// Exemplo de requisição:
    ///
    ///     POST /api/Auth/login
    ///     { "login": "admin", "senha": "..." }
    ///
    /// Use o token no header <c>Authorization: Bearer &lt;token&gt;</c> nas rotas protegidas.
    /// </remarks>
    /// <response code="200">Retorna o token e os dados do usuário.</response>
    /// <response code="401">Login ou senha inválidos.</response>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
    {
        return Ok(await authAppService.LoginAsync(request, cancellationToken));
    }

    /// <summary>
    /// Renova a sessão a partir de um refresh token válido, emitindo um novo par access/refresh.
    /// </summary>
    /// <remarks>
    /// O refresh é <b>stateless</b> (ADR 08): o anterior <b>não</b> é rotacionado nem revogado e
    /// continua válido até expirar. Não existe logout server-side — revogação de sessão é o
    /// achado F4 do laudo de julho/2026, planejado para a v0.0.2 (ATOS-81). Hoje, o único modo de
    /// invalidar sessões é rotacionar a chave de assinatura, o que derruba todas de uma vez.
    /// </remarks>
    /// <response code="200">Novo token e refresh token.</response>
    /// <response code="401">Refresh token inválido ou expirado.</response>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken = default)
    {
        return Ok(await authAppService.RefreshAsync(request.RefreshToken, cancellationToken));
    }
}
