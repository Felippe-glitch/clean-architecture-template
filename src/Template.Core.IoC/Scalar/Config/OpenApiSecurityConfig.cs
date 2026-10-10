using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Template.Core.IoC.Scalar.Config;

/// <summary>
/// Declares the Bearer (JWT) security scheme in the OpenAPI document so Scalar shows the
/// authentication field, applying the requirement only to protected operations (those with
/// <c>[Authorize]</c> and without <c>[AllowAnonymous]</c>).
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
                Description = "Enter the JWT token issued by /api/Auth/login."
            };
            return Task.CompletedTask;
        });

        options.AddOperationTransformer((operation, context, cancellationToken) =>
        {
            IList<object> metadata = context.Description.ActionDescriptor.EndpointMetadata;
            bool allowsAnonymous = metadata.OfType<IAllowAnonymous>().Any();
            bool requiresAuthorization = metadata.OfType<IAuthorizeData>().Any();

            if (requiresAuthorization && !allowsAnonymous)
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
