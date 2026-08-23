using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using Template.Core.App.Auth;
using Template.Core.Domain.Usuarios.Enums;

namespace Template.Core.IoC.Config.Auth;

public static class JwtConfig
{
    public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration configuration)
    {
        JwtSettings settings = new();
        configuration.GetSection("Jwt").Bind(settings);

        if (!settings.EstaConfigurado)
            throw new Exception("Chave do JWT não configurada (seção 'Jwt:Key').");

        if (!settings.ChaveTemForcaSuficiente)
            throw new Exception(
                $"Chave do JWT fraca: 'Jwt:Key' tem {settings.TamanhoChaveBytes} bytes e o mínimo " +
                $"para HS256 é {JwtSettings.TamanhoMinimoChaveBytes}. Gere uma nova com " +
                "'openssl rand -base64 48' e aplique em Jwt__Key.");

        services.AddSingleton(settings);
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = settings.Issuer,
                    ValidAudience = settings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)),
                    ClockSkew = TimeSpan.FromSeconds(30),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var roleClaim = context.Principal?.FindFirst(ClaimTypes.Role) ?? context.Principal?.FindFirst("role");
                        if (roleClaim is null || string.IsNullOrWhiteSpace(roleClaim.Value))
                        {
                            context.Fail("Token JWT não possui claim obrigatória de Role.");
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddSingleton<IAuthorizationHandler, PermissaoHandler>();

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            foreach (Permissao permissao in Enum.GetValues<Permissao>())
            {
                options.AddPolicy(permissao.ToString(), policy =>
                    policy.Requirements.Add(new PermissaoRequirement(permissao)));
            }
        });

        return services;
    }
}
