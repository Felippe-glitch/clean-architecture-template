using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Template.Core.IoC.Config;

public static class MapsterConfig
{
    public static IServiceCollection AddCustomMapster(this IServiceCollection services)
    {
        TypeAdapterConfig config = TypeAdapterConfig.GlobalSettings;


        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}