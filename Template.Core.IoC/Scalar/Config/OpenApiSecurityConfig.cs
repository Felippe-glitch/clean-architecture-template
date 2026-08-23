using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Template.Core.IoC.Scalar.Config;

/// <summary>
/// Declara o esquema de seguranca Bearer (JWT) no documento OpenAPI para o Scalar
/// exibir o campo de autenticacao, aplicando o requisito apenas nas operacoes
/// protegidas (com <c>[Authorize]</c> e sem <c>[AllowAnonymous]</c>).
/// </summary>
public static class OpenApiSecurityConfig
{
    private const string SchemeId = "Bearer";

    public static OpenApiOptions AddBearerSecurity(this OpenApiOptions options)
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes[SchemeId] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Informe o token JWT emitido em /api/Auth/login."
            };
            return Task.CompletedTask;
        });

        options.AddOperationTransformer((operation, context, cancellationToken) =>
        {
            IList<object> metadata = context.Description.ActionDescriptor.EndpointMetadata;
            bool permiteAnonimo = metadata.OfType<IAllowAnonymous>().Any();
            bool exigeAutorizacao = metadata.OfType<IAuthorizeData>().Any();

            if (exigeAutorizacao && !permiteAnonimo)
            {
                OpenApiSecuritySchemeReference reference = new(SchemeId, context.Document);
                operation.Security ??= [];
                operation.Security.Add(new OpenApiSecurityRequirement { [reference] = [] });
            }

            return Task.CompletedTask;
        });

        return options;
    }
}
