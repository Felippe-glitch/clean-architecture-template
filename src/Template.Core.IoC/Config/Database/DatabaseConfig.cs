using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Template.Core.Infra;
using Template.Core.Infra.Settings;

namespace Template.Core.IoC.Config.Database;

public static class DatabaseConfig
{
    public static IServiceCollection AddPostgreSqlContext(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        PostgreSqlSettings psqlSettings = new();
        configuration.GetSection("Databases:PostgreSql").Bind(psqlSettings);

        if (string.IsNullOrWhiteSpace(psqlSettings.Host))
            throw new Exception("Host not provided");

        services.AddSingleton(psqlSettings);

        services.AddDbContext<TemplateDbContext>(options =>
        {
            options.UseNpgsql(psqlSettings.ConnectionString);

            if (environment.IsDevelopment())
                options.EnableSensitiveDataLogging().EnableDetailedErrors();
        });

        return services;
    }

    public static async Task MigrateDatabaseAsync(this IServiceProvider provider, CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = provider.CreateScope();
        TemplateDbContext dbContext = scope.ServiceProvider.GetRequiredService<TemplateDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
