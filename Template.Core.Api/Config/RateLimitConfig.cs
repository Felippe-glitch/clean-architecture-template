using System.Threading.RateLimiting;

using Microsoft.AspNetCore.RateLimiting;

namespace Template.Core.Api.Config;

/// <summary>
/// Rate limiting global por IP (fixed window). Protege o portal público (endpoints
/// anônimos) e todo o restante contra scraping/abuso. Valores em <c>RateLimit</c>.
/// Fica no projeto da API por depender do framework ASP.NET Core (RateLimiting/HttpOverrides).
/// </summary>
public static class RateLimitConfig
{
    /// <summary>Nome da policy estrita para os endpoints de autenticação.</summary>
    public const string AuthPolicyName = "auth";

    public static IServiceCollection AddCustomRateLimiter(this IServiceCollection services, IConfiguration configuration)
    {
        int permitLimit = configuration.GetValue("RateLimit:PermitLimit", 100);
        int windowSeconds = configuration.GetValue("RateLimit:WindowSeconds", 60);
        int queueLimit = configuration.GetValue("RateLimit:QueueLimit", 0);

        // Politica estrita para /api/Auth (login/refresh): reduz forca-bruta de credenciais.
        int authPermitLimit = configuration.GetValue("RateLimit:Auth:PermitLimit", 10);
        int authWindowSeconds = configuration.GetValue("RateLimit:Auth:WindowSeconds", 60);

        // O IP real do cliente (X-Forwarded-For) já é resolvido pelo UseForwardedHeaders
        // configurado no Program.cs; aqui apenas particionamos por esse IP.
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        Window = TimeSpan.FromSeconds(windowSeconds),
                        QueueLimit = queueLimit,
                        AutoReplenishment = true,
                    }));

            // Aplicada via [EnableRateLimiting("auth")]; soma-se ao GlobalLimiter (o request
            // precisa passar nos dois), afunilando as tentativas de autenticacao por IP.
            options.AddPolicy(AuthPolicyName, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = authPermitLimit,
                        Window = TimeSpan.FromSeconds(authWindowSeconds),
                        QueueLimit = 0,
                        AutoReplenishment = true,
                    }));

            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();

                context.HttpContext.Response.ContentType = "application/problem+json";
                await context.HttpContext.Response.WriteAsync(
                    "{\"title\":\"Muitas requisicoes\",\"status\":429,\"detail\":\"Limite de requisicoes excedido. Tente novamente em instantes.\"}",
                    cancellationToken);
            };
        });

        return services;
    }
}
