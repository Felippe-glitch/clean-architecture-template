using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Template.Core.IoC.Scalar.Settings.Scalar;
using Scalar.AspNetCore;

namespace Template.Core.IoC.Scalar.Config;

public static class ScalarConfig
{
    // Note: AddOpenApi() is intentionally NOT here. It needs to be called in the
    // API project (Program.cs) so the XML-comments source generator can intercept
    // the call and inject summary/description into the document.
    public static WebApplication UseApiDocumentation(this WebApplication app, IConfiguration configuration)
    {
        var scalarConfig = configuration.GetSection("Scalar");
        var title = scalarConfig["Title"] ?? "Template - API";

        // Explicit AllowAnonymous: the FallbackPolicy (RequireAuthenticatedUser) from JwtConfig
        // applies to every endpoint without authorization metadata, and the OpenAPI/Scalar ones
        // have none. Without this the docs would respond 401. Only exists in Development (see Program.cs).
        app.MapOpenApi().AllowAnonymous();
        app.MapScalarApiReference(options =>
        {
            options.WithTitle(title);

            // Bearer as the default scheme: the token field is already ready in the request and
            // Scalar persists/attaches the Authorization header automatically on protected calls.
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
