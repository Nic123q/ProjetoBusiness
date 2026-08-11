using System.Net;
using System.Text.Json;

namespace ArkahBusiness.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, mensagem) = ex switch
        {
            ArgumentException => (HttpStatusCode.BadRequest, ex.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Você não tem permissão para executar esta ação."),
            KeyNotFoundException => (HttpStatusCode.NotFound, ex.Message),
            InvalidOperationException => (HttpStatusCode.Conflict, ex.Message),
            _ => (HttpStatusCode.InternalServerError, "Ocorreu um erro inesperado. Nossa equipe já foi notificada.")
        };

        if ((int)statusCode >= 500)
        {
            _logger.LogError(ex, "Erro não tratado na requisição {Method} {Path}", context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning(ex, "Requisição rejeitada: {Method} {Path} -> {StatusCode}", context.Request.Method, context.Request.Path, (int)statusCode);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var correlationId = context.Response.Headers.TryGetValue("X-Correlation-ID", out var id)
            ? id.ToString()
            : null;

        var resposta = new
        {
            erro = mensagem,
            correlationId,
            statusCode = (int)statusCode
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(resposta));
    }
}