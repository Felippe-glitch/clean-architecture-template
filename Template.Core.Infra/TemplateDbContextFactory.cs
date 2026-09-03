using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

using Template.Core.Infra.Settings;

namespace Template.Core.Infra;

/// <summary>
/// Builds a <see cref="TemplateDbContext"/> for `dotnet ef` (migrations add / database update)
/// without going through the app's DI container — the same appsettings the API reads at runtime
/// are read here too, so migrations are generated against the same connection/schema config.
/// Not used at application runtime; only picked up by the EF Core CLI tooling.
/// </summary>
public class TemplateDbContextFactory : IDesignTimeDbContextFactory<TemplateDbContext>
{
    public TemplateDbContext CreateDbContext(string[] args)
    {
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        string apiProjectPath = FindApiProjectPath();

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        PostgreSqlSettings settings = new();
        configuration.GetSection("Databases:PostgreSql").Bind(settings);

        if (string.IsNullOrWhiteSpace(settings.Host))
            throw new InvalidOperationException(
                "Databases:PostgreSql:Host could not be resolved for `dotnet ef`. Run the command " +
                "from the repository root (so Template.Core.Api/appsettings.Development.json is " +
                "found), or set the Databases__PostgreSql__* environment variables — e.g. " +
                "Databases__PostgreSql__Host=localhost.");

        DbContextOptionsBuilder<TemplateDbContext> optionsBuilder = new();
        optionsBuilder.UseNpgsql(settings.ConnectionString);

        return new TemplateDbContext(optionsBuilder.Options, settings);
    }

    private static string FindApiProjectPath()
    {
        const string apiFolderName = "Template.Core.Api";

        string[] candidates =
        [
            Path.Combine(Directory.GetCurrentDirectory(), apiFolderName),
            Path.Combine(Directory.GetCurrentDirectory(), "..", apiFolderName),
            Directory.GetCurrentDirectory(),
        ];

        return candidates.FirstOrDefault(Directory.Exists) ?? Directory.GetCurrentDirectory();
    }
}
