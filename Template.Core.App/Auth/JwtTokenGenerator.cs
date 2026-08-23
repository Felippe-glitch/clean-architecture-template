using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.IdentityModel.Tokens;

using Template.Core.Domain.Usuarios.Entity;
using System;

namespace Template.Core.App.Auth;

public class JwtTokenGenerator(JwtSettings settings) : IJwtTokenGenerator
{
    private SymmetricSecurityKey Chave => new(Encoding.UTF8.GetBytes(settings.Key));

    public (string Token, DateTime ExpiraEm) Gerar(Usuario usuario)
    {
        DateTime expiraEm = DateTime.UtcNow.AddMinutes(settings.ExpiraMinutos);

        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(ClaimTypes.Role, usuario.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        ];

        return (Escrever(claims, settings.Audience, expiraEm), expiraEm);
    }

    public (string Token, DateTime ExpiraEm) GerarRefresh(Usuario usuario)
    {
        DateTime expiraEm = DateTime.UtcNow.AddDays(settings.RefreshExpiraDias);

        // Refresh carrega o mínimo: só identifica o usuário. A audience distinta o separa do access.
        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        ];

        return (Escrever(claims, settings.RefreshAudience, expiraEm), expiraEm);
    }

    public int? ValidarRefreshToken(string refreshToken)
    {
        TokenValidationParameters parametros = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = settings.Issuer,
            ValidAudience = settings.RefreshAudience, // rejeita access tokens (audience diferente)
            IssuerSigningKey = Chave,
            ClockSkew = TimeSpan.FromSeconds(30),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256], // só o algoritmo que emitimos
        };

        try
        {
            ClaimsPrincipal principal = new JwtSecurityTokenHandler()
                .ValidateToken(refreshToken, parametros, out _);

            string? sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                          ?? principal.FindFirstValue(ClaimTypes.NameIdentifier); // MapInboundClaims pode remapear 'sub'

            return int.TryParse(sub, out int id) ? id : null;
        }
        catch
        {
            return null; // assinatura/expiração/audience inválidas
        }
    }

    private string Escrever(Claim[] claims, string audience, DateTime expiraEm)
    {
        SigningCredentials credenciais = new(Chave, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new(
            issuer: settings.Issuer,
            audience: audience,
            claims: claims,
            expires: expiraEm,
            signingCredentials: credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
