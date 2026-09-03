using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.IdentityModel.Tokens;

using Template.Core.Domain.Users.Entity;
using System;

namespace Template.Core.App.Auth;

public class JwtTokenGenerator(JwtSettings settings) : IJwtTokenGenerator
{
    private SymmetricSecurityKey SigningKey => new(Encoding.UTF8.GetBytes(settings.Key));

    public (string Token, DateTime ExpiresAt) Generate(User user)
    {
        DateTime expiresAt = DateTime.UtcNow.AddMinutes(settings.ExpirationMinutes);

        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        ];

        return (WriteToken(claims, settings.Audience, expiresAt), expiresAt);
    }

    public (string Token, DateTime ExpiresAt) GenerateRefresh(User user)
    {
        DateTime expiresAt = DateTime.UtcNow.AddDays(settings.RefreshExpirationDays);

        // Refresh carries the bare minimum: just the user's identity. The distinct audience separates it from the access token.
        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        ];

        return (WriteToken(claims, settings.RefreshAudience, expiresAt), expiresAt);
    }

    public int? ValidateRefreshToken(string refreshToken)
    {
        TokenValidationParameters parameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = settings.Issuer,
            ValidAudience = settings.RefreshAudience, // rejects access tokens (different audience)
            IssuerSigningKey = SigningKey,
            ClockSkew = TimeSpan.FromSeconds(30),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256], // only the algorithm we issue
        };

        try
        {
            ClaimsPrincipal principal = new JwtSecurityTokenHandler()
                .ValidateToken(refreshToken, parameters, out _);

            string? sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                          ?? principal.FindFirstValue(ClaimTypes.NameIdentifier); // MapInboundClaims may remap 'sub'

            return int.TryParse(sub, out int id) ? id : null;
        }
        catch
        {
            return null; // invalid signature/expiration/audience
        }
    }

    private string WriteToken(Claim[] claims, string audience, DateTime expiresAt)
    {
        SigningCredentials credentials = new(SigningKey, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new(
            issuer: settings.Issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
