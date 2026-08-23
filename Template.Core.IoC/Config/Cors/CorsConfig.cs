using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Template.Core.IoC.Config;

public static class CorsConfig
{
    public const string PolicyName = "FrontendPolicy";

    public static IServiceCollection AddCustomCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = ResolverOrigens(configuration);

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    private static string[] ResolverOrigens(IConfiguration configuration)
    {
        var origens = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        if (origens.Length == 0)
        {
            var csv = configuration["Cors:AllowedOriginsCsv"] ?? configuration["Cors:AllowedOrigins"];
            if (!string.IsNullOrWhiteSpace(csv))
                origens = csv.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        return origens
            .Select(origem => origem.TrimEnd('/'))
            .Where(origem => !string.IsNullOrWhiteSpace(origem))
            .Distinct()
            .ToArray();
    }
}
