using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Template.Core.Api.Config;

/// <summary>
/// Endurecimento de request: teto de tamanho do corpo (Kestrel) e timeout padrão por
/// requisição. Protege endpoints públicos contra payloads gigantes e requests que
/// seguram conexão. Valores em <c>RequestLimits</c>. Fica no projeto da API por
/// depender do framework ASP.NET Core (Kestrel/RequestTimeouts).
/// </summary>
public static class RequestHardeningConfig
{
    public static IServiceCollection AddRequestHardening(this IServiceCollection services, IConfiguration configuration)
    {
        // 12 MB por padrão: acomoda o upload de flyer (multipart) do EventoController
        // sem abrir espaço para abuso. Acima disso o Kestrel responde 413.
        long maxBodyBytes = configuration.GetValue("RequestLimits:MaxBodyBytes", 12L * 1024 * 1024);
        int timeoutSeconds = configuration.GetValue("RequestLimits:TimeoutSeconds", 30);

        services.Configure<KestrelServerOptions>(options => options.Limits.MaxRequestBodySize = maxBodyBytes);

        services.AddRequestTimeouts(options =>
            options.DefaultPolicy = new RequestTimeoutPolicy
            {
                Timeout = TimeSpan.FromSeconds(timeoutSeconds),
                TimeoutStatusCode = StatusCodes.Status408RequestTimeout,
            });

        return services;
    }

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app, IHostEnvironment environment)
    {
        return app.Use(async (context, next) =>
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            if (!environment.IsDevelopment())
            {
                context.Response.Headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
            }

            await next();
        });
    }
}

