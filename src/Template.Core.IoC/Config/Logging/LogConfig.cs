using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.Grafana.Loki;
using Serilog.Sinks.OpenTelemetry;

namespace Template.Core.IoC.Config;

public static class LogConfig
{
    public static void ConfigureSerilog(this IHostBuilder host, IConfiguration configuration, IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
            Serilog.Debugging.SelfLog.Enable(Console.Error);

        host.UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", environment.IsDevelopment() ? LogEventLevel.Information : LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", environment.IsDevelopment() ? LogEventLevel.Information : LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("Application", ".TemplatePortal")
                .WriteTo.Console(new JsonFormatter());

            var otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
            var otlpHeaders = configuration["OTEL_EXPORTER_OTLP_HEADERS"] ?? Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_HEADERS");

            if (!string.IsNullOrWhiteSpace(otlpEndpoint))
            {
                var headers = new Dictionary<string, string>();
                if (!string.IsNullOrWhiteSpace(otlpHeaders))
                {
                    foreach (var header in otlpHeaders.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        var parts = header.Split('=', 2);
                        if (parts.Length == 2)
                        {
                            headers[parts[0].Trim()] = Uri.UnescapeDataString(parts[1].Trim());
                        }
                    }
                }

                var endpointLogs = otlpEndpoint.TrimEnd('/');
                if (!endpointLogs.EndsWith("/v1/logs", StringComparison.OrdinalIgnoreCase))
                    endpointLogs += "/v1/logs";

                loggerConfiguration.WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = endpointLogs;
                    options.Protocol = OtlpProtocol.HttpProtobuf;
                    options.Headers = headers;
                    options.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["service.name"] = "template-portal",
                        ["deployment.environment"] = environment.EnvironmentName.ToLowerInvariant()
                    };
                });
            }

            var lokiUrl = configuration["GrafanaLoki:Url"] ?? configuration["Loki:Url"];
            if (!string.IsNullOrWhiteSpace(lokiUrl))
            {
                var credentials = new LokiCredentials
                {
                    Login = configuration["GrafanaLoki:User"] ?? configuration["Loki:User"] ?? string.Empty,
                    Password = configuration["GrafanaLoki:Password"] ?? configuration["Loki:Password"] ?? string.Empty
                };

                loggerConfiguration.WriteTo.GrafanaLoki(
                    lokiUrl,
                    credentials: !string.IsNullOrEmpty(credentials.Login) ? credentials : null,
                    labels: new[]
                    {
                        new LokiLabel { Key = "app", Value = "template-portal" },
                        new LokiLabel { Key = "env", Value = environment.EnvironmentName.ToLowerInvariant() }
                    }
                );
            }
        });
    }
}

