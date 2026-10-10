using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Template.Core.App.Auth;
using Template.Core.App.Auth.Service;
using Template.Core.App.Common;
using Template.Core.App.Users.Interfaces.Service;
using Template.Core.App.Users.Service;
using Template.Core.CrossCutting.Security;
using Template.Core.Domain.Users.Interfaces.Service;
using Template.Core.Domain.Users.Repository;
using Template.Core.Domain.Users.Service;
using Template.Core.Infra.Common;
using Template.Core.Infra.Users.Repository;
using Template.Core.IoC.Config;
using Template.Core.IoC.Config.Auth;
using Template.Core.IoC.Config.Database;

namespace Template.Core.IoC;

 /// <summary>
 /// Composition root responsible for registering the application's services, per context, into the API.
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
        services.AddScoped<IUserService, UserService>();

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        LoginLockoutSettings lockout = new();
        configuration.GetSection("RateLimit:Auth").Bind(lockout);
        services.AddSingleton(lockout);

        services.AddMemoryCache();

        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IAuthAppService, AuthAppService>();
        services.AddScoped<IUserAppService, UserAppService>();

        return services;
    }

    public static IServiceCollection AddInfra(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddPostgreSqlContext(configuration, environment);
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
