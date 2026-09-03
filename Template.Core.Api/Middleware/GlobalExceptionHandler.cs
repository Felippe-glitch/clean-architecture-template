namespace Template.Core.Api.Middleware;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Template.Core.CrossCutting.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        // Mapped business exceptions carry intentional, safe messages (fine to send to the client);
        // the _ branch represents an unexpected error (500), whose Message may expose EF Core
        // internals, table/column names, etc. That's why it's masked outside Development.
        var (statusCode, message, exposeDetail) = exception switch
        {
            EntityDeactivatedException => (StatusCodes.Status409Conflict, "A conflict occurred", true),
            EntityNotFoundException => (StatusCodes.Status404NotFound, "Resource not found.", true),
            BusinessRuleException => (StatusCodes.Status422UnprocessableEntity, "Business rule violation", true),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized.", true),
            // Request errors from Kestrel/the pipeline itself (e.g., body over the limit -> 413);
            // honors the embedded status instead of masking it as 500.
            BadHttpRequestException bad => (bad.StatusCode, "Invalid request.", false),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid argument.", true),
            _ => (StatusCodes.Status500InternalServerError, "Internal error.", false)
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = message,
            Detail = (exposeDetail || _environment.IsDevelopment())
                ? exception.Message
                : "An unexpected error occurred. Please try again later."
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
