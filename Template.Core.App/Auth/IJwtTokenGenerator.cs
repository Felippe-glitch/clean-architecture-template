using Template.Core.Domain.Usuarios.Entity;
using System;

namespace Template.Core.App.Auth;

public interface IJwtTokenGenerator
{
    /// <summary>Gera o access token (curto) e devolve o token e o instante de expiração (UTC).</summary>
    (string Token, DateTime ExpiraEm) Gerar(Usuario usuario);

    /// <summary>Gera o refresh token (JWT longo, com audience de refresh) e sua expiração (UTC).</summary>
    (string Token, DateTime ExpiraEm) GerarRefresh(Usuario usuario);

    /// <summary>Valida um refresh token statelessly. Devolve o Id do usuário se válido, ou <c>null</c>.</summary>
    int? ValidarRefreshToken(string refreshToken);
}
