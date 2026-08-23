namespace Template.Core.Api.Middleware;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Template.Core.Domain.Abstractions.Exceptions;

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
        _logger.LogError(exception, "Ocorreu uma exceção não tratada: {Message}", exception.Message);

        // Exceções de negócio mapeadas carregam mensagens intencionais e seguras (podem ir ao cliente);
        // o ramo _ representa erro não previsto (500), cujo Message pode expor internals do NHibernate,
        // nomes de tabela/coluna, etc. Por isso ele é mascarado fora de Development.
        var (statusCode, message, exposeDetail) = exception switch
        {
            EntidadeDesativadaException => (StatusCodes.Status409Conflict, "Houve um conflito", true),
            EntidadeNaoEncontradaException => (StatusCodes.Status404NotFound, "Recurso não encontrado.", true),
            RegraDeNegocioVioladaException => (StatusCodes.Status422UnprocessableEntity, "Violação de regra de negocio", true),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Não autorizado.", true),
            // Erros de request do proprio Kestrel/pipeline (ex.: corpo acima do limite -> 413);
            // honra o status embutido em vez de mascarar como 500.
            BadHttpRequestException bad => (bad.StatusCode, "Requisição inválida.", false),
            ArgumentException => (StatusCodes.Status400BadRequest, "Argumento inválido.", true),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno.", false)
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = message,
            Detail = (exposeDetail || _environment.IsDevelopment())
                ? exception.Message
                : "Ocorreu um erro inesperado. Tente novamente mais tarde."
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; 
    }
}