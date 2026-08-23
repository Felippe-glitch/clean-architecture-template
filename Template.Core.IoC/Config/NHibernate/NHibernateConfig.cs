using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Template.Core.IoC.Settings.NHibernate;

using NHibernate;
using NHibernate.Dialect;

namespace Template.Core.IoC.Config.Config;

public static class NHibernateConfig
{
    public static IServiceCollection AddPostgreSqlContext(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        NHibernateSettingsPostgres psqlSettings = new();

        configuration.GetSection("Databases:PostgreSql").Bind(psqlSettings);

        if (string.IsNullOrWhiteSpace(psqlSettings.Host))
            throw new Exception("Host não passado");

        if (string.IsNullOrWhiteSpace(psqlSettings.ConnectionString))
            throw new Exception("String de conexão não configurada");

        var databaseConfig = PostgreSQLConfiguration.Standard.ConnectionString(psqlSettings.ConnectionString)
            .Dialect<PostgreSQL82Dialect>();

        if (environment.IsDevelopment())
        {
            databaseConfig = databaseConfig.ShowSql().FormatSql();
        }

        if (!string.IsNullOrWhiteSpace(psqlSettings.Schema))
            databaseConfig = databaseConfig.DefaultSchema(psqlSettings.Schema);

        var sessionFactory = Fluently.Configure()
            .Database(databaseConfig)
            .Mappings(m => m.FluentMappings.AddFromAssemblyOf<Infra.Usuarios.Repository.UsuarioRepository>())
            .BuildSessionFactory();

        services.AddSingleton(sessionFactory);

        services.AddScoped(provider => provider.GetRequiredService<ISessionFactory>().OpenSession());

        return services;
    }
}