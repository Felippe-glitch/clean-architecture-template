using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using Template.Core.App.Auth;

namespace Template.Core.IoC.Config.Auth;

public static class JwtConfig
{
    public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration configuration)
    {
        JwtSettings settings = new();
        configuration.GetSection("Jwt").Bind(settings);

        if (!settings.IsConfigured)
            throw new Exception("JWT signing key not configured (section 'Jwt:Key').");

        if (!settings.HasSufficientKeyStrength)
            throw new Exception(
                $"Weak JWT signing key: 'Jwt:Key' has {settings.SigningKeyBytes} bytes and the minimum " +
                $"for HS256 is {JwtSettings.MinimumSigningKeyBytes}. Generate a new one with " +
                "'openssl rand -base64 48' and set it in Jwt__Key.");

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
                            context.Fail("JWT token is missing the required Role claim.");
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
}
