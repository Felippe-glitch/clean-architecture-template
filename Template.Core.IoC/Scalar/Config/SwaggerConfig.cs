using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Template.Core.IoC.Scalar.Settings.Scalar;
using Scalar.AspNetCore;

namespace Template.Core.IoC.Scalar.Config;

public static class ScalarConfig
{
    // Obs.: AddOpenApi() NAO fica aqui de proposito. Ele precisa ser chamado no
    // projeto da API (Program.cs) para o source generator de comentarios XML
    // conseguir interceptar a chamada e injetar summary/description no documento.
    public static WebApplication UseApiDocumentation(this WebApplication app, IConfiguration configuration)
    {
        var scalarConfig = configuration.GetSection("Scalar");
        var title = scalarConfig["Title"] ?? "Template - API";

        // AllowAnonymous explicito: o FallbackPolicy (RequireAuthenticatedUser) do JwtConfig vale
        // para todo endpoint sem metadata de autorizacao, e os do OpenAPI/Scalar nao tem nenhuma.
        // Sem isto a doc responde 401. So existe em Development (ver Program.cs).
        app.MapOpenApi().AllowAnonymous();
        app.MapScalarApiReference(options =>
        {
            options.WithTitle(title);

            // Bearer como esquema padrão: o campo de token já aparece pronto na request e
            // o Scalar persiste/anexa o Authorization automaticamente nas chamadas protegidas.
            options.AddPreferredSecuritySchemes("Bearer");

            var servers = scalarConfig.GetSection("Servers").Get<ServerConfig[]>();

            if (servers != null)
            {
                foreach (var server in servers)
                {
                    options.AddServer(server.Url, server.Name);
                }
            }
        }).AllowAnonymous();

        return app;
    }
}