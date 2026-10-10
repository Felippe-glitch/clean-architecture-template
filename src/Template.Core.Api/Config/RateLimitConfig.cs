using System.Threading.RateLimiting;

using Microsoft.AspNetCore.RateLimiting;

namespace Template.Core.Api.Config;

/// <summary>
/// Global per-IP rate limiting (fixed window). Protects the public portal (anonymous
/// endpoints) and everything else against scraping/abuse. Values live under <c>RateLimit</c>.
/// Lives in the API project because it depends on the ASP.NET Core framework (RateLimiting/HttpOverrides).
/// </summary>
public static class RateLimitConfig
{
    /// <summary>Name of the strict policy for the authentication endpoints.</summary>
    public const string AuthPolicyName = "auth";

    public static IServiceCollection AddCustomRateLimiter(this IServiceCollection services, IConfiguration configuration)
    {
        int permitLimit = configuration.GetValue("RateLimit:PermitLimit", 100);
        int windowSeconds = configuration.GetValue("RateLimit:WindowSeconds", 60);
        int queueLimit = configuration.GetValue("RateLimit:QueueLimit", 0);

        // Strict policy for /api/Auth (login/refresh): reduces credential brute-forcing.
        int authPermitLimit = configuration.GetValue("RateLimit:Auth:PermitLimit", 10);
        int authWindowSeconds = configuration.GetValue("RateLimit:Auth:WindowSeconds", 60);

        // The real client IP (X-Forwarded-For) is already resolved by UseForwardedHeaders
        // configured in Program.cs; here we just partition by that IP.
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        Window = TimeSpan.FromSeconds(windowSeconds),
                        QueueLimit = queueLimit,
                        AutoReplenishment = true,
                    }));

            // Applied via [EnableRateLimiting("auth")]; stacks with the GlobalLimiter (the
            // request must pass both), narrowing authentication attempts further by IP.
            options.AddPolicy(AuthPolicyName, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
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
                    "{\"title\":\"Too many requests\",\"status\":429,\"detail\":\"Request limit exceeded. Please try again shortly.\"}",
                    cancellationToken);
            };
        });

        return services;
    }
}
