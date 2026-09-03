using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Template.Core.Api.Config;

/// <summary>
/// Request hardening: request body size cap (Kestrel) and a default per-request timeout.
/// Protects public endpoints against oversized payloads and requests that hold a
/// connection open. Values live under <c>RequestLimits</c>. Lives in the API project
/// because it depends on the ASP.NET Core framework (Kestrel/RequestTimeouts).
/// </summary>
public static class RequestHardeningConfig
{
    public static IServiceCollection AddRequestHardening(this IServiceCollection services, IConfiguration configuration)
    {
        // 12 MB by default: accommodates typical multipart uploads without leaving room
        // for abuse. Above that, Kestrel responds with 413.
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
