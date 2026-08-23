using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Template.Core.App.Auth;
using Template.Core.App.Auth.Service;
using Template.Core.App.Common;
using Template.Core.App.Usuarios.Service;
using Template.Core.Domain.Usuarios.Repository;
using Template.Core.Domain.Usuarios.Service;
using Template.Core.Infra.Usuarios.Repository;
using Template.Core.IoC.Config;
using Template.Core.IoC.Config.Auth;
using Template.Core.IoC.Config.Config;

namespace Template.Core.IoC;

 /// <summary>
 /// Classe responsavel por injetar os serviços por contexto na api
 /// </summary>
public static class NativeInjectorBoostraper
{
    public static IServiceCollection AddCommonServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        return services
                .AddCustomMapster()
                .AddCustomCors(configuration)
                .AddInfra(configuration, environment)
                .AddApplication(configuration)
                .AddJwtAuth(configuration)
                .AddDomain();
    }

    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddScoped<IUsuarioService, UsuarioService>();

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        LoginLockoutSettings lockout = new();
        configuration.GetSection("RateLimit:Auth").Bind(lockout);
        services.AddSingleton(lockout);

        services.AddMemoryCache();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IAuthAppService, AuthAppService>();
        services.AddScoped<IUsuarioAppService, UsuarioAppService>();

        return services;
    }

    public static IServiceCollection AddInfra(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddPostgreSqlContext(configuration, environment);
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        return services;
    }
}
