using Serilog.Context;

namespace ArkahBusiness.API.Middlewares;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId);
        string id = string.IsNullOrEmpty(correlationId) ? Guid.NewGuid().ToString() : correlationId.ToString();

        context.Response.Headers["X-Correlation-ID"] = id;

        using (LogContext.PushProperty("CorrelationId", id))
        {
            await _next(context);
        }
    }
}