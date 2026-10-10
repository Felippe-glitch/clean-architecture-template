using Template.Core.IoC;
using Serilog;

using Template.Core.Api.Config;
using Template.Core.Api.Middleware;
using Template.Core.IoC.Config;
using Template.Core.IoC.Config.Auth;
using Template.Core.IoC.Config.Database;
using Template.Core.IoC.Scalar.Config;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Host.ConfigureSerilog(configuration, builder.Environment);
builder.Services.AddCommonServices(configuration, builder.Environment);
builder.Services.AddCustomRateLimiter(configuration);
builder.Services.AddRequestHardening(configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi(options => options.AddBearerSecurity());

builder.Services
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddProblemDetails();

builder.Services.AddHealthChecks();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));

// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("Local", policy =>
//     {
//         policy
//             .AllowAnyOrigin()
//             .AllowAnyHeader()
//             .AllowAnyMethod();
//     });
// });

var app = builder.Build();

app.UseSecurityHeaders(app.Environment);
app.UseSerilogRequestLogging();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Template Core v1");
        c.RoutePrefix = "swagger";
    });

    app.UseApiDocumentation(configuration);
}

// app.UseCors("Local");
app.UseRateLimiter();
app.UseRequestTimeouts();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

await app.Services.MigrateDatabaseAsync();
await app.Services.SeedAdminAsync(configuration);

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}

