using Mapster;

using Microsoft.Extensions.Caching.Memory;

using Template.Core.App.Auth.DataTransfer;
using Template.Core.App.Common;
using Template.Core.App.Usuarios.DataTransfer;
using Template.Core.Domain.Usuarios.Entity;
using Template.Core.Domain.Usuarios.Service;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace Template.Core.App.Auth.Service;

public class AuthAppService(
    IUsuarioService usuarioService,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator tokenGenerator,
    IMemoryCache cache,
    LoginLockoutSettings lockout) : IAuthAppService
{
    /// <summary>
    /// Mensagem única para credencial errada, conta inativa, conta inexistente e lockout.
    /// Se o lockout tivesse mensagem própria, viraria oráculo de enumeração de contas.
    /// </summary>
    private const string CredenciaisInvalidas = "Login ou senha inválidos.";

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        string chave = ChaveLockout(request.Login);
        cache.TryGetValue(chave, out int falhas);

        // Antes do lookup e do BCrypt: tira também a amplificação de CPU do hash como vetor.
        if (falhas >= lockout.LockoutFalhas)
            throw new UnauthorizedAccessException(CredenciaisInvalidas);

        Usuario? usuario = await usuarioService.RecuperarPorLogin(request.Login, cancellationToken);

        if (usuario is null || !usuario.Ativo || !passwordHasher.Verificar(request.Senha, usuario.SenhaHash))
        {
            RegistrarFalha(chave, falhas);
            throw new UnauthorizedAccessException(CredenciaisInvalidas);
        }

        cache.Remove(chave);

        return EmitirSessao(usuario);
    }

    public async Task<LoginResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        int? usuarioId = tokenGenerator.ValidarRefreshToken(refreshToken);
        if (usuarioId is null)
            throw new UnauthorizedAccessException("Refresh token inválido ou expirado.");

        Usuario usuario = await usuarioService.Validar(usuarioId.Value, cancellationToken);
        if (!usuario.Ativo)
            throw new UnauthorizedAccessException("Usuário inativo.");

        return EmitirSessao(usuario);
    }

    /// <summary>Chaveado pelo login normalizado — o mesmo critério do lookup, senão variar a caixa burla o lockout.</summary>
    private static string ChaveLockout(string login) => $"lockout:login:{Usuario.NormalizarLogin(login)}";

    /// <summary>
    /// Renova a janela a cada falha: o bloqueio dura <c>LockoutMinutos</c> a partir da
    /// última tentativa, então insistir só prolonga o próprio bloqueio.
    /// </summary>
    private void RegistrarFalha(string chave, int falhas)
    {
        cache.Set(chave, falhas + 1, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(lockout.LockoutMinutos),
        });
    }

    /// <summary>Emite um novo par access + refresh (ambos JWT, stateless).</summary>
    private LoginResponse EmitirSessao(Usuario usuario)
    {
        (string token, DateTime expiraEm) = tokenGenerator.Gerar(usuario);
        (string refresh, DateTime refreshExpiraEm) = tokenGenerator.GerarRefresh(usuario);

        return new LoginResponse
        {
            Token = token,
            ExpiraEm = expiraEm,
            RefreshToken = refresh,
            RefreshExpiraEm = refreshExpiraEm,
            Usuario = usuario.Adapt<UsuarioResponse>(),
        };
    }
}
